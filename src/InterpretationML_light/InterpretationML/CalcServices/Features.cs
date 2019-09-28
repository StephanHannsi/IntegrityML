using System;
using System.Collections.Generic;
using System.Text;
using InterpretationML.Models;
using InterpretationML;
using System.Threading.Tasks;

namespace InterpretationML.CalcServices
{
    public class Features
    {
        LoadSaveRepo _LoadSaveRepo = new LoadSaveRepo();

        public ObservationIntegrity GetObservationIntegrity(TerrestrialObservation terrestrialObservationF1, TerrestrialObservation terrestrialObservationF2)
        {
            if(terrestrialObservationF1.EdmDistance != null && terrestrialObservationF2.EdmDistance != null)
            {
                ObservationIntegrity obsInteg = new ObservationIntegrity()
                {
                    PointId = terrestrialObservationF1.PointId,
                    VzDiff = (double)(terrestrialObservationF1.VerticalAngle - (Math.PI * 2 - terrestrialObservationF2.VerticalAngle)),
                    HzDiff = (double)terrestrialObservationF1.HorizontalCircleReading - NormHzFace2((double)terrestrialObservationF2.HorizontalCircleReading),
                    EdmDiff = (double)(terrestrialObservationF1.EdmDistance - terrestrialObservationF2.EdmDistance),
                    CompTiltXDiff = (double)(terrestrialObservationF1.CompensatorTiltX - (terrestrialObservationF2.CompensatorTiltX * -1)),
                    CompTiltYDiff = (double)(terrestrialObservationF1.CompensatorTiltY - (terrestrialObservationF2.CompensatorTiltY * -1)),
                };
                return obsInteg;
            }
            ObservationIntegrity observationIntegrity = new ObservationIntegrity()
            {
                Flag = true
            };
            return observationIntegrity;
        }

        public ObservationIntegrityTime GetObservationIntegrityTime(TerrestrialRound currentRound, TerrestrialRound referenceRound, TerrestrialRound comparativelyRounds, TargetParams targetParams, string targetId)
        {
            if(currentRound.IntegrityData.ObservationIntegrity[targetId].Flag == true)
            {
                return null;
            }
            ObservationIntegrityTime obstime = new ObservationIntegrityTime()
            {
                PointId = targetId,
                Face = "MeanF1F2",
            };

            if(referenceRound.Results.LocalCoordinates.ContainsKey(targetId))
            {
                obstime.DistToRef = Get2dDist(currentRound.Results.LocalCoordinates[targetId], referenceRound.Results.LocalCoordinates[targetId]);
            }else
            {
                var task = _LoadSaveRepo.SaveLocalCoords(currentRound.Results.LocalCoordinates[targetId], referenceRound._id);
                var result = task.Result;
                referenceRound.Results.LocalCoordinates.Add(targetId, currentRound.Results.LocalCoordinates[targetId]);
                obstime.DistToRef = Get2dDist(currentRound.Results.LocalCoordinates[targetId], referenceRound.Results.LocalCoordinates[targetId]);
            }

            if (comparativelyRounds.Results.LocalCoordinates.ContainsKey(targetId))
            {
                obstime.DistToLast = Get2dDist(currentRound.Results.LocalCoordinates[targetId], comparativelyRounds.Results.LocalCoordinates[targetId]);
            }

            if(targetParams != null)
            {
                obstime.DistToAllTimeMean = Get2dDist(currentRound.Results.LocalCoordinates[targetParams.ID], targetParams.AllTimeMean.Position);
                int obstemp = Convert.ToInt32(currentRound.Observations[targetParams.ID].MeanF1F2.Temperature);
                if (targetParams.TempMean.ContainsKey(obstemp.ToString()))
                {
                    obstime.DistToTempMean = Get2dDist(currentRound.Results.LocalCoordinates[targetParams.ID], targetParams.TempMean[obstemp.ToString()].Position);
                }
                else
                {
                    string point = "0";
                    int minDeltaTemp = 1000;
                    int deltaTemp = 0;
                    foreach(var temp in targetParams.TempMean)
                    {
                        deltaTemp = Math.Abs(temp.Value.Temperature - obstemp);
                        if (minDeltaTemp > deltaTemp)
                        {
                            minDeltaTemp = deltaTemp;
                            point = temp.Key;
                        }
                    }
                    obstime.DistToTempMean = Get2dDist(currentRound.Results.LocalCoordinates[targetParams.ID], targetParams.TempMean[point].Position);
                }
            }

            return obstime;
        }

