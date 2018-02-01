using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Data.Synonym
{
    public class SynonymTerm
    {
        private string term;
        public string Term {
            get
            {
                return this.term;
            }

            set
            {
                //https://stackoverflow.com/questions/20505914/escape-special-character-in-regex
                Regex rgx = new Regex(Regex.Escape("(") + ".*)");
                this.term = rgx.Replace(value,"").Trim();
                //this.term = value.Replace()
                   
            }
        }
        public string Level { get; set; }

    }
}
