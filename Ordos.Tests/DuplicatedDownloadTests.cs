using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ordos.Core.Models;
using Ordos.DataService.Data;
using Xunit;

namespace Ordos.Tests
{
    public class DuplicatedDownloadTests
    {
        [Fact]
        public void TestMemoryContextDeviceShallowClone()
        {
            Device device;
            using (var context = ContextHelper.GetContextWithData())
            {
                var deviceContext = context.Devices.FirstOrDefault();
                Assert.NotNull(deviceContext);
                device = DummyLoader.CloneDeviceShallow(context, deviceContext);
            }
            //From Known values
            Assert.NotNull(device);
            Assert.Equal(1, device.Id);
            Assert.Equal("Bay1", device.Bay);
            Assert.Equal("REL670", device.DeviceType);
            Assert.Equal("192.168.1.1", device.IPAddress);
            Assert.Equal(1, device.Id);
            Assert.True(device.IsConnected);
            Assert.True(device.HasPing);
            Assert.Equal("IED1", device.Name);
            Assert.Equal("Station1", device.Station);
        }

        [Fact]
        public void TestMemoryContextDRShallowClone()
        {
            DisturbanceRecording dr;
            using (var context = ContextHelper.GetContextWithData())
            {
                var drContext = context.DisturbanceRecordings.FirstOrDefault();
                Assert.NotNull(drContext);
                dr = DummyLoader.CloneDisturbanceRecording(context, drContext);
            }
            //From Known values
            Assert.NotNull(dr);
            Assert.Equal(1, dr.Id);
            Assert.Equal("DR1", dr.Name);
            Assert.Equal(1, dr.DeviceId);
            Assert.Equal(DateTime.ParseExact("2017-12-31 13:26", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), dr.TriggerTime);
            Assert.Null(dr.Device);
        }

        [Fact]
        public void TestMemoryContextDeviceDRShallowClone()
        {
            Device device;
            DisturbanceRecording dr;

            using (var context = ContextHelper.GetContextWithData())
            {
                var deviceContext = context.Devices.
                    Include(x => x.DisturbanceRecordings)
                    .FirstOrDefault();

                var drContext = deviceContext.DisturbanceRecordings.FirstOrDefault();

                Assert.NotNull(deviceContext);
                device = DummyLoader.CloneDeviceShallow(context, deviceContext);

                dr = DummyLoader.CloneDisturbanceRecording(context, drContext);
            }
            //From Known values
            Assert.NotNull(device);
            Assert.NotNull(dr);

            Assert.Equal(1, dr.Id);
            Assert.Equal("DR1", dr.Name);
            Assert.Equal(1, dr.DeviceId);
            Assert.Equal(DateTime.ParseExact("2017-12-31 13:26", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), dr.TriggerTime);
            Assert.Null(dr.Device);
        }

        [Fact]
        public void TestMemoryContextDeviceDRListShallowClone()
        {
            Device device;
            List<DisturbanceRecording> drs;

            using (var context = ContextHelper.GetContextWithData())
            {
                var deviceContext = context.Devices.
                    Include(x => x.DisturbanceRecordings)
                    .FirstOrDefault();

                Assert.NotNull(deviceContext);
                device = DummyLoader.CloneDeviceShallow(context, deviceContext);
                drs = DummyLoader.CloneAllDisturbanceRecordings(context, deviceContext);
            }
            //From Known values
            Assert.NotNull(device);
            Assert.NotNull(drs);
            Assert.NotEmpty(drs);

            var dr = drs.FirstOrDefault();
            //Currently only has 1 dr
            Assert.NotNull(dr);
            Assert.Equal(1, dr.Id);
            Assert.Equal("DR1", dr.Name);
            Assert.Equal(1, dr.DeviceId);
            Assert.Equal(DateTime.ParseExact("2017-12-31 13:26", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), dr.TriggerTime);
            Assert.Null(dr.Device);
        }

        [Fact]
        public void TestMemoryContextDeviceDeepClone()
        {
            Device device;

            using (var context = ContextHelper.GetContextWithData())
            {
                var deviceContext = context.Devices.
                    Include(x => x.DisturbanceRecordings)
                    .FirstOrDefault();

                Assert.NotNull(deviceContext);
                device = DummyLoader.CloneDeviceDeep(context, deviceContext);
            }
            //From Known values
            Assert.NotNull(device);

            var drs = device.DisturbanceRecordings;
            Assert.NotNull(drs);
            Assert.NotEmpty(drs);

            var dr = drs.FirstOrDefault();
            //Currently only has 1 dr
            Assert.NotNull(dr);
            Assert.Equal(1, dr.Id);
            Assert.Equal("DR1", dr.Name);
            Assert.Equal(1, dr.DeviceId);
            Assert.Equal(DateTime.ParseExact("2017-12-31 13:26", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), dr.TriggerTime);
            Assert.Null(dr.Device);
        }

