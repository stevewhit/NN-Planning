using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNLib.NeuralNetworking.ActivationFunctions
{
    public static class ActivationFunctionController
    {
        /// <summary>
        /// Represents the activation function type assigned to a network layer 
        /// </summary>
        public enum ActivationFunctionType
        {
            ReLU,
            Sigmoid,
            None
        }

        /// <summary>
        /// Accepts an activation function type and returns the method that corresponds with it.
        /// </summary>
        public static Func<double, double> GetActivationFunctionForType(ActivationFunctionType functionType)
        {
            switch(functionType)
            {
                case ActivationFunctionType.ReLU:
                    return ApplyRectifiedLinearUnitAlgorithm;
                case ActivationFunctionType.Sigmoid:
                    return ApplySigmoidAlgorithm;
                case ActivationFunctionType.None:
                    return ReturnInput;
                default:
                    throw new NotImplementedException($"Activation function type is not supported: '{functionType}'.");
            }
        }

        /// <summary>
        /// Calculates the derivative value for a given activation function type.
        /// </summary>
        public static double GetActivationFunctionDerivative(ActivationFunctionType functionType, double value)
        {
            switch (functionType)
            {
                case ActivationFunctionType.ReLU:
                    return GetRecifiedLinearUnitDerivativeValue(value);
                case ActivationFunctionType.Sigmoid:
                    return GetSigmoidDerivativeValue(value);
                case ActivationFunctionType.None:
                    return GetInputDerivativeValue(value);
                default:
                    throw new NotImplementedException($"Activation function type is not supported: '{functionType}'.");
            }
        }

        /// <summary>
        /// Calculates the output of applying the input to the ReLU function.
        /// </summary>
        private static double ApplyRectifiedLinearUnitAlgorithm(double input)
        {
            return Math.Max(input, 0.0);
        }

        /// <summary>
        /// Calculates the output of applying the input to the sigmoid function.
        /// </summary>
        private static double ApplySigmoidAlgorithm(double input)
        {
            return 1.0 / (1.0 + Math.Exp(-1.0 * input));
        }

        /// <summary>
        /// Output = applied input.
        /// </summary>
        private static double ReturnInput(double input)
        {
            return input;
        }
        
        /// <summary>
        /// Returns the derivative value of the ReLU activation function.
        /// </summary>
        private static double GetRecifiedLinearUnitDerivativeValue(double value)
        {
            // An approximation from (https://jamesmccaffrey.wordpress.com/2017/06/23/two-ways-to-deal-with-the-derivative-of-the-relu-function/)
            return ApplySigmoidAlgorithm(value);

            /* True value except value=0.0 is un-defined so it is approximated
            return value < 0.0 ? 
                                0.0 : 
                                value == 0.0 ? 
                                            0.5 : 
                                            1.0;
            */
        }

        /// <summary>
        /// Returns the derivative value of the sigmoid activation function.
        /// </summary>
        private static double GetSigmoidDerivativeValue(double value)
        {
            return value * (1.0 - value);
        }

        /// <summary>
        /// Returns the derivative value of the 'input' (bias) activation function.
        /// </summary>
        private static double GetInputDerivativeValue(double value)
        {
            return 1.0;
        }
    }
}
