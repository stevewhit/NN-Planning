using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.Datasets;
using NNLib.NeuralNetworking.NetworkLayers;
using NNLib.NeuralNetworking.Neurons;
using SM.Data;
using SM.Data.Importing;
using NNLib.NeuralNetworking.ActivationFunctions;
using NNLib.NeuralNetworking;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace SMNN
{
	public static class RunThisSMNN
	{
	   /*
			Things to change
			- Input/output methods

			Independent Variables

			- Network
				- # hidden neurons
				- # hidden layers
			- Learning
				- Learning rate --> Pretty easy to manipulate. Don't need to worry about this.
				- (Dataset) Duration (Amount of weeks to train with) 'numTrainingWeeks'
				- (Dataset) Technical Indicators (Different periods to use)
			- Testing
				- Fault Tolerance (How close the final activations are required to be to be considered correct)
		*/

		public static void Run()
		{
            #region Variables

            var allFileStockData = new StockData();
            var dataInputFilePath = @"../../../../Data/AAPL_2017_PriceHistory.csv";

            var outputFilePath = @"../../../../TrainingOutputs.txt";
            var outputFileText = new List<string>();
            
            var minNumCycles = 5000;                                       // The minimum number of training/testing cycles to go through before worrying about checking averages. (Sometimes it takes the network a lot of cycles to begin learning)
            var checkAvgAfterCyclesNum = 2500;                              // The number of cycles to skip before checking the percent correct averages.
            var minAmountIncrease = 0.1;                                    // The minimum-allowed average increase in the amount correct for the network to continue learning.

            #endregion

            #region Independent Variables

            // Different periods for the technical indicators.
            //var indicatorPeriods = new[] { 3 };
            var indicatorPeriodsCollection = new List<int[]>
            {
                new[]{ 3, 7 },
                new[]{ 3, 7, 14},
                new[]{ 7 },
                new[]{ 7, 14 },
                new[]{ 14 },
                new[]{ 3, 14 }
            };

			// The number of weeks of data used to train the network.
			var numTrainingWeeks = 3;

			// The number of hidden layers in the network.
			var numHiddenLayers = 1;

			// The number of neurons in each network hidden layer.
			var numHiddenLayerNeurons = 5;

			// The learning rate of the neural network
			var learningRate = .001;

			#endregion
            
			try
			{
				// Import dataset from file.
				allFileStockData = StockDataImporter.ImportBarchartDailyStockDataCSV(dataInputFilePath);

				if (allFileStockData == null || !allFileStockData.IsValidData())
					throw new ArgumentException("The supplied file is either empty or contains invalid data.");
			}
			catch(Exception e)
			{
				Console.WriteLine($"ABORTING -- There was an error processing the supplied data file:\n{System.IO.Path.GetFullPath(dataInputFilePath)}\n\n\n{e.Message}\n\n{e.StackTrace}");
				return;
			}
			
			var readingDates = allFileStockData.SimpleReadings.Select(reading => reading.Date).OrderBy(reading => reading.Date).ToList();

            var tasksToWaitOn = new List<Task>();
            var watch = new Stopwatch();
            watch.Start();

            // Incremental variable that is being tested.
            foreach (var indicatorPeriods in indicatorPeriodsCollection)
            {
                var t = Task.Factory.StartNew(() =>
                {
                    var numberCycles = 0;                                           // Iterator for counting the progress of the training.
                    var resultsDict = new Dictionary<double, int>();                // The dictionary that holds the overall results for each training/testing cycle.

                    DateTime trainingStartDate, trainingEndDate, testDate = DateTime.MinValue;
                    var testDateIndex = -1;

                    while (true)
                    {
                        // Identify where the last-set test date is located
                        testDateIndex++;

                        // Identify training dates.
                        var trainingStartIndex = testDateIndex - (numTrainingWeeks * 5);
                        var trainingEndIndex = testDateIndex - 1;

                        // If we've over-extended the date list, break;
                        if (testDateIndex >= readingDates.Count())
                            break;

                        // Make sure test date and training dates are indexed within the dataset.
                        if (trainingStartIndex <= indicatorPeriods.Max() + 5)
                            continue;

                        // Establish training & testing dates.
                        trainingStartDate = readingDates[trainingStartIndex];
                        trainingEndDate = readingDates[trainingEndIndex];
                        testDate = readingDates[testDateIndex];

                        // Create training and testing datasets using the defined date-ranges.
                        var trainingDataset = GenerateDataset(allFileStockData, trainingStartDate, trainingEndDate, indicatorPeriods);
                        var testingDataset = GenerateDataset(allFileStockData, testDate, testDate, indicatorPeriods);

                        // Verify the supplied dates are valid and haven't reached the end of the dataset.
                        if (trainingDataset == null || !trainingDataset.DatasetEntries.Any() || testingDataset == null || !testingDataset.DatasetEntries.Any())
                            break;

                        //Console.WriteLine($"==========================================================================");
                        //Console.WriteLine($"Testing date: {testDate}");
                        //Console.WriteLine($"Training from: {trainingStartDate} --> {trainingEndDate}");
                        //Console.WriteLine($"--------------------------");
                        
                        // Create & initialize input layer with neurons.
                        var inputLayer = new InputLayer(ActivationFunctionController.ActivationFunctionType.None);
                        trainingDataset.DatasetEntries.First().Inputs.ToList().ForEach(input => { inputLayer.RegisterNeuron(new Neuron(inputLayer.ActivationFunctionType)); });

                        // Create & intialize output layer with neurons.
                        var outputLayer = new OutputLayer(ActivationFunctionController.ActivationFunctionType.ReLU);
                        trainingDataset.DatasetEntries.First().Outputs.ToList().ForEach(output => { outputLayer.RegisterNeuron(new Neuron(outputLayer.ActivationFunctionType)); });

                        // Create trainer and testers - Both use the training dataset during training phase
                        // because in a real situation, we don't have the 'test' dataset to train with.
                        var trainer = new NetworkTrainer { Dataset = trainingDataset };
                        var tester = new NetworkTester { Dataset = trainingDataset };

                        // Create & initialize neural network with input, output, and hidden layers.
                        var neuralNetwork = new FeedForwardNetwork(inputLayer, outputLayer, numHiddenLayers, numHiddenLayerNeurons, ActivationFunctionController.ActivationFunctionType.ReLU)
                        {
                            LearningRate = learningRate
                        };

                        // Fully connect all layer neurons to eachother (Note they have to be side by side).
                        neuralNetwork.GenerateLayerConnections();
                        neuralNetwork.ResetNetworkProgress();

                        var percentCorrectList = new List<double>();                    // Holds ALL test-trial percent correct returns.    
                        var percentCorrectIntervalAvg = 0.0;                            // Holds the LAST 'interval' average for percent correct.
                        FeedForwardNetwork bestNetworkConfiguration = neuralNetwork;    // Clones the best network configuration based on the percent correct it had.

                        while (true)
                        {
                            // Train the network with a shuffled dataset
                            trainer.Dataset.DatasetEntries.Shuffle();
                            neuralNetwork.TrainNetwork(trainer);

                            // Test the network against the training dataset with a shuffled dataset 
                            // (remember, we don't have the 'test' dataset during training)
                            tester.Dataset.DatasetEntries.Shuffle();
                            var percentCorrect = neuralNetwork.TestNetwork(tester);

                            // If this test return the HIGHEST % correct, save the layout.
                            if (!percentCorrectList.Any() || percentCorrect > percentCorrectList.Max())
                            {
                                bestNetworkConfiguration = (FeedForwardNetwork)neuralNetwork.Clone();
                            }

                            percentCorrectList.Add(percentCorrect);

                            // If the list contains more than a certain # of tests, test the
                            // average and see if it has changed much since the last interval check
                            if (percentCorrectList.Count() >= minNumCycles && percentCorrectList.Count() % checkAvgAfterCyclesNum == 0)
                            {
                                var currentIntervalAvg = percentCorrectList.GetRange(percentCorrectList.Count() - checkAvgAfterCyclesNum, checkAvgAfterCyclesNum).Average();

                                if ((currentIntervalAvg - percentCorrectIntervalAvg) < minAmountIncrease)
                                    break;

                                percentCorrectIntervalAvg = currentIntervalAvg;

                                //Console.WriteLine($"Trial({numberCycles}) -- #:({percentCorrectList.Count()}) --> Avg:({percentCorrectIntervalAvg}) --> Hi:({percentCorrectList.Max()})");
                            }

                            // If 100%, no need to continue training.
                            // Update variables and break;
                            if (percentCorrect >= 100.0)
                            {
                                percentCorrectIntervalAvg = percentCorrectList.Count() >= checkAvgAfterCyclesNum ?
                                                                        percentCorrectList.GetRange(percentCorrectList.Count() - checkAvgAfterCyclesNum, checkAvgAfterCyclesNum).Average() :
                                                                        percentCorrectList.Average();
                                break;
                            }
                        }

                        // After training has occured, test the network using the next week's 'live' 
                        // data. This would represent a true test-case scenario.
                        tester.Dataset = testingDataset;
                        var testResults_weekAfterTraining = bestNetworkConfiguration.TestNetwork(tester);

                        if (!resultsDict.ContainsKey(testResults_weekAfterTraining))
                            resultsDict.Add(testResults_weekAfterTraining, 0);

                        resultsDict[testResults_weekAfterTraining]++;

                        //Console.WriteLine($"Trial({numberCycles}) -- #:({percentCorrectList.Count()}) --> Avg:({percentCorrectIntervalAvg}) --> Hi:({percentCorrectList.Max()}) --> Final:({testResults_weekAfterTraining})");
                        Console.WriteLine($"Task: {string.Join(", ", indicatorPeriods)}\n--------\n{String.Join("\n", resultsDict.OrderBy(kvp => kvp.Key))}\n===========================");
                        
                        // Increment the number of training/testing cycles we've been through.
                        numberCycles++;
                    }

                    var successRate = (resultsDict[resultsDict.Keys.Max()] / (double)(numberCycles + 1)) * 100.0;

                    Console.WriteLine($"Final success rate: {successRate}%");
                    System.IO.File.AppendAllText(outputFilePath, $"\r\nSuccessRate: ({successRate}%)\tIndicatorPeriods: ({string.Join(", ", indicatorPeriods)})\tTrainingWeeks: ({numTrainingWeeks})\tHiddenLayers: ({numHiddenLayers})\tHiddenLayerNeurons: ({numHiddenLayerNeurons})\tLearningRate: ({learningRate})\t");
                });

                tasksToWaitOn.Add(t);
            }

            Task.WaitAll(tasksToWaitOn.ToArray());

            Console.WriteLine($"Total time: {watch.ElapsedMilliseconds}ms");
        }
        
		/// <summary>
		/// Creates a training or testing dataset using the imported dataset and a pre-defined date range. 
        /// The method used to generate the dataset depends on the most recent method update.
		/// </summary>
		private static NetworkDataset GenerateDataset(StockData importedDataset, DateTime startDate, DateTime endDate, int[] indicatorPeriods)
		{
			if (importedDataset == null || !importedDataset.IsValidData())
				throw new ArgumentNullException("Cannot generate dataset with null or invalid stock data.");
            
            var networkDataset = new NetworkDataset();

			// Generate the advanced technical indicators and only take the readings up to the end date.
			// Note: We do not want to have access to future readings when computing indicators/normalizations.
			var importedAdvancedReadings = importedDataset.GenerateAdvancedReadings(indicatorPeriods).OrderBy(reading => reading.Date).ToList();
            
            // Filter out not between our starting and ending dates and then create input and output dataset entries.
            importedAdvancedReadings.Where(reading => reading.Date >= startDate && reading.Date <= endDate).ToList().ForEach(reading =>
			{
                networkDataset.DatasetEntries.Add(reading.GenerateDatasetEntry_Method1(indicatorPeriods, importedAdvancedReadings));
			});

            // Validate dataset before returning it.
            if (!networkDataset.IsValidDataset())
                return null;

			// Shuffle the entries for the sake of training.
			networkDataset.DatasetEntries.Shuffle();
			return networkDataset;
		}

        /// <summary>
        /// Dataset generation method that uses various technical indicators and change values.
        /// </summary>
		private static DatasetEntry GenerateDatasetEntry_Method1(this AdvancedStockReading reading, int[] indicatorPeriods, IList<AdvancedStockReading> advancedReadings)
        {
            // If this reading is the last day in the dataset, return null because we can't
            // create the output values correctly since we can't see the next day's value.
            if (reading.Date >= advancedReadings.Select(parentReading => parentReading.Date).Max())
            {
                return null;
            }

            var datasetEntry = new DatasetEntry { Name = $"'{reading.Date.ToShortDateString()}' -- periods({string.Join(",", indicatorPeriods)})" };
            var readingIndex = advancedReadings.IndexOf(reading);

            // Compute the NORMALIZED technical indicator change amounts per WEEK for each period.
            // The change amounts have been normalized to every value dated BEFORE it as this 
            // eliminates back-fitting because we're only using future values.
            var smaNormalizedChangesByPeriod = StockData.ComputeNormalizedIndicatorChangeAmounts(4, indicatorPeriods, advancedReadings.Where(readingInner => readingInner.Date <= reading.Date).Select(readingInner => readingInner.SMA));
            var emaNormalizedChangesByPeriod = StockData.ComputeNormalizedIndicatorChangeAmounts(4, indicatorPeriods, advancedReadings.Where(readingInner => readingInner.Date <= reading.Date).Select(readingInner => readingInner.EMA));

            // Add normalized technical indicators, normalized in comparison to all data up until the endDate.
            indicatorPeriods.ToList().ForEach(period =>
            {
                // Normalized indicators - normalized in comparison to every value dated BEFORE it.
                // This eliminates back-fitting because we're not using future values.
                datasetEntry.Inputs.Add(reading.NormalizedCCI(advancedReadings.Where(readingInner => readingInner.Date <= reading.Date).ToList())[period]);
                datasetEntry.Inputs.Add(reading.NormalizedRSI(advancedReadings.Where(readingInner => readingInner.Date <= reading.Date).ToList())[period]);

                // Normalized change-amounts - Normalized in comparison to every value dated BEFORE it.
                // This eliminates back-fitting because we're not using future values.
                datasetEntry.Inputs.Add(smaNormalizedChangesByPeriod[period][readingIndex].Value);
                datasetEntry.Inputs.Add(emaNormalizedChangesByPeriod[period][readingIndex].Value);
            });

            var nextDayPerformance = (advancedReadings[readingIndex + 1].Close - advancedReadings[readingIndex + 1].Open) / advancedReadings[readingIndex + 1].Open;

            // Add outputs based off %increase/decrease of the next day performance.
            //datasetEntry.Outputs.Add(nextDayPerformance <= -0.01 ? 1 : 0);
            //datasetEntry.Outputs.Add(nextDayPerformance >= -0.01 && nextDayPerformance < 0.00 ? 1 : 0);
            //datasetEntry.Outputs.Add(nextDayPerformance >= 0.00 && nextDayPerformance < 0.01 ? 1 : 0);
            //datasetEntry.Outputs.Add(nextDayPerformance >= 0.01 ? 1 : 0);
            //////////////////////////////////////////////////////////////////////////
            datasetEntry.Outputs.Add(nextDayPerformance < 0.00 ? 1 : 0);
            datasetEntry.Outputs.Add(nextDayPerformance >= 0.00 ? 1 : 0);
            //////////////////////////////////////////////////////////////////////////
            //datasetEntry.Outputs.Add(nextDayPerformance <= -0.01 ? 1 : 0);
            //datasetEntry.Outputs.Add(nextDayPerformance > -0.01 && nextDayPerformance < 0.01 ? 1 : 0);
            //datasetEntry.Outputs.Add(nextDayPerformance >= 0.01 ? 1 : 0);
            //////////////////////////////////////////////////////////////////////////

            return datasetEntry;
        }
    }
}
