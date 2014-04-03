﻿using System.Collections.Generic;
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
            //public static string hash = "e9b8240f02d8f1599d85c9496a86f965"; //proper assignment hash - 412819815
            public static string hash = "fcea920f7412b5da7be0cf42b8c93759"; //1234567
            public static string logPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\test\\udpLog.txt";
            public static int crackPos = 0;
            public static int crackInt = 1000000;
        }

        static void Main(string[] args)
        {
            //make all the threads
            Thread hashThread = null;
            Thread posThread = null; 
            Thread listenThread = null;
            
            Console.WriteLine("Server has Started");

            //bind all the threads
            hashThread = new Thread(new ThreadStart(sendHash));
            posThread = new Thread(new ThreadStart(sendPos));
            listenThread = new Thread(new ThreadStart(Listener));

            //start all the threads
            hashThread.Start();
            posThread.Start();
            listenThread.Start();

            Console.WriteLine("All Threads Started. Cracking Ahoy!");  //needs user feedback to kill threads
            Console.ReadLine();
            
            //kill all the threads
            hashThread.Abort();
            posThread.Abort();
            listenThread.Abort();

            Console.WriteLine("All Threads Killed. Much Success, Many Hash");
            Console.ReadLine();
            
            Environment.Exit(0);  //kill the application and all threads
        }

        static void sendHash()
        {
            for (; ; )
            {
                UdpClient hashSender = new UdpClient();
                IPAddress bcAddress = IPAddress.Parse(IPAddress.Broadcast.ToString());  //get broadcast address
                Byte[] sendBytes = new Byte[1024]; 

                hashSender.Connect(bcAddress, 8008);
                sendBytes = Encoding.ASCII.GetBytes(Vars.hash);
                hashSender.Send(sendBytes, sendBytes.GetLength(0)); //send hash to port 8008
                Thread.Sleep(1000); //sleep so we don't flood the queue
            }
        }

        static void sendPos() 
        {
            UdpClient posSender = new UdpClient();
            Byte[] sendBytes = new Byte[1024]; 
            IPAddress bcAddress = IPAddress.Parse(IPAddress.Broadcast.ToString());
            posSender.Connect(bcAddress, 8009);

            for (; ; )
            {
                sendBytes = Encoding.ASCII.GetBytes(Vars.crackPos.ToString());
                posSender.Send(sendBytes, sendBytes.GetLength(0)); 
                Thread.Sleep(1000); //sleep for 1 second
            }
        }

        static void Listener() 
        {
            UdpClient cliListener = new UdpClient(8010);
            string returnData = "";

            Byte[] recieveBytes = new Byte[1024];
            IPEndPoint listenerEndPoint = new IPEndPoint(IPAddress.Any, 8010);

            recieveBytes = cliListener.Receive(ref listenerEndPoint);
            returnData = Encoding.ASCII.GetString(recieveBytes);
            
            if (returnData.Substring(0, 4) == "next") //check if the client has found the hash
            {
                Vars.crackPos = Vars.crackPos + Vars.crackInt;
                Log("Checked Up To: " + Vars.crackPos.ToString()); //log on position recieved
            }
            else
            {
                Log("Hash Found: " + returnData.Substring(6).ToString()); //log on hash found
                Console.WriteLine("Hash Found: " + returnData.Substring(6).ToString());
                Thread.CurrentThread.Abort(); //kill the thread
            }

            Console.WriteLine("Next Offering: " + Vars.crackPos); //position offered to the next client that connects

            cliListener.Close();
            Listener(); //recursive call
        }

        static void Log(string data)
        {
            System.IO.StreamWriter sr = new System.IO.StreamWriter(Vars.logPath, true);
            sr.WriteLine(data); //writes current lowest position
            sr.Close();
        }
    }
}
