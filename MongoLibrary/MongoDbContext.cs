using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoLibrary.Models;

namespace MongoLibrary {

    public sealed class MongoDbContext {

        private readonly IMongoDatabase _database;

        public IMongoCollection<RoutinizeCoreLog> RoutinizeCoreLogCollection { get; private set; }

        public MongoDbContext(IOptions<MongoDbOptions> options) {
            var connection = new MongoClient(options.Value.Connection);
            _database = connection.GetDatabase(options.Value.Database);
        }

        public void SetRoutinizeCoreLogCollection(string collectionName) {
            RoutinizeCoreLogCollection = _database.GetCollection<RoutinizeCoreLog>($"{ collectionName }");
        }
    }
}