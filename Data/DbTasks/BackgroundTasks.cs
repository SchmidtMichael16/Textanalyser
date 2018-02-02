using Data.Contexts;
using Data.Entities;
using Data.Result;
using Data.Synonym;
using Log;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public static List<TextResult> FindWorsdInTexts(TextContext textContext, List<string> searchTerms, bool alsoSynonyms)
        {
            List<TextResult> searchResult = new List<TextResult>();
            int exactCountMain = 0;
            List<SearchWord> searchWords = GetSynonymsFromOpenThesaurus(searchTerms);

            foreach (SearchWord searchword in searchWords)
            {
                searchword.CleanUpSynonyms();
            }

            IEnumerable<Text> allProcessedTexts = textContext.Texts.Include(s => s.Sentences).Where(t => t.Processed == true);
            foreach (Text text in allProcessedTexts)
            {
                for (int i = 0; i < searchWords.Count; i++)
                {
                    // Get all sentences of a Text that contains the exatct, similar or synoym word.
                    List<Sentence> sentencesList = text.Sentences.Where(s => s.SplitSentenceIntoWords().Where(w =>
                        w.ToLower() == searchWords[i].Term.ToLower().ToLower() ||
                        Fastenshtein.Levenshtein.Distance(w.ToLower(), searchWords[i].Term.ToLower()) == 1 ||
                        CheckIfWordISynonyms(w, searchWords[i])
                        ).Count() > 0).ToList();


                    if (sentencesList.Count > 0)
                    {
                        TextResult textResult = new TextResult(text.ID);

                        foreach (Sentence sentence in sentencesList)
                        {
                            SentenceResult sentenceResult = new SentenceResult(sentence.SentenceID, 3, 2, 1);
                            sentenceResult.MainSentence = sentence;
                            sentenceResult.PreviousSentence = GetPreviousSentence(text, sentence);
                            sentenceResult.NextSentence = GetNextSentence(text, sentence);
                            sentenceResult.CalculateResult(searchWords, alsoSynonyms);
                            sentenceResult.CalculateTotalScore();
                            // textResult.Sentences.Add(sentenceResult);
                            Console.WriteLine("hier");

                            textResult.Sentences.Add(sentenceResult);
                        }

                        searchResult.Add(textResult);
                    }


                    /*
                    int tmp = Fastenshtein.Levenshtein.Distance("mich", "sich");
                    
                    PrintSentenceList(sentencesList);
                    PrintSentenceList(sentencesList2);
                    */


                    //foreach (Sentence sentence in sentences)
                    //{
                    //    // New main sentence.
                    //    Sentence mainSentence = sentence;

                    //    // Get previous Sentence.
                    //    Sentence previousSentence = GetPreviousSentence(text, mainSentence);

                    //    // Get previous Sentence.
                    //    Sentence nextSentence = GetNextSentence(text, mainSentence);


                    //}

                    // suche nach exakten Wort im Satz 
                    // suche nach Synonym im Satz

                    // suche nach exakten Wort im vorigen Satz 
                    // suche nach Synonym im vorigen Satz

                    // suche nach exakten Wort im folgenden Satz 
                    // suche nach Synonym im folgenden Satz

                }
            }

            return searchResult;
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

        public static Synonyms GetSynonymsFromOpenThesaurus(string searchWord)
        {
            // Create a request for the URL. 
            WebRequest request = WebRequest.Create($"https://www.openthesaurus.de/synonyme/search?q={searchWord}&format=application/json");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            SeqLog.WriteNewLogMessage(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            SeqLog.WriteNewLogMessage(responseFromServer);
            // Clean up the streams and the response.
            reader.Close();
            response.Close();

            return JsonConvert.DeserializeObject<Synonyms>(responseFromServer);
        }

        public static List<SearchWord> GetSynonymsFromOpenThesaurus(List<string> words)
        {
            List<SearchWord> searchWords = new List<SearchWord>();

            for (int i = 0; i < words.Count; i++)
            {
                searchWords.Add(new SearchWord(words[i], GetSynonymsFromOpenThesaurus(words[i]).Synsets));
            }

            return searchWords;
        }

        private static Sentence GetPreviousSentence(Text text, Sentence mainSentence)
        {
            if (mainSentence.IsFirst)
            {
                return null;
            }

            return text.Sentences.Where(s => s.SentenceID == mainSentence.PreviousID).First();
        }

        private static Sentence GetNextSentence(Text text, Sentence mainSentence)
        {
            if (mainSentence.IsLast)
            {
                return null;
            }

            return text.Sentences.Where(s => s.SentenceID == mainSentence.NextID).First();
        }

        private static void PrintSentenceList(List<Sentence> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Log.SeqLog.WriteNewLogMessage($"#{i}  - {list[i].ToString()}");
            }
        }

        public static bool CheckIfWordISynonyms(string word, SearchWord searchWords)
        {
            foreach (SynonymSet synonmset in searchWords.Synonyms)
            {
                foreach (SynonymTerm synonymTerm in synonmset.Terms)
                {
                    if (word.ToLower() == synonymTerm.Term.ToLower())
                    {
                        return true;
                    }
                }

            }

            return false;
        }
    }
}
