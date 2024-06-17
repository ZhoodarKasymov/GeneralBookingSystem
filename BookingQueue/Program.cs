using System.Data;
using System.Globalization;
using System.Net;
using System.Reflection;
using AspNetCore.ReCaptcha;
using BookingQueue.BLL.DapperMappers;
using BookingQueue.BLL.Resources;
using BookingQueue.BLL.Services;
using BookingQueue.BLL.Services.Interfaces;
using BookingQueue.Common.Constants;
using BookingQueue.DAL.GenericRepository;
using BookingQueue.Middleware;
using BookingQueue.Services;
using Dapper.FluentMap;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.OpenApi.Models;
using MySql.Data.MySqlClient;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<Func<string, IDbConnection>>(sp => key =>
{
    var connectionFactory = sp.GetRequiredService<IDbConnectionFactory>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var configuration = sp.GetRequiredService<IConfiguration>();

    switch (key)
    {
        case DatabaseConstants.SessionBased:
            var session = httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                var selectedDatabase = session.GetString("SelectedDatabase");
                if (!string.IsNullOrEmpty(selectedDatabase))
                {
                    return connectionFactory.CreateConnection(selectedDatabase);
                }
            }

            httpContextAccessor.HttpContext?.Response.Redirect("/");
            return null!;
        case DatabaseConstants.Default:
            var connectionString = configuration["ConnectionStrings:DefaultConnection"];
            return new MySqlConnection(connectionString);
        default:
            throw new ArgumentException($"Service with key '{key}' is not registered.");
    }
});

builder.Services.AddReCaptcha(builder.Configuration.GetSection("GoogleReCaptcha"));
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<LocService>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddTransient<IServicesService, ServicesService>();
builder.Services.AddTransient<IAdvanceService, AdvanceService>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ICompanyService, CompanyService>();
builder.Services.AddTransient(typeof(FileUploadService));

FluentMapper.Initialize(cfg =>
{
    cfg.AddMap(new UserMap());
    cfg.AddMap(new CompanyMap());
    cfg.AddMap(new BranchMap());
});

builder.Services.AddRazorPages();
builder.Services.AddMvc()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
        {
            var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName);
            return factory.Create("SharedResource", assemblyName.Name);
        };
    });

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
    });

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "General Booking System API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.DocumentFilter<ApiControllerOperationFilter>();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configure log4net
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Add global exception handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "text/html";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var logger = LogManager.GetLogger(exceptionHandlerPathFeature?.Path);
        logger.Error("An error occurred.", exception);

        if (exceptionHandlerPathFeature?.Error is NullReferenceException)
        {
            context.Response.Redirect("/");
            return;
        }

        await context.Response.WriteAsync(
            exception?.Message ?? "An error occurred. Please try again later.");
    });
});

var supportedCultures = new[]
{
    new CultureInfo("ru"),
    new CultureInfo("uk"),
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ru", "ru"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RoleBasedAuthorizationMiddleware>();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Branch}/{action=Index}/{id?}");


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Q Solution API V1");
});

app.Run();

public class ApiControllerOperationFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Get all API controller names
        var apiControllerNames = context.ApiDescriptions
            .Where(apiDesc => apiDesc.ActionDescriptor.RouteValues["controller"] != null &&
                              apiDesc.ActionDescriptor.EndpointMetadata.Any(em => em.GetType().Name == "ApiControllerAttribute"))
            .Select(apiDesc => apiDesc.ActionDescriptor.RouteValues["controller"]?.ToLower())
            .Distinct()
            .ToList();

        // Remove non-API controller paths from the swagger document
        foreach (var path in swaggerDoc.Paths.ToList())
        {
            var controllerName = path.Key.Split('/')[1];
            if (!apiControllerNames.Contains(controllerName))
            {
                swaggerDoc.Paths.Remove(path.Key);
            }
        }
    }
}
