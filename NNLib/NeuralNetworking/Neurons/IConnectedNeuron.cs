using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib.NeuralNetworking.ActivationFunctions;

namespace NNLib.NeuralNetworking.Neurons
{
    public interface IConnectedNeuron
    {
        double ActivationLevel { get; }
        ActivationFunctionController.ActivationFunctionType ActivationFunctionType { get; }
        bool IsValidNeuron(bool clearIfInvalid = false);
        double BiasValue { get; set; }
        double Derivative_CostWRTBias { get; set; }
        double Derivative_CostWRTActivation { get; set; }
    }
}
