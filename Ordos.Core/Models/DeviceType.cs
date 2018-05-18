using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ordos.Core.Models
{
    public class DeviceType
    {
        public DeviceType() { }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Manufacturer { get; set; }

        [Required]
        public string Model { get; set; }

        //MAYBE TODO: Mover a device; No es propio del deviceType sino del dispositivo;
        [Display(Name = "FTP Username")]
        public string FtpUsername { get; set; }

        [Display(Name = "FTP Password")]
        public string FtpPassword { get; set; }

        [Required]
        public string ComtradePath { get; set; }

        [Display(Name = "Device Type")]
        public string DeviceTypeName => $"{Manufacturer} {Model}";
    }
}
