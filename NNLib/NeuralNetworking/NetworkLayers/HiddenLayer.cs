using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.Neurons;
using NNLib.NeuralNetworking.ActivationFunctions;

namespace NNLib.NeuralNetworking.NetworkLayers
{
    public class HiddenLayer : NetworkLayer
    {
        /// <summary>
        /// Instantiates a hidden network layer with 'numNeurons' registered neurons.
        /// </summary>
        public HiddenLayer(ActivationFunctionController.ActivationFunctionType activationFunctionType, int numNeurons) :
            base(activationFunctionType)
        {
            if (numNeurons < 0)
                throw new ArgumentOutOfRangeException($"Cannot create a network layer with '{numNeurons}' neurons.");

            // register 'numNeurons' neurons to this layer.
            for (int i = 0; i < numNeurons; i++)
            {
                RegisterNeuron(new Neuron(ActivationFunctionType) { Name = $"HiddenNeuron:{RegisteredNeurons.Count + 1}" });
            }
        }
    }
}
