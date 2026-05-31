using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Ambev.DeveloperEvaluation.Integration.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<WebApi.Program>
{
    // Each factory instance gets its own isolated InMemory database
    private readonly string _dbName = $"IntegrationTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace EF Core PostgreSQL with InMemory
            services.RemoveAll<DbContextOptions<DefaultContext>>();
            services.RemoveAll<DefaultContext>();
            services.AddDbContext<DefaultContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Replace MongoDB context with a no-op stub (tests only hit PostgreSQL)
            services.RemoveAll<MongoDbContext>();
            services.AddSingleton(_ => Substitute.For<MongoDbContext>());

            // Replace CartRepository with a stub (not tested here)
            services.RemoveAll<ICartRepository>();
            services.AddScoped(_ => Substitute.For<ICartRepository>());

            // Ensure schema is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<DefaultContext>().Database.EnsureCreated();
        });
    }
}
