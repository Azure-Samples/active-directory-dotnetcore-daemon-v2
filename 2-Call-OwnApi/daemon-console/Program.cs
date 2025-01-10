using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.Collections.Generic;
using System;
using System.Linq;
using TodoList_WebApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

#region Setup
// Get the Token acquirer factory instance. By default it reads an appsettings.json
// file if it exists in the same folder as the app (make sure that the 
// "Copy to Output Directory" property of the appsettings.json file is "Copy if newer").
var tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

// Add console logging or other services if you wish
tokenAcquirerFactory.Services.AddLogging(
    (loggingBuilder) => loggingBuilder.SetMinimumLevel(LogLevel.Warning)
                                      .AddConsole()
                                        );

// Create a downstream API service named 'MyApi' which comes loaded with several
// utility methods to make HTTP calls to the DownstreamApi configurations found
// in the "MyWebApi" section of your appsettings.json file.
tokenAcquirerFactory.Services.AddDownstreamApi("MyApi",
    tokenAcquirerFactory.Configuration.GetSection("MyWebApi"));
var sp = tokenAcquirerFactory.Build();


#endregion

#region Config json (can be programatic too)
/*
{
    "AzureAd": {
        "Instance": "https://login.microsoftonline.com/",
		"TenantId": "[Enter here the tenantID or domain name for your Azure AD tenant]",
		"ClientId": "[Enter here the ClientId for your application]",
		"ClientCredentials": [

            {
            "SourceType": "ClientSecret",
				"ClientSecret": "[Enter here a client secret for your application]"

            }
		]
	},

	"MyWebApi": {
        "BaseUrl": "https://localhost:44372/",
		"RelativePath": "api/TodoList",
		"RequestAppToken": true,
		"Scopes": ["[Enter here the scopes for your web API]"]  // . E.g. 'api://<API_APPLICATION_ID>/.default'

    }
}
*/
#endregion

#region Token Acquisition API - gets auth headrs, similar to MSAL

ITokenAcquirer acquirer = sp.GetRequiredService<ITokenAcquirer>();
AcquireTokenResult result = await acquirer.GetTokenForAppAsync("https://graph.microsoft.com/.default");


#endregion

#region Downstream API  - uses TokenAcquisition to make HTTP calls to resources, handles CAE etc.

IDownstreamApi downstreamApi = sp.GetRequiredService<IDownstreamApi>();



HttpResponseMessage apiResult = await downstreamApi.CallApiForAppAsync("MyWebApi");

#endregion