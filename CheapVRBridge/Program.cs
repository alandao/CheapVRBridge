using System;
using System.Net;
using System.Net.Sockets;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.IO;

namespace CheapVRBridge
{
    class Program
    {
        static void Main(string[] args)
        {
            const int listenPort = 11000;
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Console.WriteLine("Waiting for Moonlight-Android in SteamVR mode to start streaming..");
            //Receive call blocks until we get any udp packet coming in.
            byte[] blockingStatement = listener.Receive(ref remoteIPEndPoint);
            Console.WriteLine("Something broadcasted to us in the limited broadcast, assume it's the phone sending head poses.");
            Console.WriteLine("Beginning while loop of sending quaternion rotation..");

            bool done = false;

            //create a memory mapped file named "moonlightvr", with size 10000
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateNew("moonlightvr", 10000))
            {
                //open a view of our memory mapped file as "stream"
                using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                {

                    //initialize a writer which will write to our memory mapped file
                    BinaryWriter writer = new BinaryWriter(stream);

                    //Create a mutex, preventing any process from mutating our view of our quaternion data while we're writing to it.
                    bool mutexCreated;
                    Mutex mutex = new Mutex(false, "moonlightvrmutex", out mutexCreated);
                    while (!done)
                    {
                        //wait until we can acquire the mutex, the openvr moonlight driver is possibly reading our quaternion data.
                        mutex.WaitOne();

                        //TODO:: Only get the latest quaternion data. Right now we are possibly looking at old quaternion data sent in a queue.
                        byte[] bytes = listener.Receive(ref remoteIPEndPoint);
                        var quaternion = new float[bytes.Length / 4];
                        for (int i = 0; i < 4; i++)
                        {
                            byte[] floatBytes = { bytes[(i * 4) + 0], bytes[(i * 4) + 1], bytes[(i * 4) + 2], bytes[(i * 4) + 3] };
                            quaternion[i] = ReadSingle(floatBytes, 0, false);
                        }

                        //set our writer to position 0 in our stream, so we can keep overwriting our old quaternion data whenever we write.
                        stream.Position = 0;
                        //write quaternion values to memory mapped file
                        foreach (float i in quaternion)
                        {
                            writer.Write(i);
                        }

                        //release the mutex, so our openvr moonlight driver has a chance to read quaternion data.
                        mutex.ReleaseMutex();
                    }
                    writer.Close();
                    listener.Close();
                }
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
