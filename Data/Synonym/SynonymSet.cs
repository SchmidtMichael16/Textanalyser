using Data.Synonym;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public class SynonymSet
    {
        public int ID { get; set; }

        public List<Object> Categories { get; set; }

        public List<SynonymTerm> Terms { get; set; }


    }
}
