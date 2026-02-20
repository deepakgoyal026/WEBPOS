using EazyPOS.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System;

namespace EazyPOS
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
            //Ravikant
            // CORS Configuration
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowOrigin", builder =>
            //    {
            //        builder.SetIsOriginAllowed(origin =>
            //        origin.Contains("eazypos.ai", StringComparison.OrdinalIgnoreCase) ||
            //        origin.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase) ||
            //        origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
            //        origin.StartsWith("http://192.168.0.34", StringComparison.OrdinalIgnoreCase))
            //        .AllowAnyMethod()
            //        .AllowAnyHeader()
            //        .AllowCredentials();
            //    });
            //});


            //services.AddControllersWithViews()
            //    .AddNewtonsoftJson(options =>
            //    {
            //        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            //        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            //        options.SerializerSettings.DateFormatString = "yyyy-MM-dd hh:mm:ss tt";
            //    });

            services.AddSingleton<LoggerService>();
            //End ravikant

            services.AddControllers();

            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EazyPOS", Version = "v1" });
            //});
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseCors("AllowOrigin");

            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EazyPOS v1"));
            }
            else
            {

            }

            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(env.ContentRootPath),
                RequestPath = new PathString(""),
                EnableDirectoryBrowsing = true
            });

            app.UseMiddleware<APIKeyMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static IWebHost BuildWebHost(string[] args) => WebHost.CreateDefaultBuilder(args)
         .UseIISIntegration()
         .UseStartup<Startup>()
         .CaptureStartupErrors(true) // the default
         .UseSetting("detailedErrors", "true")
         .Build();
    }
}

