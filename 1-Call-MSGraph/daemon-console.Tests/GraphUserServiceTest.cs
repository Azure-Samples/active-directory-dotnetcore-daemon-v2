using System;
using System.Threading.Tasks;
using daemon_console.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Moq;
using NUnit.Framework;

namespace daemon_console.Tests;

public class GraphUserServiceTest: ServiceTestBase
{
    [Test]
    public async Task GetAllUserData_InvalidClientSecret_ThrowsProperException()
    {
        var confidentialClientApplicationService = new Mock<IConfidentialClientApplicationService>();
        confidentialClientApplicationService
            .Setup(ccas => ccas.GetAuthenticationResultAsync())
            .Throws(new MsalServiceException("invalid_client", "AADSTS7000215: Invalid client secret is provided"));

        Builder.Services.AddSingleton<IConfidentialClientApplicationService>(confidentialClientApplicationService.Object);
        Builder.Services.AddSingleton<IGraphUserService, GraphUserService>();

        var app = Builder.Build();

        var graphUserService = app.Services.GetService<IGraphUserService>();

        if (graphUserService is null)
        {
            Assert.Fail("Unable to retreive the GraphUserService");
            return;
        }

        try
        {
            await graphUserService.GetAllUserData();
        }
        catch (Exception e)
        {
            Assert.AreEqual("Incorrect client secret provided. Check the appsettings.json file.", e.Message);
            return;
        }

        Assert.Fail("No exception was thrown.");
    }
}
