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
                            //check if destination reached
                            if (Int32.Parse(temp[0]) == Program.port)
                            {
                                temp = temp.Skip(1).ToArray(); // remove destination from final message
                                Console.WriteLine(string.Join(" ", temp));
                                break;
                            }
                            //if not pass message to next node                            
                            Program.Message(string.Join(" ", temp));
                            break;
                        case "REC":
                            bool change = false;

                            lock (Program.tableLock)
                            {
                                lock (Program.neighbourLock)
                                {
                                    for (int i = 0; i < Int32.Parse(temp[0]); i++)
                                    {
                                        tuple = Row.FromS(Read.ReadLine());
                                        int newDest = tuple.Data.Item1;
                                        Row old;
                                        bool Faster = true;
                                        Console.WriteLine("broadcast from: " + temp[1]);

                                        if (Program.neighbours.ContainsKey(newDest) || newDest == Program.port) ; //TODO faster?????
                                        else
                                        {
                                            for (int j = 0; j < Program.routingTable.Count; j++)
                                            {
                                                old = Program.routingTable[j];
                                                //Check for an existing connection, if so then check if the new entry would be faster
                                                if (old.Data.Item1 == newDest)
                                                {
                                                    //If route of neighbour is more than one slower: Broadcast own routingtable
                                                    //TODO possible error point
                                                    if (tuple.Data.Item2 - 1 > old.Data.Item2)
                                                    {
                                                        change = true;
                                                        Console.WriteLine("Reverse" + Program.port);
                                                    }
                                                    if (tuple.Data.Item2 + 1 < old.Data.Item2 || (old.Data.Item3 == Int32.Parse(temp[1]) && tuple.Data.Item2 + 1 > old.Data.Item2))
                                                    {
                                                        Program.routingTable.Remove(Program.routingTable[j]);
                                                        j--; // To not skip an element
                                                        Program.routingTable.Add(new Row(newDest, tuple.Data.Item2 + 1, Int32.Parse(temp[1]))); //temp[1] is the port of the server this thread belongs to
                                                        change = true;
                                                        Console.WriteLine("Afstand naar {0} is nu {1} via {2}", newDest, tuple.Data.Item2 + 1, temp[1]);
                                                        Faster = false;
                                                    }
                                                    else
                                                    {
                                                        Faster = false;
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

                                    for (int i = 0; i < Program.routingTable.Count; i++)
                                    {
                                        if (Program.routingTable[i].Data.Item2 > (Program.routingTable.Count * 2))
                                        {
                                            Program.DuplicateDelete();
                                            Program.BroadcastDelete();
                                            change = false;
                                        }
                                    }
                                }
                            }

                            if (change) Program.BroadcastTable();
                            else Program.DuplicateDelete();
                            break;
                        case "DIS":
                            lock (Program.tableLock)
                            {
                                lock (Program.neighbourLock)
                                {
                                    //Delete connection
                                    int initiator = Int32.Parse(temp[0]);
                                    Program.neighbours.Remove(initiator);

                                    for (int i = 0; i < Program.routingTable.Count; i++)
                                    {
                                        if (Program.routingTable[i].Data.Item3 == initiator)
                                        {
                                            Program.routingTable[i] = new Row(Program.routingTable[i].Data.Item1, Program.routingTable.Count, Program.routingTable[i].Data.Item3); //Set cost of disconnected conenctions to "quasi" infinite: N - 1
                                        }
                                    }
                                }
                            }
                            Program.BroadcastTable();
                            break;
                        //TODO don't use duplicate for this
                        //TODO Implement server side receive intent of disconnect*/
                        case "DEL":
                            Program.DuplicateDelete();
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
