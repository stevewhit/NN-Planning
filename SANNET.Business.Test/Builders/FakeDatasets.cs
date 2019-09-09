using SANNET.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SANNET.Business.Test.Builders
{
    [ExcludeFromCodeCoverage]
    public static class FakeDatasets
    {
        public static IEnumerable<GetTrainingDataset1_Result> GetTrainingDataset1(int companyId, DateTime startDate, DateTime endDate)
        {
            var dataset = new List<GetTrainingDataset1_Result>();

            var date = startDate;
            while (date <= endDate)
            {
                dataset.Add(new GetTrainingDataset1_Result() { Date = date, CompanyId = companyId });
                date = date.AddDays(1);
            }

            return dataset;
        }

        public static IEnumerable<GetTestingDataset1_Result> GetTestingDataset1(int companyId, DateTime date)
        {
            return GetTrainingDataset1(companyId, date, date).Select(d => new GetTestingDataset1_Result() { Output_TriggeredFallFirst = d.Output_TriggeredFallFirst, Output_TriggeredRiseFirst = d.Output_TriggeredRiseFirst });
        }
    }
}
