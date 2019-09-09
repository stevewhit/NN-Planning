using Moq;
using SANNET.Business.Repositories;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SANNET.Business.Test.Builders
{
    [ExcludeFromCodeCoverage]
    public class MockDatasetRepository : Mock<IDatasetRepository>
    {
        public MockDatasetRepository()
        {
            SetupGetTrainingDataset1().SetupGetTestingDataset1();
        }

        public MockDatasetRepository SetupGetTrainingDataset1()
        {
            Setup(r => r.GetTrainingDataset1(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns((int companyId, DateTime startDate, DateTime endDate) =>
                {
                    return FakeDatasets.GetTrainingDataset1(companyId, startDate, endDate);
                });

            return this;
        }

        public MockDatasetRepository SetupGetTestingDataset1()
        {
            Setup(r => r.GetTestingDataset1(It.IsAny<int>(), It.IsAny<DateTime>()))
                .Returns((int companyId, DateTime date) =>
                {
                    return FakeDatasets.GetTestingDataset1(companyId, date);
                });

            return this;
        }
    }
}
