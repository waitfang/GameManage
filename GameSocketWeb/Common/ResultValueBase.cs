using GameSocketWeb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks; 

namespace UG.WebSocket.Common
{
    public static class ResultValueBase
    { 

        #region public static void RoomChangeNotify(SocketSession sessionObj,Room objRoom,string data)
        /// <summary>
        /// 给指定的账号发送data
        /// 发送整房间人员房间信息异动数据
        /// </summary>
        /// <param name="sessionObj"></param>
        /// <param name="objRoom"></param>
        /// <param name="data"></param>
        public static void RoomChangeNotify(SocketSession sessionObj, string User1Session, string data)
        {
            if (!string.IsNullOrEmpty(User1Session))
            {
                Task.Factory.StartNew(() => {
                    SendDataBySessionId(sessionObj, data, User1Session);
                });
                 
            } 
        }

        #region private static void SendDataBySessionId(SocketSession session, string data, string sessionId)
        /// <summary>
        /// 发送信息到特定的SessionID端中
        /// </summary>
        /// <param name="session"></param>
        /// <param name="data"></param>
        /// <param name="sessionId"></param>
        private static void SendDataBySessionId(SocketSession session, string data, string sessionId)
        {
            session.AppServer.GetSessionByID(sessionId).Send(data);
        }
        #endregion

        #endregion
    }
}
