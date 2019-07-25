using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.Neurons;
using NNLib.NeuralNetworking.NetworkLayers;
using NNLib.NeuralNetworking.ActivationFunctions;
using NNLib.NeuralNetworking.Datasets;

namespace NNLib.NeuralNetworking
{
    public class FeedForwardNetwork : NeuralNetwork
    {
        private double _learningRate = 0.01;

        /// <summary>
        /// The learning rate multipler to identify how large of 'steps' the network
        /// takes when learning from backpropogation (Should be a very small number).
        /// </summary>
        public double LearningRate
        {
            get
            {
                return _learningRate;
            }
            set
            {
                if (value < 0.0)
                    _learningRate = 0.0;
                else
                    _learningRate = value;
            }
        }
        
        /// <summary>
        /// Returns the network layers list sorted by input layers, then hidden layers, then output layers.
        /// </summary>
        public IList<NetworkLayer> SortedNetworkLayers => NetworkLayers.OrderBy(layer => layer.GetType() == typeof(InputLayer) ?
                                                                0 :
                                                                layer.GetType() == typeof(HiddenLayer) ?
                                                                    1 :
                                                                    layer.GetType() == typeof(OutputLayer) ?
                                                                        2 :
                                                                        3).ToList();

        /// <summary>
        /// Instantiates a feed forward network with an input and output network layer. 
        /// </summary>
        public FeedForwardNetwork(InputLayer inputLayer, OutputLayer outputLayer)
            : this(inputLayer, outputLayer, null)
        {

        }

        /// <summary>
        /// Instantiates a feed forward network with an input, output, and hidden layers.
        /// </summary>
        public FeedForwardNetwork(InputLayer inputLayer, OutputLayer outputLayer, IList<HiddenLayer> hiddenLayers)
        {
            InitializeNetwork(inputLayer, outputLayer, hiddenLayers);
        }

        /// <summary>
        /// Instantiates a feed forward network with an input layer, output layer, and 'numHiddenLayers' hidden layers.
        /// </summary>
        public FeedForwardNetwork(InputLayer inputLayer, OutputLayer outputLayer, int numHiddenLayers, int numNeuronsPerHiddenLayer, ActivationFunctionController.ActivationFunctionType hiddenLayerActivationFunctionType)
        {
            if (numHiddenLayers < 0)
                throw new ArgumentNullException("Cannot create FeedForwardNetwork with negative hidden layers.");
            if (numHiddenLayers > 0 && numNeuronsPerHiddenLayer <= 0)
                throw new ArgumentNullException("Cannot create FeedForwardNetwork hidden layers containing 'negative' amounts of neurons.");

            var hiddenLayers = new List<HiddenLayer>();

            // Create 'numHiddenLayers' hidden layers and add it to the hiddenLayers list.
            for (int hiddenLayerNum = 0; hiddenLayerNum < numHiddenLayers; hiddenLayerNum++)
            {
                hiddenLayers.Add(new HiddenLayer(hiddenLayerActivationFunctionType, numNeuronsPerHiddenLayer));
            }

            InitializeNetwork(inputLayer, outputLayer, hiddenLayers);
        }

        #region NeuralNetwork Abstract Methods

        /// <summary>
        /// Returns whether the network is valid by making sure it contains atleast 1
        /// input and output layers and that each layer is valid.
        /// </summary>
        public override bool IsValidNetwork(bool clearIfInvalid = false)
        {
            // FeedForwardNN needs atleast 1 input and 1 output layer.
            bool isValid = NetworkLayers?.OfType<InputLayer>()?.Count() == 1 &&
                           NetworkLayers?.OfType<OutputLayer>()?.Count() == 1;

            // Check each layer to make sure it's valid.
            NetworkLayers?.ToList().ForEach(layer =>
            {
                isValid &= layer != null && layer.IsValidNetworkLayer();
            });

            if (clearIfInvalid && !isValid)
            {
                NetworkLayers = new List<NetworkLayer>();
            }

            return isValid;
        }

        /// <summary>
        /// Connects network-layer neurons together based on the layer they belong to. 
        /// Ie. All input layer neurons are connected to HiddenLayer1's neurons whos neurons are all
        /// connected to HiddenLayer2's neurons.. and so forth.
        /// </summary>
        public override void GenerateLayerConnections()
        {
            if (!IsValidNetwork())
                throw new InvalidOperationException("Cannot generate layer connections on an invalid network.");

            NetworkLayer lastLayer = null;

            foreach (var thisLayer in SortedNetworkLayers)
            {
                if (lastLayer != null)
                {
                    // Connect the last layer to this layer.
                    lastLayer.ConnectAllNeurons(thisLayer);
                }

                lastLayer = thisLayer;
            }
        }

