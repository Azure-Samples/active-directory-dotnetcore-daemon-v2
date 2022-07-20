### Code in Todo API

The Todo API is built on [ASP.NET Core](https://docs.microsoft.com/aspnet/core/introduction-to-aspnet-core) and is secured using the [Microsoft Identity Web](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) library.

Within the `Program.cs` file you will see the [WebApplicationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.webapplicationbuilder) **builder** which injects all dependencies into your application.

After the **builder** is created, its [services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0) are configured to add [JSON web token validation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0) and decorate the [controllers](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-6.0) with token validation according to the configurations in your `appsettings.json`. This is why the `TenantId` and `ClientId` values must be provided.

```CSharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration);
```

The `TodoService` is then injected which will be discussed in more detail below.

```CSharp
builder.Services.AddSingleton<ITodoService, TodoService>();
```

The `TodoController` is decorated with the [Authorize](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-6.0) attribute which will check that all incoming requests according to JwtBearer-based authentication. The (AddMicrosoftIdentityWebApi)[https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.web.microsoftidentitywebapiauthenticationbuilderextensions.addmicrosoftidentitywebapi?view=azure-dotnet] configures the [Authentication scheme](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0) to validate incoming **JWT's** to ensure that they come from a trusted issuer and that the **TodoList-WebApi** is the intended audience along with checking the **JWT's** expiration date and a couple of other features.

```CSharp
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    // Controller body...
}
```

The `TodoController` then must verify the claims contained within the request to ensure it has sufficient [privileges](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent).

It's often the case that developers will need to access both **delegated** and **application** permissions from the same API. The **Microsoft Identity Library** makes this easy with the [RequiredScopeOrAppPermission](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.web.requiredscopeorapppermissionextensions.requirescopeorapppermission?view=azure-dotnet).

```CSharp
[HttpGet]
[RequiredScopeOrAppPermission(
    RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoReadClaimsKey,
    RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
public IActionResult Get()
{
    if (!Guid.TryParse(HttpContext.User.GetObjectId(), out var userIdentifier))
    {
        return BadRequest();
    }

    return Ok(_todoService.GetTodos(IsAppMakingRequest(), userIdentifier));
}
```

When each **GET** request is routed to this endpoint the retrieved **access token** is first validated to ensure that it has sufficient claims to access this endpoint. These claims could either confer **application permissions** or **delegated permissions** defined in the **appsettings.json**. If neither of these claims exist the request is rejected with a `401` response code.

This API is programmed to execute different behaviors based on whether or not the token contains **application permissions** or **delegated permissions**. When the request has **application permissions** this endpoint will return all **todo's** found within its store. When the request has **delegated permissions** it will only return **todo's** associated with the user that's signed in.

To confirm that the access token was issued by an applicaiton, and *not* from a user login flow, the **idtyp** optional claim is added. When an access token is acquired from **Azure** *only* access tokens acquired via an application flow will contain this claim. All access token attained via a user sign in flow *will not* have this claim.

The code below shows how the token is checked to confirm the claim exists.

```csharp
private bool IsAppMakingRequest()
{
    // Add in the optional 'idtyp' claim to check if the access token is coming from an application or user.
    //
    // See: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-optional-claims
    return HttpContext.User
        .Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
}
```

The `TodoService` is responsible for handling the data management of **Todo's**. **Todo's** are stored in a [thread-safe collection](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/) to allow for multi-threaded access. You can see the [concurrent dictionary documentation](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-add-and-remove-items) for further information.

```csharp
public class TodoService : ITodoService
{
    private ConcurrentDictionary<Guid, Todo> _todoStore = new ConcurrentDictionary<Guid, Todo>();

    // Rest of service...
}
```

Each method within the `TodoService` is designed to do a simple create, read, update or delete action on **Todo's** contained in the `_todoStore`. Each method also contains a `hasAppPermissions` parameter to check if the call has applicatoin permissions. If this is true the methods are written to allow access to all **Todo's** in the `_todoStore`. If this is false, the method will only interact with **Todo's** that have a `UserId` matching the `userIdentifier` parameter passed into the method.

The `GetTodo` method provides an illustrative example.

```csharp
public Todo GetTodo(bool hasAppPermissions, Guid id, Guid userIdentifier)
{
    if (hasAppPermissions)
    {
        _todoStore.TryGetValue(id, out var todo);
        return todo;
    }

    var usersTodos = _todoStore
        .Where(td => td.Value.UserId == userIdentifier)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    usersTodos.TryGetValue(id, out var userTodo);

    return userTodo;

}
```

Next, the initial scopes are extracted from the `appsettings.json` file from within the `DownstreamApi` object. These scopes will be used in the access token stored within a cache within your server and also be contained within the token which will be retrieved by the SPA after it exchanges its *access code*. The server-side token cache will be cleared out for users after they sign-out.

### Code in the daemon-console

The **daemon-console** app is a simple [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/introduction) app that acquires access tokens from Azure to make simple HTTP calls to the **TodoList-WebApi** code sample to perform basic CRUD operations.

The application makes use of [dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0) to create some simple services to perform the authorization, HTTP requests and print out the data received from the API.

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

services.AddOptions<AzureAdOptions>()
    .Configure(azureAdOptions =>
        configuration.GetSection(AzureAdOptions.AzureAd).Bind(azureAdOptions));

services.AddOptions<DownstreamApiOptions>()
    .Configure(downstreamApiOptions =>
        configuration.GetSection(DownstreamApiOptions.DownstreamApi).Bind(downstreamApiOptions));

services.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();
services.AddSingleton<ITodoService, TodoService>();
services.AddSingleton<IUploadTodosService, UploadTodosService>();
services.AddSingleton<IDataDisplayService, DataDisplayService>();

var serviceProvider = services.BuildServiceProvider();

var todoService = serviceProvider.GetService<ITodoService>();
var uploadTodosService = serviceProvider.GetService<IUploadTodosService>();
var dataDisplayService = serviceProvider.GetService<IDataDisplayService>();
```

The `ConfidentialClientApplicationService` is responsible for creating the [ConfidentialClientApplication](https://docs.microsoft.com/en-us/python/api/msal/msal.application.confidentialclientapplication?view=azure-python) that will be used throughout the application. The primary function of the service is to extract the configuration settings from the `appsettings.json` file and pass them into the generated `ConfidentialClientApplication`.

```csharp
private AzureAdOptions _azureAdOptions;
private DownstreamApiOptions _downStreamApiOptions;

public ConfidentialClientApplicationService(IOptions<AzureAdOptions> azureAdOptions, IOptions<DownstreamApiOptions> downStreamApiOptions)
{
    _azureAdOptions = azureAdOptions.Value;
    _downStreamApiOptions = downStreamApiOptions.Value;
}

private IConfidentialClientApplication _confidentialClientApplication;
private IConfidentialClientApplication ConfidentialClientApplication
{
    get
    {
        if (_confidentialClientApplication is null)
        {

            var clientSecretPlaceholderValue = "[Enter here a client secret for your application]";

            if (!string.IsNullOrWhiteSpace(_azureAdOptions.ClientSecret) &&
                _azureAdOptions.ClientSecret != clientSecretPlaceholderValue)
            {
                _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                    .WithAuthority(new Uri(_azureAdOptions.Authority))
                    .WithClientSecret(_azureAdOptions.ClientSecret)
                    .Build();
            }
            else if (_azureAdOptions.ClientCertificates.Any())
            {
                ICertificateLoader certificateLoader = new DefaultCertificateLoader();
                certificateLoader.LoadIfNeeded(_azureAdOptions.ClientCertificates.First());

                _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                    .WithAuthority(new Uri(_azureAdOptions.Authority))
                    .WithCertificate(_azureAdOptions.ClientCertificates.First().Certificate)
                    .Build();
            }
            else
            {
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
            }

        }

        return _confidentialClientApplication;
    }
}
```

The `GetAuthenticationResultAsync` calls the `ConfidentialClientApplication` to make a request to Azure and retrieve an access token. The `ConfidentialClientApplication` is automatically configured to use a cache if a token has already been retrieved.

```csharp
public async Task<AuthenticationResult> GetAuthenticationResultAsync()
{
    if (string.IsNullOrEmpty(_downStreamApiOptions.Scopes))
    {
        throw new Exception("'Scopes' must be set in the 'DownStreamApi' of appsettings.json file.");
    }

    var scopes = _downStreamApiOptions.Scopes.Split(' ');

    var authenticationResult = await ConfidentialClientApplication
        .AcquireTokenForClient(scopes)
        .ExecuteAsync();

    return authenticationResult;
}
```

The `TodoService` shows how the `ConfidentialClientApplicationService` can be leveraged to retrieve tokens from the API after receiving an **access token**.

```csharp
private IConfidentialClientApplicationService _confidentialClientApplicationService;
private DownstreamApiOptions _downStreamApiOptions;

public TodoService(
    IConfidentialClientApplicationService confidentialClientApplicationService,
    IOptions<AzureAdOptions> azureAdOptions,
    IOptions<DownstreamApiOptions> downStreamApiOptions)
{
    _confidentialClientApplicationService = confidentialClientApplicationService;
    _downStreamApiOptions = downStreamApiOptions.Value;
}

public async Task<Guid> AddAsync(Todo todo)
{
    var httpClient = await PrepareHttpClientAsync();
    var response = await httpClient.PostAsJsonAsync($"{_downStreamApiOptions.BaseUrl}api/todo", todo);

    if (response.IsSuccessStatusCode)
    {
        var todoIdResponse = (await response.Content.ReadAsStringAsync()).Trim('"');

        if (Guid.TryParse(todoIdResponse, out var todoId))
        {
            return todoId;
        }
    }

    throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
}

// More code...

private async Task<HttpClient> PrepareHttpClientAsync()
{
    var authenticationResult = await _confidentialClientApplicationService.GetAuthenticationResultAsync();

    var httpClient = new HttpClient();
    var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

    if (defaultRequestHeaders.Accept is null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
    {
        httpClient.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

    return httpClient;
}
```
