using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Text;
using System.Configuration;
using System.IO;

namespace GameDAL
{
    public delegate T Method<T>();

    public static class MethodUtility
    {
        private static readonly object ms_LogLock = new object();
        private static readonly string ms_LogFilePath = ConfigurationManager.AppSettings["LogFilePath"];

        public static T InvokeMethodWithDB<T>(Method<T> invokeMethod, string storedProcedure, object parameter)
        {
            return InvokeMethodWithDB(invokeMethod, storedProcedure, parameter, false);
        }

        public static T InvokeMethodWithDB<T>(Method<T> invokeMethod, string storedProcedure, object parameter, bool canBackup)
        {
            T retObj = default(T);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            retObj = invokeMethod();

            //if (canBackup)
            //{
            //    OperateLog(parameter, storedProcedure);
            //}

            sw.Stop();

            //操作日志
            LogByThread(storedProcedure, parameter, sw.ElapsedMilliseconds, 1);

            return retObj;
        }

        #region public static void LogByThread<T>(T initInfo, object parameter, long milliseconds, int logFlag)

        public static void LogByThread(Exception ex)
        {
            LogByThread(ex, null, 0L, 0);
        }

        public static void LogByThread(string exMessage)
        {
            LogByThread(exMessage, null, 0L, 0);
        }

        public static void LogByThread<T>(T initInfo, object parameter, long milliseconds, int logFlag)
        {
            LogByThread(initInfo, parameter, milliseconds, logFlag, HttpContext.Current);
        }

        /// <summary>
        /// 开启线程记录系统日志内容
        /// </summary>
        /// <typeparam name="T">可传参数的类型，可以为基础的强类型和Exception类型 </typeparam>
        /// <param name="initInfo">初始讯息，[异常日志为 Exception的ex，操作日志为SP的名称]</param>
        /// <param name="parameter">查询的参数内容[操作日志用，异常日志传null]</param>
        /// <param name="milliseconds">执行的时间[操作日志用，异常日志传0L]</param>
        /// <param name="logFlag">记录日志的类型，0异常日志，1转账日志，2登入模块,3报表模块，4线程异常，5第三方API日志，6Session模块，7金流模块</param>
        /// <param name="context">当前的HttpContext</param>
        public static void LogByThread<T>(T initInfo, object parameter, long milliseconds, int logFlag, HttpContext context)
        {
            string pathAndQuery = string.Empty;

            if (context != null)
            {
                pathAndQuery = context.Request.Url.PathAndQuery;
            }

            Dictionary<string, string> paras = new Dictionary<string, string>();

            paras["Type"] = ((LogType)logFlag).ToString();
            paras["Url"] = pathAndQuery;

            if (initInfo is Exception)
            {
                Exception exObj = initInfo as Exception;
                paras["LogText"] = HttpUtility.UrlEncode(exObj.Message + exObj.StackTrace);

                if (exObj.InnerException != null)
                {
                    paras["LogText"] += HttpUtility.UrlEncode(Environment.NewLine + exObj.InnerException.Message + exObj.StackTrace);

                    if (exObj.InnerException.InnerException != null)
                    {
                        paras["LogText"] += HttpUtility.UrlEncode(Environment.NewLine + exObj.InnerException.InnerException.Message + exObj.InnerException.InnerException.StackTrace);
                    }
                }
            }
            else
            {
                paras["LogText"] = HttpUtility.UrlEncode(initInfo.ToString());
            }

            JavaScriptSerializer jss = new JavaScriptSerializer();

            paras["Paras"] = GetRequestParameter(context) + "&extpara=" + HttpUtility.UrlEncode(jss.Serialize(parameter));

            if (context != null)
            {
                paras["User"] = context.User.Identity.Name;
            }
            else
            {
                paras["User"] = string.Empty;
            }

            paras["Timetaken"] = milliseconds.ToString();

            Log(LogInfo.CreateLogInfo(paras));
        }

        #endregion

