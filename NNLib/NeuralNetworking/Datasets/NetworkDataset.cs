using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNLib.NeuralNetworking.Datasets
{
    public class NetworkDataset
    {
        /// <summary>
        /// A list of inputs/output entries to train/test a neural network.
        /// </summary>
        public IList<DatasetEntry> DatasetEntries { get; set; } = new List<DatasetEntry>();

        /// <summary>
        /// Returns true/false whether the network dataset is valid by validating each 
        /// dataset entry has the same number of inputs and outputs.
        /// </summary>
        public bool IsValidDataset(bool clearIfInvalid = false)
        {
            if (DatasetEntries?.First()?.Inputs != null &&
                DatasetEntries?.First()?.Outputs != null)
            {
                return IsValidDataset(DatasetEntries.First().Inputs.Count,
                                      DatasetEntries.First().Outputs.Count,
                                      clearIfInvalid);
            }

            return false;
        }

        /// <summary>
        /// Returns true/false whether the network dataset is valid by validating each 
        /// dataset entry has a designated number of inputs and outputs.
        /// </summary>
        public bool IsValidDataset(int requiredNumberOfInputs, int requiredNumberOfOutputs, bool clearIfInvalid = false)
        {
            var isValid = false;

            if (DatasetEntries != null)
            {
                isValid = true;

                // Make sure each entry has the same number of inputs and outputs.
                DatasetEntries.ToList().ForEach(entry =>
                {
                    isValid &= entry.IsValidEntry(requiredNumberOfInputs, requiredNumberOfOutputs, clearIfInvalid);
                });
            }

            if (!isValid && clearIfInvalid)
            {
                DatasetEntries = new List<DatasetEntry>();
            }

            return isValid;
        }
    }
}
