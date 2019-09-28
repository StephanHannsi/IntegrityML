using System;
using System.Collections.Generic;
using System.Text;
using Calculation.Models;
using Calculation.calcServices;
using System.Threading.Tasks;

namespace Calculation
{
    public class Processing
    {
        LoadSaveRepo _LoadSaveRepo = new LoadSaveRepo();
        BasicGeodeticCalc _GeodeticCalc = new BasicGeodeticCalc();
        ObservationCorrectionService _ObservationCorrectionService = new ObservationCorrectionService();
        MessageSender _MessageSender = new MessageSender();

        // When looking at the code keep in mind that a lot of the code is there to handle incomplete data

        public async Task<bool> ProcessNewObs(string roundId)
        {
            TerrestrialRound round = await _LoadSaveRepo.LoadSingleRoundAsync(roundId);
            foreach(var point in round.Observations)
            {
                if(round.Observations[point.Key].Face1 != null && round.Observations[point.Key].Face2 != null && round.Observations[point.Key].MeanF1F2 == null)
                {
                    if(round.Observations[point.Key].Face1.EdmDistance != null && round.Observations[point.Key].Face2.EdmDistance != null)
                    {
                        TerrestrialObservation mean = _GeodeticCalc.MeanObservations(round.Observations[point.Key].Face1, round.Observations[point.Key].Face2);
                        mean.EdmDistance = _ObservationCorrectionService.GetCorrectedDistance(round.Station, mean);
                        await _LoadSaveRepo.SaveMeanAsync(mean, roundId);
                        await _LoadSaveRepo.SaveLocalCoords(_GeodeticCalc.GetLocalPoint(mean), roundId);
                    }
                }
            }
            return true;
        }

        public async Task<bool> ProcessStationRecord(string stationId, string roundId)
        {
            var station = await _LoadSaveRepo.LoadStationAsync(stationId);
            if (station == null)
            {
                var rounds = await _LoadSaveRepo.LoadRoundsByStationIdAsync(stationId);
                Station nstation = new Station()
                {
                    _id = stationId,
                    PointId = rounds[0].Station.PointId,
                    Position = rounds[0].Station.Position,
                    ReferenceRound = roundId,
                    TerrestrialRounds = new Dictionary<string, RoundParams>(),
                    Targets = new Dictionary<string, TargetParams>()
                };
                foreach(var round in rounds)
                {
                    RoundParams roundParams = new RoundParams()
                    {
                        ID = round._id,
                        RoundStartUtc = round.RoundStartUtc,
                        RoundEndUtc = round.RoundEndUtc,
                        FlaggedObs = new List<string>()
                    };
                    nstation.TerrestrialRounds.Add(round._id, roundParams);
                }
                await _LoadSaveRepo.SaveNewStationAsync(nstation);
            }
            else
            {
                if (!station.TerrestrialRounds.ContainsKey(roundId))
                {
                    TerrestrialRound round = await _LoadSaveRepo.LoadSingleRoundAsync(roundId);
                    RoundParams roundParams = new RoundParams()
                    {
                        ID = roundId,
                        RoundStartUtc = round.RoundStartUtc,
                        RoundEndUtc = round.RoundEndUtc,
                        FlaggedObs = new List<string>()
                    };
                    station.TerrestrialRounds.Add(roundId, roundParams);
                    await _LoadSaveRepo.UpdateStationRoundListAsync(station._id, station.TerrestrialRounds);
                }
            }
            return true;
        }

        public async Task<bool> ProcessRoundEnd(string roundId)
        {
            TerrestrialRound round = await _LoadSaveRepo.LoadSingleRoundAsync(roundId);
            await FixMean(round);
            await _LoadSaveRepo.UpdateRoundEnd(round);
            Console.WriteLine("Set stationRoundEnd: " + round.RoundEndUtc);
            _MessageSender.NewObservation(roundId);
            return true;
        }

        private async Task<bool> FixMean(TerrestrialRound round)
        {
            foreach(var obs in round.Observations)
            {
                if (obs.Value.MeanF1F2 == null)
                {
                    if (obs.Value.Face1 != null)
                    {
                        if(obs.Value.Face1.EdmDistance != null)
                        {
                            TerrestrialObservation mean = obs.Value.Face1;
                            mean.ObservationType = "MeanF1";
                            await _LoadSaveRepo.SaveMeanAsync(mean, obs.Key);
                        }
                    }

                    if (obs.Value.Face2 != null)
                    {
                        if(obs.Value.Face2.EdmDistance != null)
                        {
                            TerrestrialObservation mean = obs.Value.Face2;
                            mean.ObservationType = "MeanF2";
                            await _LoadSaveRepo.SaveMeanAsync(mean, obs.Key);
                        }
                    }
                }
            }
            return true;
        }
    }
}
