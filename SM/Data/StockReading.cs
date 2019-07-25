using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SM.Data
{
    public abstract class StockReading
    {
        public enum IntervalType
        {
            Second,
            Minute,
            Hour,
            Day,
            Month,
            Year
        }

        /// <summary>
        /// The interval that the data was imported at. 
        /// </summary>
        public IntervalType Interval { get; set; }

        /// <summary>
        /// The datetime that the stock reading occurred.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The daily opening stock price.
        /// </summary>
        public double Open { get; set; }

        /// <summary>
        /// The daily closing stock price.
        /// </summary>
        public double Close { get; set; }

        /// <summary>
        /// The daily high stock price.
        /// </summary>
        public double High { get; set; }

        /// <summary>
        /// The daily low stock price.
        /// </summary>
        public double Low { get; set; }

        /// <summary>
        /// The total amount of shares that were traded throughout the day.
        /// </summary>
        public int Volume { get; set; }

        public StockReading()
        {

        }

        public StockReading(IntervalType interval, DateTime date, double open, double close, double high, double low, int volume)
        {
            Interval = interval;
            Date = date;
            Open = open;
            Close = close;
            High = high;
            Low = low;
            Volume = volume;
        }

        /// <summary>
        /// Returns true/false whether the reading is valid.
        /// </summary>
        public bool IsValidReading(bool clearIfInvalid = false)
        {
            var isValid = Date != null && Open >= 0.0 && Close >= 0.0 && High >= 0.0 && Low >= 0.0 && Volume >= 0;

            if (!isValid && clearIfInvalid)
            {
                Open = -1.0;
                Close = -1.0;
                High = -1.0;
                Low = -1.0;
                Volume = -1;
            }

            return isValid;
        }
    }
}
