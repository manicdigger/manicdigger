using System;
using System.Collections.Generic;
using System.Text;
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

      private Server m_server;
      private int m_client;

      public void Print(object obj)
      {
         if (obj==null)
            return;
         m_server.SendMessage(m_client, obj.ToString(), Server.MessageType.Normal);
      }
   }
}
