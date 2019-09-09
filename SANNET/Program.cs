using log4net;
using NeuralNetwork.Generic.Connections;
using NeuralNetwork.Generic.Datasets;
using NeuralNetwork.Generic.Networks;
using Ninject;
using SANNET.Business.Services;
using SANNET.DataModel;
using StockMarket.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SANNET
{
    class Program
    {
        public static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            var predictionService = kernel.Get<PredictionService<Prediction, Quote, Company, NetworkConfiguration>>();

            //var trainingEntries = new List<INetworkTrainingIteration>();
            //trainingEntries.Add(new NetworkTrainingIteration()
            //{
            //    Inputs = new List<INetworkTrainingInput>() { new NetworkTrainingInput() { NeuronId = 0, ActivationLevel = 0.05 }, new NetworkTrainingInput() { NeuronId = 1, ActivationLevel = 0.1 } },
            //    Outputs = new List<INetworkTrainingOutput>() { new NetworkTrainingOutput() { NeuronId = 4, ExpectedActivationLevel = .01 }, new NetworkTrainingOutput() { NeuronId = 5, ActivationLevel = 0.99 } }
            //});
            
            //var network = new DFFNeuralNetwork(2, 1, 2, 2);
            //var inputLayer = network.Layers.First();
            //var hiddenLayer = network.Layers.Last();
            //var outputLayer = network.Layers.Skip(1).First();

            //var neuronI1 = inputLayer.Neurons.First();
            //var neuronI2 = inputLayer.Neurons.Last();

            //var neuronH1 = hiddenLayer.Neurons.First();
            //var neuronH2 = hiddenLayer.Neurons.Last();

            //var neuronO1 = outputLayer.Neurons.First();
            //var neuronO2 = outputLayer.Neurons.Last();

            //// I1 to H1
            //neuronI1.Connections.OfType<IOutgoingConnection>().First(c => c.ToNeuron == neuronH1).Weight = .15;
            //neuronH1.Connections.OfType<IIncomingConnection>().First(c => c.FromNeuron == neuronI1).Weight = .15;

            //// I1 to H2
            //neuronI1.Connections.OfType<IOutgoingConnection>().First(c => c.ToNeuron == neuronH2).Weight = .25;
            //neuronH2.Connections.OfType<IIncomingConnection>().First(c => c.FromNeuron == neuronI1).Weight = .25;

            //// I2 to H1
            //neuronI2.Connections.OfType<IOutgoingConnection>().First(c => c.ToNeuron == neuronH1).Weight = .2;
            //neuronH1.Connections.OfType<IIncomingConnection>().First(c => c.FromNeuron == neuronI2).Weight = .2;

            //// I2 to H2
            //neuronI2.Connections.OfType<IOutgoingConnection>().First(c => c.ToNeuron == neuronH2).Weight = .30;
            //neuronH2.Connections.OfType<IIncomingConnection>().First(c => c.FromNeuron == neuronI2).Weight = .30;


            ////================

            //// H1 to O1
            //neuronH1.Connections.OfType<IOutgoingConnection>().First(c => c.ToNeuron == neuronO1).Weight = .40;
            //neuronO1.Connections.OfType<IIncomingConnection>().First(c => c.FromNeuron == neuronH1).Weight = .40;

            //// H1 to O2
            //neuronH1.Connections.OfType<IOutgoingConnection>().First(c => c.ToNeuron == neuronO2).Weight = .5;
            //neuronO2.Connections.OfType<IIncomingConnection>().First(c => c.FromNeuron == neuronH1).Weight = .5;

            //// H2 to O1
            //neuronH2.Connections.OfType<IOutgoingConnection>().First(c => c.ToNeuron == neuronO1).Weight = .45;
            //neuronO1.Connections.OfType<IIncomingConnection>().First(c => c.FromNeuron == neuronH2).Weight = .45;

            //// H2 to O2
            //neuronH2.Connections.OfType<IOutgoingConnection>().First(c => c.ToNeuron == neuronO2).Weight = .55;
            //neuronO2.Connections.OfType<IIncomingConnection>().First(c => c.FromNeuron == neuronH2).Weight = .55;

            //// Input Bias'
            //neuronI1.Bias = 0;
            //neuronI2.Bias = 0;
            //neuronH1.Bias = .35;
            //neuronH2.Bias = .35;
            //neuronO1.Bias = .6;
            //neuronO2.Bias = .6;

            //var outputs = network.ApplyInputs(trainingEntries.First().Inputs);
            //network.Train(trainingEntries);

            try
            {
                try
                {
                    predictionService.GenerateAllPredictions();
                    //marketService.UpdateAllCompanyDetailsAsync().Wait();
                }
                catch (AggregateException e)
                {
                    LogRecursive(kernel.Get<ILog>(), e, "Error occured downloading stock details");
                }

                try
                {
                    //predictionService.AnalyzePredictions();
                    //marketService.UpdateAllCompaniesWithLatestQuotesAsync().Wait();
                }
                catch (AggregateException e)
                {
                    LogRecursive(kernel.Get<ILog>(), e, "Error occured downloading stock data");
                }
            }
            catch(Exception e)
            {
                kernel.Get<ILog>().Error($"A fatal error occured that stopped SANNET: {e.Message} - {e.InnerException}");
            }
            finally
            {
                predictionService.Dispose();
                kernel.Dispose();
            }
        }

        /// <summary>
        /// Logs the inner exceptions of an AggregateException separately.
        /// </summary>
        private static void LogRecursive(ILog log, AggregateException e, string message)
        {
            foreach (var innerException in e.InnerExceptions)
            {
                if (innerException is AggregateException)
                    LogRecursive(log, innerException as AggregateException, message);
                else
                    log.Error($"{message}", innerException);
            }
        }
    }
}