        #region public static void Log(LogInfo logData)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logData"></param>
        public static void Log(LogInfo logData)
        {
            string basePath = ms_LogFilePath;// + IISConfigName + "_" + SiteID;

            if (!System.IO.Directory.Exists(basePath))
            {
                System.IO.Directory.CreateDirectory(basePath);
            }

            if (!File.Exists(basePath + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
            {
                try
                {
                    lock (ms_LogLock)
                    {
                        using (StreamWriter sw = File.AppendText(basePath + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                        {
                            sw.WriteLine("#Fields: date time cs-method cs-uri-stem cs-uri-query cs(User-Agent) cs-username time-taken");

                            sw.WriteLine(logData.ToString());

                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorPath = ms_LogFilePath + "LogError";

                    if (!Directory.Exists(errorPath))
                    {
                        Directory.CreateDirectory(errorPath);
                    }

                    try
                    {
                        lock (ms_LogLock)
                        {
                            using (StreamWriter sw = File.AppendText(errorPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                            {
                                sw.WriteLine(ex.ToString());
                                sw.WriteLine(logData.ToString());

                                sw.Flush();
                                sw.Close();
                            }
                        }
                    }
                    catch { }//二次错误后日志放弃
                }
            }
            else
            {
                try
                {
                    lock (ms_LogLock)
                    {
                        using (StreamWriter sw = new StreamWriter(File.Open(basePath + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log", FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                        {
                            sw.WriteLine(logData.ToString());

                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorPath = ms_LogFilePath + "LogError";

                    if (!Directory.Exists(errorPath))
                    {
                        Directory.CreateDirectory(errorPath);
                    }

                    try
                    {
                        lock (ms_LogLock)
                        {
                            using (StreamWriter sw = File.AppendText(errorPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                            {
                                sw.WriteLine(ex.ToString());
                                sw.WriteLine(logData.ToString());

                                sw.Flush();
                                sw.Close();
                            }
                        }
                    }
                    catch { }//二次错误后日志放弃
                }
            }
        }

        #endregion

        #region public static void RequestLog(string logText)

        /// <summary>
        /// 文件记录日志
        /// </summary>
        /// <param name="logText"></param>
        public static void RequestLog(string logText)
        {
            RequestLog(logText, string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="fileName"></param>
        public static void RequestLog(string logText, string fileName)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(ConfigurationManager.AppSettings["LogFilePath"]); // + IISConfigName + "_" + SiteID

            if (!System.IO.Directory.Exists(sb.ToString()))
            {
                System.IO.Directory.CreateDirectory(sb.ToString());
            }

            sb.Append("\\" + DateTime.Now.ToString("yyyyMMdd") + "_");

            if (!fileName.Equals(""))
            {
                sb.Append(fileName);
            }

            sb.Append("log.txt");

            lock (ms_LogLock)
            {
                using (StreamWriter sw = new StreamWriter(File.Open(sb.ToString(), FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ";" + logText);
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        #endregion

        #region public static string GetRequestParameter()
        public static string GetRequestParameter()
        {
            return GetRequestParameter(HttpContext.Current);
        }

        /// <summary>
        /// 获取request.form[]的参数值
        /// </summary>
        public static string GetRequestParameter(HttpContext httpContext)
        {
            if (httpContext == null) return string.Empty;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < httpContext.Request.Form.Count; i++)
            {
                sb.Append(httpContext.Request.Form.Keys[i]).Append("=").Append(httpContext.Request.Form[i]).Append("&");
            }

            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// cluster配置缓存
        /// </summary>
        /// <param name="type">获取类型，1主db，2查询服务器，3报表服务器[应对tb_clusterconfig主键]</param>
        /// <returns></returns>
        public static string GetClusterConfigCache(int type)
        {
            string currentConnectionString = SqlHelper.WriteConnection;

            //try
            //{
            //    DataSet clusterDs = System.Web.HttpRuntime.Cache["CACHE_CLUSTER_CONFIG"] as DataSet;
            //    if (clusterDs == null || clusterDs.Tables.Count == 0 || clusterDs.Tables[0].Rows.Count == 0)
            //    {
            //        SqlParameter parameter = new SqlParameter("@c_type", SqlDbType.Int);
            //        parameter.Value = 1;
            //        clusterDs = ExecuteWithQuery(ConnectionString, CommandType.StoredProcedure, "P_ClusterConfig_QryList", parameter);
            //        try
            //        {
            //            System.Web.Caching.AggregateCacheDependency dependency = new System.Web.Caching.AggregateCacheDependency();
            //            dependency.Add(new System.Web.Caching.SqlCacheDependency("RichWeiGeneration", "tb_clusterconfig"));
            //            System.Web.HttpRuntime.Cache.Insert("CACHE_CLUSTER_CONFIG", clusterDs, dependency);
            //        }
            //        catch (Exception ex)
            //        {
            //            //出现异常则不需要写入缓存
            //            SysUtility.LogByThread(ex);
            //        }
            //    }

            //    if (clusterDs != null && clusterDs.Tables.Count > 0 && clusterDs.Tables[0].Rows.Count > 0 && type != 1)
            //    {
            //        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(currentConnectionString);
            //        foreach (DataRow row in clusterDs.Tables[0].Rows)
            //        {
            //            if ((type == 2 && row["member_key"].ToString() == "2") || (type == 3 && row["member_key"].ToString() == "3"))
            //            {
            //                //2查询服务器，3报表服务器
            //                builder["Data Source"] = row["member_ip"].ToString();
            //                break;
            //            }
            //        }

            //        currentConnectionString = builder.ToString();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    SysUtility.LogByThread(ex);
            //}

            return currentConnectionString;
        }
    }

    public enum LogType
    {
        Unhandled = 0,
        Transfer = 1,
        Login = 2,
        Report = 3,
        Thread = 4,
        ThirdParty = 5,
        Session = 6,
        CashPartment = 7,
        Logout = 8,
        PayQuery = 9
    }

    public class LogInfo
    {
        public LogInfo()
        {
            LogTime = DateTime.Now;
        }

        //date time cs-method cs-uri-stem cs-uri-query cs(User-Agent)
        public DateTime LogTime
        {
            get;
            set;
        }

        public string Type
        {
            get;
            set;
        }

        public string Url
        {
            get;
            set;
        }

        public string LogText
        {
            get;
            set;
        }

        public string Paras
        {
            get;
            set;
        }

        public string User
        {
            get;
            set;
        }

        public string Timetaken
        {
            get;
            set;
        }

        public static LogInfo CreateLogInfo(string type, string url, string logText, string paras, string user, string timetaken)
        {
            LogInfo retObj = new LogInfo();

            retObj.Type = type;
            retObj.Url = url;
            retObj.LogText = logText;
            retObj.Paras = paras;
            retObj.User = user;
            retObj.Timetaken = timetaken;

            return retObj;
        }

        public static LogInfo CreateLogInfo(Dictionary<string, string> inputs)
        {
            LogInfo retObj = new LogInfo();

            retObj.Type = inputs["Type"];
            retObj.Url = inputs["Url"];
            retObj.LogText = inputs["LogText"];
            retObj.Paras = inputs["Paras"];
            retObj.User = inputs["User"];
            retObj.Timetaken = inputs["Timetaken"];

            return retObj;
        }

        private string ConvertEmptyTag(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "-";
            }

            return input;
        }

        public override string ToString()
        {
            StringBuilder logText = new StringBuilder();

            logText.Append(LogTime.ToString("yyyy-MM-dd")).Append(" ").Append(LogTime.ToString("HH:mm:ss")).Append(" ").Append(ConvertEmptyTag(Type)).Append(" ").Append(ConvertEmptyTag(Url)).Append(" ");
            logText.Append(ConvertEmptyTag(LogText)).Append(" ").Append(ConvertEmptyTag(Paras)).Append(" ").Append(ConvertEmptyTag(User)).Append(" ").Append(ConvertEmptyTag(Timetaken));

            return logText.ToString();
        }
    }

}
