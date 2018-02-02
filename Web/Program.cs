using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Data;
using Data.Contexts;
using Data.DbTasks;
using Data.Entities;
using Hangfire;
using Hangfire.SQLite;
using Log;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Web
{
    public class Program
    {
        static string dbName;
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            var config = builder.Build();


            var dbBuilder = new DbContextOptionsBuilder<TextContext>();
            dbBuilder.UseSqlite("Data Source=" + config["ConnectionStrings:SQLiteTextDb"]);
            dbName = config["ConnectionStrings:SQLiteTextDb"];
            var textContext = new TextContext(dbBuilder.Options);
           
            JobStorage.Current = new SQLiteStorage(config["ConnectionStrings:SQLiteHangfire"]);
            //BackgroundJob.Enqueue(
            //   () => Log.SeqLog.WriteNewLogMessage("Hello current time is {Time}", DateTime.Now));

            // Create DB with test data.
            CreateDbWithTestData(textContext);

            //RecurringJob.AddOrUpdate(
            //   () => BackgroundTasks.SplitNewTexts(textContext),
            //   Cron.MinuteInterval(5));

            //List<Text> tmpList =  GetAllTexts(textContext, "ich");
            //List<Text> tmpList2 = GetAllTextsToProcess(textContext);

            BackgroundTasks.SplitNewTexts(textContext);
            List<string> words = new List<string>();
            words.Add("ich");
            words.Add("Hallo");
            words.Add("Heute");

            //BackgroundTasks.FindWorsdInTexts(textContext, words, true);

            BuildWebHost(args).Run();
        }

        public static void CreateEmptyDb(TextContext context)
        {
            // Löscht die DB falls schon vorhanden.
            context.Database.EnsureDeleted();
            Log.SeqLog.WriteNewLogMessage("Database '{dbName}' deleted!", dbName);


            // Legt die DB an.
            context.Database.EnsureCreated();
            Log.SeqLog.WriteNewLogMessage("Database '{dbName}' created!", dbName);
        }

        public static void CreateDbWithTestData(TextContext context)
        {
            // Create empty db.
            CreateEmptyDb(context);

            // Insert test data.
            SeedTestData(context);
        }

        public static void SeedTestData(TextContext context)
        {
            // Only insert data if table is empty.
            if (!context.Texts.Any())
            {
                Log.SeqLog.WriteNewLogMessage("Seeding Database '{dbName}', Table '{Table}'...", dbName, "Text");
                //context.Add<Text>(FamousSpeeches.GetChurchillSpeedch1());
                //context.Add<Text>(FamousSpeeches.GetChurchillSpeedch2());
                //context.Add<Text>(FamousSpeeches.GetKennedySpeech());
                //context.Add<Text>(FamousSpeeches.GetMandelaSpeedch());
                //context.Add<Text>(FamousSpeeches.GetJobsSpeedch1());
                //context.Add<Text>(FamousSpeeches.GetJobsSpeedch2());
                //context.Add<Text>(FamousSpeeches.GetMLutherKingSpeech());
                //context.Add<Text>(FamousSpeeches.GetObamaSpeedch());
                //context.Add<Text>(FamousSpeeches.GetGorbatschowSpeedch());
                //context.Add<Text>(FamousSpeeches.GetFiglpeedch());
                //context.Add<Text>(FamousSpeeches.GetGandhiSpeedch());
                context.Add<Text>(FamousSpeeches.GetTestText());

                int rowCount = context.SaveChanges();
                Log.SeqLog.WriteNewLogMessage("Seeding Database '{dbName}', Table '{Table}' - {rows} rows inserted", dbName, "Text", rowCount);
            }
        }

        public static List<Text> GetAllTexts(TextContext context, String title)
        {
            return context.Texts.Where(t => t.Title.Contains(title)).ToList();
        }

        public static List<Text> GetAllTextsToProcess(TextContext context)
        {
            return context.Texts.Where(t => t.Processed == false).ToList();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
