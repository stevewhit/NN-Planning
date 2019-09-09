using Microsoft.VisualStudio.TestTools.UnitTesting;
using SANNET.Business.Repositories;
using SANNET.Business.Services;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SANNET.Business.Test.Builders;

namespace SANNET.Business.Test.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DatasetServiceTest
    {
        private IDatasetRepository _repository;
        private IDatasetService _service;

        [TestInitialize]
        public void Initialize()
        {
            _repository = (new MockDatasetRepository()).Object;
            _service = new DatasetService(_repository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _repository.Dispose();
            _service.Dispose();
        }

        #region Testing DatasetService(IDatasetRepository repository)...

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DatasetService_WithNullRepository_ThrowsException()
        {
            // Act
            _service = new DatasetService(null);
        }

        [TestMethod]
        public void DatasetService_WithValidRepository_StoresRepository()
        {
            // Act
            var dataset = _service.GetTrainingDataset(1, 1, DateTime.Now.AddDays(-10), DateTime.Now);

            // Assert
            Assert.IsNotNull(dataset);
        }

        #endregion
        #region Testing IEnumerable<INetworkTrainingIteration> GetTrainingDataset(int datasetRetrievalMethodId, int companyId, DateTime startDate, DateTime endDate)...

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void GetTrainingDataset_AfterDisposed_ThrowsException()
        {
            // Arrange
            _service.Dispose();

            // Act
            var dataset = _service.GetTrainingDataset(1, 1, DateTime.Now.AddDays(-10), DateTime.Now);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetTrainingDataset_WithInvalidRetrievalMethodId_ThrowsException()
        {
            // Act
            var dataset = _service.GetTrainingDataset(-1, 1, DateTime.Now.AddDays(-10), DateTime.Now);
        }

        [TestMethod]
        public void GetTrainingDataset_WithValidRetrievalMethodId_ReturnsDataset()
        {
            // Act
            var dataset = _service.GetTrainingDataset(1, 1, DateTime.Now.AddDays(-10), DateTime.Now);

            // Assert
            Assert.IsNotNull(dataset);
            Assert.IsTrue(dataset.Count() == 11);
        }

        #endregion
        #region Testing IEnumerable<INetworkInput> GetNetworkInputs(int datasetRetrievalMethodId, int companyId, DateTime date)...

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void GetNetworkInputs_AfterDisposed_ThrowsException()
        {
            // Arrange
            _service.Dispose();

            // Act
            var dataset = _service.GetNetworkInputs(1, 1, DateTime.Now.AddDays(-10));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetNetworkInputs_WithInvalidRetrievalMethodId_ThrowsException()
        {
            // Act
            var dataset = _service.GetNetworkInputs(-1, 1, DateTime.Now.AddDays(-10));
        }

        [TestMethod]
        public void GetNetworkInputs_WithValidRetrievalMethodId_ReturnsDataset()
        {
            // Act
            var dataset = _service.GetNetworkInputs(1, 1, DateTime.Now.AddDays(-10));

            // Assert
            Assert.IsNotNull(dataset);
            Assert.IsTrue(dataset.Count() >= 0);
        }

        #endregion
        #region Testing IEnumerable<INetworkOutput> GetExpectedNetworkOutputs(int datasetRetrievalMethodId, int companyId, DateTime date)...

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void GetExpectedNetworkOutputs_AfterDisposed_ThrowsException()
        {
            // Arrange
            _service.Dispose();

            // Act
            var dataset = _service.GetExpectedNetworkOutputs(1, 1, DateTime.Now.AddDays(-10));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetExpectedNetworkOutputs_WithInvalidRetrievalMethodId_ThrowsException()
        {
            // Act
            var dataset = _service.GetExpectedNetworkOutputs(-1, 1, DateTime.Now.AddDays(-10));
        }

        [TestMethod]
        public void GetExpectedNetworkOutputs_WithValidRetrievalMethodId_ReturnsDataset()
        {
            // Act
            var dataset = _service.GetExpectedNetworkOutputs(1, 1, DateTime.Now.AddDays(-10));

            // Assert
            Assert.IsNotNull(dataset);
            Assert.IsTrue(dataset.Count() >= 0);
        }

        #endregion
    }
}