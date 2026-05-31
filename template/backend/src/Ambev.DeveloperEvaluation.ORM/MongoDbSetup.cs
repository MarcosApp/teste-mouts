using Ambev.DeveloperEvaluation.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Ambev.DeveloperEvaluation.ORM;

public static class MongoDbSetup
{
    private static bool _registered;

    public static void RegisterConventions()
    {
        if (_registered) return;
        _registered = true;

        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("DeveloperEvaluation", pack, _ => true);

        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

        if (!BsonClassMap.IsClassMapRegistered(typeof(Cart)))
        {
            BsonClassMap.RegisterClassMap<Cart>(map =>
            {
                map.AutoMap();
                map.MapIdMember(c => c.Id)
                   .SetSerializer(new GuidSerializer(BsonType.String));
            });
        }
    }
}
