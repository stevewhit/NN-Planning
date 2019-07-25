using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.ActivationFunctions;

namespace NNLib.NeuralNetworking.Neurons
{
    public class Neuron : IConnectedNeuron
    {
        /// <summary>
        /// The 'DEFINED' activation level of this neuron. This should only be set
        /// if the neuron is part of an input layer.
        /// </summary>
        private Double _activationLevel = Double.NaN;

        /// <summary>
        /// The activation function type that is used to limit the activation level
        /// between certain values.
        /// </summary>
        public ActivationFunctionController.ActivationFunctionType ActivationFunctionType { get; private set; }
        
        /// <summary>
        /// A list of input neuron connections associated with this neuron.
        /// </summary>
        public IList<NeuronConnection> InputConnections { get; private set; }

        /// <summary>
        /// A list of output neuron connections associated with this neuron.
        /// </summary>
        public IList<NeuronConnection> OutputConnections { get; private set; }

        /// <summary>
        /// The neuron bias indicating whether the activation tends to be 
        /// more 'ON' or 'OFF'
        /// </summary>
        public double BiasValue { get; set; } = 0.0;
        
        /// <summary>
        /// An optional field used to idenfity this neuron.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// A temporary derivative value indicating how much of an effect this connection's 
        /// Bias has on the overall cost of the network (Not used/updated anywhere else).
        /// </summary>
        public double Derivative_CostWRTBias { get; set; }

        /// <summary>
        /// A temporary derivative value indicating how much of an effect this connection's 
        /// activation level has on the overall cost of the network (Not used/updated anywhere else).
        /// </summary>
        public double Derivative_CostWRTActivation { get; set; }

        /// <summary>
        /// Instantiates a new neuron with no input or output connections and
        /// with an applied activation function.
        /// </summary>
        public Neuron(ActivationFunctionController.ActivationFunctionType activationFunctionType)
        {
            InputConnections = new List<NeuronConnection>();
            OutputConnections = new List<NeuronConnection>();
            ActivationFunctionType = activationFunctionType;
        }

        #region IConnectedNeuron Region

        /// <summary>
        /// Returns the activation level of this neuron by either returning the 'set' activation level,
        /// or returning a 'calculated' activation level.
        /// </summary>
        public double ActivationLevel
        {
            get
            {
                if (double.IsNaN(_activationLevel))
                    return GetCalculatedActivationLevel();
                else
                    return _activationLevel;
            }
        }
        
        /// <summary>
        /// Returns whether the neuron is valid or not by making sure all input and output 
        /// connections are valid connections.
        /// </summary>
        public bool IsValidNeuron(bool clearIfInvalid = false)
        {
            var isValid = true;

            // Make sure each input connection has 'this' neuron as the ending-point neuron
            // and that the starting-point neuron is valid.
            InputConnections.ToList().ForEach(conn =>
            {
                isValid &= this == conn.ToNeuron && conn.FromNeuron != null;
            });

            // Make sure each input connection has 'this' neuron as the starting-point neuron
            // and that the ending-point neuron is valid.
            OutputConnections.ToList().ForEach(conn =>
            {
                isValid &= this == conn.FromNeuron && conn.ToNeuron != null;
            });

            if (clearIfInvalid && !isValid)
            {
                InputConnections = new List<NeuronConnection>();
                OutputConnections = new List<NeuronConnection>();
            }

            return isValid;
        }

        #endregion

        /// <summary>
        /// Creates a neuron connection from the supplied neuron to 'this' neuron and adds
        /// the generated connection to the list of input connections.
        /// </summary>
        public void CreateInputConnection(Neuron fromNeuron)
        {
            RegisterInputConnection(new NeuronConnection(fromNeuron, this));
        }

        /// <summary>
        /// After validation, adds the supplied neuron connection to the list of input connections.
        /// </summary>
        public void RegisterInputConnection(NeuronConnection neuronConnection)
        {
            if (neuronConnection == null || !neuronConnection.IsValidNeuronConnection(false))
                throw new ArgumentNullException("Cannot add a null or invalid neuron connection to a neuron.");

            // Verify this is the END neuron.
            if (this != neuronConnection.ToNeuron)
                throw new ArgumentException("Cannot store input connection for this neuron when the connection already exists between two other neurons.");

            InputConnections.Add(neuronConnection);
        }

        /// <summary>
        /// Creates a neuron connection from 'this' neuron to the supplied neuron and adds
        /// the generated connection to the list of output connections.
        /// </summary>
        public void CreateOutputConnection(Neuron toNeuron)
        {
            RegisterOutputConnection(new NeuronConnection(this, toNeuron));
        }

        /// <summary>
        /// After validation, adds the supplied neuron connection to the list of output connections.
        /// </summary>
        public void RegisterOutputConnection(NeuronConnection neuronConnection)
        {
            if (neuronConnection == null || !neuronConnection.IsValidNeuronConnection(false))
                throw new ArgumentNullException("Cannot add a null or invalid neuron connection to a neuron.");

            // Verify this is the START neuron.
            if (this != neuronConnection.FromNeuron)
                throw new ArgumentException("Cannot store output connection for this neuron when the connection already exists between two other neurons.");

            OutputConnections.Add(neuronConnection);
        }

        /// <summary>
        /// Randomizes the associated weights of all inputs.
        /// </summary>
        public void RandomizeWeights()
        {
            InputConnections.ToList().ForEach(conn =>
            {
                conn.RandomizeWeight();
            });
        }

        /// <summary>
        /// Randomizes the associated bias of this neuron.
        /// </summary>
        public void RandomizeBias()
        {
            BiasValue = NNUtils.GenerateRandomNumber(0, 100) / 100.0;
        }

        /// <summary>
        /// Returns the calculated activation level by computing the weighted-summation of all input activation
        /// levels, and then applying the designated activation function.
        /// </summary>
        public double GetCalculatedActivationLevel(bool applyActivationFunction = true)
        {
            if (!IsValidNeuron())
                throw new InvalidOperationException("Neuron must be properly established before retrieving activation levels.");

            // Compute sum of all input-neuron's activation levels * connection weights.
            var weightedActivationLevel = InputConnections.Sum(conn => conn.FromNeuron.ActivationLevel * conn.Weight) + BiasValue;

            // Apply activation function to weighted activation level and return the results.
            if (applyActivationFunction)
                return ActivationFunctionController.GetActivationFunctionForType(ActivationFunctionType)(weightedActivationLevel);
            else
                return weightedActivationLevel;
        }

        /// <summary>
        /// Stores a hard-defined activation level for this neuron. This should only be 
        /// set if the neuron is part of an input layer.
        /// </summary>
        internal void SetActivationLevel(double activationLevel)
        {
            _activationLevel = activationLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string ToString()
        {
            var returnStr = $"<Neuron Name='{Name}' ActivationLevel='{ActivationLevel}'>";

            if (OutputConnections.Any())
            {
                returnStr += $"<OutputConnections>";

                foreach (var outputConn in OutputConnections)
                {
                    returnStr += $"<OutputConnection Weight='{outputConn.Weight}'>{outputConn.ToNeuron.ToString()}</OutputConnection>";
                }

                returnStr += $"</OutputConnections>";
            }

            returnStr += "</Neuron>";

            return returnStr;
        }
    }
}
