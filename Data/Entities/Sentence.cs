using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.Entities
{
    public class Sentence
    {

        [Required]
        [Key]
        public int TextID { get; set; }

        public int ID { get; set; }

        public int NextID { get; set; }

        public int PreviousID { get; set; }

        [Required]
        [MaxLength(15000)]
        public string Data { get; set; }
    }
}
