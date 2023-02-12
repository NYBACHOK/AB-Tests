using AbTests.Api;
using AbTests.Api.Accessors;
using AbTests.Api.Accessors.Interfaces;
using AbTests.Api.Extensions;
using AbTests.Api.Helpers;
using AbTests.Api.Managers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

#region Services

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

#endregion Services

#region Container

builder.Host.ConfigureContainer<ContainerBuilder>(_ =>
{
    _.RegisterType<SqlAccessor>()
        .WithParameter(new NamedParameter("connectionString", EnvironmentVariables.DbConnectionString))
        .AsSelf()
        .AsImplementedInterfaces();

    _.RegisterType<ApiManager>().AsSelf();

    _.RegisterType<RandomHelper>()
        .AsSelf()
        .AsImplementedInterfaces();

    _.RegisterType<CacheAccessor>()
        .AsSelf()
        .AsImplementedInterfaces();
});

#endregion Container

var app = builder.Build();

// Configure the HTTP request pipeline.

#region Middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.AddRoutes(); //Defined endpoints

#endregion Middleware

app.Run();
