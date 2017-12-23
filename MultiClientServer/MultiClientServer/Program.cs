using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MultiClientServer
{
    partial class Program
    {
        static public int port;
        static public object neighbourLock, tableLock;
        static public bool init;

        static public Dictionary<int, Connection> neighbours;
        static public List<Row> routingTable;

        static void Main(string[] args)
        {
            neighbourLock = new object();
            tableLock = new object();
            init = false;

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
                init = true;
            }
            BroadcastTable();

            new Thread(() => Input()).Start();
        }

        #region INPUT

        static private void Input()
        {
            while (true)
            {
                string[] input = Console.ReadLine().Split();
                string[] temp = input.Skip(1).ToArray();
                string data = string.Join(" ", temp);

                switch (input[0])
                {
                    case "B": Message(data); break;
                    case "C": Connect(data); break;
                    case "D": Disconnect(data); break;
                    case "R": ShowTable(data); break;
                    default:
                        Console.WriteLine("That's not a valid option");
                        break;
                }
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
                            neighbours[routingTable[i].Data.Item3].Write.WriteLine("MES " + clientPort + " " + parts[1]);
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
                    for (int i = 0; i < routingTable.Count; i++)
                    {
                        if (routingTable[i].Data.Item1 == clientPort) routingTable.Remove(routingTable[i]);
                    }
                    neighbours.Add(clientPort, new Connection(clientPort)); //TODO update routingtable
                    routingTable.Add(new Row(clientPort, 1, clientPort));
                    BroadcastTable();
                }
            }
            // Establish the connection (as client)
        }

        static private void Disconnect(string input)
        {
            lock (tableLock)     //TODO  Lock or not? Neighbour
            {
                lock (neighbourLock)
                {
                    int deadCon = Int32.Parse(input);

                    //TODO maybe Catch instead of if
                    //Check if disconnecting a true neighbour
                    if (!neighbours.ContainsKey(deadCon))
                    {
                        Console.WriteLine("Poort {0} is niet bekend", deadCon);
                        return;
                    }

                    // Signal intent to Disconnect
                    neighbours[deadCon].Write.WriteLine("DIS " + port);

                    //Delete connection
                    neighbours.Remove(deadCon);

                    //After this is the same

                    

                    for (int i = 0; i < routingTable.Count; i++)
                    {
                        if (routingTable[i].Data.Item3 == deadCon)
                        {
                            routingTable[i] = new Row(routingTable[i].Data.Item1, routingTable.Count, routingTable[i].Data.Item3); //Set cost of disconnected conenctions to "quasi" infinite: N - 1
                        }
                    }

                    if (Program.neighbours.Count < 1)
                    {
                        Program.routingTable = new List<Row>() { new Row(Program.port, 0, Program.port) };
                    }
                }
            }
            //Can be stolen
            BroadcastTable();
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
