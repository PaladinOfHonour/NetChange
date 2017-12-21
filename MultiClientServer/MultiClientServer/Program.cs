using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Program
    {
        static public int port;

        static public Dictionary<int, Connection> routingTable = new Dictionary<int, Connection>();

        static void Main(string[] args)
        {
            port = int.Parse(args[0]);
            new Server(port);

            routingTable = new Dictionary<int, Connection>();
            for (int i = 1; i < args.Length; i++)
            {
                var p = int.Parse(args[i]);
                if (!routingTable.ContainsKey(p)) routingTable.Add(p, new Connection(p));
            }

            
            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("connect"))
                {
                    int clientPort = int.Parse(input.Split()[1]);
                    if (routingTable.ContainsKey(clientPort))
                        Console.WriteLine("This connection already exists!");
                    else
                    {
                        // Establish the connction (as client)
                        routingTable.Add(clientPort, new Connection(clientPort));
                    }
                }
                else
                {
                    // Send a message
                    string[] parts = input.Split(new char[] { ' ' }, 2);
                    int port = int.Parse(parts[0]);
                    if (!routingTable.ContainsKey(port))
                        Console.WriteLine("This connection doesn't exist yet!");
                    else
                        routingTable[port].Write.WriteLine(port + ": " + parts[1]);
                }
            }
            
        }
    }
}
