using NeuralNetwork.Generic.Datasets;
using SANNET.Business.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SANNET.Business.Services
{
    public interface IDatasetService : IDisposable
    {
        IEnumerable<INetworkTrainingIteration> GetTrainingDataset(int networkConfigurationId, int companyId, DateTime startDate, DateTime endDate);
        IEnumerable<INetworkInput> GetNetworkInputs(int networkConfigurationId, int companyId, DateTime date);
    }

    public class DatasetService : IDatasetService
    {
        private bool _isDisposed = false;
        private IDatasetRepository _repository;

        public DatasetService(IDatasetRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<INetworkTrainingIteration> GetTrainingDataset(int networkConfigurationId, int companyId, DateTime startDate, DateTime endDate)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetService", "The service has been disposed.");

            return networkConfigurationId == 1 ? GetTrainingDataset1(companyId, startDate, endDate) :
                    throw new ArgumentException($"Network Configuration not supported: {networkConfigurationId}.");
        }

        public IEnumerable<INetworkInput> GetNetworkInputs(int networkConfigurationId, int companyId, DateTime date)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetService", "The service has been disposed.");

            return networkConfigurationId == 1 ? GetNetworkInputs1(companyId, date) :
                    throw new ArgumentException($"Network Configuration not supported: {networkConfigurationId}.");
        }

        #region Dataset Method 1

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
                trainingIteration.Outputs.Add(new NetworkTrainingOutput() { ExpectedActivationLevel = entry.Output_TriggeredRiseFirst});
                trainingIteration.Outputs.Add(new NetworkTrainingOutput() { ExpectedActivationLevel = entry.Output_TriggeredFallFirst});

                trainingIterations.Add(trainingIteration);
            }

            return trainingIterations;
        }

        private IEnumerable<INetworkInput> GetNetworkInputs1(int companyId, DateTime date)
        {
            var dataset = _repository.GetTrainingDataset1(companyId, date, date);

            if (dataset.Count() != 1)
                throw new InvalidOperationException("Dataset should only contain 1 entry.");

            var entry = dataset.First();
            var inputs = new List<INetworkInput>
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

            return inputs;
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
