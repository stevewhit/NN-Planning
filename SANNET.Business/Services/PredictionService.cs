using Framework.Generic.EntityFramework;
using NeuralNetwork.Generic.Datasets;
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

        public void GenerateAllPredictions()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");

            var companies = _companyService.GetCompanies();
            var networkConfigs = _networkConfigurationService.GetConfigurations();

            foreach (var company in companies)
            {
                foreach (var config in networkConfigs)
                {
                    GenerateCompanyPredictions(company.Id, config.Id);
                }
            }
        }

        public void GenerateCompanyPredictions(int companyId, int networkConfigId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");
            
            var companyPredictions = GetPredictions().Where(p => p.CompanyId == companyId);
            var quotesWithoutPredictions = _quoteService.GetQuotes().Where(q => q.CompanyId == companyId && !companyPredictions.Any(p => p.QuoteId == q.Id && p.ConfigurationId == networkConfigId));

            foreach (var quote in quotesWithoutPredictions)
            {
                GenerateQuotePrediction(quote.Id, networkConfigId);
            }
        }
        
        public void GenerateQuotePrediction(int quoteId, int networkConfigId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("PredictionService", "The service has been disposed.");

            var quote = _quoteService.FindQuote(quoteId) ?? throw new InvalidOperationException($"Invalid quoteId supplied: {quoteId}.");

            var networkConfig = _networkConfigurationService.FindConfiguration(networkConfigId);
            var trainingDatasetEntries = _datasetService.GetTrainingDataset(networkConfigId, quote.CompanyId, quote.Date.AddDays(-1).AddMonths(-1 * networkConfig.NumTrainingMonths), quote.Date.AddDays(-1));
            var predictionDayInputs = _datasetService.GetNetworkInputs(networkConfigId, quote.CompanyId, quote.Date);

            GeneratePrediction(trainingDatasetEntries, predictionDayInputs);
        }

        private void GeneratePrediction(IEnumerable<INetworkTrainingIteration> trainingDatasetEntries, IEnumerable<INetworkInput> predictionDayInputs)
        {
            if (trainingDatasetEntries == null)
                throw new ArgumentNullException("trainingDatasetEntries");

            // Setup NN


            // Train NN with dataset


            // Apply predictionDayInputs to NN


            // Read/analyze NN output to determine 


            // Generate entry in predictions table with confidence in the prediction.
        }








        //public void AnalyzePredictions()
        //{
        //    //Foreach prediction without an outcome (outside the 5-day window!), AnalyzePrediction();
        //}

        //public void AnalyzePrediction(int quoteId)
        //{
        //    //GetFutureFiveDayPerformance stored procedure; Determine if prediction was correct/incorrect and updated prediction entry.
        //}

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
                    throw new NotImplementedException();
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
