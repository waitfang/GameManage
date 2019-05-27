using GameBLL.Common;
using GameSocketWeb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuperSocket.WebSocket.SubProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
 
    public class UserInfoCommand : SubCommandBase<SocketSession>
    {
        UserInfo ClassUserInfo = new UserInfo();
        public override void ExecuteCommand(SocketSession session, SubRequestInfo requestInfo)
        { 
            string recBody = requestInfo.Body;//参数
            JObject postBody = JObject.Parse(recBody);

            //此处可以根据客户端传入的信息查询一些信息然后推送给客户端
            JObject respObj = new JObject();

            if (postBody.ContainsKey("tid"))
            {
                respObj.Add("tid", postBody["tid"]);  
                ClassUserInfo.USERID = string.IsNullOrEmpty(postBody["UserId"].ToString()) ? 0 : Convert.ToInt16(postBody["UserId"]);
                List<UserInfo> listClassUserInfo = DALCore.GetInstance().Gaming.UserLogin(ClassUserInfo);
                respObj.Add("result", "true");
                respObj.Add("data", JsonConvert.SerializeObject(listClassUserInfo));  
            }
            else
            {
                respObj.Add("result", "false");
                respObj.Add("message", "tid not exits");
            }

            session.Send(respObj.ToString());
        }
    } 
