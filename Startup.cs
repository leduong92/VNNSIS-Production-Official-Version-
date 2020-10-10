
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Certificate;

using VNNSIS.Data;
using VNNSIS.Hubs;
using VNNSIS.Models;
using VNNSIS.Repositories;

namespace VNNSIS
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            //services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme).AddCertificate();
            services.AddControllersWithViews();
            services.AddSignalR();
            services.AddMemoryCache();

            services.AddDbContext<SqlDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("SqlConnection")));
            services.AddDbContext<PostgreDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PostgreConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<SqlDbContext>().AddDefaultTokenProviders();
            
            //Password Strength Setting
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            //Setting the Account Login page
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.LoginPath = "/Account/Login"; // If the LoginPath is not set here, 
                                                      // ASP.NET Core will default to /Account/Login
                options.LogoutPath = "/Account/Logout"; // If the LogoutPath is not set here, 
                                                        // ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Account/AccessDenied"; // If the AccessDeniedPath 
                                 // is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;
            });

            services.AddDistributedMemoryCache();
            services.AddSession();
            //Register the Service
            services.AddTransient<MenuMasterRepository, MenuMasterRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddTransient<InputDefectRepository, InputDefectRepository>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddControllers().AddNewtonsoftJson();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllCors", builder =>
                {
                    builder

                    .WithOrigins("https://localhost:5001")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .SetIsOriginAllowed(delegate (string requestingOrigin)
                    {
                        return true;
                    }).Build();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseCors("AllowAllCors");
            app.UseRouting();

            app.UseAuthentication();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapHub<MoldsHub>("/moldshub");

            });
        }
    }
}
