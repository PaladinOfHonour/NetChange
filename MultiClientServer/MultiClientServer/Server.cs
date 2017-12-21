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


        }

        private void AcceptLoop(TcpListener handle)
        {
            while (true)
            {
                TcpClient client = handle.AcceptTcpClient();
                StreamReader clientIn = new StreamReader(client.GetStream());
                StreamWriter clientOut = new StreamWriter(client.GetStream());
                clientOut.AutoFlush = true;

                // The server doesn't know the port of the connection client, hence the client first sends a message with his own port as part of the protocol
                int clientPort = int.Parse(clientIn.ReadLine().Split()[1]);

                Console.WriteLine("Client sets up connection: " + clientPort);

                if (Program.routingTable.Any())
                {
                    bool var = !Program.routingTable.ContainsKey(clientPort);
                    Console.WriteLine("  the key to add: " + clientPort + " already added? : " + var);
                }
                else Console.WriteLine("FAILURE");

                lock (Program.myLock)
                {
                    // Put the new connection in the connectionlist
                    if (!Program.routingTable.ContainsKey(clientPort)) Program.routingTable.Add(clientPort, new Connection(clientIn, clientOut));
                }
            }
        }
    }
}
