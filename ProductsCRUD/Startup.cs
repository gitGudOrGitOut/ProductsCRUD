using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductsCRUD.Repositories.Interface;
using ProductsCRUD.Repositories.Concrete;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using System.IO;
using ProductsCRUD.OpenApiSecurity;
using Microsoft.IdentityModel.Logging;
using ProductsCRUD.AutomatedCacher.Interface;
using ProductsCRUD.AutomatedCacher.Concrete;
using ProductsCRUD.AutomatedCacher.Model;
using ProductsCRUD.CustomExceptionHandler;

namespace ProductsCRUD
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            if(_environment.IsDevelopment())
            {
                services.AddDbContext<Context.Context>(options => options.UseSqlServer
                    ("local"));
            } else if (_environment.IsProduction() || _environment.IsStaging())
            {
                services.AddDbContext<Context.Context>(options => options.UseSqlServer
                    (Configuration.GetConnectionString("ProductsConnectionString"),
                    sqlServerOptionsAction: sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(2),
                    errorNumbersToAdd: null)));
            }

            services.AddControllers().AddNewtonsoftJson(j =>
            {
                j.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = $"https://{Configuration["Auth0:Domain"]}/";
                options.Audience = Configuration["Auth0:Audience"];
            });

            //Using the fake repository while in development
            if(_environment.IsDevelopment())
            {
                services.AddSingleton<IProductsRepository, FakeProductsRepository>();
            }
            else
            {
                services.AddScoped<IProductsRepository, SqlProductsRepository>();
            }

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ProductsCRUD API",
                    Description = "An ASP.NET Core Web API for managing products of ThAmCo."
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                string securityDefinitionName = Configuration["SwaggerUISecurityMode"] ?? "Bearer";
                OpenApiSecurityScheme securityScheme = new OpenApiBearerSecurityScheme();
                OpenApiSecurityRequirement securityRequirement = new OpenApiBearerSecurityRequirement(securityScheme);

                if (securityDefinitionName.ToLower() == "oauth2")
                {
                    securityScheme = new OpenApiOAuthSecurityScheme(Configuration["Auth0:Domain"], Configuration["Auth0:Audience"]);
                    securityRequirement = new OpenApiOAuthSecurityRequirement();
                }

                options.AddSecurityDefinition(securityDefinitionName, securityScheme);

                options.AddSecurityRequirement(securityRequirement);
            });

            services.AddAuthorization(o =>
            {
                o.AddPolicy("GetProduct", policy =>
                    policy.RequireClaim("permissions", "read:product"));
                o.AddPolicy("GetProducts", policy =>
                    policy.RequireClaim("permissions", "read:products"));
                o.AddPolicy("GetProductPrices", policy =>
                    policy.RequireClaim("permissions", "read:product_prices"));
                o.AddPolicy("GetProductPriceHistory", policy =>
                    policy.RequireClaim("permissions", "read:product_price_history"));
                o.AddPolicy("CreateProduct", policy =>
                    policy.RequireClaim("permissions", "add:product"));
                o.AddPolicy("UpdateProduct", policy =>
                    policy.RequireClaim("permissions", "edit:product"));
                o.AddPolicy("DeleteProduct", policy =>
                    policy.RequireClaim("permissions", "delete:product"));
            });

            services.AddMvc(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });

            services.AddSingleton<IMemoryCacheAutomater, MemoryCacheAutomater>();
            services.Configure<MemoryCacheModel>(Configuration.GetSection("MemoryCache"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMemoryCacheAutomater memoryCacheAutomater, Context.Context dataContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Glossary v1");

                    if (Configuration["SwaggerUISecurityMode"]?.ToLower() == "oauth2")
                    {
                        c.OAuthClientId(Configuration["Auth0:ClientId"]);
                        c.OAuthClientSecret(Configuration["Auth0:ClientSecret"]);
                        c.OAuthAppName("GlossaryClient");
                        c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "audience", Configuration["Auth0:Audience"] } });
                        c.OAuthUsePkce();
                    }
                });
            } else if(env.IsProduction() || env.IsStaging())
            {
                app.UseMiddleware<ExceptionMiddleware>();
                memoryCacheAutomater.AutomateCache();
                dataContext.Database.Migrate();
            } else
            {
                app.UseMiddleware<ExceptionMiddleware>();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
