using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    struct Row
    {
        Tuple<int, int, int> data;

        public Row(int a, int b, int c)
        {
            data = new Tuple<int, int, int>(a, b, c);
        }

        static public Row FromS(string s)
        {
            string[] temp = s.Split(' ');
            return new Row(Int32.Parse(temp[0]), Int32.Parse(temp[1]), Int32.Parse(temp[2]));
        }

        public string ToS()
        {
            return data.Item1 + " " + data.Item2 + " " + data.Item3;
        }

        public Tuple<int, int, int> Data
        {
            get { return data; }
            set { data = value; }
        }
    }

    partial class Program
    {
        static public void BroadcastTable()
        {
            lock (tableLock)     //TODO  Lock or not? Neighbour
            {
                lock (neighbourLock)
                {
                    foreach (KeyValuePair<int, Connection> kv in neighbours)
                    {
                        kv.Value.Write.WriteLine("REC " + routingTable.Count + " " + port);
                        // seperate by spaces
                    }

                    for (int i = 0; i < routingTable.Count; i++)
                    {
                        string s = routingTable[i].ToS();
                        foreach (KeyValuePair<int, Connection> kv in neighbours) kv.Value.Write.WriteLine(s);
                    }
                }
            }
        }
    }
}
