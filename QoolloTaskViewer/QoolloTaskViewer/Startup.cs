using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QoolloTaskViewer.Db;
using QoolloTaskViewer.Db.Repositories;
using QoolloTaskViewer.Db.Repositories.Implementation;
using QoolloTaskViewer.Models;
using QoolloTaskViewer.ApiServices.Enums;

namespace QoolloTaskViewer
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
            services.AddDbContextPool<QoolloTaskViewerContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<UserModel, IdentityRole>()
                .AddEntityFrameworkStores<QoolloTaskViewerContext>();

            services.AddTransient<IUsersRepository, EFUsersRepository>();
            services.AddTransient<IDomainsRepository, EFDomainsRepository>();
            services.AddTransient<IServicesRepository, EFServicesRepository>();
            services.AddTransient<ITokensRepository, EFTokensRepository>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/login");
                options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Account/login");
            });

            services.AddControllersWithViews();
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            InitializeDatabase(app).Wait();
        }

        private async Task InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var servicesRepository = serviceScope.ServiceProvider.GetService<IServicesRepository>();
                var domainsRepository = serviceScope.ServiceProvider.GetService<IDomainsRepository>();

                foreach ((string domain, ServiceType type) in new List<(string, ServiceType)>() {
                        ( "github.com", ServiceType.GitHub ),
                        ( "gitlab.com", ServiceType.GitLab ),
                        ( "jira.atlassian.com", ServiceType.Jira ),
                }) {
                    if (await servicesRepository.FindServiceByDomainAsync(domain) == null)
                    {
                        await servicesRepository.AddServiceAsync(new ServiceModel
                        {
                            Id = new Guid(),
                            DomainId = (await domainsRepository.FindDomainByNameAsync(domain)).Id,
                            Type = type,
                        });
                    }
                }
            }
        }
    }
}
