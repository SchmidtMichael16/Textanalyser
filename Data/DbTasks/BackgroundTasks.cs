using Data.Contexts;
using Data.Entities;
using Log;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Data.DbTasks
{
    public static class BackgroundTasks
    {
        public static void SplitNewTexts(TextContext textContext)
        {
            IEnumerable<Text> textToProcess = textContext.Texts.Where(t => t.Processed == false);

            foreach (Text text in textToProcess)
            {
                //SeqLog.WriteNewLogMessage("{ID} - {Title} - {Processed}", text.ID, text.Title, text.Processed);

                // SplitSentece ist noch verbesserungswürdig.
                List<string> splitSentences = SplitSentences(text.Data);

                for (int i = 0; i < splitSentences.Count; i++)
                {
                    Sentence newSentence = new Sentence() { Data = splitSentences[i], TextID = text.ID, SentenceID = i };

                    // Every sentence has a previous sentence, except the first one.
                    if (i > 0)
                    {
                        newSentence.PreviousID = i - 1;
                    }
                    else
                    {
                        newSentence.IsFirst = true;
                    }

                    // Every sentence has a next sentence, except the last one.
                    if (i + 1 < splitSentences.Count)
                    {
                        newSentence.NextID = i + 1;
                    }
                    else
                    {
                        newSentence.IsLast = true;
                    }
                    
                    textContext.Add<Sentence>(newSentence);
                }

                text.Processed = true;
                SeqLog.WriteNewLogMessage("Text with Id:{ID} was splitted into senteces.", text.ID);
            }

            textContext.SaveChanges();
        }

        public static void FindWorsdInTexts(TextContext textContext, List<string> searchWords)
        {
            IEnumerable<Text> allProcessedTexts = textContext.Texts.Include(s=> s.Sentences).Where(t => t.Processed == true);
            foreach (Text text in allProcessedTexts)
            {
                //foreach (Sentence sentence in text.Sentences)
                //{
                //    List<string> words = SplitSentenceIntoWords(sentence.Data);

                //    for (int i = 0; i < searchWords.Count; i++)
                //    {
                //        // Search for exact match.
                //        int exactCount = words.Where(w => w.ToLower() == searchWords[i]).Count();
                //    }
                //}
            }
        }

        public static List<string> SplitSentenceIntoWords(string sentence)
        {
            List<string> words = new List<string>();
            char[] punctuation = sentence.Where(Char.IsPunctuation).Distinct().ToArray();
            words = sentence.Split().Select(x => x.Trim(punctuation)).ToList();

            return words;
        }

        private static List<string> SplitSentences(string sourceText)
        {
            // Delete multiple spaces.
            sourceText = Regex.Replace(sourceText, "[ ]{2,}", " ");

            // split the sentences with a regular expression
            List<string> splitSentences = new List<string>();
            splitSentences = Regex.Split(sourceText, @"(?<=['""A-Za-z0-9][\.\!\?])\s+(?=[A-Z])").ToList();

            // loop the sentences
            for (int i = 0; i < splitSentences.Count; i++)
            {
                // clean up the sentence one more time, trim it.
                splitSentences[i] = splitSentences[i].Replace(Environment.NewLine, string.Empty);
            }

            return splitSentences;
        }


    }
}
