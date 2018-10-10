using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ordos.Core.Models;
using Ordos.DataService.Data;
using Xunit;

namespace Ordos.Tests
{
    public class SystemContextTests
    {
        

        [Fact]
        public void TestSampleContext()
        {
            //TODO: Reconfigure SystemContext to take Options
            using (var context = ContextHelper.GetContextWithData())
            {

            }
        }
    }
}
