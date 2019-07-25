using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SM.Data
{
    public class AdvancedStockReading : StockReading
    {
        /// <summary>
        /// The dollar change-amount of the closing price over a specified number of days (KEY).
        ///     KEY = # of days for computation
        ///     VALUE = Dollar amount change over the specified period.
        /// </summary>
        public Dictionary<int, double> ChangeAmount { get; private set; }
        
        /// <summary>
        /// The current simple moving average value over a specified number of days(KEY).
        ///     KEY = # of days for computation
        ///     VALUE = SMA value over the specified period.
        /// </summary>
        public Dictionary<int, double> SMA { get; private set; }

        /// <summary>
        /// The current exponential moving average value over a specified number of days(KEY).
        ///     KEY = # of days for computation
        ///     VALUE = EMA value over the specified period.
        /// </summary>
        public Dictionary<int, double> EMA { get; private set; }

        /// <summary>
        /// The relative strength index value over a specified number of days(KEY).
        ///     KEY = # of days for computation
        ///     VALUE = RSI value over the specified period.
        /// </summary>
        public Dictionary<int, double> RSI { get; private set; }

        /// <summary>
        /// The current commodity index value over a specified number of days(KEY).
        ///     KEY = # of days for computation
        ///     VALUE = CCI value over the specified period.
        /// </summary>
        public Dictionary<int, double> CCI { get; private set; }

        /// <summary>
        /// Returns the normalized version of the dollar change-amount of the closing price
        /// over a specified number of days(KEY).
        ///     KEY = # of days for computation
        ///     VALUE = NORMALIZED - dollar change-amount value over the specified period.
        /// </summary>
        /// <param name="parentReadingsList">The list of advanced stock readings that THIS stock reading belongs to.</param>
        public Dictionary<int, double> NormalizedChangeAmount(IList<AdvancedStockReading> parentAdvReadingsList) => _NormalizeChangeAmount(parentAdvReadingsList);

        /// <summary>
        /// Returns the normalized version of the simple moving average values
        /// over a specified number of days(KEY).
        ///     KEY = # of days for computation
        ///     VALUE = NORMALIZED - SMA value over the specified period.
        /// </summary>
        /// <param name="parentReadingsList">The list of advanced stock readings that THIS stock reading belongs to.</param>
        public Dictionary<int, double> NormalizedSMA(IList<AdvancedStockReading> parentAdvReadingsList) => _NormalizeSMA(parentAdvReadingsList);
        
        /// <summary>
        /// Returns the normalized version of the exponential moving average values
        /// over a specified number of days(KEY).
        ///     KEY = # of days for computation
        ///     VALUE = NORMALIZED - EMA value over the specified period.
        /// </summary>
        /// <param name="parentReadingsList">The list of advanced stock readings that THIS stock reading belongs to.</param>
        public Dictionary<int, double> NormalizedEMA(IList<AdvancedStockReading> parentAdvReadingsList) => _NormalizeEMA(parentAdvReadingsList);
        
        /// <summary>
        /// Returns the normalized version of the Relative strength index values
        /// over a specified number of days(KEY).
        ///     KEY = # of days for computation
        ///     VALUE = NORMALIZED - RSI value over the specified period.
        /// </summary>
        /// <param name="parentReadingsList">The list of advanced stock readings that THIS stock reading belongs to.</param>
        public Dictionary<int, double> NormalizedRSI(IList<AdvancedStockReading> parentAdvReadingsList) => _NormalizeRSI(parentAdvReadingsList);
        
        /// <summary>
        /// Returns the normalized version of the commodity channel index values
        /// over a specified number of days(KEY).
        ///     KEY = # of days for computation
        ///     VALUE = NORMALIZED - CCI value over the specified period.
        /// </summary>
        /// <param name="parentReadingsList">The list of advanced stock readings that THIS stock reading belongs to.</param>
        public Dictionary<int, double> NormalizedCCI(IList<AdvancedStockReading> parentAdvReadingsList) => _NormalizeCCI(parentAdvReadingsList);

        internal AdvancedStockReading()
            : base()
        {
            ReinitializeTechnicalIndicators();
        }

        internal AdvancedStockReading(SimpleStockReading simpleReading)
            : base(simpleReading.Interval, simpleReading.Date, simpleReading.Open, simpleReading.Close, simpleReading.High, simpleReading.Low, simpleReading.Volume)
        {
            ReinitializeTechnicalIndicators();
        }

        internal AdvancedStockReading(StockReading.IntervalType interval, DateTime date, double open, double close, double high, double low, int volume)
            : base(interval, date, open, close, high, low, volume)
        {
            ReinitializeTechnicalIndicators();
        }

        /// <summary>
        /// Re-initializes the dictionaries for each of the technical indicators.
        /// </summary>
        public void ReinitializeTechnicalIndicators()
        {
            ChangeAmount = new Dictionary<int, double>();
            SMA = new Dictionary<int, double>();
            EMA = new Dictionary<int, double>();
            RSI = new Dictionary<int, double>();
            CCI = new Dictionary<int, double>();
        }
        
        /// <summary>
        /// Computes and returns a normalized dollar change-amount dictionary by using the parent advanced readings list.
        /// </summary>
        private Dictionary<int, double> _NormalizeChangeAmount(IList<AdvancedStockReading> parentAdvReadingsList)
        {
            if (ChangeAmount == null || !ChangeAmount.Keys.Any())
                throw new InvalidOperationException("Cannot generate normalized change amount until the regular change amount values have been computed");

            if (parentAdvReadingsList == null || !parentAdvReadingsList.Any())
                throw new ArgumentException("Cannot generate normalized change amount with an invalid/null parent list of advanced readings.");

            return _CalculateNormalizedIndicatorValues(ChangeAmount, parentAdvReadingsList.Select(reading => reading.ChangeAmount));
        }

        /// <summary>
        /// Computes and returns a normalized-SMA dictionary by using the parent advanced readings list.
        /// </summary>
        private Dictionary<int, double> _NormalizeSMA(IList<AdvancedStockReading> parentAdvReadingsList)
        {
            if (SMA == null || !SMA.Keys.Any())
                throw new InvalidOperationException("Cannot generate normalized SMA until the regular SMA values have been computed");

            if (parentAdvReadingsList == null || !parentAdvReadingsList.Any())
                throw new ArgumentException("Cannot generate normalized SMA with an invalid/null parent list of advanced readings.");

            return _CalculateNormalizedIndicatorValues(SMA, parentAdvReadingsList.Select(reading => reading.SMA));
        }

        /// <summary>
        /// Computes and returns a normalized-EMA dictionary by using the parent advanced readings list.
        /// </summary>
        private Dictionary<int, double> _NormalizeEMA(IList<AdvancedStockReading> parentAdvReadingsList)
        {
            if (EMA == null || !EMA.Keys.Any())
                throw new InvalidOperationException("Cannot generate normalized EMA until the regular EMA values have been computed");

            if (parentAdvReadingsList == null || !parentAdvReadingsList.Any())
                throw new ArgumentException("Cannot generate normalized EMA with an invalid/null parent list of advanced readings.");

            return _CalculateNormalizedIndicatorValues(EMA, parentAdvReadingsList.Select(reading => reading.EMA));
        }
        
        /// <summary>
        /// Computes and returns a normalized-RSI dictionary by using the parent advanced readings list.
        /// </summary>
        private Dictionary<int, double> _NormalizeRSI(IList<AdvancedStockReading> parentAdvReadingsList)
        {
            if (RSI == null || !RSI.Keys.Any())
                throw new InvalidOperationException("Cannot generate normalized RSI until the regular RSI values have been computed");

            if (parentAdvReadingsList == null || !parentAdvReadingsList.Any())
                throw new ArgumentException("Cannot generate normalized RSI with an invalid/null parent list of advanced readings.");

            return _CalculateNormalizedIndicatorValues(RSI, parentAdvReadingsList.Select(reading => reading.RSI));
        }

        /// <summary>
        /// Computes and returns a normalized-CCI dictionary by using the parent advanced readings list.
        /// </summary>
        private Dictionary<int, double> _NormalizeCCI(IList<AdvancedStockReading> parentAdvReadingsList)
        {
            if (CCI == null || !CCI.Keys.Any())
                throw new InvalidOperationException("Cannot generate normalized CCI until the regular CCI values have been computed");

            if (parentAdvReadingsList == null || !parentAdvReadingsList.Any())
                throw new ArgumentException("Cannot generate normalized CCI with an invalid/null parent list of advanced readings.");

            return _CalculateNormalizedIndicatorValues(CCI, parentAdvReadingsList.Select(reading => reading.CCI));
        }

        /// <summary>
        /// Given a dictionary of indicator values, this returns the normalized version of it by
        /// comparing values against the 'parent' indicator values.
        /// </summary>
        private static Dictionary<int, double> _CalculateNormalizedIndicatorValues(Dictionary<int, double> indicatorDictionary, IEnumerable<Dictionary<int, double>> parentIndicatorDictionaries)
        {
            if (indicatorDictionary == null || !indicatorDictionary.Any())
                throw new ArgumentNullException("Cannot compute normalized values with null or empty indicator dictionary.");

            if (parentIndicatorDictionaries == null || !parentIndicatorDictionaries.Any())
                throw new ArgumentNullException("Cannot compute normalized values with null or empty parent list-dictionary.");

            var normalizedIndicators = new Dictionary<int, double>();

            // For each of the existing indicator periods, compute normalized version of each.
            foreach (var period in new List<int>(indicatorDictionary.Keys))
            {
                // Compute the min and max values of the parent list.
                var minValue = parentIndicatorDictionaries.Where(indicatorDict => indicatorDict.ContainsKey(period)).Select(indicatorDict => indicatorDict[period]).Min();
                var maxValue = parentIndicatorDictionaries.Where(indicatorDict => indicatorDict.ContainsKey(period)).Select(indicatorDict => indicatorDict[period]).Max();

                // Apply the normalization algorithm to the technical indicator:
                // ' R_normalized = (R_current - R_min) / (R_max - R_min) '
                normalizedIndicators.Add(period, (indicatorDictionary[period] - minValue) / (maxValue - minValue));
            }

            return normalizedIndicators;
        }
    }
}
