using Serilog;
using TaskManager.Api;
using TaskManager.Application;
using TaskManager.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddPresentation(builder.Configuration)
        .AddApplication()
        .AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "TaskManager API v1"));
    }
    else
    {
        app.UseHttpsRedirection();
    }

    app.UseCors(TaskManager.Api.DependencyInjection.FrontendCorsPolicy);
    app.MapControllers();

    app.Run();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "La API se detuvo por un error no controlado durante el arranque.");
}
finally
{
    Log.CloseAndFlush();
}
