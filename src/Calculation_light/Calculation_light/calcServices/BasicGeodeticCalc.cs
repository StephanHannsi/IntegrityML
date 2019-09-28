using System;
using System.Collections.Generic;
using System.Text;
using Calculation.Models;

namespace Calculation.calcServices
{
    public class BasicGeodeticCalc
    {
        public TerrestrialObservation MeanObservations(TerrestrialObservation terrestrialObservationF1, TerrestrialObservation terrestrialObservationF2)
        {
            if(terrestrialObservationF1.EdmDistance != null && terrestrialObservationF2.EdmDistance != null)
            {
                TerrestrialObservation ObsF1F2 = new TerrestrialObservation()
                {
                    PointId = terrestrialObservationF1.PointId,
                    TimeUtc = terrestrialObservationF2.TimeUtc,
                    TargetHeight = terrestrialObservationF1.TargetHeight,

                    EdmDistance = (terrestrialObservationF1.EdmDistance + terrestrialObservationF2.EdmDistance) / 2,
                    Temperature = (terrestrialObservationF1.Temperature + terrestrialObservationF2.Temperature) / 2,
                    CompensatorTiltX = (terrestrialObservationF1.CompensatorTiltX + terrestrialObservationF2.CompensatorTiltX * -1) / 2,
                    CompensatorTiltY = (terrestrialObservationF1.CompensatorTiltY + terrestrialObservationF2.CompensatorTiltY * -1) / 2,
                    Pressure = (terrestrialObservationF1.Pressure + terrestrialObservationF2.Pressure) / 2,

                    Lockmode = terrestrialObservationF1.Lockmode,
                    ObservationType = "MeanF1F2",

                    HorizontalCircleReading = MeanHzFaces((double)terrestrialObservationF1.HorizontalCircleReading, (double)terrestrialObservationF2.HorizontalCircleReading),
                    VerticalAngle = MeanVzFaces((double)terrestrialObservationF1.VerticalAngle, (double)terrestrialObservationF2.VerticalAngle)
                };

                if (terrestrialObservationF1.Position != null)
                {
                    ObsF1F2.Position = terrestrialObservationF1.Position;
                }
                return ObsF1F2;
            }
            else if(terrestrialObservationF1.EdmDistance != null)
            {
                TerrestrialObservation ObsF1F2 = new TerrestrialObservation();
                ObsF1F2 = terrestrialObservationF1;
                ObsF1F2.ObservationType = "MeanF1F2";
                return ObsF1F2;
            }
            else if(terrestrialObservationF2.EdmDistance != null)
            {
                TerrestrialObservation ObsF1F2 = new TerrestrialObservation();
                ObsF1F2 = terrestrialObservationF2;
                ObsF1F2.ObservationType = "MeanF1F2";
                return ObsF1F2;
            }
            else
            {
                TerrestrialObservation ObsF1F2 = null;
                return ObsF1F2;
            }
        }

        public double MeanHzFaces(double face1, double face2)
        {
            face2 = face2 - Math.PI;
            if(face2 > (Math.PI*2))
            {
                face2 = face2 - (2 * Math.PI);
            }
            else if(face2 < 0)
            {
                face2 = face2 + (2 * Math.PI);
            }
            return ((face1 + face2) / 2);
        }

        public double MeanVzFaces(double face1, double face2)
        {
            face2 = Math.PI * 2 - face2;
            return ((face1 + face2) / 2);
        }

        public LocalPoint GetLocalPoint(TerrestrialObservation terrestrialObservation)
        {
            LocalPoint point = new LocalPoint();
            point.PointId = terrestrialObservation.PointId;
            point.Y = terrestrialObservation.EdmDistance * Math.Sin((double)terrestrialObservation.VerticalAngle) * Math.Sin((double)terrestrialObservation.HorizontalCircleReading);
            point.X = terrestrialObservation.EdmDistance * Math.Sin((double)terrestrialObservation.VerticalAngle) * Math.Cos((double)terrestrialObservation.HorizontalCircleReading);
            point.Z = terrestrialObservation.EdmDistance * Math.Cos((double)terrestrialObservation.VerticalAngle);
            return point;
        }


    }
}
