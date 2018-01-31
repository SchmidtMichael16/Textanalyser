using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Result
{
    public class TextResult
    {
        public TextResult(int textID)
        {
            this.TextID = textID;
        }

        public int TextID { get; set; }

        public int TotalScore
        {
            get
            {
                int totalScore = 0;

                if (this.MainSentences != null)
                {
                    totalScore += this.MainSentences.Score;
                }

                if (this.NextSentences != null)
                {
                    totalScore += this.NextSentences.Score;
                }

                if (this.PreviousSentences != null)
                {
                    totalScore += this.PreviousSentences.Score;
                }

                return totalScore;
            }
        }


        public SentenceResult MainSentences
        {
            get;

            set;
        }

        public SentenceResult NextSentences { get; set; }

        public SentenceResult PreviousSentences { get; set; }

    }
}
