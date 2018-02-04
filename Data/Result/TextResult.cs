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
            this.Sentences = new List<SentenceResult>();
        }

        public int TextID { get; set; }

        public double TotalScore
        {
            get
            {
                double totalScore = 0;

                if (this.Sentences != null)
                {
                    foreach (SentenceResult sentenceResult in this.Sentences)
                    {
                        totalScore += sentenceResult.Score;
                    }
                }

                return totalScore;
            }
        }


        public List<SentenceResult>Sentences
        {
            get;

            set;
        }


        public override string ToString()
        {
            return $"{this.TextID}  -  {this.TotalScore}";
        }
    }
}
