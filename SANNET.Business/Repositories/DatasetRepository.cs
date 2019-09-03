using Framework.Generic.EntityFramework;
using SANNET.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SANNET.Business.Repositories
{
    public interface IDatasetRepository : IDisposable
    {
        /// <summary>
        /// Returns the METHOD 1 training dataset values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values from.</param>
        /// <param name="startDate">The starting date of the returned training values.</param>
        /// <param name="endDate">The ending date of the returned training values.</param>
        /// <returns>Returns the computed training dataset values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/>.</returns>
        IEnumerable<GetTrainingDataset1_Result> GetTrainingDataset1(int companyId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Returns the performance of the company over the next five days for the specified <paramref name="date"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive SMA crosses from.</param>
        /// <param name="date">The date to return the five day performance of.</param>
        /// <param name="riseMultiplierTrigger">The multiplier-level to figure out if the stock rose to that level.</param>
        /// <param name="fallMultiplierTrigger">The multiplier-level to figure out if the stock fell to that level.</param>
        /// <returns></returns>
        IEnumerable<GetFutureFiveDayPerformance_Result> GetFutureFiveDayPerformance(int companyId, DateTime date, double riseMultiplierTrigger, double fallMultiplierTrigger);
    }

    public class DatasetRepository : IDatasetRepository
    {
        private bool _isDisposed = false;
        private IEfContext _context;
        
        public DatasetRepository(IEfContext context)
        {
            _context = context;
        }

        #region IDatasetRepository

        /// <summary>
        /// Returns the METHOD 1 training dataset values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values from.</param>
        /// <param name="startDate">The starting date of the returned training values.</param>
        /// <param name="endDate">The ending date of the returned training values.</param>
        /// <returns>Returns the computed training dataset values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/>.</returns>
        public IEnumerable<GetTrainingDataset1_Result> GetTrainingDataset1(int companyId, DateTime startDate, DateTime endDate)
        {
            return _context.ExecuteStoredProcedure<GetTrainingDataset1_Result>("GetTrainingDataset1 @companyId, @startDate, @endDate",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("startDate", startDate),
                                            new SqlParameter("endDate", endDate));
        }

        /// <summary>
        /// Returns the performance of the company over the next five days for the specified <paramref name="date"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive SMA crosses from.</param>
        /// <param name="date">The date to return the five day performance of.</param>
        /// <param name="riseMultiplierTrigger">The multiplier-level to figure out if the stock rose to that level.</param>
        /// <param name="fallMultiplierTrigger">The multiplier-level to figure out if the stock fell to that level.</param>
        /// <returns></returns>
        public IEnumerable<GetFutureFiveDayPerformance_Result> GetFutureFiveDayPerformance(int companyId, DateTime date, double riseMultiplierTrigger, double fallMultiplierTrigger)
        {
            return _context.ExecuteStoredProcedure<GetFutureFiveDayPerformance_Result>("GetFutureFiveDayPerformance @companyId, @date, @riseMultiplierTrigger, @fallMultiplierTrigger",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("date", date),
                                            new SqlParameter("riseMultiplierTrigger", riseMultiplierTrigger),
                                            new SqlParameter("fallMultiplierTrigger", fallMultiplierTrigger));
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
                    _context.Dispose();
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
