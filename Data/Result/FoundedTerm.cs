using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Result
{
    public class FoundedTerm
    {
        public FoundedTerm(SenteneType senteneType, TermType termType, string term,  int amount, double exactHit, double similiarHit, double synonymHit)
        {
            this.SenteneType = senteneType;
            this.Term = term;
            this.TermType = termType;
            this.Amount = amount;
            this.ExactHit = exactHit;
            this.SimiliarHit = similiarHit;
            this.SynonymHit = synonymHit;
        }

        public string Term { get; set; }

        public TermType TermType { get; set; }

        public SenteneType SenteneType { get; set; }

        public double ExactHit { get; set; }

        private double SimiliarHit;

        public double SynonymHit { get; set; }

        public double Score
        {
            get
            {
                if (this.SenteneType == SenteneType.Main)
                {
                    if (this.TermType == TermType.Exact)
                    {
                       return (double)this.Amount * this.ExactHit;
                    }
                    else if (this.TermType == TermType.Similar)
                    {
                        return (double)this.Amount * this.SimiliarHit;
                    }
                    else if (this.TermType == TermType.Synonym)
                    {
                        return (double)this.Amount * this.SynonymHit;
                    }
                }
                else
                {
                    if (this.TermType == TermType.Exact)
                    {
                        return (double)this.Amount * this.ExactHit / 2;
                    }
                    else if (this.TermType == TermType.Similar)
                    {
                        return (double)this.Amount * this.SimiliarHit / 2;
                    }
                    else if (this.TermType == TermType.Synonym)
                    {
                        return (double)this.Amount * this.SynonymHit / 2;
                    }
                }

                return 0;
            }
        }

        public int Amount { get; set; }

        public override string ToString()
        {
            return $"{this.SenteneType} - {this.TermType} - {this.Term} - {this.Amount}";
        }
    }
}
