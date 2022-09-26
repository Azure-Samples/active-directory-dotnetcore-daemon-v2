// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace TodoList_WebApi
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(Configuration);

            //// Comment the lines of code above and uncomment the following section if you would like to limit calls to this API to just a set of client apps
            //// The following is an example of extended token validation
            /**
            * The example below can be used do extended token validation and check for additional claims, such as:
            * -check if the caller's tenant is in the allowed tenants list via the 'tid' claim (for multi-tenant applications). 
            *  -check if the caller's account is homed or guest via the 'acct' optional claim
                * -check if the caller belongs to right roles or groups via the 'roles' or 'groups' claim, respectively
                *
                * For more information, visit: https://docs.microsoft.com/azure/active-directory/develop/access-tokens#validate-the-user-has-permission-to-access-this-data
            **/

            /**
             * Also look up Policy-based authorization in ASP.NET Core(https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies)
             */

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddMicrosoftIdentityWebApi(options =>
                  {
                      Configuration.Bind("AzureAd", options);
                      options.Events = new JwtBearerEvents();
                      options.Events.OnTokenValidated = async context =>
                      {
                          string[] allowedClientApps =
                          {
                              /* list of client ids to allow */
                          };
                          string clientappId = context?.Principal?.Claims
                              .FirstOrDefault(x => x.Type == "azp" || x.Type == "appid")?.Value;

                          if (!allowedClientApps.Contains(clientappId))
                          {
                              throw new UnauthorizedAccessException("The client app is not permitted to access this API");
                          }

                          await Task.CompletedTask;
                      };

                  }, options =>
                  {
                      Configuration.Bind("AzureAd", options);
                  });


            // The following flag can be used to get more descriptive errors in development environments
            // Enable diagnostic logging to help with troubleshooting.  For more details, see https://aka.ms/IdentityModel/PII.
            // You might not want to keep this following flag on for production
            IdentityModelEventSource.ShowPII = true;

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // Personal Identifiable Information is not written to the logs by default, to be compliant with GDPR.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
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
