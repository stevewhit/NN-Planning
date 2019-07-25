using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NNLib;
using System.Diagnostics;
using SM.Data;
using SM.Data.Importing;
using NNLib.NeuralNetworking.NetworkLayers;
using NNLib.NeuralNetworking.Neurons;
using NNLib.NeuralNetworking.ActivationFunctions;
using NNLib.NeuralNetworking;
using NNLib.NeuralNetworking.Datasets;
using SMNN;

namespace nn_console_app
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create trainable/testable network dataset from filepath. --> GetNetworkDatasetFromFilePath(string filepath).. --> ImportNetworkDatasetFromPath(string filepath).

            // Create basic nn. 

            SMNN.RunThisSMNN.Run();

        }

        
    }
}
