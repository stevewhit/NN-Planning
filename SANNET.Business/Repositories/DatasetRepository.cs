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
        /// Returns the training dataset values for the <paramref name="quoteId"/>.
        /// </summary>
        /// <param name="quoteId">The id of the quote to recieve training values for.</param>
        /// <returns>Returns the computed training dataset values for the <paramref name="quoteId"/>.
        IEnumerable<GetTrainingDataset_Result> GetTrainingDataset(int quoteId);

        /// <summary>
        /// Returns the testing dataset values for the <paramref name="quoteId"/>..
        /// </summary>
        /// <param name="quoteId">The id of the quote to recieve testing values for.</param>
        /// <returns>Returns the computed testing dataset values for the <paramref name="quoteId"/>.
        IEnumerable<GetTestingDataset_Result> GetTestingDataset(int quoteId);
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
        /// Returns the training dataset values for the <paramref name="quoteId"/>.
        /// </summary>
        /// <param name="quoteId">The id of the quote to recieve training values for.</param>
        /// <returns>Returns the computed training dataset values for the <paramref name="quoteId"/>.
        public IEnumerable<GetTrainingDataset_Result> GetTrainingDataset(int quoteId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetRepository", "The repository has been disposed.");

            return _context.ExecuteStoredProcedure<GetTrainingDataset_Result>("GetTrainingDataset @quoteId",
                                            new SqlParameter("quoteId", quoteId));
        }

        /// <summary>
        /// Returns the testing dataset values for the <paramref name="quoteId"/>..
        /// </summary>
        /// <param name="quoteId">The id of the quote to recieve testing values for.</param>
        /// <returns>Returns the computed testing dataset values for the <paramref name="quoteId"/>.
        public IEnumerable<GetTestingDataset_Result> GetTestingDataset(int quoteId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("DatasetRepository", "The repository has been disposed.");

            return _context.ExecuteStoredProcedure<GetTestingDataset_Result>("GetTestingDataset @quoteId",
                                            new SqlParameter("quoteId", quoteId));
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
