using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SM.Utility;

namespace SM.TechnicalAnalysis
{
    public static class TechnicalAnalysisComputations
    {
        /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = Datetime value that the Change percent value belongs to.
        ///     Value = Actual change percent value for the given date in decimal form.
        /// </summary>
        /// <param name="period">Period for the change amount</param>
        /// <param name="indexValuesDictionary">Dictionary with a datetime & closing price.</param>
        public static Dictionary<DateTime, double> GetChangePercent(int period, Dictionary<DateTime, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<DateTime, double>();
            }

            var indexPos = 0;
            var datePercentChangeValues = new Dictionary<DateTime, double>();

            // Compute the change percent values using the 'indexed' approach and then 
            // convert the index positions back to datetimes.
            GetChangePercent(period, indexValuesDictionary.ToDictionary(kvp => indexPos++, kvp => kvp.Value))
                    .Values
                    .Reverse()
                    .ForEach(indexValuesDictionary.Keys.Reverse(), (changePercentValue, associatedAmountDate) =>
                    {
                        datePercentChangeValues.Add(associatedAmountDate, changePercentValue);
                    });

            return datePercentChangeValues;
        }

        /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = Index value that the Change percent value belongs to.
        ///     Value = Actual change percent value for the given Index in decimal form.
        /// </summary>
        /// <param name="period">Period for the change percent</param>
        /// <param name="indexValuesDictionary">Dictionary with a Index & closing price.</param>
        public static Dictionary<int, double> GetChangePercent(int period, Dictionary<int, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<int, double>();
            }
            
            // Compute the change amounts.
            var changeAmountValues = GetChangeAmount(period, indexValuesDictionary);

            // Verify each indexKey for the computed change-amounts is present in the
            // indexValuesDictionary so it can be accessed later.
            if (changeAmountValues.Keys.Count(indexKey => indexValuesDictionary.ContainsKey(indexKey)) != changeAmountValues.Count())
            {
                throw new OperationCanceledException("An error occured computing the change amount percentages because the computed change amounts have an index not present in the original indexvaluesdictionary.");
            }

            // Dictionary that holds the simple moving average indexValuesDictionary we're going to compute.
            var changePercentageValues = new Dictionary<int, double>();
            var indexInValues = 0;

            // Reverse the dictionary
            indexValuesDictionary = indexValuesDictionary.Reverse().ToDictionary(x => x.Key, y => y.Value);

            foreach (var index in indexValuesDictionary.Keys)
            {
                // Check to make sure we have enough entries left in the dictionary.
                if (indexInValues > indexValuesDictionary.Count - period)
                {
                    break;
                }

                var currentChangeAmount = changeAmountValues[index];
                var comparedFirstClosingCost = indexValuesDictionary[index - period + 1];

                changePercentageValues.Add(index, currentChangeAmount / comparedFirstClosingCost);

                // Auto decrement variable to keep track of the current index in the indexValuesDictionary
                // We're looking at.
                indexInValues++;
            }

