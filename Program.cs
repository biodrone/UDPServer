using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System;

namespace UDPServer
{
    class Program
    {
        public class Vars //place for all the global variables
        {
            public static string hash = "e9b8240f02d8f1599d85c9496a86f965"; //proper assignment hash
            public static string logPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\test\\udpLog.txt"; //desktop
            public static int crackPos = 0;
            public static int crackInt = 1000000;
        }
        static void Main(string[] args)
        {
            //make all the threads
            Thread ThreadHash = null;
            Thread ThreadPos = null; 
            Thread ThreadListen = null;
            Thread ThreadLog = null;
            
            Console.WriteLine("Server has Started");
            Console.WriteLine("HERE COMES THE MOOOOONEEEEEEY");

            //bind all the threads
            ThreadHash = new Thread(new ThreadStart(sendHash));
            ThreadPos = new Thread(new ThreadStart(sendPos));
            ThreadListen = new Thread(new ThreadStart(Listener));
            ThreadLog = new Thread(new ThreadStart(Logger));

            //start all the threads
            ThreadHash.Start();
            ThreadPos.Start();
            ThreadListen.Start();
            ThreadLog.Start();

            Console.WriteLine("All Threads Started. Cracking Ahoy!");  //needs user feedback to kill threads
            Console.ReadLine();
            
            //kill all the threads
            ThreadHash.Abort();
            ThreadPos.Abort();
            ThreadListen.Abort();
            ThreadLog.Abort();

            Console.WriteLine("All Threads Killed. Much Success, Many Hash");
            Console.ReadLine();
            
            Environment.Exit(0);  //kill the application and all threads
        }

        static void sendHash()
        {
            for (; ; )
            {
                UdpClient udpClient = new UdpClient();
                IPAddress address = IPAddress.Parse(IPAddress.Broadcast.ToString());  //get broadcast address
                Byte[] sendBytes = new Byte[1024]; 

                udpClient.Connect(address, 8008);
                sendBytes = Encoding.ASCII.GetBytes(Vars.hash);
                udpClient.Send(sendBytes, sendBytes.GetLength(0)); //send hash to port 8008 on broadcast
                Thread.Sleep(1000); //sleep so we don't flood the queue
            }
        }

        static void sendPos() //finished
        {
            UdpClient udpClient2 = new UdpClient();
            Byte[] sendBytes = new Byte[1024]; 
            IPAddress address = IPAddress.Parse(IPAddress.Broadcast.ToString());
            udpClient2.Connect(address, 8009);

            for (; ; )
            {
                sendBytes = Encoding.ASCII.GetBytes(Vars.crackPos.ToString()); //sends as string, have to reconvert to int on client end
                udpClient2.Send(sendBytes, sendBytes.GetLength(0)); 
                Thread.Sleep(1000); //sleep for 1 second
            }
        }

        static void Listener() //finished
        {

            UdpClient udpClient3 = new UdpClient(8010);
            string returnData = "";

            Byte[] recieveBytes = new Byte[1024];
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 8010);

            recieveBytes = udpClient3.Receive(ref remoteIPEndPoint);
            returnData = Encoding.ASCII.GetString(recieveBytes);

            if (returnData == "found")
            {
                //handle found hash conditions here
            }
            else
            {
                Vars.crackPos = Vars.crackPos + Vars.crackInt;
            }

            //Vars.crackPos = Convert.ToInt32(returnData);
            Console.WriteLine("Next Offering: " + Vars.crackPos); //position recieved from server

            udpClient3.Close();

            Listener();
        }

        static void Logger() //finished
        {
            for (; ; )
            {
                if (Vars.crackPos != 0) //shouldn't log for 0
                {
                    System.IO.StreamWriter sr = new System.IO.StreamWriter(Vars.logPath);
                    sr.WriteLine(Vars.crackPos.ToString()); //writes current lowest position
                    sr.Close();

                    Thread.Sleep(2000); //dont want to thrash the balls out of the disk
                }


            }
            //Logger();
        }
    }
}
