//using FluentValidation.AspNetCore;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Project3H04.Server.Data;
using Project3H04.Server.Services;
using Project3H04.Shared;
using Project3H04.Shared.Klant;
using Project3H04.Shared.Kunstenaars;
using Project3H04.Shared.Kunstwerken;
using System.Linq;

namespace Project3H04.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                builder.WithOrigins("https://localhost:5001")
                       .AllowAnyMethod()
                       .AllowAnyHeader());
            });
            //validatie
            services.AddControllers().AddFluentValidation(fv => {
                fv.RegisterValidatorsFromAssemblyContaining<Kunstwerk_DTO.Validator>();
                fv.ImplicitlyValidateChildProperties = true;
            });
            //AUTH
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = Configuration["Auth0:Authority"];
                options.Audience = Configuration["Auth0:ApiIdentifier"];
            });
            services.AddDbContext<ApplicationDbcontext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("AzureDBContext"))); // verander naar DBContext voor localdb
            services.AddControllersWithViews();
            services.AddScoped<DataInitialiser>();
            services.AddScoped<IKunstwerkService,KunstwerkService>();
            services.AddScoped<IKunstenaarService, KunstenaarService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IKlantService, KlantService>();
            //services.AddSingleton<IOrderService, OrderService>();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataInitialiser dataInitialiser)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();  //AUTH
            app.UseAuthorization();   //AUTH
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });

            //enkel aanzetten wanneer je op local db werkt
            //dataInitialiser.InitializeData();
        }
    }
}
