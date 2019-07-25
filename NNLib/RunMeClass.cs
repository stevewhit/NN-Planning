using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.Datasets;
using NNLib.NeuralNetworking;
using NNLib.NeuralNetworking.NetworkLayers;
using NNLib.NeuralNetworking.Neurons;
using NNLib.NeuralNetworking.ActivationFunctions;
using System.Diagnostics;

namespace NNLib
{
    public class RunMeClass
    {
        public static void Run()
        {
            var watch = new Stopwatch();
            watch.Start();

            // Create input layer and add all input neurons to it.
            var inputLayer = new InputLayer(ActivationFunctionController.ActivationFunctionType.None);
            inputLayer.RegisterNeuron(new Neuron(inputLayer.ActivationFunctionType) { Name = "InputNeuron0" });
            inputLayer.RegisterNeuron(new Neuron(inputLayer.ActivationFunctionType) { Name = "InputNeuron1" });
            inputLayer.RegisterNeuron(new Neuron(inputLayer.ActivationFunctionType) { Name = "InputNeuron2" });
            inputLayer.RegisterNeuron(new Neuron(inputLayer.ActivationFunctionType) { Name = "InputNeuron3" });

            // Create output layer and add all output neurons to it.
            var outputLayer = new OutputLayer(ActivationFunctionController.ActivationFunctionType.ReLU);
            outputLayer.RegisterNeuron(new Neuron(outputLayer.ActivationFunctionType) { Name = "OutputNeuron0" });
            outputLayer.RegisterNeuron(new Neuron(outputLayer.ActivationFunctionType) { Name = "OutputNeuron1" });
            outputLayer.RegisterNeuron(new Neuron(outputLayer.ActivationFunctionType) { Name = "OutputNeuron2" });
            outputLayer.RegisterNeuron(new Neuron(outputLayer.ActivationFunctionType) { Name = "OutputNeuron3" });
            outputLayer.RegisterNeuron(new Neuron(outputLayer.ActivationFunctionType) { Name = "OutputNeuron4" });

            // Create Feed Forward neural network composed of the input, output, and hidden layers.
            var neuralNetwork = new FeedForwardNetwork(inputLayer, outputLayer, 1, 2, ActivationFunctionController.ActivationFunctionType.ReLU);

            // Inner connect ALL input --> hidden --> output layers by fully connecting their neurons to eachother.
            neuralNetwork.GenerateLayerConnections();
            
            var trainer = new NetworkTrainer();
            trainer.Dataset = GetTestTrainingDataset();

            var tester = new NetworkTester();
            tester.Dataset = trainer.Dataset;

            // Trains the fully-connected network with a TEST training dataset.
            //neuralNetwork.TrainNetwork(trainer, true);
            neuralNetwork.ResetNetworkProgress();

            var percentCorrectList = new Dictionary<double, int>();
            var maxNum = 5000;
            var resetAfterNum = 1000;

            for(int i = 0; i < maxNum; i++)
            {
                //trainer.ResetTrainingCosts();
                trainer.Dataset.DatasetEntries.Shuffle();

                if (i % resetAfterNum == 0)
                {
                    neuralNetwork.ResetNetworkProgress();
                    neuralNetwork.TrainNetwork(trainer);
                }
                else
                    neuralNetwork.TrainNetwork(trainer);

                // Shuffle dataset
                tester.Dataset.DatasetEntries.Shuffle();
                var percentCorrect = neuralNetwork.TestNetwork(tester);

                if (percentCorrectList.ContainsKey(percentCorrect))
                    percentCorrectList[percentCorrect]++;
                else
                    percentCorrectList.Add(percentCorrect, 1);


                var outputStr = "Percent Correct\t\t\tCount\n-----------------------------------";

                percentCorrectList.OrderBy(kvp => kvp.Key).ToList().ForEach(percentCorrectItem =>
                {
                    outputStr += $"\n{percentCorrectItem.Key}\t\t|\t\t{percentCorrectItem.Value}";
                });

                Console.Clear();

                Console.WriteLine($"Trial: {i}/{maxNum}\nElapsed: {watch.ElapsedMilliseconds}ms\n\n{outputStr}");
            }
        }

        private static NetworkDataset GetTestTrainingDataset()
        {
            var trainingDataset = new NetworkDataset();

            for (int i = 0; i < 6; i++)
            {
                
                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 0.0, 0.0, 0.0, 0.0},
                    Outputs = new List<double> { 1.0, 0.0, 0.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 0.0, 0.0, 0.0, 1.0 },
                    Outputs = new List<double> { 0.0, 1.0, 0.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 0.0, 0.0, 1.0, 0.0 },
                    Outputs = new List<double> { 0.0, 1.0, 0.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 0.0, 0.0, 1.0, 1.0 },
                    Outputs = new List<double> { 0.0, 0.0, 1.0, 0.0, 0.0 }
                }); 

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 0.0, 1.0, 0.0, 0.0 },
                    Outputs = new List<double> { 0.0, 1.0, 0.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 0.0, 1.0, 0.0, 1.0 },
                    Outputs = new List<double> { 0.0, 0.0, 1.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 0.0, 1.0, 1.0, 0.0 },
                    Outputs = new List<double> { 0.0, 0.0, 1.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 0.0, 1.0, 1.0, 1.0 },
                    Outputs = new List<double> { 0.0, 0.0, 0.0, 1.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 1.0, 0.0, 0.0, 0.0 },
                    Outputs = new List<double> { 0.0, 1.0, 0.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 1.0, 0.0, 0.0, 1.0 },
                    Outputs = new List<double> { 0.0, 0.0, 1.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 1.0, 0.0, 1.0, 0.0 },
                    Outputs = new List<double> { 0.0, 0.0, 1.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 1.0, 0.0, 1.0, 1.0 },
                    Outputs = new List<double> { 0.0, 0.0, 0.0, 1.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 1.0, 1.0, 0.0, 0.0 },
                    Outputs = new List<double> { 0.0, 0.0, 1.0, 0.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 1.0, 1.0, 0.0, 1.0 },
                    Outputs = new List<double> { 0.0, 0.0, 0.0, 1.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 1.0, 1.0, 1.0, 0.0 },
                    Outputs = new List<double> { 0.0, 0.0, 0.0, 1.0, 0.0 }
                });

                trainingDataset.DatasetEntries.Add(new DatasetEntry
                {
                    Inputs = new List<double> { 1.0, 1.0, 1.0, 1.0 },
                    Outputs = new List<double> { 0.0, 0.0, 0.0, 0.0, 1.0 }
                });

                trainingDataset.DatasetEntries.Shuffle();
            }

            trainingDataset.DatasetEntries.Shuffle();

            return trainingDataset;
        }
    }
}
