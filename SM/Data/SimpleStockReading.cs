using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SM.Data
{
    public class SimpleStockReading : StockReading
    {
        public SimpleStockReading()
            : base()
        {

        }

        public SimpleStockReading(IntervalType interval, DateTime date, double open, double close, double high, double low, int volume)
            : base(interval, date, open, close, high, low, volume)
        {

        }
    }
}
