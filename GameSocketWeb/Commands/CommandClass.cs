using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using SuperSocket.WebSocket.SubProtocol;
 
using Newtonsoft.Json;
using GameSocketWeb;


public class HeartBeat : SubCommandBase<SocketSession>
{
    public override void ExecuteCommand(SocketSession session, SubRequestInfo requestInfo)
    {
        try
        {
            JObject respObj = new JObject();
            respObj.Add("result", "true");
            respObj.Add("action", "heartbeat");
            respObj.Add("respTime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

            session.Send(respObj.ToString());
        }
        catch (Exception ex)
        {
            SocketSession.RequestLog(ex.ToString());
        }
    }
    } 
