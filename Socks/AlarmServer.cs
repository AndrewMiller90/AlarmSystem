using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using SharedComponents;

namespace Socks
{
    class AlarmServer :IDisposable
    {
        private TcpListener listener;

        private Boolean _running;
        public Boolean Running {get {return _running;} protected set {_running = value;} }

        public AlarmServer(int port) : this(IPAddress.Parse("192.168.200.25"), port) { }

        public AlarmServer(IPAddress address, int port) {
            listener = new TcpListener(address, port);
        }

        public void Run(ISecuritySystem device) {
            try {
                Console.WriteLine("Entered Run Routine");
                listener.Start();

                Running = true;

                //Main Server Loop
                while (true) {
                    using (TcpClient client = listener.AcceptTcpClient()) {
                        Console.WriteLine("Connection Made");
                        /*Task.Run(() => */HandleClient(client, device, TimeSpan.FromSeconds(5));                    
                    }
                }

            } catch (Exception ex) {
                //TODO: Make a log file
                Console.WriteLine(ex.Message);
            } finally {
                listener.Stop();
                Running = false;
            }
        }

        private void HandleClient(TcpClient client, ISecuritySystem device, TimeSpan timeout) {
            List<Byte> buffer = new List<Byte>();

            using (NetworkStream stream = client.GetStream()) {
                int byteRead = 0;
                DateTime startTime = DateTime.Now;
                while ((buffer.Count < 2) && (DateTime.Now - startTime <= timeout)) {
                    byteRead = stream.ReadByte();
                    if (byteRead != -1) {
                        buffer.Add((Byte)byteRead);
                    }
                }

                AlarmCommand request;
                if (AlarmCommand.TryParse(buffer.ToArray(), out request)) {
                    ProcessRequest(device, request, stream);
                }
            }
        }

        private void ProcessRequest(ISecuritySystem device, AlarmCommand request, NetworkStream stream) {
            AlarmResponse response;
            
            switch (request.Command) {
                case AlarmCommandEnum.Arm:
                    device.Arm();
                    response = new AlarmResponse(request.Command, (byte)(device.State == AlarmState.Armed ? 1 : 0));
                    break;
                case AlarmCommandEnum.Disarm:
                    device.Disarm();
                    response = new AlarmResponse(request.Command, (byte)(device.State == AlarmState.Disarmed ? 1 : 0));
                    break;
                case AlarmCommandEnum.RequestState:
                    response = new AlarmResponse(request.Command, (byte)device.State);
                    break;
                default:
                    return;
            }

            Byte[] responseBytes = response.ToBytes();
            stream.Write(responseBytes, 0, responseBytes.Length);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
