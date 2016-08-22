using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml;
using System.IO;
using System.Configuration;

namespace DigiturkApp1
{
    class MyServer
    {
        public static void Main(string[] args)
        {
            TcpListener listener = null;
            TcpClient client = null;
            string rootFolder = ConfigurationManager.AppSettings["rootFolder"];
            try
            {
                IPAddress localhost = IPAddress.Parse("127.0.0.1");
                listener = new TcpListener(localhost, 9091);
                listener.Start();

                Byte[] buffer = new Byte[256];
                String data;
                int byteCount;

                while (true)
                {
                    string xmlStr = string.Empty;
                    Thread.Sleep(10);
                    Console.Write("Waiting for a connection... ");
                    
                    client = listener.AcceptTcpClient();

                    Console.WriteLine("Connected!");
                    NetworkStream stream = client.GetStream();

                    while ((byteCount = stream.Read(buffer, 0, buffer.Length)) == buffer.Length)
                    {
                        data = System.Text.Encoding.ASCII.GetString(buffer, 0, byteCount);
                        xmlStr = String.Concat(xmlStr, data);
                    }

                    data = System.Text.Encoding.ASCII.GetString(buffer, 0, byteCount);
                    xmlStr = String.Concat(xmlStr, data);
                    Console.WriteLine("Received: {0}", xmlStr);
                    byte[] finalMessage = System.Text.Encoding.ASCII.GetBytes("Success!");
                    stream.Write(finalMessage, 0, finalMessage.Length);

                    client.Close();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlStr);
                    doc.RemoveChild(doc.FirstChild);
                    string creationDate = doc.DocumentElement.SelectSingleNode("/Data/Creation/Date").InnerText.Substring(0,10);
                    string dataType = doc.DocumentElement.SelectSingleNode("/Data/Type").InnerText;
                    string pathString = String.Concat(rootFolder, dataType, "-", creationDate, ".log");


                    //Console.WriteLine("PathString: {0}\n", pathString);
                    //using (FileStream fs = File.Create(pathString))
                    //{

                    //}
                    //XmlNode removodNode = doc.RemoveChild(doc.FirstChild);
                    //Console.WriteLine("Removed Node: {0}\n", removodNode.ToString());

                    string jsonStr = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented);
                    //Console.WriteLine("Json: {0}", jsonStr);
                    
                    if (!File.Exists(pathString))
                    {
                        using (StreamWriter sw = File.CreateText(pathString))
                        {
                            sw.WriteLine("1");
                            sw.Write(jsonStr);
                        }
                    }
                    else
                    {
                        string line;
                        string tempPath = String.Concat(rootFolder, "temp_", dataType, "-", creationDate, ".log");
                        File.Move(pathString, tempPath);
                        using (StreamReader oldStream = new StreamReader(tempPath))
                        {
                            int recordCount = Int32.Parse(oldStream.ReadLine());
                            recordCount++;
                            using (StreamWriter newStream = File.CreateText(pathString))
                            {
                                newStream.WriteLine(recordCount);
                                while (!string.IsNullOrEmpty(line = oldStream.ReadLine()))
                                {
                                    newStream.WriteLine(line);
                                }
                                newStream.Write(jsonStr);
                            }
                        }
                        File.Delete(tempPath);
                    }

                    Console.WriteLine("File created successfully\n");

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                listener.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();

        }
    }
}
