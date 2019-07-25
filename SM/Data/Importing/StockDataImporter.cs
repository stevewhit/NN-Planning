using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SM.TechnicalAnalysis;

namespace SM.Data.Importing
{
    public static class StockDataImporter
    {
        /// <summary>
        /// Imports data from a CSV file that has been downloaded from Barchart.com. The method a stockdata
        /// object with a date-filter applied to the simple readings results.
        /// </summary>
        public static StockData ImportBarchartDailyStockDataCSV(DateTime startDate, DateTime endDate, string csvFilePath)
        {
            // Imported & validated data
            var importedStockData = ImportBarchartDailyStockDataCSV(csvFilePath);

            // Apply date filter.
            importedStockData.SimpleReadings = importedStockData.SimpleReadings.Where(reading => reading.Date >= startDate && reading.Date <= endDate).ToList();

            return importedStockData;
        }

        /// <summary>
        /// Imports data from a CSV file that has been downloaded from Barchart.com
        /// </summary>
        public static StockData ImportBarchartDailyStockDataCSV(string csvFilePath)
        {
            // Imported data 
            var importedStockData = new StockData(_ImportBarchartDailyStockDataCSV(csvFilePath));

            // Verify data is valid.
            if (!importedStockData.IsValidData(true))
                return new StockData();

            return importedStockData;
        }
        
        /// <summary>
        /// Imports data from a CSV file that has been downloaded from Barchart.com. The method returns a list of 
        /// simple stock readings containing all information found in the file.
        /// </summary>
        private static IList<SimpleStockReading> _ImportBarchartDailyStockDataCSV(string csvFilePath)
        {
            if (string.IsNullOrEmpty(csvFilePath))
                throw new ArgumentNullException("Cannot import barchart data from a null filepath.");

            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException($"Barchart data file doesn't exist: '{csvFilePath}'");

            if (!csvFilePath.EndsWith(".csv"))
                throw new FileLoadException("Barchart file must be in CSV format to import using this function.");

            var importedData = new List<SimpleStockReading>();

            try
            {
                var fileRows = File.ReadAllText(csvFilePath).Split('\n');
                var header = fileRows.First();

                // Verify header column placements.
                if ((header[0].ToString().Equals("Time", StringComparison.InvariantCultureIgnoreCase) &&
                    header[1].ToString().Equals("Open", StringComparison.InvariantCultureIgnoreCase) &&
                    header[2].ToString().Equals("High", StringComparison.InvariantCultureIgnoreCase) &&
                    header[3].ToString().Equals("Low", StringComparison.InvariantCultureIgnoreCase) &&
                   (header[4].ToString().Equals("Last Price", StringComparison.InvariantCultureIgnoreCase) || header[4].ToString().Equals("Close", StringComparison.InvariantCultureIgnoreCase)) &&
                    header[5].ToString().Equals("Change", StringComparison.InvariantCultureIgnoreCase) &&
                    header[5].ToString().Equals("Volume", StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new DataMisalignedException($"Barchart file contains INVALID column-header placements not supported by this application.");
                }

                DateTime lastRowDate = new DateTime();
                foreach (var fileRow in fileRows.Reverse().Skip(2).Reverse().Skip(1))
                {
                    var splitCols = fileRow.Split(',');
                    if (splitCols.Count() > 1)
                    {
                        var date = Convert.ToDateTime(splitCols[0]);

                        if (lastRowDate.AddDays(1) > date)
                            throw new InvalidDataException($"The file contains date intervals less that 1 day apart..");

                        importedData.Add(new SimpleStockReading()
                        {
                            Interval = StockReading.IntervalType.Day,
                            Date = date,
                            Open = Convert.ToDouble(splitCols[1]),
                            High = Convert.ToDouble(splitCols[2]),
                            Low = Convert.ToDouble(splitCols[3]),
                            Close = Convert.ToDouble(splitCols[4]),
                            Volume = Convert.ToInt32(splitCols[6])
                        });
                    }
                }
            }
            catch(Exception ex)
            {
                throw new FormatException($"An error occurred importing data from the barchart file on line {importedData.Count + 2}. File: '{csvFilePath}': {ex.InnerException}");
            }

            return importedData;
        }
    }
}
