using Framework.Generic.Tests.Builders;
using SANNET.DataModel;
using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;

namespace SANNET.Business.Test.Builders.Objects
{
    [ExcludeFromCodeCoverage]
    public class TestNetworkConfiguration : NetworkConfiguration, ITestEntity
    {
        public Guid TestId { get; private set; }
        public int StoredValue { get; set; }
        public int CurrentValue { get; set; }
        public EntityState State { get; set; }
        public bool IsVirtual { get; set; }

        public TestNetworkConfiguration() : this(0, false) { }
        public TestNetworkConfiguration(bool isVirtual = false)
        {
            TestId = Guid.NewGuid();
            State = EntityState.Unchanged;
            IsVirtual = isVirtual;
        }

        public TestNetworkConfiguration(int value, bool isVirtual = false) : this(isVirtual)
        {
            StoredValue = value;
            CurrentValue = value;
        }
    }
}
