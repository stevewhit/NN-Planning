using Framework.Generic.EntityFramework;
using SANNET.DataModel;
using System;
using System.Data.Entity;
using System.Linq;

namespace SANNET.Business.Services
{
    public interface INetworkConfigurationService<N> : IDisposable where N : NetworkConfiguration
    {
        /// <summary>
        /// Returns stored configurations.
        /// </summary>
        /// <returns>Returns configurations stored in the repository.</returns>
        IDbSet<N> GetConfigurations();

        /// <summary>
        /// Finds and returns the configuration with the matching id.
        /// </summary>
        /// <param name="id">The id of the configuration to return.</param>
        /// <returns>Returns the configuration with the matching id.</returns>
        N FindConfiguration(int id);

        /// <summary>
        /// Adds the supplied <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration that is to be added.</param>
        void Add(N configuration);

        /// <summary>
        /// Updates the supplied <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration that is to be updated.</param>
        void Update(N configuration);

        /// <summary>
        /// Finds and deletes an existing configuration by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of configuration to be deleted.</param>
        void Delete(int id);

        /// <summary>
        /// Deletes the supplied <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration that is to be deleted.</param>
        void Delete(N configuration);
    }

    public class NetworkConfigurationService<N> : INetworkConfigurationService<N> where N : NetworkConfiguration
    {
        private bool _isDisposed = false;
        private readonly IEfRepository<N> _repository;

        public NetworkConfigurationService(IEfRepository<N> repository)
        {
            _repository = repository ?? throw new ArgumentNullException("repository");
        }

        #region INetworkConfigurationService<N>

        /// <summary>
        /// Returns stored configurations.
        /// </summary>
        /// <returns>Returns configurations stored in the repository.</returns>
        public IDbSet<N> GetConfigurations()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("NetworkConfigurationService", "The service has been disposed.");

            return _repository.GetEntities();
        }

        /// <summary>
        /// Finds and returns the configuration with the matching id.
        /// </summary>
        /// <param name="id">The id of the configuration to return.</param>
        /// <returns>Returns the configuration with the matching id.</returns>
        public N FindConfiguration(int id)
        {
            return GetConfigurations().FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Adds the supplied <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration that is to be added.</param>
        public void Add(N configuration)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("NetworkConfigurationService", "The service has been disposed.");

            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _repository.Add(configuration);
            _repository.SaveChanges();
        }

        /// <summary>
        /// Updates the supplied <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration that is to be updated.</param>
        public void Update(N configuration)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("NetworkConfigurationService", "The service has been disposed.");

            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _repository.Update(configuration);
            _repository.SaveChanges();
        }

        /// <summary>
        /// Finds and deletes an existing configuration by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id of configuration to be deleted.</param>
        public void Delete(int id)
        {
            var configuration = FindConfiguration(id);

            if (configuration == null)
                throw new ArgumentException($"A network configuration with the supplied id doesn't exist: {id}.");

            Delete(configuration);
        }

        /// <summary>
        /// Deletes the supplied <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration that is to be deleted.</param>
        public void Delete(N configuration)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("NetworkConfigurationService", "The service has been disposed.");

            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _repository.Delete(configuration);
            _repository.SaveChanges();
        }

        #endregion
        #region IDisposable
        /// <summary>
        /// Disposes this object and properly cleans up resources. 
        /// </summary>
        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _repository.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Disposes this object and properly cleans up resources. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
