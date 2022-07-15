using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using TodoList_WebApp.Options;
using TodoList_WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<DownstreamApiOptions>()
    .Configure(downstreamApiOptions =>
        builder.Configuration.GetSection(DownstreamApiOptions.DownstreamApi).Bind(downstreamApiOptions));

var downstreamApiOptions = builder.Configuration.GetSection(DownstreamApiOptions.DownstreamApi)
    .Get<DownstreamApiOptions>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(downstreamApiOptions.Scopes!.Split(' '))
    .AddInMemoryTokenCaches();

builder.Services.AddHttpClient<ITodoService, TodoService>(httpClient =>
{
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

app.Run();