        /// <summary>
        /// Identifies if the supplied network dataset can be used to train/test this network by
        /// verifying the number of inputs and outputs matches the number of input and output
        /// neurons, respectively.
        /// </summary>
        public override void ValidateDatasetForNetwork(NetworkDataset dataset)
        {
            if (dataset == null)
                throw new ArgumentNullException("Cannot train or test neural network with a null dataset.");

            // Verify the examples contain the same number of inputs 
            //and outputs that this network contains.
            foreach (var datasetEntry in dataset.DatasetEntries)
            {
                // Network is valid so it should only contain 1 input and 1 output layer.
                var networkInputLayer = SortedNetworkLayers.OfType<InputLayer>().First();
                var networkOutputLayer = SortedNetworkLayers.OfType<OutputLayer>().First();

                if (datasetEntry.Inputs == null || datasetEntry.Inputs.Count() != networkInputLayer.RegisteredNeurons.Count())
                    throw new ArgumentException("The supplied dataset contains 1 or more examples where the number of INPUTS does not equal the number of INPUT NEURONS.");

                if (datasetEntry.Outputs == null || datasetEntry.Outputs.Count() != networkOutputLayer.RegisteredNeurons.Count())
                    throw new ArgumentException("The supplied dataset contains 1 or more examples where the number of OUTPUTS does not equal the number of OUTPUT NEURONS.");
            }
        }

        /// <summary>
        /// Used to 'Learn' a training dataset by cycling through each provided training example to find the AVERAGE cost
        /// of the entire training dataset. After this is done, the network will back-propogate to adjust the 
        /// weights and bias' of each input/hidden/output connection.
        /// </summary>
        public override void TrainNetwork(NetworkTrainer networkTrainer)
        {
            if (!IsValidNetwork())
                throw new InvalidOperationException("Cannot teach this network yet without proper initialization.");

            if (networkTrainer == null)
                throw new ArgumentNullException("Cannot train network with a null network trainer.");

            ValidateDatasetForNetwork(networkTrainer.Dataset);
            
            networkTrainer.ResetTrainingCosts();

            for (int trainingIteration = 0; trainingIteration < networkTrainer.NumTimesToTrainNetwork; trainingIteration++)
            {
                // Create a training costs entry for the training iteration if it doesn't exist.
                if (!networkTrainer.TrainingCostsPerIteration.ContainsKey(trainingIteration))
                {
                    networkTrainer.TrainingCostsPerIteration.Add(trainingIteration, new List<double>());
                }

                networkTrainer.Dataset.DatasetEntries.ToList().ForEach(trainingEntry =>
                {
                    // Apply list of training-inputs to the input neurons.
                    // The activations are automatically updated for each layer
                    // so forward-propogation occurs naturally.
                    SortedNetworkLayers.OfType<InputLayer>().First().ApplyInputs(trainingEntry.Inputs);

                    // Dictionary to hold the output neurons and their expected output activation values.
                    var expectedNeuronOutputsDict = new Dictionary<Neuron, double>();
                    var activationCosts = new List<double>();

                    SortedNetworkLayers.OfType<OutputLayer>().First().RegisteredNeurons.ForEach(trainingEntry.Outputs, (outputNeuron, expectedActivation) =>
                    {
                        expectedNeuronOutputsDict.Add(outputNeuron, expectedActivation);
                        activationCosts.Add(CalculateActivationCost(outputNeuron.ActivationLevel, expectedActivation));
                    });

                    // Track activation cost averages to identify how smart the network is becoming.
                    networkTrainer.TrainingCostsPerIteration[trainingIteration].Add(activationCosts.Average());

                    // Back-propagates the network using the computed learning activation cost 
                    // and updates neuron and neuron connection weights and bias derivatives.
                    GenerateNetworkDerivatives(expectedNeuronOutputsDict);

                    // Adjust each of the neuron and neuron connection
                    // weights and biases.
                    AdjustNetworkWeightsAndBiases();
                });
            }
        }
        
