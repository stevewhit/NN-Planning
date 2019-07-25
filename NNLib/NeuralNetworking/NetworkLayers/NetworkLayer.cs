using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.Neurons;
using NNLib.NeuralNetworking.ActivationFunctions;

namespace NNLib.NeuralNetworking.NetworkLayers
{
    public abstract class NetworkLayer
    {
        /// <summary>
        /// Identifies the activation function type that is applied to ALL neurons in this
        /// layer upon calculation of their activation levels.
        /// </summary>
        public ActivationFunctionController.ActivationFunctionType ActivationFunctionType { get; }

        /// <summary>
        /// A list of neurons composed within this network layer.
        /// </summary>
        public IList<Neuron> RegisteredNeurons { get; private set; }
        
        /// <summary>
        /// Instantiates a network layer without any registered neurons.
        /// </summary>
        public NetworkLayer(ActivationFunctionController.ActivationFunctionType activationFunctionType)
        {
            ActivationFunctionType = activationFunctionType;
            RegisteredNeurons = new List<Neuron>();
        }
        
        /// <summary>
        /// After validation, adds the supplied neuron to the list of 'registered neurons'
        /// associated within this layer.
        /// </summary>
        public void RegisterNeuron(Neuron neuronToAdd)
        {
            if (neuronToAdd == null || !neuronToAdd.IsValidNeuron())
            {
                throw new ArgumentException("Cannot add a null or invalid neuron to a network layer.");
            }

            RegisteredNeurons.Add(neuronToAdd);
        }

        /// <summary>
        /// Removes the supplied neuron from the list of 'registered neurons' associated
        /// within this layer.
        /// </summary>
        public void RemoveNeuron(Neuron neuronToRemove)
        {
            if (neuronToRemove == null)
                throw new ArgumentException("Cannot remove a null neuron from a network layer.");

            if (RegisteredNeurons.Contains(neuronToRemove))
                RegisteredNeurons.Remove(neuronToRemove);
        }

        /// <summary>
        /// Generates a neuron connection for every neuron in 'this' layer to every neuron in 'toLayer'. 
        /// The connection is from 'this' layer to 'toLayer' so output connections are registered for 'this'
        /// layer and input connections are registered for 'toLayer'
        /// </summary>
        public void ConnectAllNeurons(NetworkLayer toLayer)
        {
            if (toLayer == null || !toLayer.IsValidNetworkLayer())
                throw new ArgumentException("Cannot connect this layer to a null or invalid layer.");

            if (!IsValidNetworkLayer())
                throw new InvalidOperationException("Cannot connect 'this' layer to another network layer because it is invalid.");

            // Generates a neuron connection for every neuron in 'this' layer
            // and connects it to every neuron in 'toLayer's registered neurons.
            foreach(var fromNeuron in RegisteredNeurons)
            {
                foreach (var toNeuron in toLayer.RegisteredNeurons)
                {
                    var neuronConn = new NeuronConnection(fromNeuron, toNeuron);

                    fromNeuron.RegisterOutputConnection(neuronConn);
                    toNeuron.RegisterInputConnection(neuronConn);
                }
            }
        }

        /// <summary>
        /// Randomizes the associated weights and bias of all input connections for each neuron
        /// in this layer.
        /// </summary>
        public void RandomizeWeightsAndBias()
        {
            RegisteredNeurons.ToList().ForEach(neuron =>
            {
                neuron.RandomizeWeights();

                // Randomize the bias value if the layer isn't an input
                // layer. Input layer must maintain 0 bias.
                if (!(this is InputLayer))
                    neuron.RandomizeBias();
            });
        }
        
        /// <summary>
        /// Returns whether the layer is valid by making sure it contains atleast 1
        /// registered neuron and that the registered neurons are all valid.
        /// </summary>
        public bool IsValidNetworkLayer(bool clearIfInvalid = false)
        {
            if (RegisteredNeurons == null)
                RegisteredNeurons = new List<Neuron>();

            bool isValid = RegisteredNeurons.Any();

            // Make sure each registered neuron is valid.
            RegisteredNeurons.ToList().ForEach(neuron =>
            {
                isValid &= neuron != null && neuron.IsValidNeuron();
            });
            
            if (clearIfInvalid && !isValid)
            {
                RegisteredNeurons = new List<Neuron>();
            }

            return isValid;
        }

        public override string ToString()
        {
            var returnStr = "<NetworkLayer>";

            foreach (var input in RegisteredNeurons)
            {
                returnStr += $"{input.ToString()}";
            }

            returnStr += "</NetworkLayer>";

            return returnStr;
        }
    }
}
