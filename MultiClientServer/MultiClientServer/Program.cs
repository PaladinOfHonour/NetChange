using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    partial class Program
    {
        static public int port;
        static public object neighbourLock, tableLock;

        static public Dictionary<int, Connection> neighbours;
        static public List<Row> routingTable;

        static void Main(string[] args)
        {
            neighbourLock = new object();
            tableLock = new object();

            port = int.Parse(args[0]);
            new Server(port);

            neighbours = new Dictionary<int, Connection>();
            routingTable = new List<Row>() { new Row(port, 0, port) };
            lock (tableLock)
            {
                lock (neighbourLock)
                {
                    for (int i = 1; i < args.Length; i++)
                    {
                        var p = int.Parse(args[i]);
                        {
                            if (!neighbours.ContainsKey(p))
                            {
                                if (p < port) neighbours.Add(p, new Connection(p));
                            }
                        }
                    }

                    for (int i = 0; i < neighbours.Count; i++)
                    {
                            routingTable.Add(new Row(neighbours.Keys.ToList()[i], 1, neighbours.Keys.ToList()[i]));
                    }
                }
            }

            BroadcastTable();

            while (true) Input();
        }

        #region INPUT

        static private void Input()
        {
            string[] input = Console.ReadLine().Split();
            string[] temp = input.Skip(1).ToArray();
            string data = string.Join(" ", temp);

            switch (input[0])
            {
                case "B": Message(data);    break;
                case "C": Connect(data);    break;
                case "D": Disconnect(data); break;
                case "R": ShowTable(data);  break;
                default:
                    Console.WriteLine("That's not a valid option");
                    break;
            }
        }

        static public void Message(string input)
        {
            // Send a message
            string[] parts = input.Split(new char[] { ' ' }, 2);
            int clientPort = int.Parse(parts[0]);

            lock (tableLock)
            {
                lock (neighbourLock)
                {
                    for (int i = 0; i < routingTable.Count; i++)
                    {
                        if (routingTable[i].Data.Item1 == clientPort)
                        {
                            // write message to client
                            Console.WriteLine("Bericht voor {0} doorgestuurd naar {1}", clientPort, routingTable[i].Data.Item3);
                            neighbours[routingTable[i].Data.Item3].Write.WriteLine("MES " + clientPort + " " +  parts[1]);
                            //MES is the header for the listener of goal client
                            //seperated by spaces
                            return;
                        }
                    }
                    Console.WriteLine("Poort {0} is niet bekend", clientPort);
                }
            }
        }
        

        static private void Connect(string input)
        {
            int clientPort = int.Parse(input.Split()[0]);

            lock (tableLock)
            {
                lock (neighbourLock)
                {
                    neighbours.Add(clientPort, new Connection(clientPort)); //TODO update routingtable
                    routingTable.Add(new Row(clientPort, 1, clientPort));
                    BroadcastTable();
                }
            }
            // Establish the connction (as client)
        }

        static private void Disconnect(string input)
        { /*
            when input: esc
                return end(proces(n))
            
            print: "Proces ended! :)" */
        }

        static private void ShowTable(string input)
        {
            for (int i = 0; i < routingTable.Count; i++)
            {
                Console.WriteLine(routingTable[i].ToS());
            }
        }

        #endregion
    }
}
