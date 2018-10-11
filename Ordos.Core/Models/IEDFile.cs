using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ordos.Core.Models
{
    public class IEDFile
    {
        [Key]
        public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; }

        public string FileName { get; set; }
        public ulong CreationTime { get; set; }
        public uint FileSize { get; set; }
    }
}
