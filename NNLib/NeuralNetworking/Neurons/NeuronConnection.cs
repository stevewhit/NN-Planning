using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking;

namespace NNLib.NeuralNetworking.Neurons
{
    public class NeuronConnection
    {
        /// <summary>
        /// The starting point(neuron) for this connection.
        /// </summary>
        public IConnectedNeuron FromNeuron { get; private set; }

        /// <summary>
        /// The ending point(neuron) for this connection.
        /// </summary>
        public IConnectedNeuron ToNeuron { get; private set; }

        /// <summary>
        /// The connection weight between the two neurons.
        /// </summary>
        public double Weight { get; set; }
        
        /// <summary>
        /// A temporary derivative value indicating how much of an effect this connection's 
        /// weight has on the overall cost of the network (Not used/updated anywhere else).
        /// </summary>
        public double Derivative_CostWRTWeight { get; set; }

        /// <summary>
        /// Instantiates a new neuron connection.
        /// </summary>
        public NeuronConnection(Neuron fromNeuron, Neuron toNeuron)
            : this(fromNeuron, toNeuron, Double.NaN)
        {
            
        }

        /// <summary>
        /// Instantiates a new neuron connection.
        /// </summary>
        public NeuronConnection(Neuron fromNeuron, Neuron toNeuron, double weight)
        {
            if (fromNeuron == null || toNeuron == null)
                throw new ArgumentNullException("Neuron connections require non-null Neurons.");
            
            ToNeuron = toNeuron;
            FromNeuron = fromNeuron;

            if (Double.IsNaN(weight))
                RandomizeWeight();
            else
                Weight = weight;

            if (!IsValidNeuronConnection(true))
            {
                throw new ArgumentException("Cannot create a neuron connection using invalid neurons OR weight.");
            }
        }

        /// <summary>
        /// Returns whether the neuron connection is valid or not by making sure it has a valid starting and ending neuron,
        /// and that the connection Weight is between 0.0 and 1.0.
        /// </summary>
        public bool IsValidNeuronConnection(bool clearIfInvalid = false)
        {
            var isValid = FromNeuron != null && FromNeuron.IsValidNeuron() && ToNeuron != null && ToNeuron.IsValidNeuron() && Weight <= 1.0d && Weight >= 0.0d;

            if (clearIfInvalid && !isValid)
            {
                ToNeuron = null;
                FromNeuron = null;
                Weight = -1.0d;
            }

            return isValid;
        }

        /// <summary>
        /// Generates a random number from 0.0 to 1.0 and stores it in the Weight field.
        /// </summary>
        public void RandomizeWeight()
        {
            Weight = NNUtils.GenerateRandomNumber(0, 100) / 100.0;
        }
    }
}
