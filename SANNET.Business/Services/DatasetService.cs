using NeuralNetwork.Generic.Datasets;
using SANNET.Business.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SANNET.Business.Services
{
    public interface IDatasetService : IDisposable
    {
        /// <summary>
        /// Returns the training dataset for the identified <paramref name="companyId"/> from <paramref name="startDate"/> to the <paramref name="endDate"/> using the identified <paramref name="datasetRetrievalMethodId"/>.
        /// </summary>
        /// <param name="datasetRetrievalMethodId">The id of which dataset method to use.</param>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="startDate">The start date of the training dataset.</param>
        /// <param name="endDate">The end date of the training dataset.</param>
        /// <returns>Returns the training dataset for the identified <paramref name="companyId"/> from <paramref name="startDate"/> to the <paramref name="endDate"/>.</returns>
        IEnumerable<INetworkTrainingIteration> GetTrainingDataset(int datasetRetrievalMethodId, int companyId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Returns the network inputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/> using the identified <paramref name="datasetRetrievalMethodId"/>.
        /// </summary>
        /// <param name="datasetRetrievalMethodId">The id of which dataset method to use.</param>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="date">The date that the network inputs should be generated for.</param>
        /// <returns>Returns the network inputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.</returns>
        IEnumerable<INetworkInput> GetNetworkInputs(int datasetRetrievalMethodId, int companyId, DateTime date);

        /// <summary>
        /// Returns the network outputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/> using the identified <paramref name="datasetRetrievalMethodId"/>.
        /// </summary>
        /// <param name="datasetRetrievalMethodId">The id of which dataset method to use.</param>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="date">The date that the network outputs should be generated for.</param>
        /// <returns>Returns the network outputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.</returns>
        IEnumerable<INetworkOutput> GetExpectedNetworkOutputs(int datasetRetrievalMethodId, int companyId, DateTime date);
    }

    public class DatasetService : IDatasetService
    {
        private bool _isDisposed = false;
        private IDatasetRepository _repository;

        public DatasetService(IDatasetRepository repository)
        {
            _repository = repository;
        }

        #region IDatasetService

        /// <summary>
        /// Returns the training dataset for the identified <paramref name="companyId"/> from <paramref name="startDate"/> to the <paramref name="endDate"/> using the identified <paramref name="datasetRetrievalMethodId"/>.
        /// </summary>
        /// <param name="datasetRetrievalMethodId">The id of which dataset method to use.</param>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="startDate">The start date of the training dataset.</param>
        /// <param name="endDate">The end date of the training dataset.</param>
        /// <returns>Returns the training dataset for the identified <paramref name="companyId"/> from <paramref name="startDate"/> to the <paramref name="endDate"/>.</returns>
        public IEnumerable<INetworkTrainingIteration> GetTrainingDataset(int datasetRetrievalMethodId, int companyId, DateTime startDate, DateTime endDate)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetService", "The service has been disposed.");

            return datasetRetrievalMethodId == 1 ? GetTrainingDataset1(companyId, startDate, endDate) :
                   datasetRetrievalMethodId == 2 ? GetTrainingDataset2(-1) :
                   throw new ArgumentException($"DatasetRetievalMethod not supported: {datasetRetrievalMethodId}.");
        }

        /// <summary>
        /// Returns the network inputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/> using the identified <paramref name="datasetRetrievalMethodId"/>.
        /// </summary>
        /// <param name="datasetRetrievalMethodId">The id of which dataset method to use.</param>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="date">The date that the network inputs should be generated for.</param>
        /// <returns>Returns the network inputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.</returns>
        public IEnumerable<INetworkInput> GetNetworkInputs(int datasetRetrievalMethodId, int companyId, DateTime date)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetService", "The service has been disposed.");

            return datasetRetrievalMethodId == 1 ? GetNetworkInputs1(companyId, date) :
                   datasetRetrievalMethodId == 2 ? GetNetworkInputs2(companyId) :
                   throw new ArgumentException($"DatasetRetievalMethod not supported: {datasetRetrievalMethodId}.");
        }

        /// <summary>
        /// Returns the network outputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/> using the identified <paramref name="datasetRetrievalMethodId"/>.
        /// </summary>
        /// <param name="datasetRetrievalMethodId">The id of which dataset method to use.</param>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="date">The date that the network outputs should be generated for.</param>
        /// <returns>Returns the network outputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.</returns>
        public IEnumerable<INetworkOutput> GetExpectedNetworkOutputs(int datasetRetrievalMethodId, int companyId, DateTime date)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetService", "The service has been disposed.");

            return datasetRetrievalMethodId == 1 ? GetTestingDataset1(companyId, date) :
                   datasetRetrievalMethodId == 2 ? GetTestingDataset2(companyId) :
                   throw new ArgumentException($"DatasetRetievalMethod not supported: {datasetRetrievalMethodId}.");
        }

        #region (Inner) Dataset Method 1

        /// <summary>
        /// Returns the training dataset for the identified <paramref name="companyId"/> from <paramref name="startDate"/> to the <paramref name="endDate"/>.
        /// </summary>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="startDate">The start date of the training dataset.</param>
        /// <param name="endDate">The end date of the training dataset.</param>
        /// <returns>Returns the training dataset for the identified <paramref name="companyId"/> from <paramref name="startDate"/> to the <paramref name="endDate"/>.</returns>
        private IEnumerable<INetworkTrainingIteration> GetTrainingDataset1(int companyId, DateTime startDate, DateTime endDate)
        {
            var dataset = _repository.GetTrainingDataset1(companyId, startDate, endDate);
            var trainingIterations = new List<INetworkTrainingIteration>();

            // Convert each dataset entry into a network training iteration
            foreach (var entry in dataset)
            {
                var trainingIteration = new NetworkTrainingIteration();

                // RSI Short
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = decimal.ToDouble(entry.RSIShortNormalized ?? throw new InvalidOperationException("Cannot set activation level to a null value."))});
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.IsRSIShortOverBought });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.IsRSIShortOverSold });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.RSIShortJustCrossedIntoOverBought });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.RSIShortJustCrossedIntoOverSold });

                // RSI Long
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = decimal.ToDouble(entry.RSILongNormalized ?? throw new InvalidOperationException("Cannot set activation level to a null value.")) });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.IsRSILongOverBought });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.IsRSILongOverSold });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.RSILongJustCrossedIntoOverBought });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.RSILongJustCrossedIntoOverSold });

                // RSI Crosses
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.RSIShortJustCrossedOverLong });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.RSIShortGreaterThanLongForAwhile });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.RSILongJustCrossedOverShort });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.RSILongGreaterThanShortForAwhile });

                // CCI Short
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.CCIShortJustCrossedAboveZero });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.CCIShortJustCrossedBelowZero });

                // CCI Long
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.CCILongJustCrossedAboveZero });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.CCILongJustCrossedBelowZero });

                // SMA Short
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.SMAShortAboveClose });

                // SMA Long
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.SMALongAboveClose });

                // SMA Crosses
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.SMAShortJustCrossedOverLong });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.SMAShortGreaterThanLongForAwhile });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.SMALongJustCrossedOverShort });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.SMALongGreaterThanShortForAwhile });

                // Outputs
                trainingIteration.Outputs.Add(new NetworkTrainingOutput() { ExpectedActivationLevel = entry.Output_TriggeredRiseFirst, Description = "Triggered Rise First"});
                trainingIteration.Outputs.Add(new NetworkTrainingOutput() { ExpectedActivationLevel = entry.Output_TriggeredFallFirst, Description = "Triggered Fall First"});

                trainingIterations.Add(trainingIteration);
            }

            return trainingIterations;
        }

        /// <summary>
        /// Returns the network inputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.
        /// </summary>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="date">The date that the network inputs should be generated for.</param>
        /// <returns>Returns the network inputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.</returns>
        private IEnumerable<INetworkInput> GetNetworkInputs1(int companyId, DateTime date)
        {
            var dataset = _repository.GetTrainingDataset1(companyId, date, date).ToList();

            if (dataset.Count() > 1)
                throw new InvalidOperationException("Returned dataset should contain no more than 1 entry.");

            var entry = dataset.First();
            return new List<INetworkInput>()
            {
                // RSI Short
                new NetworkInput() { ActivationLevel = decimal.ToDouble(entry.RSIShortNormalized ?? throw new InvalidOperationException("Cannot set activation level to a null value.")) },
                new NetworkInput() { ActivationLevel = entry.IsRSIShortOverBought },
                new NetworkInput() { ActivationLevel = entry.IsRSIShortOverSold },
                new NetworkInput() { ActivationLevel = entry.RSIShortJustCrossedIntoOverBought },
                new NetworkInput() { ActivationLevel = entry.RSIShortJustCrossedIntoOverSold },

                // RSI Long
                new NetworkInput() { ActivationLevel = decimal.ToDouble(entry.RSILongNormalized ?? throw new InvalidOperationException("Cannot set activation level to a null value.")) },
                new NetworkInput() { ActivationLevel = entry.IsRSILongOverBought },
                new NetworkInput() { ActivationLevel = entry.IsRSILongOverSold },
                new NetworkInput() { ActivationLevel = entry.RSILongJustCrossedIntoOverBought },
                new NetworkInput() { ActivationLevel = entry.RSILongJustCrossedIntoOverSold },

                // RSI Crosses
                new NetworkInput() { ActivationLevel = entry.RSIShortJustCrossedOverLong },
                new NetworkInput() { ActivationLevel = entry.RSIShortGreaterThanLongForAwhile },
                new NetworkInput() { ActivationLevel = entry.RSILongJustCrossedOverShort },
                new NetworkInput() { ActivationLevel = entry.RSILongGreaterThanShortForAwhile },

                // CCI Short
                new NetworkInput() { ActivationLevel = entry.CCIShortJustCrossedAboveZero },
                new NetworkInput() { ActivationLevel = entry.CCIShortJustCrossedBelowZero },

                // CCI Long
                new NetworkInput() { ActivationLevel = entry.CCILongJustCrossedAboveZero },
                new NetworkInput() { ActivationLevel = entry.CCILongJustCrossedBelowZero },

                // SMA Short
                new NetworkInput() { ActivationLevel = entry.SMAShortAboveClose },

                // SMA Long
                new NetworkInput() { ActivationLevel = entry.SMALongAboveClose },

                // SMA Crosses
                new NetworkInput() { ActivationLevel = entry.SMAShortJustCrossedOverLong },
                new NetworkInput() { ActivationLevel = entry.SMAShortGreaterThanLongForAwhile },
                new NetworkInput() { ActivationLevel = entry.SMALongJustCrossedOverShort },
                new NetworkInput() { ActivationLevel = entry.SMALongGreaterThanShortForAwhile }
            };
        }

        /// <summary>
        /// Returns the network outputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.
        /// </summary>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="date">The date that the network outputs should be generated for.</param>
        /// <returns>Returns the network outputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.</returns>
        private IEnumerable<INetworkOutput> GetTestingDataset1(int companyId, DateTime date)
        {
            var dataset = _repository.GetTestingDataset1(companyId, date).ToList();

            if (dataset.Count() > 1)
                throw new InvalidOperationException("Returned dataset should contain no more than 1 entry.");

            var entry = dataset.First();
            return new List<INetworkOutput>()
            {
                // Outputs
                new NetworkOutput() { ActivationLevel = entry.Output_TriggeredRiseFirst, Description = "Triggered Rise First" },
                new NetworkOutput() { ActivationLevel = entry.Output_TriggeredFallFirst, Description = "Triggered Fall First" }
            };
        }

        #endregion

        #region (Inner) Dataset Method 2

        /// <summary>
        /// Returns the training dataset for the identified <paramref name="companyId"/> from <paramref name="startDate"/> to the <paramref name="endDate"/>.
        /// </summary>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="startDate">The start date of the training dataset.</param>
        /// <param name="endDate">The end date of the training dataset.</param>
        /// <returns>Returns the training dataset for the identified <paramref name="companyId"/> from <paramref name="startDate"/> to the <paramref name="endDate"/>.</returns>
        private IEnumerable<INetworkTrainingIteration> GetTrainingDataset2(int id)
        {
            var dataset = _repository.GetTrainingDataset2(id).ToList();
            var trainingIterations = new List<INetworkTrainingIteration>();

            // Convert each dataset entry into a network training iteration
            foreach (var entry in dataset)
            {
                var trainingIteration = new NetworkTrainingIteration();
                
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.I1.Value });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.I2.Value });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.I3.Value });
                trainingIteration.Inputs.Add(new NetworkTrainingInput() { ActivationLevel = entry.I4.Value });
                trainingIteration.Outputs.Add(new NetworkTrainingOutput() { ExpectedActivationLevel = entry.O1.Value, Description="O1" });
                trainingIteration.Outputs.Add(new NetworkTrainingOutput() { ExpectedActivationLevel = entry.O2.Value, Description = "O2" });
                trainingIteration.Outputs.Add(new NetworkTrainingOutput() { ExpectedActivationLevel = entry.O3.Value, Description = "O3" });
                trainingIteration.Outputs.Add(new NetworkTrainingOutput() { ExpectedActivationLevel = entry.O4.Value, Description = "O4" });
                
                trainingIterations.Add(trainingIteration);
            }

            return trainingIterations;
        }
        




        private IEnumerable<INetworkInput> GetNetworkInputs2(int id)
        {
            var dataset = _repository.GetTrainingDataset2(id).ToList();

            if (dataset.Count() > 1)
                throw new InvalidOperationException("Returned dataset should contain no more than 1 entry.");

            var entry = dataset.First();
            return new List<INetworkInput>()
            {
                new NetworkInput() { ActivationLevel = entry.I1.Value },
                new NetworkInput() { ActivationLevel = entry.I2.Value },
                new NetworkInput() { ActivationLevel = entry.I3.Value },
                new NetworkInput() { ActivationLevel = entry.I4.Value },
            };
        }

        /// <summary>
        /// Returns the network outputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.
        /// </summary>
        /// <param name="companyId">The id of the company.</param>
        /// <param name="date">The date that the network outputs should be generated for.</param>
        /// <returns>Returns the network outputs for the <paramref name="companyId"/> on the supplied <paramref name="date"/>.</returns>
        private IEnumerable<INetworkOutput> GetTestingDataset2(int lastId)
        {
            var dataset = _repository.GetTestingDataset2(lastId).ToList();

            if (dataset.Count() > 1)
                throw new InvalidOperationException("Returned dataset should contain no more than 1 entry.");

            var entry = dataset.First();
            return new List<INetworkOutput>()
            {
                // Outputs
                new NetworkOutput() { ActivationLevel = entry.O1.Value, Description = "O1" },
                new NetworkOutput() { ActivationLevel = entry.O2.Value, Description = "O2" },
                new NetworkOutput() { ActivationLevel = entry.O3.Value, Description = "O3" },
                new NetworkOutput() { ActivationLevel = entry.O4.Value, Description = "O4" }
            };
        }

        #endregion

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
