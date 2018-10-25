using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ordos.Core.Models
{
    public class DisturbanceRecording
    {
        public DisturbanceRecording()
        {
            DRFiles = new List<DRFile>();
        }

        [Key] public int Id { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; }

        [Display(Name = "Archivos Comtrade")] public ICollection<DRFile> DRFiles { get; set; }

        [Required] [Display(Name = "Nombre")] public string Name { get; set; }

        [Required]
        [Display(Name = "Trigger Start")]
        public DateTime TriggerTime { get; set; }

        [Display(Name = "Trigger Length")] public double TriggerLength { get; set; }

        [Display(Name = "Trigger Channel")] public string TriggerChannel { get; set; }
    }
}