using SharedComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks
{
    interface ISecuritySystem
    {
        AlarmState State { get; }
        void Arm();
        void Disarm();
        void Trigger();
        void Update();
    }
}
