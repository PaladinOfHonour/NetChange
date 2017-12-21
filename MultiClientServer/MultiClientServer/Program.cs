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
                if (input.StartsWith("verbind"))
                {
                    int clientPort = int.Parse(input.Split()[1]);
                    if (routingTable.ContainsKey(clientPort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        routingTable.Add(clientPort, new Connection(clientPort));
                    }
                }
                else
                {
                    // Stuur berichtje
                    string[] delen = input.Split(new char[] { ' ' }, 2);
                    int poort = int.Parse(delen[0]);
                    if (!routingTable.ContainsKey(poort))
                        Console.WriteLine("Hier is nog geen verbinding naar!");
                    else
                        routingTable[poort].Write.WriteLine(port + ": " + delen[1]);
                }
            }
            
        }
    }
}
