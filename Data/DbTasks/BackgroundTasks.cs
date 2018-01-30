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

        public static void FindWorsdInTexts(TextContext textContext, List<string> searchWords)
        {
            TextResult searchResult = new TextResult();
            SentenceResult mainSentence;
            int exactCountMain = 0;

            IEnumerable<Text> allProcessedTexts = textContext.Texts.Include(s => s.Sentences).Where(t => t.Processed == true);

            foreach (Text text in allProcessedTexts)
            {
                for (int i = 0; i < searchWords.Count; i++)
                {
                    IEnumerable<Sentence> sentences = text.Sentences.Where(s => s.Data.ToLower().Contains(searchWords[i].ToLower()));

                    foreach (Sentence sentence in sentences)
                    {
                        List<string> words = SplitSentenceIntoWords(sentence.Data);

                        // Search for exact match in main sentence.
                        //searchResult.MainSentences.
                        exactCountMain += words.Where(w => w.ToLower() == searchWords[i]).Count() * 3;

                        // Search for synonyms in main sentence.

                        // Search for similar term in main sentence.
                    }

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
