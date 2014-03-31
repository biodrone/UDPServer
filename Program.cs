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
        public class Vars
        {
            public static string hash = "e9b8240f02d8f1599d85c9496a86f965"; //proper assignment hash
            public static string logPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); //desktop
            public static int crackPos = 0;
        }
        static void Main(string[] args)
        {
            //make all the threads
            Thread ThreadHash = null;
            Thread ThreadPos = null; 
            Thread ThreadListen = null;
            Thread ThreadLog = null;
            
            Console.WriteLine("Server has Started");

            //bind all the threads
            ThreadHash = new Thread(new ThreadStart(sendHash));
            ThreadPos = new Thread(new ThreadStart(sendPos)); //bind the threads to functions
            ThreadListen = new Thread(new ThreadStart(Listener));
            ThreadLog = new Thread(new ThreadStart(Logger));

            //start all the threads
            ThreadHash.Start();
            ThreadPos.Start();
            ThreadListen.Start();
            ThreadLog.Start();

            Console.WriteLine("All Threads Started. Cracking Ahoy!");  //user feedback
            Console.ReadLine();
            
            //kill all the threads
            ThreadHash.Abort();
            ThreadPos.Abort();
            ThreadListen.Abort();
            ThreadLog.Abort();

            Console.WriteLine("All Threads Killed. Much Success, Many Hash");
            
            Environment.Exit(0);  //kill the application and all threads
        }

        static void sendHash()
        {
            UdpClient udpClient = new UdpClient();
            IPAddress address = IPAddress.Parse(IPAddress.Broadcast.ToString());  //get broadcast address
            Byte[] sendBytes = new Byte[1024]; // buffer to read the data into 1 kilobyte at a time

            udpClient.Connect(address, 8008); //open a connection to that location on port 8008
            sendBytes = Encoding.ASCII.GetBytes(Vars.hash);
            udpClient.Send(sendBytes, sendBytes.GetLength(0)); //send information to the port

            //udpClient.Close();  //dont know where to put this one, maybe need a loop?
        }

        static void sendPos()
        {
            //this will run having no effect on the main thread
            //which you are entering text

            UdpClient udpClient2 = new UdpClient();
            Byte[] sendBytes = new Byte[1024]; // buffer to send the data 1 Kiltobyte at a time
            IPAddress address = IPAddress.Parse(IPAddress.Broadcast.ToString());
            udpClient2.Connect(address, 8009); //open a connection to that location on port 8009

            for (; ; )
            {
                sendBytes = Encoding.ASCII.GetBytes(Vars.crackPos.ToString()); //sends as string, have to reconvert to int on client end
                udpClient2.Send(sendBytes, sendBytes.GetLength(0)); //send information to the port
                Thread.Sleep(1000);//sleep for 1 second
            }
        }

        static void Listener() //to listen for messages from clients
        {
            Byte[] clientPleaser = new Byte[1024];
            string test = "";

            for (; ; )
            {
                IPEndPoint client = new IPEndPoint(IPAddress.Any, 8010);  //open port 8010
                UdpClient srvListener = new UdpClient();
                clientPleaser = srvListener.Receive(ref client);
                //next 2 lines should work to convert input from client into an integer !!!RUN IT WITH DEBUG!!!
                test = Encoding.ASCII.GetString(clientPleaser); //needs to be an int so that it can go into the crackPos and be used
                Vars.crackPos = Convert.ToInt32(test);
            }
        }

        static void Logger() //not finished
        {
            int i = 0;

            System.IO.StreamWriter sr = new System.IO.StreamWriter(Vars.logPath); //not appending because the file would be fucking huge
            for (; ; )
            {
                
                sr.WriteLine(Vars.crackPos.ToString()); //writes current lowest position
                sr.WriteLine(i);
                Thread.Sleep(2000); //dont want to thrash the balls out of the disk

                i++;
                sr.Close();
            }
        }
    }
}
