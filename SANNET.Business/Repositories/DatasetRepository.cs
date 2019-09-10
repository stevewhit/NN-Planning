using Framework.Generic.EntityFramework;
using SANNET.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

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
        /// Returns the Dataset1 testing dataset values for the company for the specified <paramref name="date"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values for.</param>
        /// <param name="date">The date of the returned testing values.</param>
        /// <returns>Returns the computed testing dataset values for the company for the specified <paramref name="date"/>.</returns>
        IEnumerable<GetTestingDataset1_Result> GetTestingDataset1(int companyId, DateTime date);
    }

    [ExcludeFromCodeCoverage]
    public class DatasetRepository : IDatasetRepository
    {
        private bool _isDisposed = false;
        private IEfContext _context;
        
        public DatasetRepository(IEfContext context)
        {
            _context = context;
        }

        #region IDatasetRepository<D>

        /// <summary>
        /// Returns the Dataset1 training dataset values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values from.</param>
        /// <param name="startDate">The starting date of the returned training values.</param>
        /// <param name="endDate">The ending date of the returned training values.</param>
        /// <returns>Returns the computed training dataset values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/>.</returns>
        public IEnumerable<GetTrainingDataset1_Result> GetTrainingDataset1(int companyId, DateTime startDate, DateTime endDate)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetRepository", "The repository has been disposed.");

            return _context.ExecuteStoredProcedure<GetTrainingDataset1_Result>("GetTrainingDataset1 @companyId, @startDate, @endDate",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("startDate", startDate),
                                            new SqlParameter("endDate", endDate));
        }

        /// <summary>
        /// Returns the Dataset1 testing dataset values for the company for the specified <paramref name="date"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values for.</param>
        /// <param name="date">The date of the returned testing values.</param>
        /// <returns>Returns the computed testing dataset values for the company for the specified <paramref name="date"/>.</returns>
        public IEnumerable<GetTestingDataset1_Result> GetTestingDataset1(int companyId, DateTime date)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetRepository", "The repository has been disposed.");

            return _context.ExecuteStoredProcedure<GetTestingDataset1_Result>("GetTestingDataset1 @companyId, @date",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("date", date));
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
