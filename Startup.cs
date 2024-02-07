/*
 * File: Startup.cs
 * Date: October 11, 2023
 * Description: This file defines the Startup class responsible for configuring the application.
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TravelAgency.Models;
using TravelAgency.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System;

namespace TravelAgency
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
            // Configure CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3000") // Adjust this if your client app has a different origin
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                    });
            });

            // Configure MongoDB settings
            services.Configure<DatabaseSettings>(
                Configuration.GetSection(nameof(DatabaseSettings)));

            services.AddSingleton<IDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

            // Add services
            services.AddSingleton<UserService>();
            services.AddSingleton<TrainServices>();
            services.AddSingleton<TicketService>();
            services.AddSingleton<TravelerService>();

            // Add controllers
            services.AddControllers();

            // JWT configuration
            var jwtSettings = Configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettings);
            Console.WriteLine($"Key: {jwtSettings.Key}");

            var jwtKey = jwtSettings?["Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration");
            var key = Encoding.ASCII.GetBytes(jwtKey);

            // Configure JWT authentication
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(token =>
            {
                token.RequireHttpsMetadata = false;
                token.SaveToken = true;
                token.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Get<JwtSettings>().Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Get<JwtSettings>().Audience,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use CORS policy
            app.UseCors("AllowSpecificOrigin");

            // Print MongoDB Connection String
            var connectionString = Configuration["DatabaseSettings:ConnectionString"];
            Console.WriteLine("MongoDB Connection String:");
            Console.WriteLine(connectionString);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
