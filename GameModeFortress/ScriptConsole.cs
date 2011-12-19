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
      public ScriptConsole(Server s, int client_id)
      {
         m_server = s;
         m_client = client_id;
      }

      public void InjectConsoleCommands(IScriptInterpreter interpreter)
      {
         interpreter.SetFunction("out", new Action<object>(Print));
         interpreter.SetFunction("materials", new Action(PrintMaterials));
         interpreter.SetFunction("materials_between", new Action<double, double>(PrintMaterials));
         interpreter.SetFunction("find_material", new Action<string>(FindMaterial));
         interpreter.SetFunction("position", new Action(PrintPosition));
         interpreter.SetFunction("get_position", new Func<Vector3i>(GetPosition));
         interpreter.SetVariable("turtle", Turtle);
         interpreter.SetFunction("put_block", new Action<double, double, double, double>(PutBlock));
      }

      private Server m_server;
      private int m_client;

      public void Print(object obj)
      {
         if (obj == null)
            return;
         m_server.SendMessage(m_client, obj.ToString(), Server.MessageType.Normal);
      }

      public void PrintMaterials()
      {
         PrintMaterials(0, 255);
      }

      public void PrintMaterials(double start, double end)
      {
         for (int i = (int)start; i < end; i++)
         {
            Print(string.Format("{0}: {1}", i, m_server.d_Data.Name[i]));
         }
      }

      public void FindMaterial(string search_string)
      {
         for (int i = 0; i < 255; i++)
         {
            if (m_server.d_Data.Name[i].Contains(search_string))
            {
               Print(string.Format("{0}: {1}", i, m_server.d_Data.Name[i]));
            }
         }
      }

      public void PrintPosition()
      {
         var client = m_server.GetClient(m_client);
         var pos = GetPosition();
         Print(string.Format("Position: X {0}, Y {1}, Z{2}", pos.x, pos.y, pos.z));
      }

      public Vector3i GetPosition()
      {
         var client = m_server.GetClient(m_client);
         return m_server.PlayerBlockPosition(client);
      }

      public void PutBlock(double x, double y, double z, double material)
      {
         //m_server.CreateBlock((int)x, (int)y, (int)z, m_client, new Item() { BlockId = (int)material, ItemClass = ItemClass.Block, BlockCount = 1 });
         m_server.Place((int)x, (int)y, (int)z, (int)material);
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
      public enum Orientation
      {
         North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest,
         Up, UpNorth, UpNorthEast, UpEast, UpSouthEast, UpSouth, UpSouthWest, UpWest, UpNorthWest,
         Down, DownNorth, DownNorthEast, DownEast, DownSouthEast, DownSouth, DownSouthWest, DownWest, DownNorthWest,
      }

      #region --> Turtle API

      public Vector3i position = new Vector3i( 0, 0, 0 );
      public double x { get { return position.x; } }
      public double y { get { return position.y; } }
      public double z { get { return position.z; } }

      public Orientation orientation;

      public void set_player_position()
      {
         position = Console.GetPosition();
      }

      public double material = 0;

      public void put()
      {
         Console.PutBlock(x, y, z, material);
      }

      #endregion
   }

  public delegate void Action<T1,T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);



}
