using Akka.Actor;
using CircuitBreaker.BackgroundTasks;
using CircuitBreaker.Integration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddHostedService<ApiPollingBackgroundTask>();

builder.Services.AddSingleton<HttpClientProxyConfiguration>(_ => new HttpClientProxyConfiguration(2, 15));
builder.Services.AddSingleton<IHttpClientProxy, HttpClientProxy>();

builder.Services.AddSingleton<ActorSystem>(_ => ActorSystem.Create("test"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
