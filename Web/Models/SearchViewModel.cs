using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models
{
    public class SearchViewModel
    {
        [Required]
        public string Word1 { get; set; }

        public string Word2 { get; set; }

        public string Word3 { get; set; }

        public bool AlsoSynonyms { get; set; }
    }
}
