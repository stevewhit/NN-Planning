using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNLib.NeuralNetworking.Datasets
{
    public class NetworkTester
    {
        /// <summary>
        /// Identifies the testing strategies that can be implemented when testing
        /// a neural network. 
        /// </summary>
        public enum TestingStrategyTypes
        {
            HighestValue,
            FaultTolerance
        }

        /// <summary>
        /// The strategy type that is implemented when testing a network.
        /// </summary>
        public TestingStrategyTypes TestingStrategyType { get; set; }
        
        /// <summary>
        /// The fault tolerance of each neuron activation when testing.
        /// Used because neuron activations will never be exactly 1 or 0
        /// so some level of 'error' is acceptable.
        /// </summary>
        public double FaultTolerance { get; set; }

        /// <summary>
        /// The dataset that is used to test a neural network.
        /// </summary>
        public NetworkDataset Dataset { get; set; }

        public NetworkTester()
        {
            Dataset = new NetworkDataset();

            TestingStrategyType = TestingStrategyTypes.HighestValue;
            FaultTolerance = 0.1;
        }
    }
}
