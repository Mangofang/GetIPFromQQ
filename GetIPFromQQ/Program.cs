using System;
using SharpPcap;
using SharpPcap.LibPcap;
using PacketDotNet;
using System.Text.RegularExpressions;
using System.Threading;

namespace GetIPFromQQ
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("正在检测当前网卡信息...");
            var devices = LibPcapLiveDeviceList.Instance;
            int count = 0;
            foreach (var i in devices)
            {
                Console.WriteLine(string.Format("[{0}] ",count) + i.Interface.FriendlyName);
                count++;
            }
            point_1:
            Console.WriteLine("选择监听的目标网卡编号");
            try
            {
                count = int.Parse(Console.ReadLine());
            }
            catch (Exception ex)
            {
                Console.WriteLine("请确认网卡编号是否有误！");
                goto point_1; 
            }
            var device = devices[count];
            device.Open(DeviceMode.Promiscuous);
            string filter = "udp";
            device.Filter = filter;
            Console.WriteLine("按任意键可退出程序...");
            Console.WriteLine("正在捕获目标地址...");

            Thread.Sleep(2000);

            device.OnPacketArrival += new PacketArrivalEventHandler(PacketHandler);
            device.StartCapture();
            Console.ReadKey();
            device.StopCapture();
            device.Close();
        }
        private static void PacketHandler(object sender, CaptureEventArgs e)
        {
            var ent = EthernetPacket.ParsePacket(LinkLayers.Ethernet, e.Packet.Data);
            var ip = ent.PayloadPacket;
            var udp = ip.PayloadPacket;
            var data_len = udp.PayloadData.Length;
            Match match = Regex.Match(ip.ToString(), @"DestinationAddress=(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})");
            string DestinationAddress = match.Groups[1].Value;
            if (data_len == 72 && !DestinationAddress.Contains("192.168"))
            {
                Console.WriteLine("已捕获目标IP：" + DestinationAddress);
            }
        }
    }
}
