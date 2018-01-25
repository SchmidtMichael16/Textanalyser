using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.Entities
{
    public class Text
    {
        public int ID { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [MaxLength(15000)]
        public string Data { get; set; }

        public string Author { get; set; }

        public bool Processed { get; set; }

        public virtual List<Sentence> Sentences { get; set; }
    }
}
