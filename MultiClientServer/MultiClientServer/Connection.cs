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

            attemptConnection:
            try { client = new TcpClient("localhost", port); }
            catch
            {
                Thread.Sleep(5);
                goto attemptConnection;
            }

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
                    Row tuple;

                    switch (input[0])
                    {
                        case "MES":
                            Console.WriteLine(string.Join(" ", temp));
                            break;
                        case "REC":
                            bool change = false;
                            for (int i = 0; i < Int32.Parse(temp[0]); i++)
                            {
                                tuple = Row.FromS(Read.ReadLine());
                                lock (Program.tableLock) { 
                                    lock (Program.neighbourLock)
                                    {
                                        int newDest = tuple.Data.Item1;
                                        Row old;
                                        bool Faster = true;                     //bool fix

                                        for (int j = 0; j < Program.routingTable.Count; j++)
                                        {
                                            old = Program.routingTable[j];
                                            //Check for an existing connection, if so then check if the new entry would be faster
                                            if (old.Data.Item1 == newDest)
                                            {

                                                if (tuple.Data.Item2 + 1 < old.Data.Item2)
                                                {
                                                    Program.routingTable.Remove(Program.routingTable[j]);
                                                    j--; // To not skip an element
                                                    Program.routingTable.Add(new Row(newDest, tuple.Data.Item2 + 1, Int32.Parse(temp[1]))); //temp[1] is the port of the server this thread belongs to
                                                    change = true;
                                                    Faster = false;
                                                }
                                                else
                                                {

                                                    Faster = false; // boolfix
                                                }
                                            }
                                        }
                                        if (Faster)
                                        {
                                            Program.routingTable.Add(new Row(newDest, tuple.Data.Item2 + 1, Int32.Parse(temp[1])));
                                            change = true;
                                        }
                                    }

                                }
                            }
                            if (change) Program.BroadcastTable();
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
