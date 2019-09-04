using log4net;
using Ninject;
using SANNET.Business.Services;
using SANNET.DataModel;
using StockMarket.DataModel;
using System;
using System.Reflection;

namespace SANNET
{
    class Program
    {
        public static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            var predictionService = kernel.Get<PredictionService<Prediction, Quote, Company, NetworkConfiguration>>();

            try
            {
                predictionService.GenerateAllPredictions();
                //marketService.UpdateAllCompanyDetailsAsync().Wait();
            }
            catch (AggregateException e)
            {
                LogRecursive(kernel.Get<ILog>(), e, "Error occured downloading stock details");
            }

            try
            {
                predictionService.AnalyzePredictions();
                //marketService.UpdateAllCompaniesWithLatestQuotesAsync().Wait();
            }
            catch (AggregateException e)
            {
                LogRecursive(kernel.Get<ILog>(), e, "Error occured downloading stock data");
            }

            predictionService.Dispose();
            kernel.Dispose();
        }

        /// <summary>
        /// Logs the inner exceptions of an AggregateException separately.
        /// </summary>
        private static void LogRecursive(ILog log, AggregateException e, string message)
        {
            foreach (var innerException in e.InnerExceptions)
            {
                if (innerException is AggregateException)
                    LogRecursive(log, innerException as AggregateException, message);
                else
                    log.Error($"{message}", innerException);
            }
        }
    }
}
