using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents {
    class Program {
        public static void Main() {
            Byte[] array = { 1, 2, 3 };
            Console.WriteLine(Crc8.ComputeChecksum(array));
            Console.WriteLine(Crc8.ComputeChecksum(1, 2, 3));
        }
    }
}
