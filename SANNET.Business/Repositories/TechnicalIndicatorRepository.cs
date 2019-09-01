using Framework.Generic.EntityFramework;
using SANNET.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SANNET.Business.Repositories
{
    public interface ITechnicalIndicatorRepository : IDisposable
    {
        /// <summary>
        /// Returns the RSI values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values from.</param>
        /// <param name="startDate">The starting date of the returned RSI values.</param>
        /// <param name="endDate">The ending date of the returned RSI values.</param>
        /// <param name="period">The period that the RSI values are computed for.</param>
        /// <returns>Returns the computed RSI values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/></returns>
        IEnumerable<GetRelativeStrengthIndex_Result> GetRelativeStrengthIndex(int companyId, DateTime startDate, DateTime endDate, int period);

        /// <summary>
        /// Computes the RSI values over both the <paramref name="shortPeriod"/> and <paramref name="longPeriod"/> and returns the number of days each
        /// periodic RSI values are above or below the other one.
        /// </summary>
        /// <param name="companyId">The id of the company to receive RSI crosses from.</param>
        /// <param name="startDate">The starting date of the returned RSI crosses.</param>
        /// <param name="endDate">The ending date of the returned RSI crosses.</param>
        /// <param name="shortPeriod">The short-period that the RSI values are computed for.</param>
        /// <param name="longPeriod">The long-period that the RSI values are computed for.</param>
        IEnumerable<GetRelativeStrengthIndexCrosses_Result> GetRelativeStrengthIndexCrosses(int companyId, DateTime startDate, DateTime endDate, int shortPeriod, int longPeriod);

        /// <summary>
        /// Returns the CCI values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values from.</param>
        /// <param name="startDate">The starting date of the returned CCI values.</param>
        /// <param name="endDate">The ending date of the returned CCI values.</param>
        /// <param name="period">The period that the CCI values are computed for.</param>
        /// <returns>Returns the computed CCI values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/></returns>
        IEnumerable<GetCommodityChannelIndex_Result> GetCommodityChannelIndex(int companyId, DateTime startDate, DateTime endDate, int period);

        /// <summary>
        /// Computes the CCI values over both the <paramref name="shortPeriod"/> and <paramref name="longPeriod"/> and returns the number of days each
        /// periodic CCI values are above or below the other one.
        /// </summary>
        /// <param name="companyId">The id of the company to receive CCI crosses from.</param>
        /// <param name="startDate">The starting date of the returned CCI crosses.</param>
        /// <param name="endDate">The ending date of the returned CCI crosses.</param>
        /// <param name="shortPeriod">The short-period that the CCI values are computed for.</param>
        /// <param name="longPeriod">The long-period that the CCI values are computed for.</param>
        IEnumerable<GetCommodityChannelIndexCrosses_Result> GetCommodityChannelIndexCrosses(int companyId, DateTime startDate, DateTime endDate, int shortPeriod, int longPeriod);

        /// <summary>
        /// Returns the SMA values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values from.</param>
        /// <param name="startDate">The starting date of the returned SMA values.</param>
        /// <param name="endDate">The ending date of the returned SMA values.</param>
        /// <param name="period">The period that the SMA values are computed for.</param>
        /// <returns>Returns the computed SMA values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/></returns>
        IEnumerable<GetSimpleMovingAverage_Result> GetSimpleMovingAverage(int companyId, DateTime startDate, DateTime endDate, int period);

        /// <summary>
        /// Computes the SMA values over both the <paramref name="shortPeriod"/> and <paramref name="longPeriod"/> and returns the number of days each
        /// periodic SMA values are above or below the other one.
        /// </summary>
        /// <param name="companyId">The id of the company to receive SMA crosses from.</param>
        /// <param name="startDate">The starting date of the returned SMA crosses.</param>
        /// <param name="endDate">The ending date of the returned SMA crosses.</param>
        /// <param name="shortPeriod">The short-period that the SMA values are computed for.</param>
        /// <param name="longPeriod">The long-period that the SMA values are computed for.</param>
        IEnumerable<GetSimpleMovingAverageCrosses_Result> GetSimpleMovingAverageCrosses(int companyId, DateTime startDate, DateTime endDate, int shortPeriod, int longPeriod);

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

    public class TechnicalIndicatorRepository : ITechnicalIndicatorRepository
    {
        private bool _isDisposed = false;
        private IEfContext _context;
        
        public TechnicalIndicatorRepository(IEfContext context)
        {
            _context = context;
        }

        #region ITechnicalIndicatorRepository

        /// <summary>
        /// Returns the RSI values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values from.</param>
        /// <param name="startDate">The starting date of the returned RSI values.</param>
        /// <param name="endDate">The ending date of the returned RSI values.</param>
        /// <param name="period">The period that the RSI values are computed for.</param>
        /// <returns>Returns the computed RSI values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/></returns>
        public IEnumerable<GetRelativeStrengthIndex_Result> GetRelativeStrengthIndex(int companyId, DateTime startDate, DateTime endDate, int period)
        {
            return _context.ExecuteStoredProcedure<GetRelativeStrengthIndex_Result>("GetRelativeStrengthIndex @companyId, @startDate, @endDate, @rsiPeriod",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("startDate", startDate),
                                            new SqlParameter("endDate", endDate),
                                            new SqlParameter("rsiPeriod", period));
        }

        /// <summary>
        /// Computes the RSI values over both the <paramref name="shortPeriod"/> and <paramref name="longPeriod"/> and returns the number of days each
        /// periodic RSI values are above or below the other one.
        /// </summary>
        /// <param name="companyId">The id of the company to receive RSI crosses from.</param>
        /// <param name="startDate">The starting date of the returned RSI crosses.</param>
        /// <param name="endDate">The ending date of the returned RSI crosses.</param>
        /// <param name="shortPeriod">The short-period that the RSI values are computed for.</param>
        /// <param name="longPeriod">The long-period that the RSI values are computed for.</param>
        public IEnumerable<GetRelativeStrengthIndexCrosses_Result> GetRelativeStrengthIndexCrosses(int companyId, DateTime startDate, DateTime endDate, int shortPeriod, int longPeriod)
        {
            return _context.ExecuteStoredProcedure<GetRelativeStrengthIndexCrosses_Result>("GetRelativeStrengthIndexCross @companyId, @startDate, @endDate, @rsiPeriodShort, @rsiPeriodLong",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("startDate", startDate),
                                            new SqlParameter("endDate", endDate),
                                            new SqlParameter("rsiPeriodShort", shortPeriod),
                                            new SqlParameter("rsiPeriodLong", longPeriod));
        }

        /// <summary>
        /// Returns the CCI values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values from.</param>
        /// <param name="startDate">The starting date of the returned CCI values.</param>
        /// <param name="endDate">The ending date of the returned CCI values.</param>
        /// <param name="period">The period that the CCI values are computed for.</param>
        /// <returns>Returns the computed CCI values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/></returns>
        public IEnumerable<GetCommodityChannelIndex_Result> GetCommodityChannelIndex(int companyId, DateTime startDate, DateTime endDate, int period)
        {
            return _context.ExecuteStoredProcedure<GetCommodityChannelIndex_Result>("GetCommodityChannelIndex @companyId, @startDate, @endDate, @cciPeriod",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("startDate", startDate),
                                            new SqlParameter("endDate", endDate),
                                            new SqlParameter("cciPeriod", period));
        }

        /// <summary>
        /// Computes the CCI values over both the <paramref name="shortPeriod"/> and <paramref name="longPeriod"/> and returns the number of days each
        /// periodic CCI values are above or below the other one.
        /// </summary>
        /// <param name="companyId">The id of the company to receive CCI crosses from.</param>
        /// <param name="startDate">The starting date of the returned CCI crosses.</param>
        /// <param name="endDate">The ending date of the returned CCI crosses.</param>
        /// <param name="shortPeriod">The short-period that the CCI values are computed for.</param>
        /// <param name="longPeriod">The long-period that the CCI values are computed for.</param>
        public IEnumerable<GetCommodityChannelIndexCrosses_Result> GetCommodityChannelIndexCrosses(int companyId, DateTime startDate, DateTime endDate, int shortPeriod, int longPeriod)
        {
            return _context.ExecuteStoredProcedure<GetCommodityChannelIndexCrosses_Result>("GetCommodityChannelIndexCrosses @companyId, @startDate, @endDate, @cciPeriodShort, @cciPeriodLong",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("startDate", startDate),
                                            new SqlParameter("endDate", endDate),
                                            new SqlParameter("cciPeriodShort", shortPeriod),
                                            new SqlParameter("cciPeriodLong", longPeriod));
        }

        /// <summary>
        /// Returns the SMA values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/>.
        /// </summary>
        /// <param name="companyId">The id of the company to receive values from.</param>
        /// <param name="startDate">The starting date of the returned SMA values.</param>
        /// <param name="endDate">The ending date of the returned SMA values.</param>
        /// <param name="period">The period that the SMA values are computed for.</param>
        /// <returns>Returns the computed SMA values for the company from the <paramref name="startDate"/> to the <paramref name="endDate"/> over the supplied <paramref name="period"/></returns>
        public IEnumerable<GetSimpleMovingAverage_Result> GetSimpleMovingAverage(int companyId, DateTime startDate, DateTime endDate, int period)
        {
            return _context.ExecuteStoredProcedure<GetSimpleMovingAverage_Result>("GetSimpleMovingAverage @companyId, @startDate, @endDate, @smaPeriod",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("startDate", startDate),
                                            new SqlParameter("endDate", endDate),
                                            new SqlParameter("smaPeriod", period));
        }

        /// <summary>
        /// Computes the SMA values over both the <paramref name="shortPeriod"/> and <paramref name="longPeriod"/> and returns the number of days each
        /// periodic SMA values are above or below the other one.
        /// </summary>
        /// <param name="companyId">The id of the company to receive SMA crosses from.</param>
        /// <param name="startDate">The starting date of the returned SMA crosses.</param>
        /// <param name="endDate">The ending date of the returned SMA crosses.</param>
        /// <param name="shortPeriod">The short-period that the SMA values are computed for.</param>
        /// <param name="longPeriod">The long-period that the SMA values are computed for.</param>
        public IEnumerable<GetSimpleMovingAverageCrosses_Result> GetSimpleMovingAverageCrosses(int companyId, DateTime startDate, DateTime endDate, int shortPeriod, int longPeriod)
        {
            return _context.ExecuteStoredProcedure<GetSimpleMovingAverageCrosses_Result>("GetSimpleMovingAverageCrosses @companyId, @startDate, @endDate, @smaPeriodShort, @smaPeriodLong",
                                            new SqlParameter("companyId", companyId),
                                            new SqlParameter("startDate", startDate),
                                            new SqlParameter("endDate", endDate),
                                            new SqlParameter("smaPeriodShort", shortPeriod),
                                            new SqlParameter("smaPeriodLong", longPeriod));
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
