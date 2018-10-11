using Ordos.Core.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        //public int DeviceTypeId { get; set; }

        [Display(Name = "Tipo de IED")]
        public string DeviceType { get; set; }

        /// <summary>
        /// List of all the DRs that the IED have.
        /// Each DR can contain multiple IEDFiles.
        /// </summary>
        [Display(Name = "Oscilografias")]
        public ICollection<DisturbanceRecording> DisturbanceRecordings { get; set; }

        public ICollection<IEDFile> IEDFiles { get; set; }

        [Required]
        [Display(Name = "Dispositivo")]
        public string Name { get; set; }

        [Required]
        [IPAddressCheck]
        [Display(Name = "Dirección IP")]
        public string IPAddress { get; set; }

        [Required]
        [Display(Name = "Estación")]
        public string Station { get; set; }

        [Required]
        [Display(Name = "Bahía")]
        public string Bay { get; set; }

        [Required]
        [Display(Name = "Nemotécnico Bahía")]
        public string BayId { get; set; }

        [Display(Name = "Estado")]
        public bool IsConnected { get; set; }

        [Display(Name = "Ping")]
        public bool HasPing { get; set; }

        [NotMapped]
        [Display(Name = "IED")]
        public string FullName => $"{Station} - {Bay} - {Name}";

        public override string ToString() => FullName;
    }
}

