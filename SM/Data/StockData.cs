using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SM.TechnicalAnalysis;

namespace SM.Data
{
    public class StockData
    {
        /// <summary>
        /// A list of simple readings that make up this stock data.
        /// </summary>
        public IList<SimpleStockReading> SimpleReadings { get; set; }
                
        public StockData()
        {
            SimpleReadings = new List<SimpleStockReading>();
        }

        public StockData(IList<SimpleStockReading> simpleReadingsToLoad)
        {
            SimpleReadings = simpleReadingsToLoad;
        }

        /// <summary>
        /// Returns true/false if the data is valid by verifying unique date-readings are present and that each
        /// reading itself is valid.
        /// </summary>
        public bool IsValidData(bool clearIfInvalid = false)
        {
            return IsValidData(SimpleReadings, clearIfInvalid);
        }

        /// <summary>
        /// Returns true/false if the data is valid by verifying unique date-readings are present and that each
        /// reading itself is valid.
        /// </summary>
        public bool IsValidData(IList<SimpleStockReading> simpleReadings, bool clearIfInvalid = false)
        {
            // Verify unique dates and that each reading is valid.
            var isValid = simpleReadings != null &&
                          simpleReadings.Select(reading => reading.Date).Distinct().Count() == simpleReadings.Count() &&
                          SimpleReadings.Count(reading => !reading.IsValidReading()) == 0;

            if (!isValid && clearIfInvalid)
            {
                SimpleReadings = null;
            }

            return isValid;
        }

        /// <summary>
        /// Normalizes the computed difference between two indicator values separated by 'valueDistance' entries at each period. 
        /// </summary>
        /// <param name="indicatorChangeAmountsByPeriod">The computed change amount values for each KEY=period.</param>
        public static IEnumerable<double> NormalizeIndicatorChangeAmounts(Dictionary<int, List<double?>> indicatorChangeAmountsByPeriod)
        {
            if (indicatorChangeAmountsByPeriod == null)
                throw new ArgumentNullException("Cannot normalize indicator change amounts with a null dataset.");
            
            throw new NotImplementedException();
        }

        /// <summary>
        /// Computes the NORMALIZED difference between two indicator values that are separated by 'valueDistance' entries, for every value in the technical indicator
        /// values list and for each indicator period supplied.
        /// </summary>
        /// <param name="valueDistance">The distance away from the original value that should be used to perform the calculation.</param>
        /// <param name="indicatorPeriods">The periods that should be used and returned in the comparisons.</param>
        /// <param name="technicalIndicatorValues">A list of technical indicator values (dict)</param>
        /// <returns>Returns a dictionary containing KEY = period; VALUE = differences of entries separated by 'valueDistance'</returns>
        public static Dictionary<int, List<double?>> ComputeNormalizedIndicatorChangeAmounts(int valueDistance, int[] indicatorPeriods, IEnumerable<Dictionary<int, double>> technicalIndicatorValues)
        {
            var indicatorChangeAmountsByPeriod = ComputeIndicatorChangeAmounts(valueDistance, indicatorPeriods, technicalIndicatorValues);
            var normalizedChangeAmouts = new Dictionary<int, List<double?>>();

            // Calculate the normalized values for each period.
            foreach (var period in indicatorChangeAmountsByPeriod.Keys)
            {
                normalizedChangeAmouts.Add(period, new List<double?>());

                var maxChangeAmount = indicatorChangeAmountsByPeriod[period].Max();
                var minChangeAmount = indicatorChangeAmountsByPeriod[period].Min();

                foreach (var indicatorVal in indicatorChangeAmountsByPeriod[period])
                {
                    var blah = indicatorVal.HasValue ?
                                                            (indicatorVal.Value - minChangeAmount) / (maxChangeAmount - minChangeAmount) :             // ' R_normalized = (R_current - R_min) / (R_max - R_min) '
                                                            null;

                    normalizedChangeAmouts[period].Add(indicatorVal.HasValue ?
                                                            (indicatorVal.Value - minChangeAmount) / (maxChangeAmount - minChangeAmount) :             // ' R_normalized = (R_current - R_min) / (R_max - R_min) '
                                                            null);
                }
            }

            return normalizedChangeAmouts;
        }

        /// <summary>
        /// Computes the difference between two indicator values that are separated by 'valueDistance' entries, evaluated at the 
        /// indicator period.
        /// </summary>
        /// <param name="valueDistance">The distance away from the original value that should be used to perform the calculation.</param>
        /// <param name="indicatorPeriod">The period that should be used and returned in the comparisons.</param>
        /// <param name="technicalIndicatorValues">A list of technical indicator values (dict)</param>
        /// <returns>Returns a dictionary containing KEY = period; VALUE = differences of entries separated by 'valueDistance'</returns>
        public static List<double?> ComputeIndicatorChangeAmounts(int valueDistance, int indicatorPeriod, IEnumerable<Dictionary<int, double>> technicalIndicatorValues)
        {
            return ComputeIndicatorChangeAmounts(valueDistance, new[]{indicatorPeriod}, technicalIndicatorValues).First().Value;
        }

