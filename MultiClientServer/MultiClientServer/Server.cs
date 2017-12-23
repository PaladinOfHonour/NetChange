using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace MultiClientServer
{
    class Server
    {
        public Server(int port)
        {
            // Listen to connections on the given port
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();

            // Set the window title to the port id of the server
            Console.Title = port.ToString();

            // Start a seperate thread that accepts connections
            new Thread(() => AcceptLoop(server)).Start();

            //HandleConnections(server);
        }


        private void AcceptLoop(TcpListener handle)
        {
            while (true)
            {
                if (Program.init)
                {
                    TcpClient client = handle.AcceptTcpClient();
                    StreamReader clientIn = new StreamReader(client.GetStream());
                    StreamWriter clientOut = new StreamWriter(client.GetStream());
                    clientOut.AutoFlush = true;
                    // The server doesn't know the port of the connection client, hence the client first sends a message with his own port as part of the protocol
                    int clientPort = int.Parse(clientIn.ReadLine().Split()[1]);

                    Console.WriteLine("Client sets up connection: " + clientPort);

                    lock (Program.tableLock)
                    {
                        lock (Program.neighbourLock)
                        {
                            for (int i = 0; i < Program.routingTable.Count; i++)
                            {
                                if (Program.routingTable[i].Data.Item1 == clientPort) Program.routingTable.Remove(Program.routingTable[i]);
                            }
                            // Put the new connection in the connectionlist
                            Program.neighbours.Add(clientPort, new Connection(clientIn, clientOut));
                            Program.routingTable.Add(new Row(clientPort, 1, clientPort));
                            Program.BroadcastTable();
                        }
                    }
                }
            }
        }
    }
}
