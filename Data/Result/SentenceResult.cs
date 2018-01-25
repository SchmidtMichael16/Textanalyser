using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Result
{
    public class SentenceResult
    {
        public int SentenceID { get; set; }

        public int Score { get; set; }

        public string SentenceData { get; set; }

        public List<string> SentenceSummary { get; set; }
    }
}
