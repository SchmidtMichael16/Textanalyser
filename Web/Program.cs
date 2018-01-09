using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Data.Contexts;
using Data.Entities;
using Data;
using Hangfire;
using Hangfire.SQLite;

namespace Web
{
    public class Program
    {
        static string dbName = "Text.db";
        public static void Main(string[] args)
        {

            var builder = new DbContextOptionsBuilder<TextContext>();
            builder.UseSqlite("Data Source=" + dbName);

            var textContext = new TextContext(builder.Options);

            CreateDbWithTestData(textContext);

            //List<Text> tmpList = GetAllTexts(textContext, "Traum");
            JobStorage.Current = new Hangfire.SQLite.SQLiteStorage("Data Source=hangfire.db;");
            
            BackgroundJob.Enqueue(
                () => Log.SeqLog.WriteNewLogMessage("Hello current time is {Time}", DateTime.Now));

            RecurringJob.AddOrUpdate(
                () => Log.SeqLog.WriteNewLogMessage("This is the recurring job - Hello current time is {Time}", DateTime.Now), Cron.MinuteInterval(2));

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
                context.Add<Text>(FamousSpeeches.GetChurchillSpeedch1());
                context.Add<Text>(FamousSpeeches.GetChurchillSpeedch2());
                context.Add<Text>(FamousSpeeches.GetGandhiSpeedch());

                int rowCount = context.SaveChanges();
                Log.SeqLog.WriteNewLogMessage("Seeding Database '{dbName}', Table '{Table}' - {rows} rows inserted", dbName, "Text", rowCount);
            }
        }

        public static List<Text> GetAllTexts(TextContext context, String title)
        {
            return context.Texts.Where(t => t.Title.Contains(title)).ToList();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>().ConfigureAppConfiguration((builderContext, config) =>
                {
                    IHostingEnvironment env = builderContext.HostingEnvironment;
                    config.SetBasePath(env.ContentRootPath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .Build();
    }
}
