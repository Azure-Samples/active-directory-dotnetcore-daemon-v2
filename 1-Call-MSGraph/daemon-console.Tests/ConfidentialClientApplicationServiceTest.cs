using System;
using System.Threading.Tasks;
using daemon_console.Services;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace daemon_console.Tests;

public class ConfidentialClientApplicationServiceTest : ServiceTestBase
{
    [Test]
    public async Task GetAuthenticationResultAsync_NoSecretAndNoCertificateSet_ThrowsExpectedException()
    {
        Builder.Configuration["AzureAd:ClientSecret"] = "";
        Builder.Configuration["AzureAd:Certificate"] = null;
        Builder.Services.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();

        var app = Builder.Build();

        var confidentialClientApplicationService = app.Services.GetService<IConfidentialClientApplicationService>();

        if (confidentialClientApplicationService is null)
        {
            Assert.Fail("Unable to retrieve the ConfidentialClientApplicationService");
            return;
        }

        try
        {
            await confidentialClientApplicationService.GetAuthenticationResultAsync();
        }
        catch (Exception e)
        {
            Assert.AreEqual("You must choose between using client secret or certificate. Please update appsettings.json file.", e.Message);
            return;
        }

        Assert.Fail("No exception was thrown.");
    }

    [Test]
    public async Task GetAuthenticationResultAsync_NoScopesSet_ThrowsExpectedException()
    {
        Builder.Configuration["DownStreamApi:Scopes"] = "";
        Builder.Services.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();

        var app = Builder.Build();

        var confidentialClientApplicationService = app.Services.GetService<IConfidentialClientApplicationService>();

        if (confidentialClientApplicationService is null)
        {
            Assert.Fail("Unable to retrieve the ConfidentialClientApplicationService");
            return;
        }

        try
        {
            await confidentialClientApplicationService.GetAuthenticationResultAsync();
        }
        catch (Exception e)
        {
            Assert.AreEqual("'Scopes' must be set in the 'DownStreamApi' of appsettings.json file.", e.Message);
            return;
        }

        Assert.Fail("No exception was thrown.");
    }
}
