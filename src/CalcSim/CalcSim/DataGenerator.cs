using System;
using System.Collections.Generic;
using System.Text;

namespace CalcSim
{
    public class DataGenerator
    {

        private bool _Fail = false;

        public TerrestrialObservation GetTerrestrialObservation(string target, Config config, Random rnd, int step, bool fail)
        {
            // create a new observation
            TerrestrialObservation observation = new TerrestrialObservation {
                PointId = target,
                ObservationType = "Face1",
                // Not used ritght now ... 
                Pressure = 930,
                PrismConstant = 0,
                TargetHeight = 0,
                CompensatorTiltX = 0,
                CompensatorTiltY = 0,
            };

            _Fail = fail;
            double j = step * Increment(config.Interval);

            // Add calculated data to the observation
            observation.Temperature = Math.Round(config.Temperature + Math.Sin(j * Math.PI) * 0.8, 1);
            TerrestrialPosition position = GetCoords(config.Targets[target], j, rnd);
            TerrestrialObservation obsnew = AddMeasurements(position, observation);

            return obsnew;
        }

        private double Increment(int interval)
        {
            double lub = (2.0 / (1440.0 / interval - 1.0));
            return Convert.ToDouble(lub);
        }

        // Created coordinates
        private TerrestrialPosition GetCoords(Target target, double j, Random rnd)
        {
            double rand = target.RandomScale;
            if (_Fail)
            {
                rand = rand * 4;
                Console.WriteLine("Adding outlier");
            }
            TerrestrialPosition position = new TerrestrialPosition
            {
                Easting = target.Position.Easting + Math.Sin(j * Math.PI) * target.MoveScale + (rnd.NextDouble()*2-1) * rand,
                Northing = target.Position.Northing + Math.Cos(j * Math.PI) * target.Abplattung * target.MoveScale + (rnd.NextDouble()*2-1) * rand,
                Elevation = target.Position.Elevation + rnd.NextDouble() * rand
            };
            return position;
        }

        // Crayted measurements from coordinates
        private TerrestrialObservation AddMeasurements(TerrestrialPosition coords, TerrestrialObservation observation)
        {
            observation.EdmDistance = Math.Sqrt(Math.Pow(coords.Easting,2) + Math.Pow(coords.Northing, 2) + Math.Pow(coords.Elevation, 2));
            observation.HorizontalCircleReading = Math.Atan2(coords.Easting, coords.Northing);
            observation.VerticalAngle = Math.Atan2(Math.Sqrt(Math.Pow(coords.Easting, 2) + Math.Pow(coords.Northing, 2)), coords.Elevation);
            return observation;
        }

        // Calculates Face2 from Face1
        public TerrestrialObservation FlipFace(TerrestrialObservation observation)
        {
            observation.HorizontalCircleReading = observation.HorizontalCircleReading + Math.PI;
            if (observation.HorizontalCircleReading > Math.PI * 2)
            {
                observation.HorizontalCircleReading = observation.HorizontalCircleReading - Math.PI * 2;
            }

            observation.VerticalAngle = Math.PI * 2 - observation.VerticalAngle;
            observation.ObservationType = "Face2";
            observation.TimeUtc.AddSeconds(30);
            return observation;
        }
    }
}
