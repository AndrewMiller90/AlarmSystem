using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents
{
    public enum AlarmState : byte {
        Disarmed = 0,
        Armed = 1,
        Triggered = 2,
    }

    public enum AlarmCommandEnum : byte {
        RequestState = 1,
        Arm = 2,
        Disarm = 3
    }

    public class AlarmResponse  {
        public AlarmCommandEnum CommandCodeEcho { get; set; }
        public Byte ResponseData { get; set; }

        public AlarmResponse() { }
        public AlarmResponse(AlarmCommandEnum commandCodeEcho, Byte responseData) {
            this.CommandCodeEcho = commandCodeEcho;
            this.ResponseData = responseData;
        }

        public Byte[] ToBytes() {
            Byte[] bytes = { (byte) CommandCodeEcho, ResponseData };
            return bytes.Concat(new byte[] { Crc8.ComputeChecksum(bytes) } ).ToArray();
        }

        public static AlarmResponse Parse(Byte[] bytes) {
            if (bytes == null) throw new ArgumentNullException("no expression provided");
            if ((Crc8.ComputeChecksum(bytes) != 0) || (bytes.Length != 3)) throw new Exception("Unable to parse expression");

            return new AlarmResponse((AlarmCommandEnum)bytes[0], bytes[1]);
        }

        public static Boolean TryParse(Byte[] bytes, out AlarmResponse response) {
            response = null;

            if (bytes == null) return false;
            if ((Crc8.ComputeChecksum(bytes) != 0) || (bytes.Length != 3)) return false;

            response = new AlarmResponse((AlarmCommandEnum)bytes[0], bytes[1]);
            return true;
        }

    }

    public class AlarmCommand
    {
        public AlarmCommandEnum Command { get; set; }

        public AlarmCommand() { }
        public AlarmCommand(AlarmCommandEnum command) {
            this.Command = command;
        }
        
        public Byte[] ToBytes() {
            Byte[] bytes = { (byte) Command };
            return bytes.Concat(new byte[] { Crc8.ComputeChecksum(bytes) } ).ToArray();
        }

        public static AlarmCommand Parse(Byte[] bytes) {
            if (bytes == null) throw new ArgumentNullException("no expression provided");
            if ((Crc8.ComputeChecksum(bytes) != 0) || (bytes.Length != 2)) throw new Exception("Unable to parse expression");

            return new AlarmCommand((AlarmCommandEnum) bytes[0] );
        }

        public static Boolean TryParse(Byte[] bytes, out AlarmCommand command) {
            command = null;

            if (bytes == null) return false;
            if ((Crc8.ComputeChecksum(bytes) != 0) || (bytes.Length != 2)) return false;

            command = new AlarmCommand((AlarmCommandEnum)bytes[0]);
            return true;
        }
    }

    internal static class Crc8 {
        static byte[] table = new byte[256];
        // x8 + x7 + x6 + x4 + x2 + 1
        const byte poly = 0xd5;

        public static byte ComputeChecksum(params byte[] bytes) {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0) {
                foreach (byte b in bytes) {
                    crc = table[crc ^ b];
                }
            }
            return crc;
        }

        static Crc8() {
            for (int i = 0; i < 256; ++i) {
                int temp = i;
                for (int j = 0; j < 8; ++j) {
                    if ((temp & 0x80) != 0) {
                        temp = (temp << 1) ^ poly;
                    }
                    else {
                        temp <<= 1;
                    }
                }
                table[i] = (byte)temp;
            }
        }
    }
}
