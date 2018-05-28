using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MuteVolume
{
    static class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool supportVolume = false;
            int supportVolumeValue = 0;

            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    if (supportVolume)
                    {
                        VolumeControl.Set(supportVolumeValue);
                    }
                }
            });
            
            while (true)
            {
                //---listen at the specified IP and port no.---
                // IPAddress localAdd = IPAddress.Parse(SERVER_IP);
                TcpListener listener = new TcpListener(IPAddress.Any, PORT_NO);
                Console.WriteLine("Listening...");
                listener.Start();

                //---incoming client connected---
                TcpClient client = listener.AcceptTcpClient();

                //---get the incoming data through a network stream---
                NetworkStream nwStream = client.GetStream();
                byte[] buffer = new byte[client.ReceiveBufferSize];

                //---read incoming stream---
                int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                //---convert the data received into a string---
                string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received : " + dataReceived);

                try
                {
                    if (dataReceived.StartsWith("Set "))
                    {
                        var volume = int.Parse(dataReceived.Substring(4));
                        VolumeControl.Set(volume);
                    }

                    if (dataReceived.StartsWith("Support OFF"))
                    {
                        supportVolume = false;
                    }
                    else
                    {
                        if (dataReceived.StartsWith("Support "))
                        {
                            var volume = int.Parse(dataReceived.Substring(8));
                            supportVolume = true;
                            supportVolumeValue = volume;
                            VolumeControl.Set(volume);
                        }
                    }                    
                }
                catch (Exception)
                {
                }

                //---write back the text to the client---
                Console.WriteLine("Sending back : " + dataReceived);
                nwStream.Write(buffer, 0, bytesRead);
                client.Close();
                listener.Stop();
                Console.ReadLine();
            }
        }
    }
}