        /// <summary>
        /// Used to test a trained neural network by cycling through each of the provided testing examples
        /// to find the overall percentage of correct input/output combinations.
        /// </summary>
        public override double TestNetwork(NetworkTester networkTester)
        {
            if (!IsValidNetwork())
                throw new InvalidOperationException("Cannot test this network yet without proper initialization.");

            if (networkTester == null)
                throw new ArgumentNullException("Cannot test network with a null network tester.");

            ValidateDatasetForNetwork(networkTester.Dataset);
            
            var numTestCasesCorrect = networkTester.Dataset.DatasetEntries.ToList().Count(testingEntry =>
            {
                // Apply test inputs.
                SortedNetworkLayers.OfType<InputLayer>().First().ApplyInputs(testingEntry.Inputs);

                // Identify expected & actual outputs from the applied inputs.
                var actualOutputs = SortedNetworkLayers.OfType<OutputLayer>().First().GetOutputActivationLevels();
                var expectedOutputs = testingEntry.Outputs;

                if (networkTester.TestingStrategyType == NetworkTester.TestingStrategyTypes.HighestValue)
                {
                    return actualOutputs.IndexOf(actualOutputs.Max()) == expectedOutputs.IndexOf(expectedOutputs.Max());
                }
                else if (networkTester.TestingStrategyType == NetworkTester.TestingStrategyTypes.FaultTolerance)
                {
                    var numCorrectNeuronActivations = 0;

                    // Determine if each of the actual neuron activations is within
                    // a predefined fault tolerance of the expected activation level.
                    expectedOutputs.ForEach(actualOutputs, (expected, actual) =>
                    {
                        if (expected < actual + (networkTester.FaultTolerance / 2.0) &&
                            expected > actual - (networkTester.FaultTolerance / 2.0))
                        {
                            numCorrectNeuronActivations++;
                        }
                    });

                    // Return true/false if ALL neuron activation levels are correct for this test.
                    return numCorrectNeuronActivations == actualOutputs.Count();
                }
                else
                    throw new NotSupportedException("Network tester contains a testing strategy type that is not supported.");                             
            });
            
            return 100.0 * (double)numTestCasesCorrect / (double)networkTester.Dataset.DatasetEntries.Count();
        }

        /// <summary>
        /// Randomizes the weights and biases of each neuron and neuron connection.
        /// </summary>
        public override void ResetNetworkProgress()
        {
            if (!IsValidNetwork())
                throw new InvalidOperationException("Cannot reset this network until it is valid.");

            NetworkLayers.ToList().ForEach(layer => layer.RandomizeWeightsAndBias());
        }

        /// <summary>
        /// Clones the current neural network and removes the reference attached.
        /// </summary>
        public override object Clone()
        {
            return (FeedForwardNetwork)this.MemberwiseClone();
        }

        #endregion

        #region Backpropogation Algorithms

        /// <summary>
        /// Updates the weights and biases of each neuron and neuron connection
        /// in the network based on multiplying the learningRate by the associated
        /// derivatives.
        /// </summary>
        private void AdjustNetworkWeightsAndBiases()
        {
            foreach (var layer in SortedNetworkLayers)
            {
                foreach (var neuron in layer.RegisteredNeurons)
                {
                    neuron.BiasValue -= LearningRate * neuron.Derivative_CostWRTBias;

                    foreach (var outConn in neuron.OutputConnections)
                    {
                        outConn.Weight -= LearningRate * outConn.Derivative_CostWRTWeight;
                    }
                }
            }
        }

        /// <summary>
        /// Computes and stores the neuron and neuron connection derivatives for the
        /// network by back-propogating the output-neuron activation costs that
        /// were provided.
        /// </summary>
        private void GenerateNetworkDerivatives(Dictionary<Neuron, double> expectedNeuronOutputsDict)
        {
            if (expectedNeuronOutputsDict == null)
            {
                throw new ArgumentNullException("Cannot adjust the network with a null 'costs' list..");
            }
            
            // Verify each output layer neuron is represented in the costs dictionary.
            foreach (var neuron in SortedNetworkLayers.OfType<OutputLayer>().First().RegisteredNeurons)
            {
                if (!expectedNeuronOutputsDict.ContainsKey(neuron))
                {
                    throw new ArgumentException("To adjust the values using total costs, each neuron in the output layer must be represented in the total-cost adjustment dictionary.");
                }
            }

            // Reset each of the neuron derivative variables so that they can be incremented/modified accurately.
            ResetNeuronDerivatives();

            foreach (var networkLayer in SortedNetworkLayers.Reverse())
            {
                foreach (var neuron in networkLayer.RegisteredNeurons)
                {
                    var aL = neuron.ActivationLevel;                                // The activation level for THIS neuron                                                                                      
                    var zL = neuron.GetCalculatedActivationLevel(false);            // The Intermediate value 'Z' --> the neuron's activation level without applying the activation function.
                    var yL = networkLayer is OutputLayer ?                          // The expected output of the neuron (This value is ignored if the neuron doesn't belong to the output layer).
                                    expectedNeuronOutputsDict[neuron] :
                                    0.1234;

                    var dC0_daL = networkLayer is OutputLayer ?                                                                         // The derivative of the cost with respect to the activation of this neuron (How the cost changes as the activation changes).   
                                                2.0 * (aL - yL) :                                                                       // This value depends on if the neuron belongs to the output layer or not.
                                                neuron.Derivative_CostWRTActivation;
                    var daL_dzL = ActivationFunctionController.GetActivationFunctionDerivative(neuron.ActivationFunctionType, zL);      // The derivative of the Activation of this neuron with respect to Z (How the activation changes as the Z changes).
                    var dC0_dbL = 1.0 * daL_dzL * dC0_daL;                                                                              // The derivative of the cost with respect to the bias of this neuron (How the cost changes as the bias changes).

                    neuron.Derivative_CostWRTActivation = dC0_daL;                 // The neuron stores this derivative so that it may be used to update future values.
                    neuron.Derivative_CostWRTBias = dC0_dbL;                       // The neuron stores this derivative so that it may be used to update future bias values.

                    // Foreach input connection, compute the appropriate derivatives to 
                    // indicate how much the weight and input neuron's activation levels
                    // have on the overall cost.
                    foreach (var inputConn in neuron.InputConnections)
                    {
                        var dzL_dwL = inputConn.FromNeuron.ActivationLevel;         // The derivative of Z with respect to the INPUT weight of this layer (How Z changes as the INPUT weight changes).
                        var dC0_dwL = dzL_dwL * daL_dzL * dC0_daL;                  // The derivative of the cost with respect to the INPUT weight (How the cost changes as the INPUT weight changes).
                        inputConn.Derivative_CostWRTWeight = dC0_dwL;               // The neuron stores this derivative so that it may be used to update future weight values.

                        var dzL_daLMinus1 = inputConn.Weight;                                   // The derivative of the intermediateZ with respect to the activation of the input-connecting neuron (How Z changes as the input-connecting neuron's activation changes).
                        var dC0_daLMinus1 = dzL_daLMinus1 * daL_dzL * dC0_daL;                  // The derivative of the cost with respect to the activation of the input-connecting neuron (How the cost changes as the input-connecting neuron's activation changes).

                        // The derivative of sums is the sum of derivatives. 
                        // ie. Tell the input-connecting neuron how much of an effect its activation
                        //     level has on the overall cost of the network. This effect is going to 
                        //     be a sum of all derivative values for each of the neurons the input-
                        //     connecting neuron connects to.
                        inputConn.FromNeuron.Derivative_CostWRTActivation += dC0_daLMinus1;
                    }
                }
            }
        }

