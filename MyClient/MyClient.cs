using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyClient
{
    class MyClient
    {
        static void Main(string[] args)
        {
            //Task t1, t2;
            for (int i = 0; i < 401; i++)
            {
                MakeRequest("C:\\Users\\ilke\\Documents\\TestFile1.txt", "Task1");
            }

            //t1 = Task.Factory.StartNew(() => MakeRequest("C:\\Users\\ilke\\Documents\\TestFile1.txt", "Task1"));
            //t2 = Task.Factory.StartNew(() => MakeRequest("C:\\Users\\ilke\\Documents\\TestFile2.txt", "Task2"));
            //t1.Wait();
            //t2.Wait();

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();

        }

        static void MakeRequest(string pathString, string name)
        {
            try
            {
                string message = "";
                using (StreamReader sr = new StreamReader(pathString))
                {
                    message = sr.ReadToEnd();
                }
                TcpClient client = new TcpClient("localhost", 9091);
                while (!client.Connected)
                {
                    Thread.Sleep(10);
                    client.Connect("localhost", 9091);
                }
                Console.WriteLine("{0} connected!\n", name);

                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                Console.WriteLine("{0} Sent: {1}", name, message);

                Byte[] responseData;
                int byteCount;
                String responseString = "";
                while (!responseString.Equals("Success!", StringComparison.Ordinal))
                {
                    Thread.Sleep(10);
                    responseData = new Byte[256];
                    byteCount = stream.Read(responseData, 0, responseData.Length);
                    responseString = System.Text.Encoding.ASCII.GetString(responseData, 0, byteCount);
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        Console.WriteLine("{0} Received: {1}\n", name, responseString);
                    }
                }

                stream.Close();
                client.Close();

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}
