using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CheapVRBridge
{
    class Program
    {
        static void Main(string[] args)
        {
            bool done = false;

            Console.WriteLine("Hello, World!");

            const int listenPort = 11000;
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (!done)
                {
                    byte[] bytes = listener.Receive(ref remoteIPEndPoint);
                    var quaternion = new float[bytes.Length / 4];
                    for (int i = 0; i < 4; i++)
                    {
                        //get the 4 bytes making up the float in big endian form,
                        //swap the bytes, and then get the float from the byte array

                        byte[] floatBytes = { bytes[(i * 4) + 0], bytes[(i * 4) + 1], bytes[(i * 4) + 2], bytes[(i * 4) + 3] };
                        quaternion[i] = ReadSingle(floatBytes, 0, false);
                    }

                    //Console.WriteLine("Received broadcast from {0} :\n [{1}]\n", remoteIPEndPoint.ToString(),
                        //string.Join(", ", quaternion));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
            }
        }

        private static float ReadSingle(byte[] data, int offset, bool littleEndian)
        {
            if (BitConverter.IsLittleEndian != littleEndian)
            {   // other-endian; reverse this portion of the data (4 bytes)
                byte tmp = data[offset];
                data[offset] = data[offset + 3];
                data[offset + 3] = tmp;
                tmp = data[offset + 1];
                data[offset + 1] = data[offset + 2];
                data[offset + 2] = tmp;
            }
            return BitConverter.ToSingle(data, offset);
        }
    }
}