        /// <summary>
        /// Resets all derivatives for each neuron and neuron-connection to 0.0.
        /// </summary>
        private void ResetNeuronDerivatives()
        {
            foreach (var networkLayer in SortedNetworkLayers)
            {
                foreach (var neuron in networkLayer.RegisteredNeurons)
                {
                    neuron.Derivative_CostWRTActivation = 0.0;
                    neuron.Derivative_CostWRTBias = 0.0;

                    foreach (var inputConn in neuron.InputConnections)
                    {
                        inputConn.Derivative_CostWRTWeight = 0.0;
                    }

                    foreach (var outputConn in neuron.OutputConnections)
                    {
                        outputConn.Derivative_CostWRTWeight = 0.0;
                    }
                }
            }
        }
        
        /// <summary>
        /// Given the actual and expected activation levels, this method will return the TOTAL activation cost by
        /// summing up the squares of the differences between the actual and expected activation levels.
        /// </summary>
        private double CalculateAverageActivationCost(IList<double> actualActivationLevels, IList<double> expectedActivationLevels)
        {
            if (actualActivationLevels == null || expectedActivationLevels == null || actualActivationLevels.Count() != expectedActivationLevels.Count())
                throw new ArgumentException("The actual and expected activation level lists must contain the same number of entries.");

            var totalActivationCost = new List<double>();
            
            actualActivationLevels.ForEach(expectedActivationLevels, (actualAct, expectedAct) =>
            {
                totalActivationCost.Add(Math.Pow(actualAct - expectedAct, 2));
            });

            return totalActivationCost.Average();
        }

        #endregion

        /// <summary>
        /// Initializes the network using mandatory input and output layers, and optional hidden layers.
        /// </summary>
        private void InitializeNetwork(InputLayer inputLayer, OutputLayer outputLayer, IList<HiddenLayer> hiddenLayers)
        {
            if (inputLayer == null || !inputLayer.IsValidNetworkLayer())
                throw new ArgumentNullException("Cannot create FeedForwardNetwork with null or invalid input layer.");
            if (outputLayer == null || !outputLayer.IsValidNetworkLayer())
                throw new ArgumentNullException("Cannot create FeedForwardNetwork with null or invalid output layer.");

            NetworkLayers = new List<NetworkLayer>();
            NetworkLayers.Add(inputLayer);
            NetworkLayers.Add(outputLayer);

            // Try to add hidden layers.
            if (hiddenLayers != null)
            {
                hiddenLayers.ToList().ForEach(layer =>
                {
                    if (layer == null || !layer.IsValidNetworkLayer())
                        throw new ArgumentException("Cannot form a FeedForwardNetwork with null or invalid hidden layers.");

                    NetworkLayers.Add(layer);
                });
            }

            // Verify network is valid.
            if (!IsValidNetwork(true))
            {
                throw new ArgumentException("Unable to generate feed forward network using the supplied arguments.");
            }
        }
    }
}
