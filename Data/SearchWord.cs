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

        public void CleanUpSynonyms()
        {
            for (int i = 0; i < this.Synonyms.Count; i++)
            {
                for (int j = 0; j<this.Synonyms[i].Terms.Count;j++)
                {
                    if (this.Synonyms[i].Terms[j].Term.ToLower() == this.Term.ToLower())
                    {
                        this.Synonyms[i].Terms.RemoveAt(j);
                        j--;
                    }
                }
                
            }
        }

        public override string ToString()
        {
            return $"{this.Term}";
        }
    }
}
