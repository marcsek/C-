using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Linq;
namespace ConsoleCS
{
    class Program
    {
        static void Main(string[] args)
        {
            List<KeyStroke> detectedKeys = new List<KeyStroke>();
            ClientNetwork clientNetwork = new ClientNetwork();

            clientNetwork.startServer();
            while (true)
            {
                char key = Console.ReadKey().KeyChar;
                if (key != 0 && key!=27 && key !=9)
                {
                    /*Console.Clear();*/
                    clientNetwork.sendMessageToServer("Time: "+ DateTime.Now.ToString()+" Key: "+key);
                    detectedKeys.Add(new KeyStroke(DateTime.Now.ToString(), key));
                }
                else if (key == 27)
                {
                    Console.WriteLine("\n");
                    for (int i = 0; i < detectedKeys.Count; i++)
                    {
                        Console.WriteLine("Time: {0} Key: {1}", detectedKeys[i].time, detectedKeys[i].key);
                    }
                    clientNetwork.disconnectFromServer();
                }
                else if (key == 9)
                {
                    for (int i = 0; i < detectedKeys.Count; i++)
                    {
                        var path = @"C:\Users\jakub\source\repos\ConsoleCS\daco.txt";
                        string text = detectedKeys[i].time +" "+ Char.ToString(detectedKeys[i].key);
                        File.AppendAllText(path, text + Environment.NewLine);
                    }
                    clientNetwork.sendFileToServer();
                }
            }
        }
        public class KeyStroke
            {
                public string time;
                public char key;

                public KeyStroke(string time, char key)
                {
                    this.time = time;
                    this.key = key;
                }
            }
    }
    class ClientNetwork
    {
        TcpClient client;
        NetworkStream stream;
        StreamReader sr;
        public void startServer()
        {
            connection:
            try
            {
                client = new TcpClient("127.0.0.1", 1302);
                stream = client.GetStream();
                sr = new StreamReader(stream);

                sendMessageToServer("DEbil");
            }
            catch(Exception e)
            {
                Console.WriteLine("connection failed");
                goto connection;
            }
        }
        public void sendMessageToServer(string message)
        {
            try
            {
                int byteCount = Encoding.ASCII.GetByteCount(message+1);
                byte[] byteData = Encoding.ASCII.GetBytes(message);
                Array.Resize(ref byteData, byteCount);

                byteData = prepareByteArray(0, byteData);
                Console.WriteLine("sending message ");
                stream.Write(byteData, 0, byteData.Length);

                string response = sr.ReadLine();
                Console.WriteLine(response);
            }
            catch(Exception e)
            {
                Console.WriteLine("failed to send (trying to reconnect)");
                disconnectFromServer();
                startServer();
            }
        }
        public void sendFileToServer()
        {
            try
            {
                byte[] byteData = System.IO.File.ReadAllBytes(@"C:\Users\jakub\source\repos\ConsoleCS\daco.txt");
                Array.Resize(ref byteData, byteData.Length + 1);

                byteData = prepareByteArray(1, byteData);

                Console.WriteLine("sending file");
                stream.Write(byteData, 0, byteData.Length);

                string response = sr.ReadLine();
                Console.WriteLine(response);
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to send (trying to reconnect)");
                disconnectFromServer();
                startServer();
            }
        }

        private byte[] prepareByteArray(int fileType, byte[] byteData)
        {
            byte medzi = 0;
            byte temp = 0;
            for (int i = 0; i < byteData.Length; i++)
            {
                if (i == 0)
                {
                    medzi = byteData[i + 1];
                    byteData[i + 1] = byteData[i];
                    byteData[i] =(byte) fileType;
                }
                else if (i < byteData.Length - 1)
                {
                    temp = byteData[i + 1];
                    byteData[i + 1] = medzi;
                    medzi = temp;
                }
            }
            return byteData;
        }

        public void disconnectFromServer()
        {
            client.Close();
            stream.Close();
            sr.Close();
        }
    }
}