            // Reverse and return the change percent values.
            return changePercentageValues.Reverse().ToDictionary(x => x.Key, y => y.Value);
        }

        /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = DateTime value that the Change amount value belongs to.
        ///     Value = Actual change amount value for the given date.
        /// </summary>
        /// <param name="period">Period for the change amount</param>
        /// <param name="indexValuesDictionary">Dictionary with a datetime & closing price.</param>
        public static Dictionary<DateTime, double> GetChangeAmount(int period, Dictionary<DateTime, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<DateTime, double>();
            }

            var indexPos = 0;
            var datedAmountChangeValues = new Dictionary<DateTime, double>();

            // Compute the change amount values using the 'indexed' approach and then 
            // convert the index positions back to datetimes.
            GetChangeAmount(period, indexValuesDictionary.ToDictionary(kvp => indexPos++, kvp => kvp.Value))
                    .Values
                    .Reverse()
                    .ForEach(indexValuesDictionary.Keys.Reverse(), (changeAmountValue, associatedAmountDate) =>
                    {
                        datedAmountChangeValues.Add(associatedAmountDate, changeAmountValue);
                    });

            return datedAmountChangeValues;
        }

        /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = Index value that the Change amount value belongs to.
        ///     Value = Actual change amount value for the given Index.
        /// </summary>
        /// <param name="period">Period for the change amount</param>
        /// <param name="indexValuesDictionary">Dictionary with a Index & closing price.</param>
        public static Dictionary<int, double> GetChangeAmount(int period, Dictionary<int, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<int, double>();
            }

            // Dictionary that holds the change amount indexValuesDictionary we're going to compute.
            var changeAmountValues = new Dictionary<int, double>();
            var indexInValues = 0;

            // Reverse the dictionary
            indexValuesDictionary = indexValuesDictionary.Reverse().ToDictionary(x => x.Key, y => y.Value);

            foreach (var index in indexValuesDictionary.Keys)
            {
                // Check to make sure we have enough entries left in the dictionary.
                if (indexInValues > indexValuesDictionary.Count - period)
                {
                    break;
                }

                var changeAmount = indexValuesDictionary.Values.ToList()[indexInValues] -
                                 indexValuesDictionary.Values.ToList()[indexInValues + period - 1];

                changeAmountValues.Add(index, changeAmount);

                // Auto decrement variable to keep track of the current index in the indexValuesDictionary
                // We're looking at.
                indexInValues++;
            }

            // Reverse and return the sma values.
            return changeAmountValues.Reverse().ToDictionary(x => x.Key, y => y.Value);
        }

        /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = DateTime value that the RSI value belongs to.
        ///     Value = Actual RSI value for the given Date.
        /// </summary>
        /// <param name="period">Period for the RSI</param>
        /// <param name="indexValuesDictionary">Dictionary with a datetime & closing price.</param>
        public static Dictionary<DateTime, double> GetRelativeStrengthIndex(int period, Dictionary<DateTime, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<DateTime, double>();
            }

            var indexPos = 0;
            var datedRSIValues = new Dictionary<DateTime, double>();

            // Compute the RSI values using the 'indexed' approach and then 
            // convert the index positions back to datetimes.
            GetRelativeStrengthIndex(period, indexValuesDictionary.ToDictionary(kvp => indexPos++, kvp => kvp.Value))
                    .Values
                    .Reverse()
                    .ForEach(indexValuesDictionary.Keys.Reverse(), (rsiValue, associatedRSIDate) =>
                    {
                        datedRSIValues.Add(associatedRSIDate, rsiValue);
                    });

            return datedRSIValues;
        }

        /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = Index value that the RSI value belongs to.
        ///     Value = Actual RSI value for the given index.
        /// </summary>
        /// <param name="period">Period for the RSI</param>
        /// <param name="indexValuesDictionary">Dictionary with an index & closing price.</param>
        public static Dictionary<int, double> GetRelativeStrengthIndex(int period, Dictionary<int, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<int, double>();
            }

            // Dictionary that holds the change in price from the previous day.
            var changeValues = GetChangeAmount(2, indexValuesDictionary);

            // Dictionary that holds the simple moving average indexValuesDictionary we're going to compute. --> Item1 = avgGain; Item2 = avgLoss; Item3 = rsiValue
            var rsiValues = new Dictionary<int, Tuple<double, double, double>>();
            var changeIndex = 1;

            foreach (var entry in changeValues)
            {
                // Need atleast 'period' entries to compute RSI
                if (changeIndex < period)
                {
                    changeIndex++;
                    continue;
                }

                var avgGain = changeValues.Values.ToList().GetRange(changeIndex - period, period).Where(changeVal => changeVal > 0).Sum() / period;
                var avgLoss = Math.Abs(changeValues.Values.ToList().GetRange(changeIndex - period, period).Where(changeVal => changeVal < 0).Sum() / period);
                var rsi = 100.0;

                // By definition, if the avgLoss < 0 --> rsi = 100.0
                // And that removes any divide by 0 cases.
                if (avgLoss != 0)
                {
                    var relativeStrength = 0.0;

                    // First index treated differently because there are no existing avgGain/avgLoss values.
                    if (changeIndex == period)
                        relativeStrength = avgGain / avgLoss;
                    else
                    {
                        var gain = entry.Value > 0 ? entry.Value : 0.0;
                        var loss = entry.Value < 0 ? Math.Abs(entry.Value) : 0.0;

                        // Compute a 'smoothed' rsi value using previous avgGain and avgLoss values.
                        relativeStrength = ((rsiValues.Values.ElementAt((changeIndex - period) - 1).Item1 * (period - 1) + gain) / period) / 
                                            ((rsiValues.Values.ElementAt((changeIndex - period) - 1).Item2 * (period - 1) + loss) / period);
                    }

                    rsi = 100.0 - (100.0 / (1.0 + relativeStrength));
                }






                //// First index
                //if (changeIndex == period)
                //{
                //    avgGain = changeValues.Values.ToList().GetRange(changeIndex - period, period).Where(val => val > 0).Sum() / period;
                //    avgLoss = Math.Abs(changeValues.Values.ToList().GetRange(changeIndex - period, period).Where(val => val < 0).Sum() / period);
                //}
                //else
                //{
                //    var gain = entry.Value > 0 ? entry.Value : 0.0;
                //    var loss = entry.Value < 0 ? Math.Abs(entry.Value) : 0.0;

                //    avgGain = (rsiValues.Values.ElementAt((changeIndex - period) - 1).Item1 * (period - 1) + gain) / period;
                //    avgLoss = (rsiValues.Values.ElementAt((changeIndex - period) - 1).Item2 * (period - 1) + loss) / period;
                //}

                //var relativeStrength = avgGain / avgLoss;
                //var rsi = 100.0 - (100.0 / (1.0 + relativeStrength));

                rsiValues.Add(entry.Key, Tuple.Create(avgGain, avgLoss, rsi));

                changeIndex++;
            }

            return rsiValues.ToDictionary(x => x.Key, y => y.Value.Item3);
        }

        /// /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = DateTime value that the Moving Average value belongs to.
        ///     Value = Actual Moving average value for the given date.
        /// </summary>
        /// <param name="period">Period for the Moving Average</param>
        /// <param name="indexValuesDictionary">Dictionary with a datetime & closing price.</param>
        public static Dictionary<DateTime, double> GetSimpleMovingAverage(int period, Dictionary<DateTime, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<DateTime, double>();
            }

            var indexPos = 0;
            var datedSMAValues = new Dictionary<DateTime, double>();

            // Compute the SMA values using the 'indexed' approach and then 
            // convert the index positions back to datetimes.
            GetSimpleMovingAverage(period, indexValuesDictionary.ToDictionary(kvp => indexPos++, kvp => kvp.Value))
                    .Values
                    .Reverse()
                    .ForEach(indexValuesDictionary.Keys.Reverse(), (smaValue, associatedSMADate) =>
                    {
                        datedSMAValues.Add(associatedSMADate, smaValue);
                    });

            return datedSMAValues;
        }

        /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = Index value that the Moving Average value belongs to.
        ///     Value = Actual Moving average value for the given index.
        /// </summary>
        /// <param name="period">Period for the Moving Average</param>
        /// <param name="indexValuesDictionary">Dictionary with an index & closing price.</param>
        public static Dictionary<int, double> GetSimpleMovingAverage(int period, Dictionary<int, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<int, double>();
            }

            // Dictionary that holds the simple moving average indexValuesDictionary we're going to compute.
            var smaValues = new Dictionary<int, double>();
            var indexInValues = 0;

            // Reverse the dictionary
            indexValuesDictionary = indexValuesDictionary.Reverse().ToDictionary(x => x.Key, y => y.Value);

            foreach (var index in indexValuesDictionary.Keys)
            {
                // Check to make sure we have enough entries left in the dictionary.
                if (indexInValues > indexValuesDictionary.Count - period)
                {
                    break;
                }

                // Compute the average of <period> entries
                var numbersToAverage = indexValuesDictionary.Values.ToList().GetRange(indexInValues, period);
                var average = numbersToAverage.Sum() / numbersToAverage.Count;

                // Add the calculations to our return dictionary.
                smaValues.Add(index, average);

                // Auto decrement variable to keep track of the current index in the indexValuesDictionary
                // We're looking at.
                indexInValues++;
            }

            // Reverse and return the sma values.
            return smaValues.Reverse().ToDictionary(x => x.Key, y => y.Value);
        }

        /// /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = DateTime value that the Exponential Moving Average value belongs to.
        ///     Value = Actual Exponential Moving average value for the given date.
        /// </summary>
        /// <param name="period">Period for the Exponential Moving Average</param>
        /// <param name="indexValuesDictionary">Dictionary with a datetime & closing price.</param>
        public static Dictionary<DateTime, double> GetExponentialMovingAverage(int period, Dictionary<DateTime, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<DateTime, double>();
            }

            var indexPos = 0;
            var datedEMAValues = new Dictionary<DateTime, double>();

            // Compute the RSI values using the 'indexed' approach and then 
            // convert the index positions back to datetimes.
            GetExponentialMovingAverage(period, indexValuesDictionary.ToDictionary(kvp => indexPos++, kvp => kvp.Value))
                    .Values
                    .Reverse()
                    .ForEach(indexValuesDictionary.Keys.Reverse(), (emaValue, associatedEMADate) =>
                    {
                        datedEMAValues.Add(associatedEMADate, emaValue);
                    });

            return datedEMAValues;
        }

        /// /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = Index value that the Exponential Moving Average value belongs to.
        ///     Value = Actual Exponential Moving average value for the given index.
        /// </summary>
        /// <param name="period">Period for the Exponential Moving Average</param>
        /// <param name="indexValuesDictionary">Dictionary with an index & closing price.</param>
        public static Dictionary<int, double> GetExponentialMovingAverage(int period, Dictionary<int, double> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<int, double>();
            }

            // Dictionary that holds the exponential moving average indexValuesDictionary we're going to compute.
            var emaValues = new Dictionary<int, double>();
            var indexInValues = 0;
            var smoothingConstant = (2.0 / (period + 1));

            // Skip the first period - 1 entries. 
            foreach (var indexVal in indexValuesDictionary.Skip(period - 1))
            {
                // Check to make sure we have enough entries left in the dictionary.
                if (indexInValues > indexValuesDictionary.Count - period)
                {
                    break;
                }

                // If no EMA values have been calculated yet, use the SMA as a replacement.
                var prevEMAValue = emaValues.Count() == 0 ?
                                    indexValuesDictionary.Values.ToList().GetRange(indexInValues, period).Average() :
                                    emaValues.Last().Value;

                // Compute EMA by using the smoothing constant, the last EMA value, and the SMA(if required).
                var emaValue = (smoothingConstant * (indexVal.Value - prevEMAValue)) + prevEMAValue;

                emaValues.Add(indexVal.Key, emaValue);

                // Auto decrement variable to keep track of the current index in the indexValuesDictionary
                // We're looking at.
                indexInValues++;
            }

            return emaValues;
        }

        /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = Datetime value that the CCI value belongs to.
        ///     Value = Actual CCI value for the given Datetime.
        /// </summary>
        /// <param name="period">Period for the CCI</param>
        /// <param name="indexValuesDictionary">Entire dataset dictionary with a Datetime & Tuple containing (highPrice, lowPrice, closePrice).</param>
        public static Dictionary<DateTime, double> GetCommodityChannelIndex(int period, Dictionary<DateTime, Tuple<double, double, double>> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<DateTime, double>();
            }

            var indexPos = 0;
            var datedCCIValues = new Dictionary<DateTime, double>();

            // Compute the CCI values using the 'indexed' approach and then 
            // convert the index positions back to datetimes.
            GetCommodityChannelIndex(period, indexValuesDictionary.ToDictionary(kvp => indexPos++, kvp => kvp.Value))
                    .Values
                    .Reverse()
                    .ForEach(indexValuesDictionary.Keys.Reverse(), (cciValue, associatedCCIDate) =>
                    {
                        datedCCIValues.Add(associatedCCIDate, cciValue);
                    });

            return datedCCIValues;
        }

        /// <summary>
        /// Computes and returns a dictionary containing:
        ///     Key = Index value that the CCI value belongs to.
        ///     Value = Actual CCI value for the given index.
        /// </summary>
        /// <param name="period">Period for the CCI</param>
        /// <param name="indexValuesDictionary">Entire dataset dictionary with an index & Tuple containing (highPrice, lowPrice, closePrice).</param>
        public static Dictionary<int, double> GetCommodityChannelIndex(int period, Dictionary<int, Tuple<double, double, double>> indexValuesDictionary)
        {
            // If there aren't any indexValuesDictionary passed, return empty indexValuesDictionary
            if (!indexValuesDictionary.Any() || period <= 0 || indexValuesDictionary.Count < period)
            {
                return new Dictionary<int, double>();
            }

            // Leveling constant to make most CCI values between -100 and 100
            const double levelingConstant = 0.015;

            // Dictionary that holds the commodity channel index values we're going to compute.
            var cciValues = new Dictionary<int, double>();
            var indexInValues = 0;

            // Reverse the dictionary
            indexValuesDictionary = indexValuesDictionary.Reverse().ToDictionary(x => x.Key, y => y.Value);

            foreach (var entry in indexValuesDictionary)
            {
                // Check to make sure we have enough entries left in the dictionary.
                if (indexInValues > indexValuesDictionary.Count - period)
                {
                    break;
                }

                // Typical price is the average of the highPrice, lowPrice, and closePrice
                var typicalPrice = (entry.Value.Item1 + entry.Value.Item2 + entry.Value.Item3) / 3.0;

                // List of 'period' typical prices that are used to compute CCI
                var previousTypicalPrices = indexValuesDictionary.Values.ToList().GetRange(indexInValues, period).Select(dictEntry => (dictEntry.Item1 + dictEntry.Item2 + dictEntry.Item3) / 3.0).ToList();

                // The Period - SMA of Typical Price
                var smaTpAvg = previousTypicalPrices.Average();

                // Summation of sma<typicalPrice> - typical price / period
                var meanDeviation = previousTypicalPrices.Any() ? previousTypicalPrices.Sum(tp => Math.Abs(tp - smaTpAvg)) / period : 0.0;

                // Calculate the CCI value
                var cciValue = (typicalPrice - smaTpAvg) / (levelingConstant * meanDeviation);

                // Add the calculations to our return dictionary
                cciValues.Add(entry.Key, cciValue);

                // Increment our index tracker.
                indexInValues++;
            }

            // Reverse and return the cci values.
            return cciValues.Reverse().ToDictionary(x => x.Key, y => y.Value);
        }
    }
}
