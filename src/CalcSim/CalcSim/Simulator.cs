using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace CalcSim
{
    public class Simulator
    {
        private DataGenerator _DataGenerator = new DataGenerator();
        private MessageSender _MessageSender = new MessageSender();
        private Config _Config = null;

        public Simulator(string config)
        {
            // Loads configuration
            _Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(config));

            // Sets starting parameters
            Random rnd = new Random();
            int i = 0;
            int tag = 0;
            int day = 1440 / _Config.Interval - 1;
            int timeTofail = Convert.ToInt32(day * _Config.Days * _Config.AddOutliers);
            bool fail = false;
            List<TerrestrialObservation> observations = new List<TerrestrialObservation>();
            Dictionary<string, List<string>> failList = new Dictionary<string, List<string>>();

            // Documantation for outliers
            foreach(var target in _Config.Targets)
            {
                failList.Add(target.Key, new List<string>());
            }

            Console.WriteLine("Ready to generate " + (day * _Config.Days).ToString() + " Observations");
            Console.ReadLine();

            while (i < day * _Config.Days)
            {
                i++;
                // new message 
                TerrestrialRoundMessage message = new TerrestrialRoundMessage
                {
                    DeviceId = _Config.DeviceId,
                    RoundStartUtc = _Config.StartTime.AddMinutes(i * _Config.Interval),
                    Station = _Config.Station
                };
                message.RoundId = CalcRoundId(_Config.DeviceId, message.RoundStartUtc, _Config.Station);

                // Send station record
                _MessageSender.SendStationRecord(message);

                // Crating observation message
                foreach (var target in _Config.Targets)
                {
                    // deciding if the new observation is an outlier
                    if (i > timeTofail)
                    {
                        if(failList[target.Key].Count == 0)
                        {
                            if (rnd.NextDouble() > 0.9)
                            {
                                fail = true;
                                failList[target.Key].Add(i.ToString());
                            }
                        }else if(i - Convert.ToInt16(failList[target.Key][failList[target.Key].Count - 1]) > 4) // this prevents that there are 2 outliers too close to another
                        {
                            if (rnd.NextDouble() > 0.9)
                            {
                                fail = true;
                                failList[target.Key].Add(i.ToString());
                            }
                        }
                    }

                    //generating the observation
                    var obs = _DataGenerator.GetTerrestrialObservation(target.Key, _Config, rnd, i, fail);

                    // save observations to the list
                    obs.TimeUtc = message.RoundStartUtc.AddMinutes(tag);
                    observations.Add(obs);
                    TerrestrialObservation face2 = clone(obs);
                    observations.Add(_DataGenerator.FlipFace(face2));

                    // resetting parameters for next loop
                    fail = false;
                    tag++;
                }

                // adding observations to message and sending the message
                message.Observations = observations.ToArray();
                _MessageSender.SendObservationRecord(message);
                message.Observations = null;
                message.RoundEndUtc = message.RoundStartUtc.AddMinutes(tag + 1);
                _MessageSender.SendRoundEnd(message);

                //Resetting values for next loop
                tag = 0;
                observations.Clear();
            }

            foreach(var target in failList)
            {
                File.WriteAllLines(_Config.FailSavePath + _Config.DeviceId + target.Key + ".csv", (target.Value.ToArray()));
            }
            Console.WriteLine("Finished sending messages");
            Console.ReadLine();
        }

        // Calculates a individual round Id to identify the round
        private string CalcRoundId(string deviceId, DateTime roundStartUtc, TerrestrialStation station)
        {
            var roundId = $"{deviceId}{station.SerialNumber}{roundStartUtc:yyyyMMddHHmmss}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(roundId)).Trim('=');
        }

        private TerrestrialObservation clone(TerrestrialObservation terrestrialObservation)
        {
            string desi = JsonConvert.SerializeObject(terrestrialObservation);
            return JsonConvert.DeserializeObject<TerrestrialObservation>(desi);
        }
    }
}
