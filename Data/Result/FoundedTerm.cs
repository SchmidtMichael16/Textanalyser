using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Result
{
    public class FoundedTerm
    {
        public FoundedTerm(SenteneType senteneType, TermType termType, string term,  int amount)
        {
            this.SenteneType = senteneType;
            this.Term = term;
            this.TermType = termType;
            this.Amount = amount;
        }

        public string Term { get; set; }

        public TermType TermType { get; set; }

        public SenteneType SenteneType { get; set; }

        public int Amount { get; set; }

        public override string ToString()
        {
            return $"{this.SenteneType} - {this.TermType} - {this.Term} - {this.Amount}";
        }
    }
}
