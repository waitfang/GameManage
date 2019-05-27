using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSocket.WebSocket.SubProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UG.WebSocket
{
    public class Gamelobby : SubCommandBase<SocketSession>
    { 
        public override void ExecuteCommand(SocketSession session, SubRequestInfo requestInfo)
        {
            try
            {
                //接收客户端回传的心跳然后回复固定的心跳内容
                string recBody = requestInfo.Body;
                JObject postBody = JObject.Parse(recBody);

                //此处可以根据客户端传入的信息查询一些信息然后推送给客户端

                JObject respObj = new JObject();
                string strReturn = "";
                if (postBody.ContainsKey("name"))
                {
                    respObj.Add("result", "true");
                    respObj.Add("message", "ok");
                    respObj.Add("respTime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    respObj.Add("tid", postBody["tid"]);
                    strReturn = respObj.ToString();
                }
                else
                {
                    //var values = JObject.FromObject(gamelobbyInfo2).ToObject<Dictionary<string, object>>(); 
                    gamelobbyInfo2.Add("result", true);
                    gamelobbyInfo2.Add("message", "ok");
                    gamelobbyInfo2.Add("respTime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                    gamelobbyInfo2["tid"]=postBody["tid"];
                    strReturn = JsonConvert.SerializeObject(gamelobbyInfo2); 
                }

                session.Send(strReturn);
            }
            catch (Exception ex)
            {
                SocketSession.RequestLog(ex.ToString());
            }
        }

      
        public static Dictionary<string, object> gamelobbyInfo = new Dictionary<string, object>();
        public static Dictionary<string, object> gamelobbyInfo2 = new Dictionary<string, object>();
        static Gamelobby()
        { 
            gamelobby model = new gamelobby();
            model.id = "A1";
            model.name = "艾欧尼亚";
            //明细 
            model.online = "5";
            model.game3v3 = "3";
            model.game4v4 = "4";
            model.game5v5 = "50";
            //model.objlist.Add(objgamelobbyList); 
            gamelobbyInfo.Add(model.id, model);

            model = new gamelobby();
            model.id = "A2";
            model.name = "祖安";
            //明细 
            model.online = "51";
            model.game3v3 = "31";
            model.game4v4 = "41";
            model.game5v5 = "50";
            //model.objlist.Add(objgamelobbyList);
            gamelobbyInfo.Add(model.id, model);
            gamelobbyInfo2.Add("data", gamelobbyInfo);
        }
    }

   
    public class gamelobby
    {

        public string id { get; set; }
        public string name { get; set; } 
        public string online { get; set; }
        public string game3v3 { get; set; }
        public string game4v4 { get; set; }
        public string game5v5 { get; set; }

    }
}
