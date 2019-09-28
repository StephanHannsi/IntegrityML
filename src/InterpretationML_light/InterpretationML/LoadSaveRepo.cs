using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using InterpretationML.Models;

namespace InterpretationML
{
    public class LoadSaveRepo
    {
        private IMongoCollection<TerrestrialRound> _MongoCollectionRounds = null;
        private IMongoCollection<Station> _MongoCollectionStations = null;

        public LoadSaveRepo()
        {
            _MongoCollectionRounds = ConnectToMongo();
            _MongoCollectionStations = ConnectToMongoStations();
            Console.WriteLine("Got Collection");

        }

        private IMongoCollection<TerrestrialRound> ConnectToMongo()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            Console.WriteLine("Connected client");
            var database = client.GetDatabase("IntegrityML");
            Console.WriteLine("Connected db");
            return database.GetCollection<TerrestrialRound>("terrestrial-rounds");
        }

        private IMongoCollection<Station> ConnectToMongoStations()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("IntegrityML");
            return database.GetCollection<Station>("terrestrial-stations");
        }

        public async Task<TerrestrialRound> LoadSingleRoundAsync(string roundId)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s._id, roundId);
            return await _MongoCollectionRounds.Find(filter).SingleAsync();
        }

        public async Task<string> SaveIntegrityData(IntegrityData integrityData, string roundId)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s._id, roundId);
            var update = Builders<TerrestrialRound>.Update.Set(u => u.IntegrityData, integrityData);
            await _MongoCollectionRounds.UpdateOneAsync(filter, update);
            return roundId;
        }

        public async Task<string> SaveLocalCoords(LocalPoint coordinates, string roundId)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s._id, roundId);
            var update = Builders<TerrestrialRound>.Update.Set(u => u.Results.LocalCoordinates[coordinates.PointId], coordinates);
            await _MongoCollectionRounds.UpdateOneAsync(filter, update);
            return roundId;
        }

        public async Task<Station> LoadStationAsync(string stationId)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id, stationId);
            return await _MongoCollectionStations.Find(filter).SingleAsync();
        }

        public async Task<TargetParams> LoadTargetParamsAync(string stationId, string targetId)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id, stationId);
            var doc = await _MongoCollectionStations.Find(filter).FirstOrDefaultAsync();
            if(doc.Targets.ContainsKey(targetId))
            {
                return doc.Targets[targetId];
            }else
            {
                return null;
            }
            
        }

        public async Task<string> UpdateStationAsync(Station station)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id, station._id);
            await _MongoCollectionStations.FindOneAndReplaceAsync(filter, station);
            return station._id;
        }

        public async Task<string> UpdateTerrestrialRound(string stattionId, RoundParams round)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id, stattionId);
            var update = Builders<Station>.Update.Set(s => s.TerrestrialRounds[round.ID], round);
            await _MongoCollectionStations.UpdateOneAsync(filter, update);
            return round.ID;
        }

        public async Task<string> UpdateTargetsAsync(string stationId, Dictionary<string,TargetParams> targets)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id, stationId);
            var update = Builders<Station>.Update.Set(s => s.Targets, targets);
            await _MongoCollectionStations.UpdateOneAsync(filter, update);
            return stationId;
        }

        public async Task<string> SaveTargetAsync(TargetParams target, string stationId)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id, stationId);
            var update = Builders<Station>.Update.Set(u => u.Targets[target.ID], target);
            await _MongoCollectionStations.UpdateOneAsync(filter, update);
            return target.ID;
        }
    }
}
