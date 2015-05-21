using SharedComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Socks
{
    class Program
    {
        static void Main(string[] args)
        {
            

#if DEBUG
            AlarmServer server = new AlarmServer(IPAddress.Parse("192.168.200.25"), 17000);
            ISecuritySystem system = new TestSystem();
#else
            AlarmServer server = new AlarmServer(IPAddress.Parse("192.168.200.25"), 17000);
            ISecuritySystem system = new AlarmSystem(AlarmState.Armed);
#endif
            
            while (true) {
                if (!server.Running) {
                    Console.WriteLine("Server is not running");
                    Task t = Task.Run( () => server.Run(system));
                    while (!server.Running) { }
                    Console.WriteLine("Server is Running");
                }

                system.Update();
            } /**/
            
        }
    }
}
