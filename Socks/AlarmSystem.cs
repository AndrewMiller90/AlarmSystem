using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Diagnostics;
using SharedComponents;
using RaspberryPiDotNet;
using System.IO.Ports;

namespace Socks
{

    class TestSystem : ISecuritySystem
    {
        public AlarmState State { get; protected set; }

        public TestSystem() { 
            State = AlarmState.Armed;
        }
        public void Trigger() {
            if (State != AlarmState.Armed) return;
            State = AlarmState.Triggered;

            Console.WriteLine("System Triggered");
            OnTriggered();
        }
        public void Disarm() {
            Console.WriteLine("System Disarmed");
            State = AlarmState.Disarmed;
        }
        public void Arm() {
            if (State != AlarmState.Disarmed) return;
            State = AlarmState.Armed;

            Console.WriteLine("System Armed");
        }

        public void Update() { }
            
        private void OnTriggered() {
            Console.WriteLine(string.Format("Emails would be sent now to {0}", string.Join(", ", EmailHelper.GetEmailList())));
            Console.WriteLine("Alarm would be activated now");
        }
    }

    class AlarmSystem :ISecuritySystem
    {
        private TriggerSource[] Trip = new TriggerSource[] {new GpioTrigger(new GPIOMem(GPIOPins.GPIO_23, GPIODirection.In), PinState.Low),
                                                            new GpioTrigger(new GPIOMem(GPIOPins.GPIO_24, GPIODirection.In), PinState.Low)};
        private GPIOMem DisarmedState = new GPIOMem(GPIOPins.GPIO_14, GPIODirection.Out);
        private GPIOMem ArmedState = new GPIOMem(GPIOPins.GPIO_15, GPIODirection.Out);
        private GPIOMem TriggeredState = new GPIOMem(GPIOPins.GPIO_18, GPIODirection.Out);

        private AlarmState _state;
        public AlarmState State { 
            get { return _state; } 
            protected set { 
                if (value != _state) { 
                    _state = value; 
                    OnStateChanged(); 
                }
            } 
        }

        public AlarmSystem() { State = AlarmState.Disarmed; }
        public AlarmSystem(AlarmState state) {
            this.State = state;
        }

        public void Trigger()
        {
            if (State != AlarmState.Armed) return;
            State = AlarmState.Triggered;
        }
        public void Disarm()
        {
            State = AlarmState.Disarmed;
        }
        public void Arm()
        {
            if (State != AlarmState.Disarmed) return;
            State = AlarmState.Armed;
        }

        public void Update() {
            if (State == AlarmState.Disarmed ) return;
            foreach (TriggerSource pin in Trip) {
                pin.Update();
            }
            if ((from item in Trip where item.Triggered).Any && State != AlarmState.Triggered) State = AlarmState.Triggered; 
        }

        private void OnStateChanged() {
            DisarmedState.Write(false);
            ArmedState.Write(false);
            TriggeredState.Write(false);

            switch(State) {
                case AlarmState.Disarmed:
                    DisarmedState.Write(true);
                    break;
                case AlarmState.Armed:
                    ArmedState.Write(true);
                    break;
                case AlarmState.Triggered:
                    TriggeredState.Write(true);
                    EmailHelper.SendAlarmEmail();
                    break;
                default:
                    return;
            }
        }

        

    }
}
