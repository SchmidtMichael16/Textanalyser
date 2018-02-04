using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Web.Data;
using Web.Models;
using Web.Services;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using Hangfire;
using Hangfire.SQLite;
using Data.Contexts;

namespace Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add hangfire service.
            var HangfireOptions = new SQLiteStorageOptions();
            services.AddHangfire(x => x.UseSQLiteStorage(Configuration.GetConnectionString("SQLiteHangfire"), HangfireOptions));

            // Add SignalR
            services.AddSignalR();

            services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });

            services.AddMvc()
                .AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = "Resources"; })
                .AddDataAnnotationsLocalization();

            var dbBuilder = new DbContextOptionsBuilder<TextContext>();
            dbBuilder.UseSqlite("Data Source=" + Configuration.GetConnectionString("SQLiteTextDb"));
            services.AddScoped<TextContext>(_ => new TextContext(dbBuilder.Options));

            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    var supportedCultures = new List<CultureInfo>
                    {
                        //new CultureInfo("en-US"),
                        new CultureInfo("en"),
                        //new CultureInfo("de-DE"),
                        new CultureInfo("de")
                    };

                    opts.DefaultRequestCulture = new RequestCulture("en-US");
                    // Formatting numbers, dates, etc.
                    opts.SupportedCultures = supportedCultures;
                    // UI strings that we have localized.
                    opts.SupportedUICultures = supportedCultures;
                });
               

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            // Add google authentication.
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                //googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                //googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];

                googleOptions.ClientId = "856878470856-5o21npb03983diio340j1a8pfcss6sds.apps.googleusercontent.com";
                googleOptions.ClientSecret = "k0d2QY1_mlGAaKhqWjZKLZ1z";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseAuthentication();

            // For SignalR
            app.UseSignalR(routes =>
            {
                routes.MapHub<SearchHub>("search");
            });

            // For MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            // For hangfire server and dashboard
            app.UseHangfireServer(new BackgroundJobServerOptions { WorkerCount = 1 });
            app.UseHangfireDashboard();


        }
    }
}
