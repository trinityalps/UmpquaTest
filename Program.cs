using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text.RegularExpressions;
using System.IO;

namespace test
{
    public class Program
    {
        static List<Listener> listeners = new List<Listener>();
        public static int port = 4000;
        public static int uentries;
        public static int dentries;
        public static int usum;

        static void Main(string[] args)
        {
            String filename = "numbers.log";
            File.Create(AppDomain.CurrentDomain.BaseDirectory + @"\" + filename);
            Console.WriteLine("TCP Server started on Port: " + port);
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Starting listener: " + i);
                Listener tcplistener;
                tcplistener = new Listener();
                tcplistener.serverstart();
                listeners.Add(tcplistener);
            }
            Thread logger = new Thread(new ThreadStart(logging));
            logger.Start();
        }

        public static void terminate()
        {
            foreach (Listener listen in listeners)
            {
                listen.terminate();
            }
            Console.WriteLine("Every listener has been closed");
        }

        static void logging()
        {
            while (true)
            {
                //            i.The difference since the last report of the count of new unique numbers that have been received.

                //                 ii.The difference since the last report of the count of new duplicate numbers that have been received.

                //        iii.The total number of unique numbers received for this run of the Application.

                //iv.Example text for #8: Received 50 unique numbers, 2 duplicates. Unique total: 567231
                Console.WriteLine("Recieved " + uentries + " unique numbers, " + dentries + " duplicates. Unique total: " + usum);
                Thread.Sleep(1000 * 10); 
            }
        }


    }
    public class Listener
    {
        Thread listenThread;
        string mes;
        TcpListener tcplistener;
        String regex = "^[0-9]{9}$";
        String filename = "numbers.log";


        public void serverstart()
        {
            this.tcplistener = new TcpListener(IPAddress.Any, Program.port);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }
        private void ListenForClients()
        {
            while (true)
            {
                Thread clientThread = new Thread(new ParameterizedThreadStart(TCPComm));
            }
        }
        private void TCPComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            byte[] message = new byte[512];
            int bytesRead;
            while (true)
            {
                bytesRead = 0;
                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 512);
                }
                catch
                {
                    break;
                }
                if (bytesRead == 0)
                {
                    break;
                }
                ASCIIEncoding encoder = new ASCIIEncoding();
                mes = encoder.GetString(message, 0, bytesRead);
                if (mes == "terminate")
                {
                    Program.terminate();
                }
                bool ismatch = Regex.IsMatch(mes, regex, RegexOptions.Singleline);
                if (ismatch)
                {
                    mes = mes + ",";
                    String fullfile = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\" + filename);
                    String[] s = fullfile.Split(',');
                    if (s.Contains(mes)) {
                        Program.dentries++;
                        terminate();
                    }
                    else
                    {
                        Program.uentries++;
                        Program.usum++;
                        mes = mes + ",";
                        File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + @"\" + filename, mes);
                    }
                }
            }
        }

        public void terminate()
        {
            tcplistener.Stop();
        }
    }
}