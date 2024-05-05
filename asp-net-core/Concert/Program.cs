using Fluenv;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddFluentEnvironmentVariables();

var app = builder.Build();

app.Logger.LogInformation("{guitarist} playing {guitar}",
    builder.Configuration["Band:LeadGuitarist"],
    builder.Configuration["Guitar"]
);

app.Run();
