using System;
using System.ComponentModel.DataAnnotations;

namespace Ordos.Core.Models
{
    public class DRFile
    {
        public DRFile() { }

        [Key]
        public int Id { get; set; }

        public int DisturbanceRecordingId { get; set; }
        public DisturbanceRecording DisturbanceRecording { get; set; }

        public byte[] FileData { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public long FileSize { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }
    }
}
