// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BasicTestApp.AuthTest;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebAssembly.Net.Http.HttpClient;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BasicTestApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await SimulateErrorsIfNeededForTest();

            // We want the culture to be en-US so that the tests for bind can work consistently.
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<Index>("root");

            builder.Services.AddWebAssemblyHttpClient(options => options.Credentials = FetchCredentialsOption.Include);
            builder.Services.AddSingleton<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            builder.Services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("NameMustStartWithB", policy =>
                    policy.RequireAssertion(ctx => ctx.User.Identity.Name?.StartsWith("B") ?? false));
            });

            await builder.Build().RunAsync();
        }

        // Supports E2E tests in StartupErrorNotificationTest
        private static async Task SimulateErrorsIfNeededForTest()
        {
            var currentUrl = DefaultWebAssemblyJSRuntime.Instance.Invoke<string>("getCurrentUrl");
            if (currentUrl.Contains("error=sync"))
            {
                throw new InvalidTimeZoneException("This is a synchronous startup exception");
            }

            await Task.Yield();

            if (currentUrl.Contains("error=async"))
            {
                throw new InvalidTimeZoneException("This is an asynchronous startup exception");
            }
        }
    }
}
