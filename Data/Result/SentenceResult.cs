using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Result
{
    public class SentenceResult
    {
        public SentenceResult(int sentenceID)
        {
            this.FoundedTerms = new List<FoundedTerm>();

        }

        public int SentenceID { get; set; }

        public List<FoundedTerm> FoundedTerms { get; set; }

        public int Score
        {
            get
            {
                return this.CalculateTotalScore();
            }

        }

        public string SentenceData { get; set; }

        public List<string> SentenceSummary { get; set; }

        private int CalculateTotalScore()
        {
            int totalScore = 0;

            if (this.FoundedTerms == null)
            {
                return 0;
            }

            for (int i = 0; i < this.FoundedTerms.Count; i++)
            {
                if (this.FoundedTerms[i].TermType == TermType.Exact)
                {
                    totalScore += 3 * this.FoundedTerms[i].Amount;
                }
                else if (this.FoundedTerms[i].TermType == TermType.Similar)
                {
                    totalScore += 1 * this.FoundedTerms[i].Amount;
                }
                else if (this.FoundedTerms[i].TermType == TermType.Synonym)
                {
                    totalScore += 1 * this.FoundedTerms[i].Amount;
                }
            }

            return totalScore;
        }
    }
}
