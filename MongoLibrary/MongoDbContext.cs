using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoLibrary.Models;

namespace MongoLibrary {

    public sealed class MongoDbContext {

        private readonly IMongoDatabase _database;

        public IMongoCollection<RoutinizeCoreLog> RoutinizeCoreLogCollection { get; private set; }
        
        public IMongoCollection<GenericLog> RoutinizeAccountLogCollection { get; private set; }
        
        public IMongoCollection<GenericLog> RoutinizeCooperationLogCollection { get; private set; }

        public MongoDbContext(IOptions<MongoDbOptions> options) {
            var connection = new MongoClient(options.Value.Connection);
            _database = connection.GetDatabase(options.Value.Database);
        }

        public void SetRoutinizeCoreLogCollection(string collectionName) {
            RoutinizeCoreLogCollection = _database.GetCollection<RoutinizeCoreLog>($"{ collectionName }");
        }

        public void SetRoutinizeDataLogCollection(string collectionName) {
            RoutinizeAccountLogCollection = _database.GetCollection<GenericLog>($"{ collectionName }");
        }

        public void SetRoutinizeCooperationLogCollection(string collectionName) {
            RoutinizeCooperationLogCollection = _database.GetCollection<GenericLog>($"{ collectionName }");
        }
    }
}