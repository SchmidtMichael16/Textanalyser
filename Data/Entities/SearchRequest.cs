using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Data.Entities
{
    public class SearchRequest
    {
        public SearchRequest(string searchWord1, string searchWord2, string searchWord3, bool alsoSynonyms)
        {
            this.SearchWord1 = searchWord1.Trim();
            this.SearchWord2 = searchWord2.Trim();
            this.SearchWord3 = searchWord3.Trim();
            this.AlsoSynonyms = alsoSynonyms;
        }

        public int ID { get; set; }

        public string SearchWord1 { get; set; }

        public string SearchWord2 { get; set; }

        public string SearchWord3 { get; set; }

        public bool AlsoSynonyms { get; set; }

        public string ClientId { get; set; }



        public List<string> GetSearchWords()
        {
            List<string> retList = new List<string>();


            if (this.SearchWord1 != string.Empty && this.SearchWord1 != null)
            {
                retList.Add(this.SearchWord1);
            }

            if (this.SearchWord2 != string.Empty && this.SearchWord2 != null)
            {
                retList.Add(this.SearchWord2);
            }

            if (this.SearchWord3 != string.Empty && this.SearchWord3 != null)
            {
                retList.Add(this.SearchWord3);
            }

            return retList;
        }
    }
}