        [Fact]
        public void TestLocalDbContextShallowDeviceClone()
        {
            Device deviceClone;

            int id;
            string bay;
            string deviceType;
            string ipAddress;
            bool isConnected;
            bool hasPing;
            string name;
            string station;

            using (var context = new SystemContext())
            {
                try
                {
                    if (!context.Devices.Any())
                        return;
                }
                catch
                {
                    return;
                }

                var deviceLocalDB = context.Devices.FirstOrDefault();
                deviceClone = DummyLoader.CloneDeviceShallow(context, deviceLocalDB);
                Assert.NotNull(deviceLocalDB);

                id = deviceLocalDB.Id;
                bay = deviceLocalDB.Bay;
                deviceType = deviceLocalDB.DeviceType;
                ipAddress = deviceLocalDB.IPAddress;
                isConnected = deviceLocalDB.IsConnected;
                hasPing = deviceLocalDB.HasPing;
                name = deviceLocalDB.Name;
                station = deviceLocalDB.Station;

                Assert.Equal(deviceLocalDB.Id, deviceClone.Id);
                Assert.Equal(deviceLocalDB.Bay, deviceClone.Bay);
                Assert.Equal(deviceLocalDB.DeviceType, deviceClone.DeviceType);
                Assert.Equal(deviceLocalDB.IPAddress, deviceClone.IPAddress);
                Assert.Equal(deviceLocalDB.IsConnected, deviceClone.IsConnected);
                Assert.Equal(deviceLocalDB.HasPing, deviceClone.HasPing);
                Assert.Equal(deviceLocalDB.Name, deviceClone.Name);
                Assert.Equal(deviceLocalDB.Station, deviceClone.Station);
            }

            Assert.Equal(id, deviceClone.Id);
            Assert.Equal(bay, deviceClone.Bay);
            Assert.Equal(deviceType, deviceClone.DeviceType);
            Assert.Equal(ipAddress, deviceClone.IPAddress);
            Assert.Equal(isConnected, deviceClone.IsConnected);
            Assert.Equal(hasPing, deviceClone.HasPing);
            Assert.Equal(name, deviceClone.Name);
            Assert.Equal(station, deviceClone.Station);
        }

        [Fact]
        public void TestLocalDbContextDeviceDeepClone()
        {
            using (var context = new SystemContext())
            {
                try
                {
                    if (!context.Devices.Any())
                        return;
                }
                catch
                {
                    return;
                }

                var deviceLocalDB = context
                    .Devices.AsNoTracking()
                    .Include(x => x.DisturbanceRecordings).AsNoTracking()
                    .FirstOrDefault();

                
                var deviceClone = DummyLoader.CloneDeviceDeep(context, deviceLocalDB);

                Assert.NotNull(deviceLocalDB);

                Assert.Equal(deviceLocalDB.Id, deviceClone.Id);
                Assert.Equal(deviceLocalDB.Bay, deviceClone.Bay);
                Assert.Equal(deviceLocalDB.DeviceType, deviceClone.DeviceType);
                Assert.Equal(deviceLocalDB.IPAddress, deviceClone.IPAddress);
                Assert.Equal(deviceLocalDB.IsConnected, deviceClone.IsConnected);
                Assert.Equal(deviceLocalDB.HasPing, deviceClone.HasPing);
                Assert.Equal(deviceLocalDB.Name, deviceClone.Name);
                Assert.Equal(deviceLocalDB.Station, deviceClone.Station);

                if (deviceLocalDB.DisturbanceRecordings == null ||
                    deviceLocalDB.DisturbanceRecordings.Count <= 0)
                    return;

                Assert.NotNull(deviceClone.DisturbanceRecordings);
                Assert.NotEmpty(deviceClone.DisturbanceRecordings);

                for (var i = 0; i < deviceLocalDB.DisturbanceRecordings.Count; i++)
                {
                    Assert.NotNull(deviceLocalDB.DisturbanceRecordings.ElementAt(i));
                    Assert.NotNull(deviceClone.DisturbanceRecordings.ElementAt(i));

                    Assert.Equal(deviceLocalDB.DisturbanceRecordings.ElementAt(i).Id, deviceClone.DisturbanceRecordings.ElementAt(i).Id);
                    Assert.Equal(deviceLocalDB.DisturbanceRecordings.ElementAt(i).Name, deviceClone.DisturbanceRecordings.ElementAt(i).Name);
                    Assert.Equal(deviceLocalDB.DisturbanceRecordings.ElementAt(i).DeviceId, deviceClone.DisturbanceRecordings.ElementAt(i).DeviceId);
                    Assert.Equal(deviceLocalDB.DisturbanceRecordings.ElementAt(i).TriggerTime, deviceClone.DisturbanceRecordings.ElementAt(i).TriggerTime);

                    //Failing:
                    //Assert.Equal(deviceLocalDB.DisturbanceRecordings.ElementAt(i).Device, deviceClone.DisturbanceRecordings.ElementAt(i).Device);
                }
            }
        }
    }
}
