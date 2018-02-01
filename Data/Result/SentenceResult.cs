using Data.DbTasks;
using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Result
{
    public class SentenceResult
    {
        public SentenceResult(int sentenceID, double exactHit, double similiarHit, double synonymHit)
        {
            this.FoundedTerms = new List<FoundedTerm>();
            this.ExactHit = exactHit;
            this.SimiliarHit = similiarHit;
            this.SynonymHit = synonymHit;

        }

        public int SentenceID { get; set; }

        public List<FoundedTerm> FoundedTerms { get; set; }

        public double Score
        {
            get;
            set;
        }

        public string SentenceData { get; set; }

        public List<string> SentenceSummary { get; set; }

        public Sentence MainSentence { get; set; }

        public Sentence PreviousSentence { get; set; }

        public Sentence NextSentence { get; set; }

        public double ExactHit { get; set; }

        private double SimiliarHit;

        public double SynonymHit { get; set; }

        public void CalculateTotalScore()
        {
            double totalScore = 0;

            foreach (FoundedTerm foundedTerm in this.FoundedTerms)
            {

                if (foundedTerm.SenteneType == SenteneType.Main)
                {
                    if (foundedTerm.TermType == TermType.Exact)
                    {
                        totalScore += (double)foundedTerm.Amount * this.ExactHit;
                    }
                    else if (foundedTerm.TermType == TermType.Similar)
                    {
                        totalScore += (double)foundedTerm.Amount * this.SimiliarHit;
                    }
                    else if (foundedTerm.TermType == TermType.Synonym)
                    {
                        totalScore += (double)foundedTerm.Amount * this.SynonymHit;
                    }
                }
                else
                {
                    if (foundedTerm.TermType == TermType.Exact)
                    {
                        totalScore += (double)foundedTerm.Amount * this.ExactHit / 2;
                    }
                    else if (foundedTerm.TermType == TermType.Similar)
                    {
                        totalScore += (double)foundedTerm.Amount * this.SimiliarHit / 2;
                    }
                    else if (foundedTerm.TermType == TermType.Synonym)
                    {
                        totalScore += (double)foundedTerm.Amount * this.SynonymHit / 2;
                    }
                }
            }

            this.Score = totalScore;
        }

        public void CalculateResult(List<SearchWord> searchWords, bool alsoSynonyms)
        {
            if (this.MainSentence != null)
            {
                this.GetSentenceResult(this.MainSentence, searchWords, SenteneType.Main, alsoSynonyms);
            }

            if (this.PreviousSentence != null)
            {
                this.GetSentenceResult(this.PreviousSentence, searchWords, SenteneType.Previous, alsoSynonyms);
            }

            if (this.NextSentence != null)
            {
                this.GetSentenceResult(this.NextSentence, searchWords, SenteneType.Next, alsoSynonyms);
            }
        }

        private void GetSentenceResult(Sentence sentence, List<SearchWord> searchWords, SenteneType sentenceType, bool alsoSynonyms)
        {
            List<string> words = sentence.SplitSentenceIntoWords();

            foreach (SearchWord searchWord in searchWords)
            {
                int count = 0;

                // Search for exact match in sentence.
                count = words.Where(w => w.ToLower() == searchWord.Term.ToLower()).Count();
                if (count > 0)
                {
                    this.FoundedTerms.Add(new FoundedTerm(sentenceType, TermType.Exact, searchWord.Term, count));
                }

                // Search for similiar word in sentence.
                count = words.Where(w => Fastenshtein.Levenshtein.Distance(w.ToLower(), searchWord.Term.ToLower()) == 1).Count();
                if (count > 0)
                {
                    this.FoundedTerms.Add(new FoundedTerm(sentenceType, TermType.Similar, searchWord.Term, count));
                }

                // Search for synonyms in sentence.
                if (alsoSynonyms)
                {
                    count = words.Where(w => BackgroundTasks.CheckIfWordISynonyms(w, searchWord)).Count();
                    if (count > 0)
                    {
                        this.FoundedTerms.Add(new FoundedTerm(sentenceType, TermType.Synonym, searchWord.Term, count));
                    }
                }
            }
        }
    }
}
