using NeuralNetwork.Generic.Datasets;
using SANNET.Business.Repositories;
using SANNET.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SANNET.Business.Services
{
    public interface IDatasetService : IDisposable
    {
        /// <summary>
        /// Returns the training dataset values for the <paramref name="quoteId"/>.
        /// </summary>
        /// <param name="quoteId">The id of the quote to recieve training values for.</param>
        /// <returns>Returns the training dataset values for the <paramref name="quoteId"/>.
        IEnumerable<INetworkTrainingIteration> GetTrainingDataset(int quoteId);

        /// <summary>
        /// Returns the network inputs for the <paramref name="quoteId"/>.
        /// </summary>
        /// <param name="date">The date that the network inputs should be generated for.</param>
        /// <returns>Returns the network inputs for the <paramref name="quoteId"/>.
        IEnumerable<INetworkInput> GetNetworkInputs(int quoteId);

        /// <summary>
        /// Returns the network outputs for the <paramref name="quoteId"/>.
        /// </summary>
        /// <param name="date">The date that the network outputs should be generated for.</param>
        /// <returns>Returns the network outputs for the <paramref name="quoteId"/>.
        IEnumerable<INetworkOutput> GetExpectedNetworkOutputs(int quoteId);
    }

    public class DatasetService : IDatasetService
    {
        private bool _isDisposed = false;
        private IDatasetRepository _repository;

        public DatasetService(IDatasetRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException("repository");
        }

        #region IDatasetService

        /// <summary>
        /// Returns the training dataset values for the <paramref name="quoteId"/>.
        /// </summary>
        /// <param name="quoteId">The id of the quote to recieve training values for.</param>
        /// <returns>Returns the training dataset values for the <paramref name="quoteId"/>.
        public IEnumerable<INetworkTrainingIteration> GetTrainingDataset(int quoteId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetService", "The service has been disposed.");

            var dataset = _repository.GetTrainingDataset(quoteId)?.ToList();

            return ConvertToTrainingIterations(dataset);
        }

        /// <summary>
        /// Converts each of the <paramref name="source"/> items to a trainingIteration.
        /// </summary>
        /// <param name="source">The source dataset that is to be converted to training iterations.</param>
        /// <returns>Returns the converted <paramref name="source"/> items as trainingIterations.</returns>
        private IEnumerable<INetworkTrainingIteration> ConvertToTrainingIterations(IEnumerable<GetTrainingDataset_Result> source)
        {
            var trainingIterations = new List<INetworkTrainingIteration>();

            if (source != null && source.Count() >= 1)
            {
                var properties = source.First().GetType().GetProperties();
                var inputProperties = properties.Where(p => p.Name.StartsWith("I_"));
                var outputProperties = properties.Where(p => p.Name.StartsWith("O_"));

                // Convert each dataset entry into a network training iteration
                foreach (var entry in source)
                {
                    var trainingIteration = new NetworkTrainingIteration();
                    
                    // Dynamically add all inputs
                    foreach (var inputProperty in inputProperties)
                    {
                        trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = (bool)inputProperty.GetValue(entry) ? 1.0 : 0.0, Description = inputProperty.Name });
                    }

                    // Dynamically add all outputs
                    foreach (var outputProperty in outputProperties)
                    {
                        trainingIteration.Outputs.Add(new NetworkTrainingOutput() { ExpectedActivationLevel = (bool)outputProperty.GetValue(entry) ? 1.0 : 0.0, Description = outputProperty.Name });
                    }

                    trainingIterations.Add(trainingIteration);
                }
            }

            return trainingIterations;
        }

        /// <summary>
        /// Returns the network inputs for the <paramref name="quoteId"/>.
        /// </summary>
        /// <param name="date">The date that the network inputs should be generated for.</param>
        /// <returns>Returns the network inputs for the <paramref name="quoteId"/>.
        public IEnumerable<INetworkInput> GetNetworkInputs(int quoteId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetService", "The service has been disposed.");

            var dataset = _repository.GetTestingDataset(quoteId)?.ToList();
            if (dataset?.Count() != 1)
                throw new DataMisalignedException("Dataset should only ever return 1 entry.");

            return ConvertToNetworkInputs(dataset.First());
        }

        /// <summary>
        /// Converts the <paramref name="source"/> item to NetworkInputs.
        /// </summary>
        /// <param name="source">The source dataset item.</param>
        /// <returns>Returns the converted <paramref name="source"/> as NetworkInputs.</returns>
        private IEnumerable<INetworkInput> ConvertToNetworkInputs(GetTestingDataset_Result source)
        {
            var networkInputs = new List<INetworkInput>();

            if (source != null)
            {
                var properties = source.GetType().GetProperties();
                var inputProperties = properties.Where(p => p.Name.StartsWith("I_"));

                // Dynamically add all inputs
                foreach (var inputProperty in inputProperties)
                {
                    networkInputs.Add( new NetworkInput() { ActivationLevel = (bool)inputProperty.GetValue(source) ? 1.0 : 0.0, Description = inputProperty.Name });
                }
            }

            return networkInputs;
        }

        /// <summary>
        /// Returns the network outputs for the <paramref name="quoteId"/>.
        /// </summary>
        /// <param name="date">The date that the network outputs should be generated for.</param>
        /// <returns>Returns the network outputs for the <paramref name="quoteId"/>.
        public IEnumerable<INetworkOutput> GetExpectedNetworkOutputs(int quoteId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetService", "The service has been disposed.");

            var dataset = _repository.GetTestingDataset(quoteId)?.ToList();
            if (dataset?.Count() != 1)
                throw new DataMisalignedException("Dataset should only ever return 1 entry.");

            return ConvertToNetworkOutputs(dataset.First());
        }

        /// <summary>
        /// Converts the <paramref name="source"/> item to NetworkOutputs.
        /// </summary>
        /// <param name="source">The source dataset item.</param>
        /// <returns>Returns the converted <paramref name="source"/> as NetworkOutputs.</returns>
        private IEnumerable<INetworkOutput> ConvertToNetworkOutputs(GetTestingDataset_Result source)
        {
            var networkOutputs = new List<INetworkOutput>();

            if (source != null)
            {
                var properties = source.GetType().GetProperties();
                var outputProperties = properties.Where(p => p.Name.StartsWith("O_"));

                // Dynamically add all outputs
                foreach (var outputProperty in outputProperties)
                {
                    networkOutputs.Add(new NetworkOutput() { ActivationLevel = (bool)outputProperty.GetValue(source) ? 1.0 : 0.0, Description = outputProperty.Name });
                }
            }

            return networkOutputs;
        }
        
        #endregion
        #region IDisposable
        /// <summary>
        /// Disposes this object and properly cleans up resources. 
        /// </summary>
        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _repository.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Disposes this object and properly cleans up resources. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
