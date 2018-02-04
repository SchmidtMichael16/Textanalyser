using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Contexts;
using Data.DbTasks;
using Data.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Web.Models;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SQLite;
using Microsoft.Extensions.Configuration;
using Data;
using Data.Entities;
using Data.Synonym;
using System.Net;
using System.IO;
using Log;
using Newtonsoft.Json;
using Microsoft.Extensions.Localization;

namespace Web.Controllers
{
    [Route("api/search")]
    public class SearchController : Controller
    {
        private IHubContext<SearchHub> _hubContext;

        private readonly TextContext textContext;

        private readonly IStringLocalizer<SharedResources> _sharedLocalizer;


        public SearchController(TextContext textContext, IHubContext<SearchHub> hubContext, IStringLocalizer<SharedResources> sharedLocalizer)
        {
            this.textContext = textContext;
            this._sharedLocalizer = sharedLocalizer;
            this._hubContext = hubContext;
        }

        [Route("SearchApp")]
        public IActionResult Index()
        {
            return View();
        }



        //[HttpPost("")]
        //public IActionResult Index(SearchViewModel search)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        //return View(name);
        //        List<string> searchWords = new List<string> { search.Word1, search.Word2, search.Word3 };
        //        //return new ObjectResult(BackgroundTasks.FindWorsdInTexts(this.textContext, searchWords, search.AlsoSynonyms));
        //        //return $"post request {search.Word1}  {search.Word2}   {search.Word3}  {search.AlsoSynonyms}";
        //        return View();
        //    }
        //    else
        //    {
        //        return View(null);
        //    }


        //}

        // GET api/simple
        [HttpGet]
        public void Get()
        {

            string word1 = HttpContext.Request.Query["Word1"].ToString();
            string word2 = HttpContext.Request.Query["Word2"].ToString();
            string word3 = HttpContext.Request.Query["Word3"].ToString();
            string sAlsoSynoyms = HttpContext.Request.Query["AlsoSynonyms"].ToString().ToLower();
            bool alsoSynonyms = false;

            if (sAlsoSynoyms == "true")
            {
                alsoSynonyms = true;
            }

            SearchRequest searchRequest = new SearchRequest(word1, word2, word3, alsoSynonyms);

            List<TextResult> result = new List<TextResult>();  //BackgroundTasks.FindWorsdInTexts(this.textContext, searchwords, alsoSynonyms);

            this._hubContext.Clients.All.InvokeAsync("SendMessageToClient", this._sharedLocalizer["SearchingFor"] + ": || " + word1 + " - " + word2 + " - " + word3 + " || " + this._sharedLocalizer["SearchOption"] + " = " + alsoSynonyms);
            
            Task.Factory.StartNew(() =>
            {
                Log.SeqLog.WriteNewLogMessage("Createing background search jobfor {searchword1}  {searchword2}  {searchword3}  also Synonyms: {alsoSynonyms}", searchRequest.SearchWord1, searchRequest.SearchWord2, searchRequest.SearchWord3, searchRequest.AlsoSynonyms);
                BackgroundJob.Enqueue(
                       () => this.FindWorsdInTexts(searchRequest));

                //BackgroundJob.Enqueue(
                //    () => Log.SeqLog.WriteNewLogMessage("This should be a SearchJob {Time}", DateTime.Now));
            });

            //this.FindWorsdInTexts(searchRequest);

            
            //return new ObjectResult(result);

        }

        public void FindWorsdInTexts(SearchRequest searchRequest)
        {
            Log.SeqLog.WriteNewLogMessage("Searching for {searchword1}  {searchword2}  {searchword3}  also Synonyms: {alsoSynonyms}", searchRequest.SearchWord1, searchRequest.SearchWord2, searchRequest.SearchWord3, searchRequest.AlsoSynonyms);
            List<TextResult> searchResult = new List<TextResult>();
            List<string> searchTerms = searchRequest.GetSearchWords();
            int exactCountMain = 0;

            // Get Synonyms.
            List<SearchWord> searchWords = GetSynonymsFromOpenThesaurus(searchTerms);

            foreach (SearchWord searchword in searchWords)
            {
                searchword.CleanUpSynonyms();
            }

            IEnumerable<Text> allProcessedTexts = this.textContext.Texts.Include(s => s.Sentences).Where(t => t.Processed == true);
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
                            sentenceResult.CalculateResult(searchWords, searchRequest.AlsoSynonyms);
                            sentenceResult.CalculateTotalScore();
                            // textResult.Sentences.Add(sentenceResult);
                            //Console.WriteLine("hier");

                            textResult.Sentences.Add(sentenceResult);
                        }

                        searchResult.Add(textResult);
                    }
                }
            }

            this._hubContext.Clients.All.InvokeAsync("SendMessageToClient", $"Result for || {searchRequest.SearchWord1}  {searchRequest.SearchWord2}  {searchRequest.SearchWord3}  ||  also Synonyms: {searchRequest.AlsoSynonyms}:");

            List<TextResult> bestResults =  searchResult.OrderByDescending(sr => sr.TotalScore).Take(5).ToList();

            foreach (TextResult result in bestResults)
            {
                //this._hubContext.Clients.All.InvokeAsync("SendMessageToClient", $"{JsonConvert.SerializeObject(result).ToString()} \n\n");
                this._hubContext.Clients.All.InvokeAsync("SendMessageToClient", $"{this.PrintResult(result)}");
            }
           

        }
        public Synonyms GetSynonymsFromOpenThesaurus(string searchWord)
        {
            if (searchWord != null && searchWord != string.Empty)
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

            return null;
        }

        public List<SearchWord> GetSynonymsFromOpenThesaurus(List<string> words)
        {
            List<SearchWord> searchWords = new List<SearchWord>();

            for (int i = 0; i < words.Count; i++)
            {
                List<SynonymSet> synonyms = this.GetSynonymsFromOpenThesaurus(words[i]).Synsets;
                if (synonyms != null)
                {
                    searchWords.Add(new SearchWord(words[i], synonyms));
                }
            }

            return searchWords;
        }

        private Sentence GetPreviousSentence(Text text, Sentence mainSentence)
        {
            if (mainSentence.IsFirst)
            {
                return null;
            }

            return text.Sentences.Where(s => s.SentenceID == mainSentence.PreviousID).First();
        }

        private Sentence GetNextSentence(Text text, Sentence mainSentence)
        {
            if (mainSentence.IsLast)
            {
                return null;
            }

            return text.Sentences.Where(s => s.SentenceID == mainSentence.NextID).First();
        }

        public bool CheckIfWordISynonyms(string word, SearchWord searchWords)
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

        private string PrintResult(TextResult result)
        {
            string retString = "";

            retString = "TextID = " + result.TextID + "</br>";
            retString += "Score = " + result.TotalScore + "</br>";

            List<SentenceResult> bestSentenceResults = result.Sentences.OrderByDescending(s => s.Score).Take(3).ToList();

            foreach (SentenceResult sentenceResult in bestSentenceResults)
            {
                retString += "SentenceID = " + sentenceResult.SentenceID + "</br>";
                retString += "Score = " + sentenceResult.Score + "</br>";
  

                foreach (FoundedTerm foundedTerm in sentenceResult.FoundedTerms)
                {
                    retString += "Score = " + foundedTerm.Score + " | Term = " + foundedTerm.Term +  " | SentenceTyp = " + foundedTerm.SenteneType + " | MatchTyp = " + foundedTerm.TermType + " | Amount = " + foundedTerm.Amount + "</br>";
                }
            }

            return retString;
        }

    }
}