using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.Entities
{
    public class Sentence
    {

        [Required]
        public int TextID { get; set; }

        public int SentenceID { get; set; }

        public int NextID { get; set; }

        public int PreviousID { get; set; }

        public bool IsFirst { get; set; }

        public bool IsLast { get; set; }

        [Required]
        [MaxLength(15000)]
        public string Data { get; set; }
    }
}
