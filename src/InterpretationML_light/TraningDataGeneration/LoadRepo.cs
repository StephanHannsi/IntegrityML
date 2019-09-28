using System;
using System.Collections.Generic;
using System.Text;
using InterpretationML.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace TraningDataGeneration
{
    public class LoadRepo
    {
        private IMongoCollection<BsonDocument> _MongoCollection = null;

        public LoadRepo()
        {
            _MongoCollection = ConnectToMongo();
            Console.WriteLine("Got Collection");
        }

        private IMongoCollection<BsonDocument> ConnectToMongo()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            Console.WriteLine("Connected client");
            var database = client.GetDatabase("IntegrityML");
            Console.WriteLine("Connected db");
            return database.GetCollection<BsonDocument>("terrestrial-rounds");
        }

        public async Task<TerrestrialRound[]>GetTerrestrialRoundsByStation(string stationId)
        {
            BsonDocument filter = new BsonDocument("StationId", stationId);
            var docList = (await _MongoCollection.FindAsync(filter)).ToList();
            List<TerrestrialRound> rounds = new List<TerrestrialRound>();
            foreach (var doc in docList)
            {
                rounds.Add(BsonSerializer.Deserialize<TerrestrialRound>(doc));
            }
            return rounds.ToArray();
        }
    }
}