        public TargetParams GetNewTargetParams(TerrestrialRound terrestrialRound, string targetId)
        {
            TargetParams targetParams = new TargetParams()
            {
                ID = targetId,
                RoundProcessed = terrestrialRound._id,
                AllTimeMean = new AllTimeMean()
                {
                    count = 1,
                    Position = terrestrialRound.Results.LocalCoordinates[targetId],
                },
                TempMean = new Dictionary<string, TempMean>(),
            };
            int temp = Convert.ToInt32(terrestrialRound.Observations[targetId].MeanF1F2.Temperature);
            targetParams.TempMean.Add(temp.ToString(), new TempMean() { Temperature = temp, count = 1, Position = terrestrialRound.Results.LocalCoordinates[targetId] });
            return targetParams;
        }

        public TargetParams UpdateTargetParams(TargetParams targetParams, TerrestrialRound terrestrialRound, string targetId)
        {
            targetParams.RoundProcessed = terrestrialRound._id;
            targetParams.AllTimeMean.Position = MeanWeightedLocalPoint(targetParams.AllTimeMean.Position, terrestrialRound.Results.LocalCoordinates[targetId], targetParams.AllTimeMean.count);
            targetParams.AllTimeMean.count++;
            string temp = (Convert.ToInt32(terrestrialRound.Observations[targetId].MeanF1F2.Temperature)).ToString();
            if (targetParams.TempMean.ContainsKey(temp))
            {
                targetParams.TempMean[temp].Position = MeanWeightedLocalPoint(targetParams.TempMean[temp].Position, terrestrialRound.Results.LocalCoordinates[targetId], targetParams.TempMean[temp].count);
                targetParams.TempMean[temp].count++;
            }
            else
            {
                targetParams.TempMean.Add(temp, new TempMean() { Temperature = Convert.ToInt32(temp), count = 1, Position = terrestrialRound.Results.LocalCoordinates[targetId] });
            }
            return targetParams;
        }

        private double NormHzFace2(double face2)
        {
            face2 = face2 - Math.PI;
            if (face2 > (Math.PI * 2))
            {
                face2 = face2 - (2 * Math.PI);
            }
            else if (face2 < 0)
            {
                face2 = face2 + (2 * Math.PI);
            }
            return face2;
        }

        private double Get3dDist(LocalPoint point1, LocalPoint point2)
        {
            //Gets the 3D distance between 2 coordinates
            return Math.Sqrt(Math.Pow((double)(point2.X-point1.X), 2) + Math.Pow((double)(point2.Y - point1.Y), 2) + Math.Pow((double)(point2.Z - point1.Z), 2));
        }

        private double Get2dDist(LocalPoint point1, LocalPoint point2)
        {
            return Math.Sqrt(Math.Pow((double)(point2.X - point1.X), 2) + Math.Pow((double)(point2.Y - point1.Y), 2));
        }

        private LocalPoint MeanWeightedLocalPoint(LocalPoint localPoint1, LocalPoint localPoint2, int count)
        {
            LocalPoint point = new LocalPoint()
            {
                X = (localPoint1.X * count + localPoint2.X) / (count + 1),
                Y = (localPoint1.Y * count + localPoint2.Y) / (count + 1),
                Z = (localPoint1.Z * count + localPoint2.Z) / (count + 1),
                PointId = localPoint1.PointId
            };
            return point;
        }
    }
}