        /// <summary>
        /// Computes the difference between two indicator values that are separated by 'valueDistance' entries, for every value in the technical indicator
        /// values list and for each indicator period supplied.
        /// </summary>
        /// <param name="valueDistance">The distance away from the original value that should be used to perform the calculation.</param>
        /// <param name="indicatorPeriods">The periods that should be used and returned in the comparisons.</param>
        /// <param name="technicalIndicatorValues">A list of technical indicator values (dict)</param>
        /// <returns>Returns a dictionary containing KEY = period; VALUE = differences of entries separated by 'valueDistance'</returns>
        public static Dictionary<int, List<double?>> ComputeIndicatorChangeAmounts(int valueDistance, int[] indicatorPeriods, IEnumerable<Dictionary<int, double>> technicalIndicatorValues)
        {
            if (valueDistance < 0)
                throw new ArgumentException("To compute change amounts, distance must be positive.");

            if (indicatorPeriods == null)
                throw new ArgumentNullException("To compute change amounts, the indicator periods must not be null.");

            if (technicalIndicatorValues == null || !technicalIndicatorValues.Any() || technicalIndicatorValues.Count() <= valueDistance)
                throw new ArgumentException("To compute change amounts, the number of indicator values must be greater than the valueDistance.");

            var changeAmountsPerPeriod = new Dictionary<int, List<double?>>();

            // Reverse the list so we'll compute backwards instead of skipping entries.
            var reversedTechnicalIndicatorValues = technicalIndicatorValues.Reverse().ToList();
            
            // Compute the indicator change amounts for each period.
            indicatorPeriods.Distinct().ToList().ForEach(period =>
            {
                var reversedIndex = 0;
                changeAmountsPerPeriod.Add(period, new List<double?>());

                // Reverse the list and compute the differences.
                reversedTechnicalIndicatorValues.ToList().ForEach(indicatorVal =>
                {
                    // We need atleast 'valueDistance' number of entries left to compute difference.
                    if (reversedIndex + valueDistance < reversedTechnicalIndicatorValues.Count())
                    {
                        // The supplied technical indicator dictionaries must contain each of the provided indicator periods
                        if (indicatorVal.ContainsKey(period) && reversedTechnicalIndicatorValues[reversedIndex + valueDistance].ContainsKey(period))
                        {
                            // Compute the change-difference between this value and the value 'valueDistance' away.
                            changeAmountsPerPeriod[period].Add(indicatorVal[period] - reversedTechnicalIndicatorValues[reversedIndex + valueDistance][period]);
                        }
                        else
                            changeAmountsPerPeriod[period].Add(null);
                    }
                    else
                        changeAmountsPerPeriod[period].Add(null);

                    reversedIndex++;
                });
            });

            // Re-reverse the change amounts because we initially reversed the 
            // list to compute the change values.
            return changeAmountsPerPeriod.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Reverse<double?>().ToList());
        }

        /// <summary>
        /// Converts the simple readings in this object to advanced readings. The technical indicators for each
        /// advanced reading will be evaluated/computed using each of the identified 'indicatorPeriods' and the
        /// overall readings-results will be filtered using the start and end dates.
        /// </summary>
        public IList<AdvancedStockReading> GenerateAdvancedReadings(int periodRangeMin, int periodRangeMax)
        {
            // Swap periods if they're in wrong order.
            if (periodRangeMin > periodRangeMax)
            {
                var temp = periodRangeMin;
                periodRangeMin = periodRangeMax;
                periodRangeMax = temp;
            }

            return _GenerateAdvancedReadings(SimpleReadings, Enumerable.Range(periodRangeMin, periodRangeMax - periodRangeMin + 1).ToArray());
        }

        /// <summary>
        /// Converts the simple readings in this object to advanced readings. The technical indicators for each
        /// advanced reading will be evaluated/computed using each of the identified 'indicatorPeriods' and the
        /// overall readings-results will be filtered using the start and end dates.
        /// </summary>
        public IList<AdvancedStockReading> GenerateAdvancedReadings(int[] indicatorPeriods)
        {
            return _GenerateAdvancedReadings(SimpleReadings, indicatorPeriods);
        }

