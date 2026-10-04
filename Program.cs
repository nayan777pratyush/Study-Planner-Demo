using _10_project_webiste.Data;
using _10_project_webiste.Services;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
string? dotenvConnectionString = null;

if (builder.Environment.IsDevelopment())
{
    var envFilePath = Path.Combine(builder.Environment.ContentRootPath, ".env.local");
    if (File.Exists(envFilePath))
    {
        DotNetEnv.Env.Load(
            envFilePath,
            new DotNetEnv.LoadOptions(clobberExistingVars: true));
        dotenvConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
    }
}

var rawConnectionString = dotenvConnectionString
    ?? builder.Configuration.GetConnectionString("Neon")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");
var neonConnectionString = ToNpgsqlConnectionString(rawConnectionString);
if (!string.IsNullOrWhiteSpace(neonConnectionString))
{
    builder.Configuration["ConnectionStrings:Neon"] = neonConnectionString;
}

builder.Services.AddRazorPages();
builder.Services.AddScoped<IStudyPlannerService, StudyPlannerService>();
builder.Services.AddDbContext<StudyDbContext>(options =>
    options.UseNpgsql(
        neonConnectionString 
        ?? "Host=localhost;Database=study_sprint;Username=postgres;Password=not-configured"
        ));

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

static string? ToNpgsqlConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString)
        || !Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
        || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        return connectionString;
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2)
    {
        throw new ArgumentException("The PostgreSQL URL must include a username and password.");
    }

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort || uri.Port < 0 ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require
    };

    foreach (var parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
    {
        var parts = parameter.Split('=', 2);
        if (parts.Length != 2)
        {
            continue;
        }

        var name = Uri.UnescapeDataString(parts[0]);
        var value = Uri.UnescapeDataString(parts[1].Replace('+', ' '));
        if (name.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
        {
            builder.SslMode = value.ToLowerInvariant() switch
            {
                "disable" => SslMode.Disable,
                "allow" => SslMode.Allow,
                "prefer" => SslMode.Prefer,
                "require" => SslMode.Require,
                "verify-ca" => SslMode.VerifyCA,
                "verify-full" => SslMode.VerifyFull,
                _ => throw new ArgumentException("The PostgreSQL URL has an unsupported SSL mode.")
            };
        }
        else if (name.Equals("channel_binding", StringComparison.OrdinalIgnoreCase))
        {
            builder.ChannelBinding = value.ToLowerInvariant() switch
            {
                "disable" => ChannelBinding.Disable,
                "prefer" => ChannelBinding.Prefer,
                "require" => ChannelBinding.Require,
                _ => throw new ArgumentException("The PostgreSQL URL has an unsupported channel-binding mode.")
            };
        }
    }

    return builder.ConnectionString;
}
