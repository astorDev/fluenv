var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(c => { c.SingleLine = true; c.IncludeScopes = false; });

var app = builder.Build();

app.Logger.LogInformation("Playing guitar: {guitar}, drums: {drums}", 
    builder.Configuration["Guitar"], 
    builder.Configuration["Drums"]
);

app.MapGet("/", () => "Hello World!");

app.Run();