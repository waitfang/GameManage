using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuperSocket.WebSocket;
using System.Configuration;
using System.IO;
using Newtonsoft.Json.Linq; 

namespace GameSocketWeb
{
    public class SocketSession: WebSocketSession<SocketSession>
    {
        /// <summary>
        /// 存储当前在线的链接Key
        /// Value[0]SessionID
        /// Value[1]UserName
        /// </summary>
        internal static Dictionary<string, string> OnlineSession = new Dictionary<string, string>();

        public string Token
        {
            get;
            set;
        }

        public string User
        {
            get;
            set;
        }
        public string PlatformID
        {
            get;
            set;
        }
         

        private static bool IsValidateClient(string token)
        {
            string applyClient = ConfigurationManager.AppSettings["ValidateClient"];

            if (!string.IsNullOrEmpty(applyClient) && applyClient.ToLower().IndexOf(token + '|') != -1)
            {
                return true;
            }

            return false;
        }

        #region public static void RequestLog(string logText)

        /// <summary>
        /// 文件记录日志
        /// </summary>
        /// <param name="logText"></param>
        public static void RequestLog(string logText)
        {
            Task task = new Task(()=> {
                RequestLog(logText, string.Empty);
            });

            task.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="fileName"></param>
        public static void RequestLog(string logText, string fileName)
        {
            try
            {
                string s_Log = ConfigurationManager.AppSettings["LogFilePath"] + DateTime.Now.ToString("yyyyMMdd") + "_";
                if (!fileName.Equals(""))
                {
                    s_Log += fileName;
                }

                s_Log += "log.txt";

                using (StreamWriter sw = new StreamWriter(File.Open(s_Log, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ";" + logText);

                    sw.Flush();
                    sw.Close();
                }
            }
            catch
            {

            }
        }

        #endregion
    }
}
