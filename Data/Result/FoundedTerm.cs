using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Result
{
    public class FoundedTerm
    {
        public FoundedTerm(string term, TermType termType, int amount)
        {
            this.Term = term;
            this.TermType = termType;
            this.Amount = amount;
        }

        public string Term { get; set; }

        public TermType TermType { get; set; }

        public int Amount { get; set; }
    }
}
