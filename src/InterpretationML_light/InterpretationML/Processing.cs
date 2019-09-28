using System;
using System.Collections.Generic;
using System.Text;
using InterpretationML.CalcServices;
using InterpretationML.Models;
using System.Threading.Tasks;

namespace InterpretationML
{
    public class Processing
    {
        LoadSaveRepo _LoadSaveRepo = new LoadSaveRepo();
        Features _Features = new Features();
        TerrestrialRound _Round = new TerrestrialRound();
        TerrestrialRound _PreviRound = new TerrestrialRound();
        TerrestrialRound _RefRound = new TerrestrialRound();
        Station _Station = new Station();
        
        public async Task<bool>HandleObservationRecord(string message)
        {
            _Round = await _LoadSaveRepo.LoadSingleRoundAsync(message);
            _Station = await _LoadSaveRepo.LoadStationAsync(_Round.StationId);
            ProcessNewObs(_Round,_Station);
            if(_Station.ReferenceRound != _Round._id)
            {
                _RefRound = await _LoadSaveRepo.LoadSingleRoundAsync(_Station.ReferenceRound);
                string[] keys = new string[_Station.TerrestrialRounds.Count];
                _Station.TerrestrialRounds.Keys.CopyTo(keys, 0);
                _PreviRound = await _LoadSaveRepo.LoadSingleRoundAsync(keys[Array.IndexOf(keys, _Round._id) - 1]);
                ProcessNewObsTime(_Round, _PreviRound, _RefRound);
                if(IsInteger(message))
                {
                    ProcessStationTargets(_Round, _Station);
                    await _LoadSaveRepo.UpdateTargetsAsync(_Station._id, _Station.Targets);
                }
            }

            await _LoadSaveRepo.SaveIntegrityData(_Round.IntegrityData, _Round._id);
            await _LoadSaveRepo.UpdateTerrestrialRound(_Station._id,_Station.TerrestrialRounds[_Round._id]);
            return true;
        }

        private bool ProcessNewObs(TerrestrialRound round, Station station)
        {
            Console.WriteLine("Process single round: " + round._id);

            foreach(var point in round.Observations)
            {
                if (round.Observations[point.Key].Face1 != null && round.Observations[point.Key].Face2 != null)
                {
                    Console.WriteLine("Process new Obs: " + point.Key);
                    if (round.IntegrityData == null)
                    {
                        round.IntegrityData = new IntegrityData()
                        {
                            ObservationIntegrity = new Dictionary<string, ObservationIntegrity>()
                        };
                        round.IntegrityData.ObservationIntegrity.Add(point.Key, _Features.GetObservationIntegrity(round.Observations[point.Key].Face1, round.Observations[point.Key].Face2));
                        if(round.IntegrityData.ObservationIntegrity[point.Key].Flag == true)
                        {
                            station.TerrestrialRounds[round._id].FlaggedObs.Add(point.Key);
                        }
                    }
                    else
                    {
                        if (!round.IntegrityData.ObservationIntegrity.ContainsKey(point.Key))
                        {
                            round.IntegrityData.ObservationIntegrity.Add(point.Key, _Features.GetObservationIntegrity(round.Observations[point.Key].Face1, round.Observations[point.Key].Face2));
                            if (round.IntegrityData.ObservationIntegrity[point.Key].Flag == true)
                            {
                                station.TerrestrialRounds[round._id].FlaggedObs.Add(point.Key);
                            }
                        }                        
                    }
                }else
                {
                    station.TerrestrialRounds[round._id].FlaggedObs.Add(point.Key);
                }

            }
            if(station.TerrestrialRounds[round._id].FlaggedObs.Count != 0)
            {
                station.TerrestrialRounds[round._id].Flagged = true;
            }
            _Round.IntegrityData = round.IntegrityData;
            _Station.TerrestrialRounds = station.TerrestrialRounds;
            Console.WriteLine("saved round");
            return true;
        }

        private bool ProcessNewObsTime(TerrestrialRound lastRound, TerrestrialRound previRound, TerrestrialRound refRound)
        {
            Console.WriteLine("Process 2 rounds: " + lastRound._id +"and: " + previRound._id);
            foreach (var obs in lastRound.Observations)
            {
                if(!_Station.TerrestrialRounds[lastRound._id].FlaggedObs.Contains(obs.Key) && !_Station.TerrestrialRounds[previRound._id].FlaggedObs.Contains(obs.Key))//lastRound.Observations[obs.Key].MeanF1F2 != null)
                {
                    Console.WriteLine("Process new Obs: " + obs.Key);
                    if (lastRound.IntegrityData.ObservationIntegrityTime == null)
                    {
                        lastRound.IntegrityData.ObservationIntegrityTime = new Dictionary<string, ObservationIntegrityTime>();
                    }
                    if(!lastRound.IntegrityData.ObservationIntegrityTime.ContainsKey(obs.Key))
                    {
                        TargetParams targetParams = new TargetParams();
                        targetParams = _Station.Targets.ContainsKey(obs.Key) ? _Station.Targets[obs.Key] : null;
                        lastRound.IntegrityData.ObservationIntegrityTime.Add(obs.Key, _Features.GetObservationIntegrityTime(lastRound, refRound, previRound, targetParams, obs.Key));
                    }
                }
            }
            _Round.IntegrityData = lastRound.IntegrityData;
            Console.WriteLine("saved round");
            return true;
        }

        private bool IsInteger(string roundId)
        {
            return true;
        }

        private bool ProcessStationTargets(TerrestrialRound round, Station station)
        {
            Console.WriteLine("Start Station Taget processing: " + round._id);
            Console.WriteLine("Integ data: true, roundid: " + round._id);
            foreach (var target in round.Observations)
            {
                if (!station.TerrestrialRounds[round._id].FlaggedObs.Contains(target.Key))
                {
                    if (!station.Targets.ContainsKey(target.Key))
                    {
                        Console.WriteLine("Adding new Target: " + target.Key);
                        station.Targets.Add(target.Key, _Features.GetNewTargetParams(round, target.Key));
                    }
                    else
                    {
                        if (station.Targets[target.Key].RoundProcessed != round._id)
                        {
                            Console.WriteLine("Updating Target: " + target.Key);
                            station.Targets[target.Key] = _Features.UpdateTargetParams(station.Targets[target.Key], round, target.Key);
                        }
                    }
                }
            }
            _Station.Targets = station.Targets;
            return true;
        }
    }
}
