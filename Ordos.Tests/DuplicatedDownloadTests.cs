using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ordos.Tests
{
    public class DuplicatedDownloadTests
    {
        [Fact]
        public void TestDoNotDownloadDuplicates()
        {
            var context = ContextHelper.GetContextWithData();
        }
    }
}
