using System;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using BusinessLogic;
using BusinessLogic.Interfaces;
using DataAccess.Configuration;
using DataAccess.Context;
using DataAccess.HttpClients;
using DataAccess.HttpClients.Interfaces;
using DataAccess.Repositories;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShowAPI.Filters;
using ShowAPI.HostedServices;
using ShowAPI.Policies;
using Swashbuckle.AspNetCore.Swagger;

namespace ShowAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.Configure<ScraperOptions>(Configuration.GetSection("ScraperOptions"));
            services.Configure<ShowApiOptions>(Configuration.GetSection("ShowApiOptions"));
            var options = new ScraperOptions();
            Configuration.Bind("ScraperOptions", options);
            services.AddDbContext<ScraperContext>(opts =>
            {
                opts.UseSqlServer(Configuration.GetConnectionString("ScraperDatabase"));
            });
            services.AddHttpClient<IScraperHttpClient, ScraperHttpClient>(client =>
                {
                    client.BaseAddress = new Uri(options.BaseUri);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
                .AddPolicyHandler(PolicyHolder.GetDefaultPolicy(options.HttpClientRetryCount,
                    options.HttpClientTimeoutSeconds));
            services.AddScoped<IScrapeRepository, ScrapeRepository>();
            services.AddScoped<IShowWithCastRepository, ShowWithCastRepository>();
            services.AddScoped<IScopedService, Scraper>();
            services.AddScoped<IShowWithCastData, ShowWithCastData>();
            services.AddHostedService<TimedHostedService>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Show API",
                    Version = "v1",
                    Description = "Test Web API application"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddMvc(mvcOptions => { mvcOptions.Filters.Add<OperationCancelledExceptionFilter>(); });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Show API");
                c.RoutePrefix = string.Empty;
            });
            app.UseMvc();
        }
    }
}