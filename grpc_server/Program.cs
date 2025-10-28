using FluentValidation;
using grpc_server.Interceptors;
using grpc_server.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ValidationInterceptor>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<DemoService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
