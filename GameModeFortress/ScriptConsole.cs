// Copyright (c) 2011 by Henon <meinrad.recheis@gmail.com>
using System;
using System.Collections.Generic;
using System.Text;
using Jint.Delegates;
using ManicDigger;
using ManicDiggerServer;

namespace GameModeFortress
{
    public class ScriptConsole
    {
        public ScriptConsole (Server s,int client_id)
        {
            m_server = s;
            m_client = client_id;
        }

        public delegate void Action<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);

        public void InjectConsoleCommands (IScriptInterpreter interpreter)
        {
            interpreter.SetFunction("out", new System.Action<object>(Print));
            interpreter.SetFunction("materials", new Jint.Delegates.Action(PrintMaterials));
            interpreter.SetFunction("materials_between", new Jint.Delegates.Action<double, double>(PrintMaterials));
            interpreter.SetFunction("find_material", new System.Action<string>(FindMaterial));
            interpreter.SetFunction("position", new Jint.Delegates.Action(PrintPosition));
            interpreter.SetFunction("get_position", new Jint.Delegates.Func<Vector3i>(GetPosition));
            interpreter.SetVariable("turtle", Turtle);
            interpreter.SetFunction("set_block", new Action<double, double, double, double>(SetBlock));
            interpreter.SetFunction("get_block", new Jint.Delegates.Func<double, double, double, int>(GetBlock));
            interpreter.SetFunction("get_height", new Jint.Delegates.Func<double, double, double>(GetHeight));
            interpreter.SetFunction("clear", new Jint.Delegates.Action(Clear));
        }

        private Server m_server;
        private int m_client;

        public void Print (object obj)
        {
            if (obj == null)
                return;
            m_server.SendMessage(m_client, obj.ToString(), Server.MessageType.Normal);
        }

        public void PrintMaterials ()
        {
            PrintMaterials(0, 255);
        }

        public void PrintMaterials (double start,double end)
        {
            for (int i = (int)start; i < end; i++)
            {
                Print(string.Format("{0}: {1}", i, m_server.d_Data.Name[i]));
            }
        }

        public void FindMaterial (string search_string)
        {
            for (int i = 0; i < 255; i++)
            {
                if (m_server.d_Data.Name[i].Contains(search_string))
                {
                    Print(string.Format("{0}: {1}", i, m_server.d_Data.Name[i]));
                }
            }
        }

        public void PrintPosition ()
        {
            var client = m_server.GetClient(m_client);
            var pos = GetPosition();
            Print(string.Format("Position: X {0}, Y {1}, Z{2}", pos.x, pos.y, pos.z));
        }

        public Vector3i GetPosition ()
        {
            var client = m_server.GetClient(m_client);
            return m_server.PlayerBlockPosition(client);
        }

        public void SetBlock (double x,double y, double z, double material)
        {
            //m_server.CreateBlock((int)x, (int)y, (int)z, m_client, new Item() { BlockId = (int)material, ItemClass = ItemClass.Block, BlockCount = 1 });
            m_server.Place((int)x, (int)y, (int)z, (int)material);
        }

        public int GetBlock (double x, double y, double z)
        {
            return m_server.d_Map.GetBlock((int)x, (int)y, (int)z);
        }

        public double GetHeight (double x, double y)
        {
            return (double)ManicDigger.MapUtil.blockheight(m_server.d_Map, 0, (int)x, (int)y);
        }

        public void Clear()
        {
            m_server.ClearInterpreter(m_client);
        }

        Turtle m_turtle;

        public Turtle Turtle
        {
            get
            {
                if (m_turtle == null)
                    m_turtle = new Turtle { Console = this };
                return m_turtle;
            }
        }
    }

    public class Turtle
    {

        public ScriptConsole Console;
        //public enum Orientation
        //{
        //   North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest,
        //   Up, UpNorth, UpNorthEast, UpEast, UpSouthEast, UpSouth, UpSouthWest, UpWest, UpNorthWest,
        //   Down, DownNorth, DownNorthEast, DownEast, DownSouthEast, DownSouth, DownSouthWest, DownWest, DownNorthWest,
        //}

        public Vector3i position = new Vector3i(0, 0, 0);

        public double x { get { return position.x; } }

        public double y { get { return position.y; } }

        public double z { get { return position.z; } }

        //public Orientation orientation;

        public void set_player_position ()
        {
            position = Console.GetPosition();
        }

        public double material = 0;

        public void put ()
        {
            Console.SetBlock(x, y, z, material);
        }

        public Vector3i direction = new Vector3i(0, -1, 0); // turtle looks north by default

        public void look_north ()
        {
            direction = new Vector3i(0, -1, 0);
        }

        public void look_east ()
        {
            direction = new Vector3i(-1, 0, 0);
        }

        public void look_south ()
        {
            direction = new Vector3i(0, 1, 0);
        }

        public void look_west ()
        {
            direction = new Vector3i(1, 0, 0);
        }

        public void look_up ()
        {
            direction = new Vector3i(0, 0, 1);
        }

        public void look_down ()
        {
            direction = new Vector3i(0, 0, -1);
        }

        public void forward ()
        {
            position = new Vector3i(position.x + direction.x, position.y + direction.y, position.z + direction.z);
        }

        public void back ()
        {
            position = new Vector3i(position.x - direction.x, position.y - direction.y, position.z - direction.z);
        }

        public void save () // push the current turtle position and direction to the stack
        {
            m_stack.Push(new Vector3i[] {position, direction});
        }

        public void load () // pop position and direction from the stack and set them
        {
            var array = m_stack.Pop();
            if (array == null)
                return;
            position = array[0];
            direction = array[1];
        }

        private FastStack<Vector3i[]> m_stack = new FastStack<Vector3i[]>();

        public void status ()
        {
            Console.Print("Turtle status:");
            Console.Print("Position: " + position);
            Console.Print("Orientation: " + DirectionToString(direction) + "  " + direction);
        }

        public static string DirectionToString (Vector3i dir)
        {
            if (dir.x == 0)
            {
                if (dir.y == 0)
                {
                    if (dir.z == 1)
                        return "Up";
                    else if (dir.z == -1) 
                        return "Down";
                }
                if (dir.z == 0)
                {
                    if (dir.y == 1)
                        return "South";
                    else if (dir.y == -1)
                        return "North";
                }
            } else if (dir.y == 0 && dir.z == 0)
            {
                if (dir.x == 1)
                    return "West";
                else if (dir.x == -1)
                    return "East";
            }
            return "Unknown direction";
        }

        //public void turn_right()
        //{

        //}


        //public void turn_left()
        //{

        //}

    }

    public delegate void Action<T1,T2, T3, T4> (T1 t1, T2 t2, T3 t3, T4 t4);
}