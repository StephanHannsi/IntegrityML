using System;
using System.Collections.Generic;
using System.Text;
using Calculation.Models;

namespace Calculation.calcServices
{
    public class ObservationCorrectionService
    {
        // this is here for reasons ... like handling real data ... also most of the code was removed in this version of the programm
        public double GetCorrectedDistance(TerrestrialStation station, TerrestrialObservation observation)
        {
            return (double)observation.EdmDistance;
        }
    }
}
