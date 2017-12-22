using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace MultiClientServer
{
    class Connection
    {
        public StreamReader Read;
        public StreamWriter Write;

        // Connection has 2 constructors: this constructor gets used when we become a CLIENT to another SERVER
        public Connection(int port)
        {
            TcpClient client;
            client = new TcpClient("localhost", port);
            //Console.WriteLine(client.Connected);
            while (!client.Connected)
            {
                Console.WriteLine("Sleep");
                Thread.Sleep(1);
                client.Connect("localhost", port);
            }

            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // The server can't see which port we're a client of, we need to signal this seperately
            Write.WriteLine("Port: " + Program.port);

            // Start the reader loop
            new Thread(ReaderThread).Start();
        }

        // This constructor gets used when we're the SERVER and a CLIENT establishes a connection with us
        public Connection(StreamReader read, StreamWriter write)
        {
            Read = read; Write = write;

            // Start the reader loop
            new Thread(ReaderThread).Start();
        }

        // WATCH OUT: After connection has been established, one can forget the client/server (you won't be able to retrive this from the connection-object either!)

        // This loop reads incoming messages and prints them
        public void ReaderThread()
        {
            try
            {
                while (true)
                {
                    string[] input = Read.ReadLine().Split();
                    string[] temp = input.Skip(1).ToArray();

                    switch (input[0])
                    {
                        case "MES":
                            Console.WriteLine(string.Join(" ", temp));
                            break;
                        case "REC":
                            //HANDLE REC
                            break;
                        default:
                            Console.WriteLine("Client made an invalid request");
                            break;
                    }
                }
            }
            catch { Console.WriteLine("READ EXCEPTION"); } // Connection is broken
        }
    }
}
