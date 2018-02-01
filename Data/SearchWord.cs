using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public class SearchWord
    {
        public SearchWord(string searchTerm, List<SynonymSet> synonyms)
        {
            this.Term = searchTerm;
            this.Synonyms = synonyms;
        }

        public string Term { get; set; }

        public List<SynonymSet> Synonyms { get; set; }
    }
}
