using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        public  List<string> SplitSentenceIntoWords()
        {
            List<string> words = new List<string>();
            char[] punctuation = this.Data.Where(Char.IsPunctuation).Distinct().ToArray();
            words = this.Data.Split().Select(x => x.Trim(punctuation)).ToList();

            return words;
        }

        public override string ToString()
        {
            return $"TID-{this.TextID}  -  SID-{this.SentenceID}  -  {this.Data}"; 
        }
        
        
    }
}
