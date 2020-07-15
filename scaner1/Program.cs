using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace scaner1
{
    class Program
    {
        public static ManualResetEvent connectDone = new ManualResetEvent(false);

        private static string host = "172.24.188.21";
        private const string host2 = "172.24.188.52";

        private static List<string> ipAddressList = new List<string>();

        private const string dot = ".";

        private static string[] allLanInterfaces;

        private static string mainLanInterface;

        private static string maskOfMainLanInterface;

        static void Main(string[] args)
        {
       
            mainLanInterface = GetAllLocalIPv4(NetworkInterfaceType.Ethernet).FirstOrDefault();

            IPAddress ipOfMainLanInterface = IPAddress.Parse(mainLanInterface);

            maskOfMainLanInterface = GetSubnetMask(ipOfMainLanInterface).ToString();

            Console.WriteLine("Subnet mask: {0}", maskOfMainLanInterface);



            for (int i = 1; i < 255; i++)
            {
                int pos = mainLanInterface.LastIndexOf(dot);
                mainLanInterface = mainLanInterface.Substring(0, pos + 1);
                mainLanInterface += i.ToString();

                ipAddressList.Add(mainLanInterface);
            }
            
            //ipAddressList.Remove(GetAllLocalIPv4(NetworkInterfaceType.Ethernet).FirstOrDefault());


            int listLenght = ipAddressList.Count();

            Scan("172.24.188.1");

            for (int i = listLenght-1; i >= 0; i--)
            {
                bool result = Scan(ipAddressList[i]);
                if (result == false)
                {
                    ipAddressList.Remove(ipAddressList[i]);
                }
                //Console.WriteLine(ipAddressList[i]);
                //Console.WriteLine(i);
            }

            listLenght = ipAddressList.Count();


            Console.WriteLine("This is the port scanner v1.0");

            allLanInterfaces = GetAllLocalIPv4(NetworkInterfaceType.Ethernet);
            for(int n = 0; n < allLanInterfaces.Length; n++)
            {
                Console.WriteLine(allLanInterfaces[n]);
            }

            //Scan(ipAddressList);

            PingHost(host2);

        }

        public static bool Scan(string hostM)
        {
            bool scannable = false;
            //IPAddress IpAddr = IPAddress.Parse(mainLanInterface);
            IPAddress IpAddr = IPAddress.Parse(hostM);

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
                    scannable = false;
                }
                else
                {
                    MySoc.Close();
                    Console.WriteLine("Port 8888 is opened.");
                    scannable = true;
            }
            return scannable;

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

        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            if (pingable == true)
            {
                Console.WriteLine("Ping is correct");
            }

            return pingable;
        }

            public static IPAddress GetSubnetMask(IPAddress address)
            {
                foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
                {
                    foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            if (address.Equals(unicastIPAddressInformation.Address))
                            {
                                return unicastIPAddressInformation.IPv4Mask;
                            }
                        }
                    }
                }
                throw new ArgumentException($"Can't find subnetmask for IP address '{address}'");
            }
        }
}
