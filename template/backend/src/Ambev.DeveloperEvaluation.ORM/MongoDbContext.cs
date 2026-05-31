using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.ORM;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoConnection")
            ?? "mongodb://developer:ev%40luAt10n@localhost:27017";

        var mongoUrl = new MongoUrl(connectionString);
        var client = new MongoClient(mongoUrl);
        _database = client.GetDatabase(mongoUrl.DatabaseName ?? "developer_evaluation");
    }

    public IMongoCollection<Cart> Carts =>
        _database.GetCollection<Cart>("carts");
}
