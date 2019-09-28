using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using InterpretationML.Models;
using System.Globalization;

namespace TraningDataGeneration
{
    public class Program
    {
        private LoadRepo _LoadRepo = new LoadRepo();
        private string _StationId = null;
        private string[] _UsedTargets = null;
        private string _SavePath = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            if (args.Length == 3)
            {
                Program prog = new Program();
                prog.dostuff(args);
            }
            Console.ReadLine();
        }

        private void dostuff(string[] args)
        {
            _StationId = args[0];
            _UsedTargets = File.ReadAllLines(args[2]);
            _SavePath = args[1];
            while (true)
            {
                Console.WriteLine("Availible progs: \n Press 1: Print integ Data files \n Press 2: Generate training data \n Press 3: Print coordinates");
                string lub = Console.ReadLine();
                switch (lub)
                {
                    case "1":
                        PrintIntegDataTime(_StationId);
                        Console.WriteLine("Printed data");
                        break;
                    case "2":
                        GenerateMlLearningData();
                        Console.WriteLine("generated training data");
                        break;
                    case "3":
                        PrintCoords();
                        Console.WriteLine("Printed all Coordinates");
                        break;
                    case "exit":
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }

            }
        }


        private void PrintIntegDataTime(string stationId)
        {
            var rounds = _LoadRepo.GetTerrestrialRoundsByStation(stationId).Result;
            List<string> targets = new List<string>();
            foreach(var round in rounds)
            {
                foreach(var obs in round.Observations)
                {
                    if(!targets.Contains(obs.Key))
                    {
                        targets.Add(obs.Key);
                    }
                }
            }
            foreach(var obs in targets)
            {
                List<string> Data = new List<string>();
                string[] line = new string[5];
                foreach (var round in rounds)
                {
                    if(round.IntegrityData != null && round.IntegrityData.ObservationIntegrityTime != null && round.IntegrityData.ObservationIntegrityTime.ContainsKey(obs))
                    {
                        line[0] = round.IntegrityData.ObservationIntegrityTime[obs].DistToRef.ToString(CultureInfo.InvariantCulture);
                        line[1] = round.IntegrityData.ObservationIntegrityTime[obs].DistToLast.ToString(CultureInfo.InvariantCulture);
                        line[2] = round.IntegrityData.ObservationIntegrityTime[obs].DistToAllTimeMean.ToString(CultureInfo.InvariantCulture);
                        line[3] = round.IntegrityData.ObservationIntegrityTime[obs].DistToTempMean.ToString(CultureInfo.InvariantCulture);
                        line[4] = ((double)(round.Observations[obs].MeanF1F2.Temperature)).ToString(CultureInfo.InvariantCulture);
                        Data.Add(string.Join(",", line));
                    }
                    else
                    {
                        for(int i = 0; i< 5; i++)
                        {
                            line[i] = "0";
                        }
                        Data.Add(string.Join(",", line));
                    }
                }
                File.WriteAllLines(_SavePath + "Print_"+ stationId + "_" + obs+ ".csv" , Data);
            }

        }

        private void PrintCoords()
        {
            TerrestrialRound[] rounds = _LoadRepo.GetTerrestrialRoundsByStation(_StationId).Result;
            Dictionary<string, List<string>> obs = new Dictionary<string, List<string>>();
            foreach(var traget in _UsedTargets)
            {
                obs.Add(traget, new List<string>());
            }
            foreach(var round in rounds)
            {
                foreach(var target in round.Results.LocalCoordinates)
                {
                    if(obs.ContainsKey(target.Key))
                    {
                        string line = ((double)target.Value.X).ToString(CultureInfo.InvariantCulture) + "," + ((double)target.Value.Y).ToString(CultureInfo.InvariantCulture) + "," + ((double)target.Value.Z).ToString(CultureInfo.InvariantCulture);
                        obs[target.Key].Add(line);
                    }
                }
            }
            foreach(var dings in obs)
            {
                File.WriteAllLines(_SavePath + "Coords_" + _StationId + "_" + dings.Key + ".csv", dings.Value);
            }
            Console.WriteLine("Done");
        }

        private void GenerateMlLearningData()
        {
            // make setlist
            foreach (string target in _UsedTargets)
            {
                var rounds = _LoadRepo.GetTerrestrialRoundsByStation(_StationId).Result;
                rounds = rounds.Skip(1).Take(rounds.Length - 2).ToArray();
                List<int[]> chunks = new List<int[]>();
                List<int> chunk = new List<int>();

                foreach (TerrestrialRound round in rounds)
                {
                    if (round.IntegrityData != null && round.IntegrityData.ObservationIntegrityTime != null && round.IntegrityData.ObservationIntegrityTime.ContainsKey(target))
                    {
                        chunk.Add(Array.IndexOf(rounds, round));
                    }
                    else
                    {
                        int[] achunk = chunk.ToArray();
                        Array.Sort(achunk);
                        chunks.Add(achunk);
                        chunk.Clear();
                    }
                }
                int[] bchunk = chunk.ToArray();
                Array.Sort(bchunk);
                chunks.Add(bchunk);
                chunk.Clear();

                List<string> Data = new List<string>();
                string[] line = new string[27];
                
                foreach (var chunk1 in chunks)
                {
                    if(chunk1.Count() >= 6)
                    {
                        int set = 0;
                        while ((set + 6) <= chunk1.Count())
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                line[26 - i * 5] = Math.Round(rounds[chunk1[set + i]].IntegrityData.ObservationIntegrityTime[target].DistToRef, 5, MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture);
                                line[26 - (i * 5 + 1)] = Math.Round(rounds[chunk1[set + i]].IntegrityData.ObservationIntegrityTime[target].DistToTempMean, 5, MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture);
                                line[26 - (i * 5 + 2)] = Math.Round(rounds[chunk1[set + i]].IntegrityData.ObservationIntegrityTime[target].DistToAllTimeMean, 5, MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture);
                                line[26 - (i * 5 + 3)] = Math.Round(rounds[chunk1[set + i]].IntegrityData.ObservationIntegrityTime[target].DistToLast, 5, MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture);
                                line[26 - (i * 5 + 4)] = ((double)rounds[chunk1[set + i]].Observations[target].MeanF1F2.Temperature).ToString(CultureInfo.InvariantCulture);
                            }
                            line[1] = ((double)rounds[chunk1[set + 5]].Observations[target].MeanF1F2.Temperature).ToString(CultureInfo.InvariantCulture);
                            line[0] = Math.Round(rounds[chunk1[set + 5]].IntegrityData.ObservationIntegrityTime[target].DistToRef, 5, MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture);
                            Data.Add(string.Join(",", line));
                            set++;
                        }
                    }
                }
                File.WriteAllLines(_SavePath + _StationId + target + ".csv", Data);
                chunks.Clear();
            }
        }
    }
}
