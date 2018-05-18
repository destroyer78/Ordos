using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ordos.Core.Utilities
{
    public static class ComtradeExtensions
    {
        public static DateTime GetTriggerDateTime(string cfgFilename)
        {
            return GetDRDateTimes(cfgFilename).FirstOrDefault();
        }

        public static DateTime GetTriggerDateTime(IEnumerable<string> cfgFileLines)
        {
            return GetDRDateTimes(cfgFileLines).FirstOrDefault();
        }

        public static IEnumerable<DateTime> GetDRDateTimes(IEnumerable<string> cfgFileLines)
        {
            var res = new List<DateTime>();

            foreach (var line in cfgFileLines)
            {
                var tryParse = DateTime.TryParse(line, out DateTime result);
                if (tryParse)
                    res.Add(result);
            }
            if (res.Count.Equals(0))
                res.Add(DateTime.Now);
            return res;
        }

        public static IEnumerable<DateTime> GetDRDateTimes(string cfgFilename)
        {
            return GetDRDateTimes(System.IO.File.ReadAllLines(cfgFilename));
        }

        public static IEnumerable<string> ReadLines(Stream stream, Encoding encoding)
        {
            using (var reader = new StreamReader(stream, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
