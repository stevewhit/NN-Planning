using Framework.Generic.EntityFramework;
using NeuralNetwork.Generic.Datasets;
using NeuralNetwork.Generic.Layers;
using NeuralNetwork.Generic.Networks;
using NeuralNetwork.Generic.Neurons;
using SANNET.DataModel;
using StockMarket.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SANNET.Business.Services
{
    public interface IPredictionService<P> : IDisposable where P : Prediction
    {
        /// <summary>
        /// Returns stored predictions.
        /// </summary>
        /// <returns>Returns predictions stored in the repository.</returns>
        IDbSet<P> GetPredictions();

        /// <summary>
        /// Finds and returns the prediction with the matching id.
        /// </summary>
        /// <param name="id">The id of the prediction to return.</param>
        /// <returns>Returns the prediction with the matching id.</returns>
        P FindPrediction(int id);

        /// <summary>
        /// Adds the supplied <paramref name="prediction"/>.
        /// </summary>
        /// <param name="prediction">The prediction that is to be added.</param>
        void Add(P prediction);

        /// <summary>
        /// Updates the supplied <paramref name="prediction"/>.
        /// </summary>
        /// <param name="prediction">The prediction that is to be updated.</param>
        void Update(P prediction);

        /// <summary>
        /// Finds and deletes an existing prediction by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of prediction to be deleted.</param>
        void Delete(int id);

        /// <summary>
        /// Deletes the supplied <paramref name="prediction"/>.
        /// </summary>
        /// <param name="prediction">The prediction that is to be deleted.</param>
        void Delete(P prediction);
        
        void GenerateAllPredictions();
        void GenerateCompanyPredictions(int companyId, int networkConfigId);
        void GenerateQuotePrediction(int quoteId, int networkConfigId);
        void AnalyzePredictions();
    }

    public class PredictionService<P, Q, C, N> : IPredictionService<P> where P : Prediction where Q : Quote where C : Company where N : NetworkConfiguration
    {
        private bool _isDisposed = false;

        private readonly IEfRepository<P> _predictionRepository;
        private readonly IQuoteService<Q> _quoteService;
        private readonly ICompanyService<C> _companyService;
        private readonly IDatasetService _datasetService;
        private readonly INetworkConfigurationService<N> _networkConfigurationService;

        public PredictionService(IEfRepository<P> predictionRepository, IQuoteService<Q> quoteService, ICompanyService<C> companyService, IDatasetService datasetService, INetworkConfigurationService<N> networkConfigurationService)
        {
            _predictionRepository = predictionRepository ?? throw new ArgumentNullException("predictionRepository");
            _quoteService = quoteService ?? throw new ArgumentNullException("quoteService");
            _companyService = companyService ?? throw new ArgumentNullException("companyService");
            _datasetService = datasetService ?? throw new ArgumentNullException("datasetService");
            _networkConfigurationService = networkConfigurationService ?? throw new ArgumentNullException("networkConfigurationService");
        }

        #region IPredictionService<P>

        /// <summary>
        /// Returns stored predictions.
        /// </summary>
        /// <returns>Returns predictions stored in the repository.</returns>
        public IDbSet<P> GetPredictions()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");

            return _predictionRepository.GetEntities();
        }

        /// <summary>
        /// Finds and returns the prediction with the matching id.
        /// </summary>
        /// <param name="id">The id of the prediction to return.</param>
        /// <returns>Returns the prediction with the matching id.</returns>
        public P FindPrediction(int id)
        {
            return GetPredictions().FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Adds the supplied <paramref name="prediction"/>.
        /// </summary>
        /// <param name="prediction">The prediction that is to be added.</param>
        public void Add(P prediction)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");

            if (prediction == null)
                throw new ArgumentNullException("prediction");

            _predictionRepository.Add(prediction);
            _predictionRepository.SaveChanges();
        }

        /// <summary>
        /// Updates the supplied <paramref name="prediction"/>.
        /// </summary>
        /// <param name="prediction">The prediction that is to be updated.</param>
        public void Update(P prediction)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");

            if (prediction == null)
                throw new ArgumentNullException("prediction");

            _predictionRepository.Update(prediction);
            _predictionRepository.SaveChanges();
        }

        /// <summary>
        /// Finds and deletes an existing prediction by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of prediction to be deleted.</param>
        public void Delete(int id)
        {
            var prediction = FindPrediction(id);

            if (prediction == null)
                throw new ArgumentException($"A prediction with the supplied id doesn't exist: {id}.");

            Delete(prediction);
        }

        /// <summary>
        /// Deletes the supplied <paramref name="prediction"/>.
        /// </summary>
        /// <param name="prediction">The prediction that is to be deleted.</param>
        public void Delete(P prediction)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");

            if (prediction == null)
                throw new ArgumentNullException("prediction");

            _predictionRepository.Delete(prediction);
            _predictionRepository.SaveChanges();
        }

        /// <summary>
        /// Generates quote predictions for every company's network configuration.
        /// </summary>
        public void GenerateAllPredictions()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");

            var companies = _companyService.GetCompanies().ToList();
            var networkConfigs = _networkConfigurationService.GetConfigurations().ToList();

            foreach (var company in companies.Where(c => c.Id == 1))
            {
                foreach (var config in networkConfigs.Where(c => c.Id == 1))
                {
                    GenerateCompanyPredictions(company.Id, config.Id);
                }
            }
        }
        
        /// <summary>
        /// Generates quote predictions for a company using the specified network configuration.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="networkConfigId"></param>
        public void GenerateCompanyPredictions(int companyId, int networkConfigId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");

            var companyPredictions = GetPredictions().Where(p => p.CompanyId == companyId);
            var companyQuotes = _quoteService.GetQuotes().Where(q => q.CompanyId == companyId).ToList();
            var quotesWithoutPredictions = companyQuotes.Where(q => !companyPredictions.Any(p => p.QuoteId == q.Id && p.ConfigurationId == networkConfigId)).ToList();

            foreach (var quote in quotesWithoutPredictions)
            {
                try
                {
                    GenerateQuotePrediction(quote.Id, networkConfigId);
                }
                catch(Exception e)
                {
                    var blah = e;
                    // This is not how it should be handled. This is temporary!
                }
            }
        }
        
        /// <summary>
        /// Generates quote predictions for a specific quote(date) using the specified network configuration.
        /// </summary>
        /// <param name="quoteId"></param>
        /// <param name="networkConfigId"></param>
        public void GenerateQuotePrediction(int quoteId, int networkConfigId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");

            var quote = _quoteService.FindQuote(quoteId) ?? throw new InvalidOperationException($"Cannot find quote with id: {quoteId}.");
            var networkConfig = _networkConfigurationService.FindConfiguration(networkConfigId) ?? throw new InvalidOperationException($"Cannot find network configuration with id: {networkConfigId}.");

            var trainingStartDate = quote.Date.AddDays(-1).AddMonths(-1 * networkConfig.NumTrainingMonths);
            var trainingEndDate = quote.Date.AddDays(-1);

            // Generate datasets
            var trainingDatasetEntries = _datasetService.GetTrainingDataset(networkConfigId, quote.CompanyId, trainingStartDate, trainingEndDate);
            var predictionDayInputs = _datasetService.GetNetworkInputs(networkConfigId, quote.CompanyId, quote.Date);

            // Create and train a neural network using the network config and dataset entries.
            var trainedNetwork = CreateTrainedNetwork(networkConfig, trainingDatasetEntries);

            var networkInputLayer = trainedNetwork.Layers.OfType<IInputLayer>().First();
            SyncInputNeuronsAndNeuronInputs(predictionDayInputs, networkInputLayer);

            // Apply inputs to network.
            var outputs = trainedNetwork.ApplyInputs(predictionDayInputs);

            // Generate and add prediction with confidence in the prediction.
            Add((P)new Prediction()
            {
                ConfigurationId = networkConfigId,
                CompanyId = quote.CompanyId,
                QuoteId = quoteId,
                TrainingStartDate = trainingStartDate,
                TrainingEndDate = trainingEndDate,
                PredictedOutcome = string.Join(",", outputs.Select(o => $"[({o.ActivationLevel * 100.0}%) {o.Description}]"))
            });
        }





        // Things to fix:
        // 1. Need to train NN for each prediction much longer than just the 2 months. re-iterate over the dataset multiple times until it gets more accurate??
        // 2. Add async logic into this using BELOW examples.
        // 3. Add try catch blocks where necessary to make sure this doesn't blow up if one prediction goes bad.
        // 4. Remember your .ToList() returns so that we're not passing IEnumerables around and run into context transaction exceptions..
        









        ///// <summary>
        ///// Downloads and stores quotes for all active companies up to max range of quotes.
        ///// </summary>
        //public async Task UpdateAllCompaniesWithLatestQuotesAsync()
        //{
        //    if (_isDisposed)
        //        throw new ObjectDisposedException("MarketService", "The service has been disposed.");

        //    // Asyncronously download stock data for each company.
        //    var runningTasks = _companyService.GetCompanies().Where(c => c.RetrieveQuotesFlag).ToList().Select(c => GetLatestQuotesForCompanyAsync(c)).ToList();
        //    var taskExceptions = new List<Exception>();

        //    while (runningTasks.Any())
        //    {
        //        try
        //        {
        //            var completedTask = await Task.WhenAny(runningTasks);
        //            runningTasks.Remove(completedTask);

        //            // Process any completed task by adding the new quote data
        //            _quoteService.AddRange(await completedTask);
        //        }
        //        catch (Exception e)
        //        {
        //            taskExceptions.Add(e);
        //        }
        //    }

        //    if (taskExceptions.Any())
        //        throw new AggregateException(taskExceptions);
        //}

        ///// <summary>
        ///// Asycronously downloads and returns quotes for a given company, up to a max range. 
        ///// </summary>
        ///// <param name="company">The company to store the quotes for.</param>
        ///// <returns>Returns the downloaded quotes for the given company.</returns>
        //private async Task<IEnumerable<Q>> GetLatestQuotesForCompanyAsync(C company)
        //{
        //    var companyQuotes = company.Quotes.ToList();

        //    // If quotes are stored for this company, return the last date a quote was stored.
        //    var lastStoredQuoteDate = companyQuotes.Any() ? companyQuotes.Max(q => q.Date).Date : DateTime.Now.AddMonths(-1 * MAX_MONTHS_TO_DOWNLOAD).AddDays(-1).Date;
        //    var todaysDate = DateTime.Now.Date;

        //    return await Task.Run(() =>
        //    {
        //        // Download quotes in chuncks based off the date difference between the last stored quote date and today's date.
        //        var downloadedQuotes = lastStoredQuoteDate.AddDays(1) >= todaysDate ? new[] { _downloader.DownloadPreviousDayQuote(company.Symbol) } :
        //                               lastStoredQuoteDate.AddDays(5) >= todaysDate ? _downloader.DownloadQuotesFiveDays(company.Symbol) :
        //                               lastStoredQuoteDate.AddMonths(1) >= todaysDate ? _downloader.DownloadQuotesOneMonth(company.Symbol) :
        //                               lastStoredQuoteDate.AddMonths(3) >= todaysDate ? _downloader.DownloadQuotesThreeMonths(company.Symbol) :
        //                               lastStoredQuoteDate.AddMonths(5) >= todaysDate ? _downloader.DownloadQuotesFiveMonths(company.Symbol) :
        //                               lastStoredQuoteDate.AddYears(1) >= todaysDate ? _downloader.DownloadQuotesOneYear(company.Symbol) :
        //                               lastStoredQuoteDate.AddYears(2) >= todaysDate ? _downloader.DownloadQuotesTwoYears(company.Symbol) :
        //                               _downloader.DownloadQuotesTwoYears(company.Symbol);

        //        // Remove any dates that are already stored for the company.
        //        var quotes = downloadedQuotes.Where(dq => !companyQuotes.Any(cq => cq.Date == dq.Date)).OrderBy(dq => dq.Date).ToList();

        //        // Store company for each quote.
        //        return quotes.ForEach<Q>(q => { q.Company = company; q.CompanyId = company.Id; });
        //    });
        //}









        /// <summary>
        /// Dynamically creates a neural network using details from the networkConfiguration and the trainingDatasetEntries. After creation, the network is the
        /// trained using the trainingDatasetEntries and returned;
        /// </summary>
        /// <param name="networkConfiguration"></param>
        /// <param name="trainingDatasetEntries"></param>
        /// <returns></returns>
        private IDFFNeuralNetwork CreateTrainedNetwork(N networkConfiguration, IEnumerable<INetworkTrainingIteration> trainingDatasetEntries)
        {
            if (networkConfiguration == null)
                throw new ArgumentNullException("networkConfiguration");

            if (trainingDatasetEntries == null || !trainingDatasetEntries.Any())
                throw new ArgumentNullException("trainingDatasetEntries");

            var numInputs = trainingDatasetEntries.First().Inputs?.Count() ?? throw new ArgumentException("TrainingDatasetEntries contains entry with null inputs.");
            var numOutputs = trainingDatasetEntries.First().Outputs?.Count() ?? throw new ArgumentException("TrainingDatasetEntries contains entry with null outputs.");
            var numHiddenLayers = networkConfiguration.NumHiddenLayers;
            var numHiddenLayerNeurons = networkConfiguration.NumHiddenLayerNeurons;

            // Setup network.
            IDFFNeuralNetwork network = new DFFNeuralNetwork(numInputs, numHiddenLayers, numHiddenLayerNeurons, numOutputs);
            network.RandomizeNetwork();

            var inputLayer = network.Layers.OfType<IInputLayer>().First();
            var outputLayer = network.Layers.OfType<IOutputLayer>().First();

            // For each of the input & output entries in the training dataset, 
            // update the neuronId to correlate with what is initialized in the neural network.
            // Otherwise, the dataset will be considered invalid when training begins.
            foreach (var trainingEntry in trainingDatasetEntries)
            {
                SyncInputNeuronsAndNeuronInputs(trainingEntry.Inputs, inputLayer);
                SyncOutputNeuronsAndNeuronOutputs(trainingEntry.Outputs, outputLayer);
            }

            // Train network.
            network.Train(trainingDatasetEntries);

            return network;
        }

        /// <summary>
        /// Updates each NeuronId of the network inputs to coordinate with the neuronIds of the input layer neurons.
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="inputLayer"></param>
        private void SyncInputNeuronsAndNeuronInputs(IEnumerable<INetworkInput> inputs, IInputLayer inputLayer)
        {
            if (inputs == null)
                throw new ArgumentNullException("inputs");

            if (inputLayer == null)
                throw new ArgumentNullException("inputLayer");

            var inputNeurons = inputLayer.Neurons.OfType<IInputNeuron>();
            if (inputs.Count() != inputNeurons.Count())
                throw new ArgumentException("The number of network inputs must equal the number of neurons in the input layer.");

            // Update each of the network input neuronIds to coordinate with the input layer neuron neuronIds.
            using (var inputsEnumerator = inputs.GetEnumerator())
            using (var inputNeuronsEnumerator = inputNeurons.GetEnumerator())
            {
                while (inputsEnumerator.MoveNext() && inputNeuronsEnumerator.MoveNext())
                {
                    var networkNeuron = inputNeuronsEnumerator.Current;
                    var networkInput = inputsEnumerator.Current;

                    networkInput.NeuronId = networkNeuron.Id;
                    networkNeuron.Description = networkInput.Description;
                }
            }
        }
        
        /// <summary>
        /// Updates each NeuronId of the network outputs to coordinate with the neuronIds of the output layer neurons.
        /// </summary>
        /// <param name="outputs"></param>
        /// <param name="outputLayer"></param>
        private void SyncOutputNeuronsAndNeuronOutputs(IEnumerable<INetworkOutput> outputs, IOutputLayer outputLayer)
        {
            if (outputs == null)
                throw new ArgumentNullException("outputs");

            if (outputLayer == null)
                throw new ArgumentNullException("outputLayer");

            var outputNeurons = outputLayer.Neurons.OfType<IOutputNeuron>();
            if (outputs.Count() != outputNeurons.Count())
                throw new ArgumentException("The number of network outputs must equal the number of neurons in the output layer.");

            // Update each of the network output neuronIds to coordinate with the output layer neuron neuronIds.
            using (var outputsEnumerator = outputs.GetEnumerator())
            using (var outputNeuronsEnumerator = outputNeurons.GetEnumerator())
            {
                while (outputsEnumerator.MoveNext() && outputNeuronsEnumerator.MoveNext())
                {
                    var networkNeuron = outputNeuronsEnumerator.Current;
                    var networkInput = outputsEnumerator.Current;

                    networkInput.NeuronId = networkNeuron.Id;
                    networkNeuron.Description = networkInput.Description;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void AnalyzePredictions()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");
            
            var predictionsToAnalyze = new List<P>();
            var quotes = _quoteService.GetQuotes();

            // Filter out predictions that are invalid, have ActualOutcomes already, contain invalid quoteIds, and those that have less that 5 remaining quotes for the company. 
            foreach (var prediction in GetPredictions().Where(p => p != null && (p.ActualOutcome == null || p.ActualOutcome == string.Empty)))
            {
                var existingQuote = quotes.FirstOrDefault(q => q.Id == prediction.Id);
                if (existingQuote != null && quotes.Count(q => q.CompanyId == existingQuote.CompanyId && q.Id > prediction.QuoteId) >= 5)
                {
                    predictionsToAnalyze.Add(prediction);
                }
            }

            // Update actual outcome for each prediction.
            foreach (var prediction in predictionsToAnalyze)   
            {
                UpdatePredictionWithActualOutcome(prediction);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quoteId"></param>
        private void UpdatePredictionWithActualOutcome(P prediction)
        {
            var quote = _quoteService.FindQuote(prediction.QuoteId);

            prediction.ActualOutcome = _datasetService.GetFiveDayPerformanceDescription(prediction.CompanyId, quote.Date);
            Update(prediction);
        }
        
        #endregion
        #region IDisposable
        /// <summary>
        /// Disposes this object and properly cleans up resources. 
        /// </summary>
        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _predictionRepository.Dispose();
                    _quoteService.Dispose();
                    _companyService.Dispose();
                    _datasetService.Dispose();
                    _networkConfigurationService.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Disposes this object and properly cleans up resources. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}