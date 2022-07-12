using System;
using System.IO;
using System.Threading.Tasks;
using daemon_console.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Moq;
using NUnit.Framework;
using Newtonsoft.Json;

namespace daemon_console.Tests;

public class DataDisplayServiceTest : ServiceTestBase
{
    [Test]
    public async Task DisplayAllUsers_WithUserData_PrintsProperMessage()
    {
        using var sampleUserResponse = System.IO.File.OpenText(@"./sample-responses/sample-users-response.json");
        using var jsonReader = new JsonTextReader(sampleUserResponse);

        var serializer = new JsonSerializer();
        var userData = serializer.Deserialize<GraphServiceUsersCollectionPage>(jsonReader);

        if (userData is null)
        {
            Assert.Fail("Unable to retrieve sample response.");
            return;
        }

        var graphUserService = new Mock<IGraphUserService>();

        graphUserService
            .Setup(gus => gus.GetAllUserData())
            .Returns(Task<IGraphServiceUsersCollectionPage>.FromResult((IGraphServiceUsersCollectionPage) userData));

        Builder.Services.AddSingleton<IGraphUserService>(graphUserService.Object);
        Builder.Services.AddSingleton<IDataDisplayService, DataDisplayService>();

        var app = Builder.Build();

        var dataDisplayService = app.Services.GetService<IDataDisplayService>();

        if (dataDisplayService is null)
        {
            Assert.Fail("Unable to retreive the DataDisplayService");
            return;
        }

        var stringWriter = new StringWriter();

        Console.SetOut(stringWriter);

        await dataDisplayService.DisplayAllUsersAsync();

        var formattedJson = JsonConvert.SerializeObject(userData, Formatting.Indented, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        var expectedOutput = @$"Found {userData.Count} users in tenant.
{formattedJson}
";

        Assert.AreEqual(expectedOutput, stringWriter.ToString());
    }

    [Test]
    public async Task DisplayAllUsers_InsufficientScopes_ThrowsProperException()
    {
        var graphUserService = new Mock<IGraphUserService>();

        graphUserService
            .Setup(gus => gus.GetAllUserData())
            .Throws(new ServiceException(new Error()
            {
                Code = "Authorization_RequestDenied",
            }));

        Builder.Services.AddSingleton<IGraphUserService>(graphUserService.Object);
        Builder.Services.AddSingleton<IDataDisplayService, DataDisplayService>();

        var app = Builder.Build();

        var dataDisplayService = app.Services.GetService<IDataDisplayService>();

        if (dataDisplayService is null)
        {
            Assert.Fail("Unable to retreive the DataDisplayService");
            return;
        }

        try
        {
            await dataDisplayService.DisplayAllUsersAsync();
        }
        catch (Exception e)
        {
            Assert.AreEqual("Application has insufficient privileges to access user data. Be sure to add 'User.Read.All' delegated permission to your application in the Azure Portal.", e.Message);
            return;
        }

        Assert.Fail("No exception was thrown.");
    }
}
