using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNLib.NeuralNetworking.Datasets
{
    public class DatasetEntry
    {
        /// <summary>
        /// An optional name variable to help with identification.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A list of input values for one pass of the neural network.
        /// </summary>
        public IList<double> Inputs { get; set; } = new List<double>();

        /// <summary>
        /// A list of expected output values for one pass of the neural network.
        /// </summary>
        public IList<double> Outputs { get; set; } = new List<double>();

        /// <summary>
        /// Returns true/false whether the entry contains a certain number of inputs 
        /// </summary>
        public bool IsValidEntry(int numInputs, int numOutputs, bool clearIfInvalid = false)
        {
            var isValid = (Inputs?.Count == numInputs) && (Outputs?.Count == numOutputs);

            if (!isValid && clearIfInvalid)
            {
                Inputs = new List<double>();
                Outputs = new List<double>();
            }

            return isValid;
        }
    }
}
