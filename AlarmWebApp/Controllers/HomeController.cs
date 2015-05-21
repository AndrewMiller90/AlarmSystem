using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Sockets;
using SharedComponents;

namespace AlarmWebApp.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        private static AlarmState? State;

        public ActionResult Index()
        {
            return View();
        }

        [HttpGetAttribute]
        public ActionResult AlarmControlView() {
            //If the form was submitted and there's a valid state, toggle it.
            if ((TempData["state"] != null) && (State != null)) {
                AlarmCommandEnum commandValue = 
                    (State == AlarmState.Disarmed ? 
                        AlarmCommandEnum.Arm : 
                        AlarmCommandEnum.Disarm);
                AlarmResponse response = IssueAlarmCommand(new AlarmCommand(commandValue));

                if (response == null || response.ResponseData == 0) return View("Error");
            }
            
            //First time access or toggle state success.
            State = GetState();
            ViewBag.State = State;

            return View();
        }

        [HttpPostAttribute]
        public ActionResult AlarmControlView(AlarmCommand command) {
            TempData["state"] = new object();
            return RedirectToAction("AlarmControlView");
        }

        private AlarmState? GetState() {
            AlarmState? state = null;
            AlarmResponse response = IssueAlarmCommand(new AlarmCommand(AlarmCommandEnum.RequestState));
            if (response != null) state = (AlarmState)response.ResponseData;

            return state;
        }
        
        private AlarmResponse IssueAlarmCommand(AlarmCommand command) {
            using (TcpClient client = new TcpClient()) {
#if DEBUG
                client.Connect(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("192.168.200.25"), 17000));
#else
                client.Connect("reitzmanblue.gotgeeks.com", 17000);
#endif
                Console.WriteLine("ConnectionAccepted");
                using (NetworkStream stream = client.GetStream()) {
                    Byte[] commandBytes = command.ToBytes();
                    stream.Write(commandBytes, 0, commandBytes.Length);

                    Console.WriteLine("Command Sent");

                    return GetResponse(stream, TimeSpan.FromSeconds(10));
                }
            }
        }

        private AlarmResponse GetResponse(NetworkStream stream, TimeSpan timeout) {
            DateTime startTime = DateTime.Now;
            AlarmResponse response = null;

            Byte[] buffer = new Byte[3];
            int bufferPointer = 0;
            while ((DateTime.Now - startTime) <= timeout && (response == null)) {
                int readByte;
                if ((readByte = stream.ReadByte()) != -1) {
                    buffer[bufferPointer] = (byte)readByte;
                    bufferPointer++;
                }

                if (bufferPointer == 3) {
                    AlarmResponse.TryParse(buffer, out response);
                    break;
                }
            }

            return response;
        }

    }
}
