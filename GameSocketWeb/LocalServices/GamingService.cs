/*
 本地服务查询db的逻辑类，
1、把匹配数据传输给客户端，
2、匹配成功后通过socket通知前端玩家
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace GameSocketWeb.LocalServices
{
    public class GamingService
    {
        /// <summary>
        /// 是否持续运行
        /// </summary>
        public bool IsRuning
        {
            get;
            set;
        }

        /// <summary>
        /// 同步等待对象
        /// </summary>
        private AutoResetEvent waitHandler = new AutoResetEvent(false);

        /// <summary>
        /// 本次处理通知的结果
        /// </summary>
        private bool procNotifyResult = false;

        /// <summary>
        /// 处理比赛比对成功向所有房间用户发送请求，确认每一个房间都有相应表示确定匹配完成
        /// </summary>
        /// <param name="roomDetail">包含比赛ID和房间SessionId信息</param>
        /// <returns></returns>
        public bool ProcessMatchedNotification(string roomDetail)
        {
            try
            {
                using (var ws = new WebSocketSharp.WebSocket(ConfigurationManager.AppSettings["WebSocketUrl"]))
                {
                    ws.Connect();

                    //发送通知房间列表和比赛ID给所有房间用户
                    ws.Send("GameMatchedNotify " + roomDetail);

                    ws.OnMessage += Ws_OnMessage;
                    //最多等待30秒
                    waitHandler.WaitOne(30000);
                }
            }
            catch (Exception ex)
            {
                SocketSession.RequestLog(ex.ToString());
            }

            return procNotifyResult;
        }

        private void Ws_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            JObject resultObj = JsonConvert.DeserializeObject(e.Data) as JObject;

            if (resultObj != null && resultObj["result"] != null && resultObj["result"].ToString().ToLower() == "true")
            {
                procNotifyResult = true;
            }
            else
            {
                procNotifyResult = false;
            }
            //有结果通知直接退出上述线程等待句柄
            waitHandler.Set();
        }
         
    }
}
