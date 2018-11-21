using System;
using System.Collections.Generic;
using System.Text;
using IEC61850.Common;
using Xunit;

namespace Ordos.Tests
{
    public class IEC61850DependenciesTests
    {
        [Fact]
        public void TestCreateMMSValue()
        {
            var mmsValue = new MmsValue(true);
            Assert.NotNull(mmsValue);
        }

        [Fact]
        public void TestGetMMSType()
        {
            var expected = MmsType.MMS_BOOLEAN;
            var mmsValue = new MmsValue(true);

            var actual = mmsValue.GetType();
            Assert.Equal(expected, actual);
        }
    }
}
