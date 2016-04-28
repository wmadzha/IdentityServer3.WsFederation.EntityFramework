﻿using System.Collections.Generic;
using System.Linq;
using Host.Config;
using IdentityServer3.Core.Configuration;
using IdentityServer3.EntityFramework;
using IdentityServer3.WsFederation.Configuration;
using IdentityServer3.WsFederation.Models;
using IdentityServer3.WsFederation.Services;
using Owin;
using Serilog;
using WsFederationPlugin.EntityFramework;
using WsFederationPlugin.EntityFramework.Extensions;

namespace Host
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Trace(outputTemplate: "{Timestamp} [{Level}] ({Name}){NewLine} {Message}{NewLine}{Exception}")
                .CreateLogger();

            var factory = new IdentityServerServiceFactory()
                .UseInMemoryUsers(Users.Get())
                .UseInMemoryClients(Clients.Get())
                .UseInMemoryScopes(Scopes.Get());

            var options = new IdentityServerOptions
            {
                SiteName = "IdentityServer3 with WS-Federation",
                SigningCertificate = Certificate.Get(),
                Factory = factory,
                PluginConfiguration = ConfigurePlugins
            };
            app.Map("/core", core =>
            {
                core.UseIdentityServer(options);
            });
        }

        private static void ConfigurePlugins(IAppBuilder pluginApp, IdentityServerOptions options)
        {
            var efConfig = new EntityFrameworkServiceOptions
            {
                ConnectionString = "IdSvr3Config"
            };
            
            // pre-populate the test DB from the in-memory config
            ConfigureRelyingParties(RelyingParties.Get(), efConfig);

            var factory = new WsFederationServiceFactory(options.Factory);
            factory.RegisterRelyingPartyService(efConfig);

            var wsFedOptions = new WsFederationPluginOptions(options) {Factory = factory};

            pluginApp.UseWsFederationPlugin(wsFedOptions);
        }

        private static void ConfigureRelyingParties(IEnumerable<RelyingParty> relyingParties, EntityFrameworkServiceOptions options)
        {
            using (var context = new RelyingPartyConfigurationDbContext(options.ConnectionString))
            {
                if (!context.RelyingParties.Any())
                {
                    foreach (var rp in relyingParties)
                    {
                        var e = rp.ToEntity();
                        context.RelyingParties.Add(e);
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}