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

            for (int i = 1; i < args.Length; i++)
            {
                var p = int.Parse(args[i]);
                lock (neighbourLock)
                {
                    if (!neighbours.ContainsKey(p))
                    {
                        if (p < port) neighbours.Add(p, new Connection(p));
                    }
                }
            }

            for (int i = 0; i < neighbours.Count; i++)
            {
                lock (tableLock)
                {
                    routingTable.Add(new Row(neighbours.Keys.ToList()[i], 1, neighbours.Keys.ToList()[i]));
                }
            }

            while (true) Input();
        }

        private void Recompile()
        {

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

        static private void Message(string input)
        {
            // Send a message
            string[] parts = input.Split(new char[] { ' ' }, 2);
            int clientPort = int.Parse(parts[0]);
            if (!neighbours.ContainsKey(clientPort))
                Console.WriteLine("This connection doesn't exist yet!");
            else
            {
                neighbours[clientPort].Write.WriteLine("MES " + port + ": " + parts[1]);
                //MES is the header for the listener of goal client
            }
        }

        static private void Connect(string input)
        {
            int clientPort = int.Parse(input.Split()[0]);

            lock (neighbourLock)
            {
                if (neighbours.ContainsKey(clientPort))
                    Console.WriteLine("This connection already exists!");
                else
                {
                    neighbours.Add(clientPort, new Connection(clientPort)); //TODO update routingtable
                    lock (tableLock) routingTable.Add(new Row(clientPort, 1, clientPort));
                }
            }
            // Establish the connction (as client)
        }

        static private void Disconnect(string input)
        {

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
