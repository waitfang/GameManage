using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSocket.WebSocket.SubProtocol;
using System;
using System.Collections.Generic;
using UG.BLL.Common;
using UG.Model;
using UG.WebSocket.Common;

namespace UG.WebSocket
{
    //获取大厅列表基本数据
    public class GamelobbyState : SubCommandBase<SocketSession>
    {
        public override void ExecuteCommand(SocketSession session, SubRequestInfo requestInfo)
        {
            //此处可以根据客户端传入的信息查询一些信息然后推送给客户端 
            JObject respObj = new JObject();
            string strReturn = "";
            List<ResultBase> ResultValue = new List<ResultBase>();
            ResultBase objResultBase = new ResultBase();
            try
            {
                //接收客户端回传的心跳然后回复固定的心跳内容
                string recBody = requestInfo.Body;
                JObject postBody = JObject.Parse(recBody);
                respObj.Add("result", "true");
                respObj.Add("message", "ok");
                respObj.Add("respTime", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                respObj.Add("tid", postBody["tid"]);
                string User = session.User;// postBody["User"].ToString();//session.User;//登录者id
                string siteid = session.SiteId;
                if (postBody.ContainsKey("ts_id"))
                {
                    int ts_id = APIUtility.IsNumeric(postBody["ts_id"].ToString()) ? Convert.ToInt16(postBody["ts_id"]) : 0;
                    object objresult = OnlineManager.QueryGameAreasState(User,ts_id, siteid);
                    respObj.Add("data", JsonConvert.SerializeObject(objresult));
                    strReturn = respObj.ToString();
                }
                else
                { 
                    object objresult = OnlineManager.QueryGameAreasState(User, 0, siteid);
                    respObj.Add("data", JsonConvert.SerializeObject(objresult));
                    strReturn = respObj.ToString();
                }
                session.Send(strReturn);
            }
            catch (Exception ex)
            {
                respObj.Add("data", ResultValueBase.ResultValue(false, "【网络异常,请联系管理员！】"));
                strReturn = respObj.ToString();
                session.Send(strReturn);
                SocketSession.RequestLog(ex.ToString());
            }
        }
    }
}
