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
                    Buffer.BlockCopy(bytes, 0, quaternion, 0, bytes.Length);
                    Console.WriteLine("Received broadcast from {0} :\n [{1}]\n", remoteIPEndPoint.ToString(),
                        string.Join(", ", quaternion));
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
    }
}
