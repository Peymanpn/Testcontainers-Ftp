using FtpPlayground;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IFtpService, FtpService>();

var app = builder.Build();

app.MapGet("/", () =>
{
    return Results.Ok("Hey There!");
});

app.MapPost("/ftp/test", async (IFtpService ftpService, FtpConnectionDetails connectionDetails) =>
{
    var (isSuccess, errorMessage) = await ftpService.TestConnection(connectionDetails);
    if (isSuccess)
    {
        return Results.Ok("FTP connection successful.");
    }
    else
    {
        return Results.Problem($"FTP connection failed: {errorMessage}");
    }
});

await app.RunAsync();

#pragma warning disable S1118 // Intentional, needed for integration tests
public partial class Program { }
#pragma warning restore S1118 // Intentional, needed for integration tests
