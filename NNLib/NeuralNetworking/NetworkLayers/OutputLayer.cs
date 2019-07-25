using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.Neurons;
using NNLib.NeuralNetworking.ActivationFunctions;

namespace NNLib.NeuralNetworking.NetworkLayers
{
    public class OutputLayer : NetworkLayer
    {
        /// <summary>
        /// Instantiates an output network layer without any registered neurons.
        /// </summary>
        public OutputLayer(ActivationFunctionController.ActivationFunctionType activationFunctionType) :
            base(activationFunctionType)
        {

        }

        /// <summary>
        /// Returns a list of output values for this layer.
        /// </summary>
        public IList<double> GetOutputActivationLevels()
        {
            return RegisteredNeurons.Select(neuron => neuron.ActivationLevel).ToList();
        }
    }
}
