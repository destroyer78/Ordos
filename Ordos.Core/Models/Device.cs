using System;
using Ordos.Core.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Ordos.Core.Models
{
    public class Device
    {
        public Device()
        {
            DisturbanceRecordings = new List<DisturbanceRecording>();
        }

        [Key]
        public int Id { get; set; }

        public int DeviceTypeId { get; set; }

        [Display(Name = "Device Type")]
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// List of all the DRs that the IED have.
        /// Each DR can contain multiple IEDFiles.
        /// </summary>
        [Display(Name = "Disturbance Recordings")]
        public ICollection<DisturbanceRecording> DisturbanceRecordings { get; set; }

        [Required]
        [Display(Name = "Device Name")]
        public string Name { get; set; }

        [Required]
        [IPAddressCheck]
        [Display(Name = "IP Address")]
        public string IPAddress { get; set; }

        [Required]
        [Display(Name = "Station Name")]
        public string Station { get; set; }

        [Required]
        [Display(Name = "Bay Name")]
        public string Bay { get; set; }

        [Display(Name = "Status")]
        public bool IsConnected { get; set; }

        [Display(Name = "Ping")]
        public bool HasPing { get; set; }

        [NotMapped]
        [Display(Name = "IED")]
        public string FullName => $"{Station} - {Bay} - {Name}";
    }
}

