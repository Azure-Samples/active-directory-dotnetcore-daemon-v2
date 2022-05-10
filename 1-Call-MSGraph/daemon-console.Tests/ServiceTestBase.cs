using System;
using System.Threading.Tasks;
using daemon_console.Options;
using daemon_console.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;

namespace daemon_console.Tests;

public abstract class ServiceTestBase
{
    protected WebApplicationBuilder? _builder;
    protected WebApplicationBuilder Builder
    {
        get
        {
            if (_builder is null)
            {
                _builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();

                _builder.Services.Configure<AzureAdOptions>(
                    _builder.Configuration.GetSection(AzureAdOptions.AzureAd));

                _builder.Services.Configure<DownstreamApiOptions>(
                    _builder.Configuration.GetSection(DownstreamApiOptions.DownstreamApi));
            }

            return _builder;
        }
    }

    [SetUp]
    public void Setup()
    {
        // Reset the builder for each test.
        _builder = null;
    }
}
