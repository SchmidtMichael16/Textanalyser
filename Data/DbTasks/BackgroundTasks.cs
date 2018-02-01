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

        public static void FindWorsdInTexts(TextContext textContext, List<string> searchTerms)
        {
            List<TextResult> searchResult = new List<TextResult>();
            int exactCountMain = 0;
            List<SearchWord> searchWords =  GetSynonymsFromOpenThesaurus(searchTerms);

            IEnumerable<Text> allProcessedTexts = textContext.Texts.Include(s => s.Sentences).Where(t => t.Processed == true);
            foreach (Text text in allProcessedTexts)
            {
                for (int i = 0; i < searchTerms.Count; i++)
                {
                    var pattern = new Regex(@"\W");
                    // TODO Contains kann hier nicht verwendet werden --> findet auch "sich" wenn suchwort "ich" ist.
                    //var q = pattern.Split(myText).Any(w => words.Contains(w));
                    //https://stackoverflow.com/questions/4874371/how-to-check-if-any-word-in-my-liststring-contains-in-text
                    //IEnumerable<Sentence> sentences = text.Sentences.Where(s => s.Data.ToLower().Contains(searchWords[i].ToLower()));

                    //List<Sentence> sentencesListOld = text.Sentences.Where(s => s.Data.ToLower().Contains(searchWords[i].ToLower())).ToList();
                    List<Sentence> sentencesList = text.Sentences.Where(s => SplitSentenceIntoWords(s.Data).Where(w => 
                        w.ToLower() == searchTerms[i].ToLower().ToLower() ||
                        Fastenshtein.Levenshtein.Distance(w.ToLower(), searchTerms[i].ToLower()) == 1
                        ).Count()> 0).ToList();

                    /*
                    int tmp = Fastenshtein.Levenshtein.Distance("mich", "sich");
                    
                    PrintSentenceList(sentencesList);
                    PrintSentenceList(sentencesList2);
                    */

                    Console.WriteLine("hier");
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

        private static SentenceResult GetSentenceResult(Sentence sentence, string word)
        {
            SentenceResult sentenceResult = new SentenceResult(sentence.SentenceID);

            List<string> words = SplitSentenceIntoWords(sentence.Data);

            // Search for exact match in sentence.
            sentenceResult.FoundedTerms.Add(new FoundedTerm(word, TermType.Exact, words.Where(w => w.ToLower() == word.ToLower()).Count()));

            // Search for similiar word in sentence.
            // TODO Levenstein-Distanz
            return sentenceResult;
        }

        private static SentenceResult GetSentenceResult(Sentence sentence, string word, Synonyms synonyms)
        {
            SentenceResult sentenceResult = GetSentenceResult(sentence, word);

            // Search for synonyms in sentence.
            // TODO 

            return sentenceResult;
        }

        private static void PrintSentenceList(List<Sentence> list)
        {
            for (int i = 0; i<list.Count;i++)
            {
                Log.SeqLog.WriteNewLogMessage($"#{i}  - {list[i].ToString()}");
            }
        }
    }
}