        /// <summary>
        /// Converts the simple readings in this object to advanced readings. The technical indicators for each
        /// advanced reading will be evaluated/computed using each of the identified 'indicatorPeriods' and the
        /// overall readings-results will be filtered using the start and end dates.
        /// </summary>
        public IList<AdvancedStockReading> GenerateAdvancedReadings(DateTime startDate, DateTime endDate, int[] indicatorPeriods)
        {
            if (indicatorPeriods == null || !indicatorPeriods.Any())
                throw new ArgumentNullException("Cannot compute advanced technical indicators with null/empty periods.");

            // Swap training dates if they're in wrong order.
            if (startDate > endDate)
            {
                var temp = startDate;
                startDate = endDate;
                endDate = temp;
            }

            // Verify imported dataset contains the training dates.
            if (startDate < SimpleReadings.Select(reading => reading.Date).Min() ||
                endDate > SimpleReadings.Select(reading => reading.Date).Max())
            {
                throw new ArgumentException("To generate date-filtered advanced readings, dates must be contained within the provided import dataset.");
            }
            // Verify the identified start date is atleast 'maxPeriods + 1' entries away from the start of the dataset so technical indicators can be computed properly.
            else if (SimpleReadings.Select(reading => reading.Date).Count(date => date <= startDate) < indicatorPeriods.Max() + 1)
            {
                throw new ArgumentException("The identified start date must be atleast 'maxPeriods' entries away from the start of the dataset so technical indicators can be computed properly.");
            }

            // Compute the advanced readings using the entire dataset and then
            // filter the results using the provided date range. If not done
            // in this order, the advanced technical indicators will be wrong.
            return _GenerateAdvancedReadings(SimpleReadings, indicatorPeriods).Where(reading => reading.Date >= startDate && reading.Date <= endDate).ToList();
        }
        
        /// <summary>
        /// Converts a list of simple readings to a list of advanced stock readings by generating the technical
        /// indicators that make up the advanced reading using defined periods.
        /// </summary>
        private IList<AdvancedStockReading> _GenerateAdvancedReadings(IList<SimpleStockReading> simpleReadings, int[] indicatorPeriods)
        {
            if (indicatorPeriods == null || !indicatorPeriods.Any())
                throw new ArgumentNullException("Cannot compute advanced technical indicators with null/empty periods.");

            // Order the periods in ascending order and remove any duplicate periods.
            indicatorPeriods = indicatorPeriods.OrderBy(period => period).Distinct().ToArray();

            if (!IsValidData(simpleReadings))
                throw new ArgumentException("Cannot compute advanced technical indicators with null, incomplete, or invalid data readings (Please make sure dates are all unique).");

            if (indicatorPeriods.Min() < 2)
                throw new ArgumentException("The minimum period is '2' thus it also cannot compute advanced technical indicators.");
            
            // Need atleast the highest period to perform calculations.
            if (simpleReadings.Count() < indicatorPeriods.Max())
                throw new ArgumentException("An insufficient amount of simple data readings were supplied to compute technical indicators.");

            // Convert each of the simple readings to advanced readings.
            var advancedReadings = simpleReadings.Select(simpleReading => new AdvancedStockReading(simpleReading)).ToList();

            // Re-initialize each technical indicator.
            advancedReadings.ToList().ForEach(advReading =>
            {
                advReading.ReinitializeTechnicalIndicators();
            });

            // Compute each of the technical indicators for each of the supplied periods.
            foreach (var period in indicatorPeriods)
            { 
                // Compute ChangeAmount(period)
                var changeAmountValues = TechnicalAnalysisComputations.GetChangeAmount(
                                                                period,
                                                                advancedReadings.ToDictionary(reading => reading.Date, reading => reading.Close));
                // Compute EMA(period)
                var emaPeriodValues = TechnicalAnalysisComputations.GetExponentialMovingAverage(
                                                                period,
                                                                advancedReadings.ToDictionary(reading => reading.Date, reading => reading.Close));
                // Compute SMA(period)
                var smaPeriodValues = TechnicalAnalysisComputations.GetSimpleMovingAverage(
                                                                period,
                                                                advancedReadings.ToDictionary(reading => reading.Date, reading => reading.Close));
                // Compute RSI(period)
                var rsiPeriodValues = TechnicalAnalysisComputations.GetRelativeStrengthIndex(
                                                                period,
                                                                advancedReadings.ToDictionary(reading => reading.Date, reading => reading.Close));
                // Compute CCI(period)
                var cciPeriodValues = TechnicalAnalysisComputations.GetCommodityChannelIndex(
                                                                period,
                                                                advancedReadings.ToDictionary(reading => reading.Date, reading => Tuple.Create(reading.High, reading.Low, reading.Close)));

                // Update the technical indicator lists for each daily stock data.
                advancedReadings.ToList().ForEach(reading =>
                {
                    if (changeAmountValues.ContainsKey(reading.Date))
                        reading.ChangeAmount.Add(period, changeAmountValues[reading.Date]);

                    if (emaPeriodValues.ContainsKey(reading.Date))
                        reading.EMA.Add(period, emaPeriodValues[reading.Date]);

                    if (smaPeriodValues.ContainsKey(reading.Date))
                        reading.SMA.Add(period, smaPeriodValues[reading.Date]);

                    if (rsiPeriodValues.ContainsKey(reading.Date))
                        reading.RSI.Add(period, rsiPeriodValues[reading.Date]);

                    if (cciPeriodValues.ContainsKey(reading.Date))
                        reading.CCI.Add(period, cciPeriodValues[reading.Date]);
                });
            }

            return advancedReadings;
        }
    }
}
