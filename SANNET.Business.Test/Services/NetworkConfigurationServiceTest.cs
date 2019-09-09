using Framework.Generic.EntityFramework;
using Framework.Generic.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SANNET.Business.Services;
using SANNET.Business.Test.Builders.Objects;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SANNET.Business.Test.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class NetworkConfigurationServiceTest
    {
        private MockEfContext _mockContext;
        private IEfRepository<TestNetworkConfiguration> _repository;
        private INetworkConfigurationService<TestNetworkConfiguration> _service;

        [TestInitialize]
        public void Initialize()
        {
            _mockContext = new MockEfContext(typeof(TestNetworkConfiguration));
            _repository = new EfRepository<TestNetworkConfiguration>(_mockContext.Object);
            _service = new NetworkConfigurationService<TestNetworkConfiguration>(_repository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockContext.Object.Dispose();
            _repository.Dispose();
            _service.Dispose();
        }

        #region Testing NetworkConfigurationService(IEfRepository<N> repository)

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NetworkConfigurationService_WithNullRepository_ThrowsException()
        {
            // Act
            _service = new NetworkConfigurationService<TestNetworkConfiguration>(null);
            // -- Add  ?? throw new ArgumentNullException("repository") to constructor.
        }

        [TestMethod]
        public void NetworkConfigurationService_WithValidRepository_StoresRepository()
        {
            // Act
            var networkConfigs = _service.GetConfigurations();

            // Assert
            Assert.IsNotNull(networkConfigs);
        }

        #endregion
        #region Testing IDbSet<T> GetConfigurations()

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void GetConfigurations_AfterDisposed_ThrowsException()
        {
            // Arrange
            _service.Dispose();

            // Act
            var configs = _service.GetConfigurations();
        }

        [TestMethod]
        public void GetConfigurations_WithValidRepository_ReturnsConfigurations()
        {
            // Arrange
            var configToAdd = new TestNetworkConfiguration(999);
            _repository.Add(configToAdd);

            // Act
            var networkConfigs = _service.GetConfigurations();

            // Assert
            Assert.IsNotNull(networkConfigs);
            Assert.IsTrue(networkConfigs.Count() == 1);
        }

        #endregion
        #region Testing T FindConfiguration(int id)

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void FindConfiguration_AfterDisposed_ThrowsException()
        {
            // Arrange
            _service.Dispose();

            // Act
            var networkConfig = _service.FindConfiguration(1);
        }

        [TestMethod]
        public void FindConfiguration_WithValidId_ReturnsNetworkConfiguration()
        {
            // Arrange
            var id = 123;
            var configToAdd = new TestNetworkConfiguration()
            {
                Id = id
            };

            _repository.Add(configToAdd);

            // Act
            var networkConfig = _service.FindConfiguration(id);

            // Assert
            Assert.IsNotNull(networkConfig);
            Assert.IsTrue(networkConfig == configToAdd);
        }

        [TestMethod]
        public void FindConfiguration_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var configToAdd = new TestNetworkConfiguration()
            {
                Id = 123
            };

            _repository.Add(configToAdd);

            // Act
            var networkConfig = _service.FindConfiguration(234);

            // Assert
            Assert.IsNull(networkConfig);
        }

        #endregion
        #region Testing void Add(N configuration)

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Add_NetworkConfiguration_AfterDisposed_ThrowsException()
        {
            // Arrange
            var configToAdd = new TestNetworkConfiguration()
            {
                Id = 123
            };

            _service.Dispose();

            // Act
            _service.Add(configToAdd);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NetworkConfiguration_WithNullNetworkConfiguration_ThrowsException()
        {
            // Act
            _service.Add(configuration: null);
        }

        [TestMethod]
        public void Add_NetworkConfiguration_WithValidNetworkConfiguration_AddsNetworkConfigurationToRepository()
        {
            // Arrange
            var configToAdd = new TestNetworkConfiguration()
            {
                Id = 123
            };

            // Act
            _service.Add(configToAdd);

            var addedConfiguration = _service.FindConfiguration(configToAdd.Id);

            // Assert
            Assert.IsTrue(addedConfiguration == configToAdd);
        }

        #endregion
        #region Testing void Update(N configuration)

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Update_AfterDisposed_ThrowsException()
        {
            // Arrange
            var configToUpdate = new TestNetworkConfiguration()
            {
                Id = 123
            };

            _service.Dispose();

            // Act
            _service.Update(configToUpdate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Update_WithNullConfig_ThrowsException()
        {
            // Act
            _service.Update(null);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void Update_WithNonExistingConfig_ThrowsException()
        {
            // Arrange
            var configToUpdate = new TestNetworkConfiguration()
            {
                Id = 123
            };

            // Act
            _service.Update(configToUpdate);
        }

        [TestMethod]
        public void Update_WithExistingValidConfig_UpdatesConfigInRepository()
        {
            // Arrange
            var configToUpdate = new TestNetworkConfiguration()
            {
                Id = 123
            };

            _service.Add(configToUpdate);

            // Act
            configToUpdate.Id = 234;
            _service.Update(configToUpdate);

            // Assert
            Assert.IsTrue(configToUpdate.Id == 234);
        }

        #endregion
        #region Testing void Delete(int id)

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Delete_Id_AfterDisposed_ThrowsException()
        {
            // Arrange
            var configToDelete = new TestNetworkConfiguration()
            {
                Id = 123
            };

            _service.Dispose();

            // Act
            _service.Delete(configToDelete.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Delete_Id_WithNonExistingConfigId_ThrowsException()
        {
            // Arrange
            var configToDelete = new TestNetworkConfiguration()
            {
                Id = 123
            };

            // Act
            _service.Delete(configToDelete.Id);
        }

        [TestMethod]
        public void Delete_Id_WithValidConfigId_DeletesConfigInRepository()
        {
            // Arrange
            var configToDelete = new TestNetworkConfiguration()
            {
                Id = 123
            };

            _service.Add(configToDelete);

            // Act
            _service.Delete(configToDelete.Id);

            var deletedConfig = _service.FindConfiguration(configToDelete.Id);

            // Assert
            Assert.IsNull(deletedConfig);
        }

        #endregion
        #region Testing void Delete(N configuration)

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Delete_N_AfterDisposed_ThrowsException()
        {
            // Arrange
            var configToDelete = new TestNetworkConfiguration()
            {
                Id = 123
            };

            _service.Dispose();

            // Act
            _service.Delete(configToDelete);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Delete_N_WithNullConfig_ThrowsException()
        {
            // Act
            _service.Delete(null);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void Delete_N_WithNonExistingConfigId_ThrowsException()
        {
            // Arrange
            var configToDelete = new TestNetworkConfiguration()
            {
                Id = 123
            };

            // Act
            _service.Delete(configToDelete);
        }

        [TestMethod]
        public void Delete_N_WithValidConfigId_DeletesConfigInRepository()
        {
            // Arrange
            var configToDelete = new TestNetworkConfiguration()
            {
                Id = 123
            };

            _service.Add(configToDelete);

            // Act
            _service.Delete(configToDelete);

            var deletedConfig = _service.FindConfiguration(configToDelete.Id);

            // Assert
            Assert.IsNull(deletedConfig);
        }

        #endregion
        #region Testing void Dispose()

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Dispose_DisposesRepository()
        {
            // Act
            _service.Dispose();
            var configs = _service.GetConfigurations();
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void Dispose_AfterDisposal_KeepsRepositoryDisposed()
        {
            // Act
            _service.Dispose();
            _service.Dispose();
            var configs = _service.GetConfigurations();
        }

        #endregion
    }
}
