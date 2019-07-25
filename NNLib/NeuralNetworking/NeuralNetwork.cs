using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.NetworkLayers;
using NNLib.NeuralNetworking.Datasets;

namespace NNLib.NeuralNetworking
{
    public abstract class NeuralNetwork : ICloneable
    {
        /// <summary>
        /// The associated network layers for this neural network.
        /// </summary>
        public IList<NetworkLayer> NetworkLayers { get; protected set; }
        
        /// <summary>
        /// Initializes the neural network layers.
        /// </summary>
        public NeuralNetwork()
        {
            NetworkLayers = new List<NetworkLayer>();
        }

        /// <summary>
        /// Computes and returns the activation cost between the actual and expected activation levels. (How wrong
        /// </summary>
        public static double CalculateActivationCost(double actualActivationLevel, double expectedActivationLevel)
        {
            return Math.Pow(actualActivationLevel - expectedActivationLevel, 2);
        }

        #region Abstract Methods

        public abstract bool IsValidNetwork(bool clearIfInvalid = false);
        public abstract void GenerateLayerConnections();
        public abstract void ValidateDatasetForNetwork(NetworkDataset dataset);
        public abstract void TrainNetwork(NetworkTrainer networkTrainer);
        public abstract double TestNetwork(NetworkTester networkTester);
        public abstract void ResetNetworkProgress();
        public abstract object Clone();
        #endregion
    }
}
