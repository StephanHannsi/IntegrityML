using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Calculation.Models;
using System.Linq;

namespace Calculation
{
    public class LoadSaveRepo
    {
        private IMongoCollection<TerrestrialRound> _MongoCollectionRounds = null;
        private IMongoCollection<Station> _MongoCollectionStations = null;

        public LoadSaveRepo()
        {
            _MongoCollectionRounds = ConnectToMongoRounds();
            _MongoCollectionStations = ConnectToMongoStations();
            Console.WriteLine("Got Collection");

        }

        private IMongoCollection<Station> ConnectToMongoStations()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("IntegrityML");
            return database.GetCollection<Station>("terrestrial-stations");
        }

        private IMongoCollection<TerrestrialRound> ConnectToMongoRounds()
        {
            MongoClient client = new MongoClient("mongodb://localhost:27017");
            Console.WriteLine("Connected client");
            var database = client.GetDatabase("IntegrityML");
            Console.WriteLine("Connected db");
            return database.GetCollection<TerrestrialRound>("terrestrial-rounds");
        }

        public async Task<string> SaveStationRecordAsync(TerrestrialRoundMessage message)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s._id, message.RoundId);
            if(!(await _MongoCollectionRounds.CountDocumentsAsync(filter) > 0))
            {
                TerrestrialRound document = new TerrestrialRound{ };
                document = MapRound(message);
                await _MongoCollectionRounds.InsertOneAsync(document);
                return message.RoundId;
            }
            return null;
        }

        public async Task<string> SaveObservationAsync(TerrestrialRoundMessage message)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s._id, message.RoundId);
            TerrestrialRound round = await _MongoCollectionRounds.Find(filter).SingleAsync();
            foreach (var obj in message.Observations)
            {
                if (round.Observations == null)
                {
                    Dictionary<string, Target> dict = new Dictionary<string, Target>();
                    Target target = new Target { };
                    dict.Add(obj.PointId, target);
                    switch (obj.ObservationType)
                    {
                        case "Face1":
                            if (obj.EdmDistance != 0)
                            {
                                dict[obj.PointId].Face1 = obj;
                            }
                            else
                            {
                                dict[obj.PointId].Face1 = null;
                            }
                            break;
                        case "Face2":
                            if (obj.EdmDistance != 0)
                            {
                                dict[obj.PointId].Face2 = obj;
                            }
                            else
                            {
                                dict[obj.PointId].Face2 = null;
                            }
                            break;
                        default:
                            Console.WriteLine("unknown ObservationType: " + obj.ObservationType);
                            break;
                    }
                    round.Observations = dict;
                }
                else
                {
                    if (round.Observations.ContainsKey(obj.PointId))
                    {
                        switch (obj.ObservationType)
                        {
                            case "Face1":
                                round.Observations[obj.PointId].Face1 = obj;
                                break;
                            case "Face2":
                                round.Observations[obj.PointId].Face2 = obj;
                                break;
                            default:
                                Console.WriteLine("unknown ObservationType: " + obj.ObservationType);
                                break;
                        }
                        round.Observations[obj.PointId].MeanF1F2 = null;
                    }
                    else
                    {
                        Target target = new Target { };
                        round.Observations.Add(obj.PointId, target);
                        switch (obj.ObservationType)
                        {
                            case "Face1":
                                round.Observations[obj.PointId].Face1 = obj;
                                break;
                            case "Face2":
                                round.Observations[obj.PointId].Face2 = obj;
                                break;
                            default:
                                Console.WriteLine("unknown ObservationType: " + obj.ObservationType);
                                break;
                        }
                    }
                }
            }
            var update = Builders<TerrestrialRound>.Update.Set(s => s.Observations, round.Observations);
            await _MongoCollectionRounds.UpdateOneAsync(filter, update);
            return message.RoundId;
        }

        public async Task<string> SaveMeanAsync(TerrestrialObservation observation, string roundId)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s._id, roundId);
            var update = Builders<TerrestrialRound>.Update.Set(u => u.Observations[observation.PointId].MeanF1F2, observation);
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

        public async Task<string> SaveRoundEndAsync(TerrestrialRoundMessage message)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s._id, message.RoundId);
            var update = Builders<TerrestrialRound>.Update.Set(s => s.RoundEndUtc, message.RoundEndUtc);
            await _MongoCollectionRounds.UpdateOneAsync(filter, update);
            return message.RoundId;
        }

        public async Task<string> UpdateRefRound(string roundId, string refround)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s._id, roundId);
            var update = Builders<TerrestrialRound>.Update.Set(s => s.RefRound, refround);
            await _MongoCollectionRounds.UpdateOneAsync(filter, update);
            return roundId;
        }

        private TerrestrialRound MapRound(TerrestrialRoundMessage message)
        {
            TerrestrialRound round = new TerrestrialRound
            {
                _id = message.RoundId,
                DeviceId = message.DeviceId,
                StationId = message.StationId,
                RoundStartUtc = message.RoundStartUtc,
                RoundEndUtc = message.RoundEndUtc,
                Station = message.Station
            };
            round.Results = new Results()
            {
                LocalCoordinates = new Dictionary<string, LocalPoint>(),
                ProjectCoordinates = new Dictionary<string, ProjectCoordinates>(),
            };
            return round;
        }

        public async Task<TerrestrialRound> LoadSingleRoundAsync(string roundId)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s._id, roundId);
            return await _MongoCollectionRounds.Find(filter).SingleAsync();
        }

        public async Task<TerrestrialRound[]> LoadRoundsByStationIdAsync(string stationId)
        {
            var filter = Builders<TerrestrialRound>.Filter.Eq(s => s.StationId, stationId);
            return (await _MongoCollectionRounds.FindAsync(filter)).ToList().ToArray();
        }

        public async Task<string> SaveNewStationAsync(Station station)
        {
            await _MongoCollectionStations.InsertOneAsync(station);
            return station._id;
        }

        public async Task<Station> LoadStationAsync(string stationId)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id,stationId);
            var doc = (await _MongoCollectionStations.FindAsync(filter)).ToList();
            if (doc.Count == 0)
            {
                return null;
            }else
            {
                return doc[0];
            }
        }

        public async Task<string> UpdateStationRoundListAsync(string stationId, Dictionary<string,RoundParams> rPrams)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id, stationId);
            var update = Builders<Station>.Update.Set(s => s.TerrestrialRounds, rPrams);
            await _MongoCollectionStations.UpdateOneAsync(filter, update);
            return stationId;
        }

        public async Task<string> UpdateStationRoundParamsAsync(string stationId, RoundParams rParams)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id, stationId);
            var update = Builders<Station>.Update.Set(s => s.TerrestrialRounds[rParams.ID], rParams);
            await _MongoCollectionStations.UpdateOneAsync(filter, update);
            return rParams.ID;
        }

        public async Task<string>UpdateRoundEnd(TerrestrialRound round)
        {
            var filter = Builders<Station>.Filter.Eq(s => s._id, round.StationId);
            var update = Builders<Station>.Update.Set(u => u.TerrestrialRounds[round._id].RoundEndUtc, round.RoundEndUtc);
            await _MongoCollectionStations.UpdateOneAsync(filter, update);
            return round._id;
        }
    }
}
