using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RaspberryPiDotNet;

namespace Socks {
    class TriggerSource {
        public Boolean Triggered {get; protected set;}
        public DateTime TriggerTime { get; protected set; }

        public void Update() {
            throw new NotImplementedException();
        }

        public void Clear() {
            Triggered = false;
            TriggerTime = DateTime.MinValue;
        }
    }

    class GpioTrigger : TriggerSource {
        private PinState TriggerState;
        private GPIOMem GpioPin;

        public GpioTrigger(GPIOMem gpioMem, PinState triggerState) {
            this.GpioPin = gpioMem;
            this.TriggerState = triggerState;
        }

        public void Update() {
            if (GpioPin.Read() == TriggerState) {
                Triggered = true;
                TriggerTime = DateTime.Now;
            }
        }

    }
}
