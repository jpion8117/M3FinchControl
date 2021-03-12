using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3FinchControl
{
    class userProgramming
    {
        enum Commands
        {
            Foreward,
            Back,
            Right,
            Left,
            GetTemperature,
            GetLightLevel,
            SetLED1,
            SetLED2,
            SetLED3,
            ResetLED,
            Beep1,
            Beep2,
            Beep3,
            WaitCustom,
            Wait1,
            Wait30
        }
    }
}
