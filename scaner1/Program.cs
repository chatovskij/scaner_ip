using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;


namespace scaner1
{
    class Program
    {
        public static ManualResetEvent connectDone = new ManualResetEvent(false);

        private const string host = "172.24.188.21";

        static void Main(string[] args)
        {
            Console.WriteLine("This is the port scanner");
            Console.WriteLine(GetAllLocalIPv4(NetworkInterfaceType.Ethernet).FirstOrDefault());
            Console.WriteLine(GetAllLocalIPv4(NetworkInterfaceType.Wireless80211).FirstOrDefault());
            Scan();
        }

        public static void Scan()
        {
            IPAddress IpAddr = IPAddress.Parse(host);

                //Create socket
                IPEndPoint IpEndP = new IPEndPoint(IpAddr, 8888);
                Socket MySoc = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Stream, ProtocolType.Tcp);
                //Trying to connect to the needed host
                IAsyncResult asyncResult = MySoc.BeginConnect(IpEndP,
                                 new AsyncCallback(ConnectCallback), MySoc);


                if (!asyncResult.AsyncWaitHandle.WaitOne(30, false))
                {
                    MySoc.Close();
                    Console.WriteLine("Port 8888 is closed.");
                }
                else
                {
                    MySoc.Close();
                    Console.WriteLine("Port 8888 is opened.");
                }

        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket SockClient = (Socket)ar.AsyncState;
                SockClient.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception e)
            {

            }
        }

        public static string[] GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }
    }
}
