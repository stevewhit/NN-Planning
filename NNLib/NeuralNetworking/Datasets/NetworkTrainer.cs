using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NNLib.NeuralNetworking.Datasets
{
    public class NetworkTrainer
    {
        /// <summary>
        /// The number of times to run the 'TrainingDataset' through the 
        /// neural network. 
        /// </summary>
        private int _numTimesToTrainNetwork;
        
        /// <summary>
        /// The number of times to run the 'TrainingDataset' through the 
        /// neural network. 
        /// </summary>
        public int NumTimesToTrainNetwork
        {
            get
            {
                return _numTimesToTrainNetwork;
            }
            set
            {
                _numTimesToTrainNetwork = value < 0 ?
                                                1 :
                                                value;
            }
        }

        /// <summary>
        /// The dataset that is used to train a neural network.
        /// </summary>
        public NetworkDataset Dataset { get; set; }

        /// <summary>
        /// Holds a list of training costs for each iteration
        /// that the trainer trains a network.
        /// </summary>
        public Dictionary<int, IList<double>> TrainingCostsPerIteration { get; set; }

        public NetworkTrainer(int numTimesToTrainNetwork = 1)
        {
            Dataset = new NetworkDataset();
            NumTimesToTrainNetwork = numTimesToTrainNetwork;
            TrainingCostsPerIteration = new Dictionary<int, IList<double>>();
        }

        /// <summary>
        /// Resets the list of training costs averages.
        /// </summary>
        public void ResetTrainingCosts()
        {
            TrainingCostsPerIteration = new Dictionary<int, IList<double>>();
        }
    }
}
