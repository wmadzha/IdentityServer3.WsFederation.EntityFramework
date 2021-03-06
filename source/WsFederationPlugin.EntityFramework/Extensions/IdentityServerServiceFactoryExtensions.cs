﻿/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Linq;
using IdentityServer3.Core.Configuration;
using IdentityServer3.EntityFramework;
using IdentityServer3.WsFederation.EntityFramework;
using IdentityServer3.WsFederation.Services;

namespace IdentityServer3.WsFederation.Configuration
{
    public static class WsFederationServiceFactoryExtensions
    {
        public static void RegisterRelyingPartyService(this WsFederationServiceFactory factory,
            EntityFrameworkServiceOptions options)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (options == null) throw new ArgumentNullException("options");

            if (options.SynchronousReads)
            {
                factory.Register(new Registration<EntityFrameworkServiceOptions>(options));
            }

            factory.Register(
                new Registration<IRelyingPartyConfigurationDbContext>(
                    resolver => new RelyingPartyConfigurationDbContext(options.ConnectionString, options.Schema)));
            factory.RelyingPartyService = new Registration<IRelyingPartyService, RelyingPartyService>();
        }
    }
}