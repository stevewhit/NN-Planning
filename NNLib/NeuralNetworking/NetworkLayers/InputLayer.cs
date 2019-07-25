using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.Neurons;
using NNLib.NeuralNetworking.ActivationFunctions;

namespace NNLib.NeuralNetworking.NetworkLayers
{
    public class InputLayer : NetworkLayer
    {
        /// <summary>
        /// Instantiates an input network layer without any registered neurons.
        /// </summary>
        public InputLayer(ActivationFunctionController.ActivationFunctionType activationFunctionType) :
            base(activationFunctionType)
        {

        }

        /// <summary>
        /// Accepts a list of inputs (activation level inputs) and applys them to
        /// the registered input neurons.
        /// </summary>
        public void ApplyInputs(IList<double> layerInputs)
        {
            if (layerInputs == null || layerInputs.Count() != RegisteredNeurons.Count())
                throw new ArgumentException("The supplied layer inputs does not equal the number of INPUT NEURONS.");
            
            // Apply the inputs to the activation levels of each input neuron.
            for (int inputNum = 0; inputNum < layerInputs.Count(); inputNum++)
            {
                RegisteredNeurons[inputNum].SetActivationLevel(layerInputs[inputNum]);
            }
        }
    }
}
