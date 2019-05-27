using GameDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization; 

namespace GameBLL.Common
{
    #region public static class SysUtility
    /// <summary>
    /// 
    /// </summary>
    public static class SysUtility
    {
        #region Global Variables
        
        private static readonly object ms_LogLock = new object();
        private static readonly object ms_RequestLogLock = new object();

        /// <summary>
        /// 
        /// </summary>
        public static readonly string ms_LogFilePath = ConfigurationManager.AppSettings["LogFilePath"];
        #endregion

        #region public static void LogByThread<T>(T initInfo, Dictionary<string, object> parameter, long milliseconds, int logFlag)

        public static void LogByThread(Exception ex)
        {
            LogByThread<Exception>(ex, null, 0L, 0);
        }

        public static void LogByThread<T>(T initInfo, Dictionary<string, object> parameter, long milliseconds, int logFlag)
        {
            LogByThread<T>(initInfo, parameter, milliseconds, logFlag, HttpContext.Current);
        }

        /// <summary>
        /// 开启线程记录系统日志内容
        /// </summary>
        /// <typeparam name="T">可传参数的类型，可以为基础的强类型和Exception类型</typeparam>
        /// <param name="initInfo">初始讯息，[异常日志为 Exception的ex，操作日志为SP的名称]</param>
        /// <param name="parameter">查询的参数内容[操作日志用(必要参数 商户号:merchantid 商号交易码:merchantorderid)，异常日志传null]</param>
        /// <param name="milliseconds">执行的时间[操作日志用，异常日志传0L]</param>
        /// <param name="logFlag">记录日志的类型，0异常日志，1金流操作日志</param>
        /// <param name="context"></param>
        public static void LogByThread<T>(T initInfo, Dictionary<string, object> parameter, long milliseconds, int logFlag, HttpContext context)
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

            LogInfo logs = LogInfo.CreateLogInfo(paras);
            string merid = parameter != null && parameter.ContainsKey("merchantid") ? parameter["merchantid"].ToString() : string.Empty;
            merid = parameter != null && parameter.ContainsKey("MerchantId") ? parameter["MerchantId"].ToString() : merid;
            merid = parameter != null && parameter.ContainsKey("merchant_id") && !string.IsNullOrEmpty(parameter.ContainsKey("merchant_id").ToString()) ? parameter["merchant_id"].ToString() : merid;
            
            Log(logs, merid);

            if (logFlag == 1)
            {
                string merorderid = parameter != null && parameter.ContainsKey("merchantorderid") ? parameter["merchantorderid"].ToString() : string.Empty;
                merorderid = parameter != null && parameter.ContainsKey("MerchantOrderId") ? parameter["MerchantOrderId"].ToString() : merorderid; 
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

        #region public static void Log(LogInfo logData)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logData"></param>
        public static void Log(LogInfo logData, string merchantId)
        {
            merchantId = merchantId.Replace('|', '&');

            if (!System.IO.Directory.Exists(ms_LogFilePath + merchantId))
            {
                System.IO.Directory.CreateDirectory(ms_LogFilePath + merchantId);
            }

            if (!File.Exists(ms_LogFilePath + merchantId + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
            {
                try
                {
                    lock (typeof(SysUtility))
                    {
                        using (StreamWriter sw = File.AppendText(ms_LogFilePath + merchantId + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                        {
                            sw.WriteLine("#Fields: date time cs-method cs-uri-stem cs-uri-query cs(User-Agent) cs-username time-taken");

                            sw.WriteLine(logData.ToString());

                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
                catch// (Exception ex)
                {

                }
            }
            else
            {
                try
                {
                    lock (typeof(SysUtility))
                    {
                        using (StreamWriter sw = File.AppendText(ms_LogFilePath + merchantId + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                        {
                            sw.WriteLine(logData.ToString());

                            sw.Flush();
                            sw.Close();

                        }
                    }
                }
                catch// (Exception ex)
                {

                }
            }
        }
        #endregion

        #region 普通的文本日志写入方法

        /// <summary>
        /// 文件记录日志
        /// </summary>
        /// <param name="logText"></param>
        public static void RequestLog(string logText)
        {
            Thread thread = new Thread(delegate()
            {
                RequestLog(logText, string.Empty);
            });

            thread.Start();
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
                string s_Log = ms_LogFilePath + DateTime.Now.ToString("yyyyMMdd") + "_";
                if (!fileName.Equals(""))
                {
                    s_Log += fileName;
                }

                s_Log += "log.txt";
                lock (ms_RequestLogLock)
                {
                    using (StreamWriter sw = new StreamWriter(File.Open(s_Log, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                    {
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ";" + logText);

                        sw.Flush();
                        sw.Close();
                    }
                }
                
            }
            catch
            {

            }
        }

        #endregion

        #region 数据有效性验证方法

        /// <summary>
        /// 数据有效性验证
        /// </summary>
        /// <param name="input">需要验证的数据</param>
        /// <param name="validateType">验证类型</param>
        /// <param name="strLength">字符长度</param>
        /// <param name="inputName">需要验证的数据名称</param>
        /// <param name="canEmpty">是否可以为空</param>
        /// <returns></returns>
        public static string ValidationData(string input, ValidateType validateType, int strLength, string inputName, bool canEmpty)
        {
            bool tooltip = false;
            //允许空的类型不需要继续判断
            if (input.Length == 0 && canEmpty)
            {
                return "";
            }

            //空字符串判断
            if (input.Length == 0 && !canEmpty)
            {
                return "参数‘" + inputName + "’的值为：string.Empty ";
            }

            //限定长度判断
            if (strLength > 0 && input.Length > strLength)
            {
                //小数类型的值需要判断是否有小数点
                if (validateType == ValidateType.Number)
                {
                    string newInputs = input.Split('.')[0];
                    if (newInputs.Length > strLength)
                    {
                        return "参数‘" + inputName + "’长度应该在‘" + strLength + "’位以内,[值:" + input + "]";
                    }
                }
                else
                {
                    return "参数‘" + inputName + "’长度应该在‘" + strLength + "’位以内,[值:" + input + "]";
                }
            }

            //tinyint类型的判断
            if (validateType == ValidateType.Integer && strLength == 3)
            {
                int val = 0;
                if (!int.TryParse(input, out val))
                {
                    tooltip = true;
                }

                //tinyint类型不能大于255
                if (val > 255)
                {
                    tooltip = true;
                }
            }
            else if (validateType == ValidateType.Boolean)//bool类型判断
            {
                bool val = false;
                if (!bool.TryParse(input, out val))
                {
                    tooltip = true;
                }
            }
            else if (validateType == ValidateType.DateTime || validateType == ValidateType.Date) //日期类型判断
            {
                DateTime dateTime;
                if (!DateTime.TryParse(input, out dateTime))
                {
                    tooltip = true;
                }
                else
                {
                    if (dateTime < Convert.ToDateTime("1900-01-01") || dateTime > Convert.ToDateTime("2079-06-06"))
                    {
                        return "参数‘" + inputName + "’,时间格式值必须介于1900-01-01  到  2079-06-06 之间";
                    }
                }
            }
            else if (validateType != ValidateType.None && input.Length > 0)
            {
                //其他类型需要用正则表达式来验证
                string strReg = MatchingLibrary(validateType);
                if (strReg.Length > 0)
                {
                    if (!Regex.IsMatch(input, strReg))
                    {
                        tooltip = true;
                    }
                    else
                    {
                        //int 10位限制，10个9将会Int溢出所以要验证
                        if (validateType == ValidateType.Integer)
                        {
                            int val = 0;
                            if (!int.TryParse(input, out val))
                            {
                                tooltip = true;
                            }

                            if (strLength == 5)
                            {
                                //smallint类型不能大于255
                                if (val > 65535)
                                {
                                    tooltip = true;
                                }
                            }
                        }

                        //小数 decimal 类型的验证，一般decimal类型为 15,2 的规则
                        if (validateType == ValidateType.Number)
                        {
                            decimal val = 0M;
                            if (!decimal.TryParse(input, out val))
                            {
                                tooltip = true;
                            }
                        }
                    }
                }
                else
                {
                    return "无类型‘" + validateType + "’的验证表达式，参数名称:‘" + inputName + "’[值:" + input + "]";
                }
            }

            if (tooltip)
            {
                return "参数‘" + inputName + "’不是有效的‘" + validateType + "’格式,[值:" + input + "]";
            }

            return string.Empty;
        }

        /// <summary>
        /// 正则表达式匹配库，根据传入的验证类型返回相应的正则表达式
        /// </summary>
        /// <param name="validateType">验证类型</param>
        /// <returns></returns>
        private static string MatchingLibrary(ValidateType validateType)
        {
            switch (validateType)
            {
                case ValidateType.UserName: //用户名，只能输入由数字和26个英文字母下划线组成的字符串
                    return @"^[A-Za-z0-9_]+$";
                case ValidateType.Password: //密码， 长度在4~20之间，只能包含字符、数字和下划线
                    return @"\w{3,19}$";
                case ValidateType.Tel: //电话号码， 支持分机和固话，以及手机
                    //return @"(^(0[0-9]{2,3}\-)?([2-9][0-9]{6,7})+(\-[0-9]{1,4})?$)|(^((\(\d{3}\))|(\d{3}\-))?(1[358]\d{9})$)";
                    //电话格式不需要严谨验证
                    return @"\d{0,20}";
                case ValidateType.Email: //邮箱
                    return @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
                case ValidateType.IDCard: //身份证[护照]18位，字母或者数字
                    return @"^[A-Za-z0-9]+$";
                case ValidateType.Number: //数字或者小数保留2位
                    return @"\d+[\.]?\d{0,2}";
                case ValidateType.Integer: //正整数
                    return @"^\+?[0-9]*$";
                case ValidateType.SiteNumber:
                    return @"^[A-Za-z0-9]+$";
                case ValidateType.TransCode:
                    //return @"^\d{10,18}$";
                    return @"^[A-Za-z0-9_]+$";
                case ValidateType.BankCard: //0-50位的数字组合
                    return @"\d{0,50}";
                case ValidateType.RegisterAccountRole://【账号长度4-15个字符,以及仅限含有字母和数字的组合】
                    return @"^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{4,15}$";
                case ValidateType.TelPhone:
                    return @"^1\d{10}$";
                case ValidateType.TelPhone_TW:
                    return @"^[1][3-8]\d{9}$|^([6|9])\d{7}$|^[0][9]\d{8}$|^[6]([8|6])\d{5}$";
                case ValidateType.RealName: //真实姓名，只允许输入中文以及英文和数字
                    return @"^[\u4e00-\u9fa5A-Za-z0-9?:·]+$";
                case ValidateType.KBGHRegisterAccountRole://【账号长度4-10个字符,以及仅限含有字母和数字的组合】
                    return @"^(?![0-9]+$)(?![a-zA-Z]+$)[0-9A-Za-z]{4,10}$";
                case ValidateType.ApiKey://必须是字母和数字的组合，不能有中文和空格
                    return @"^[A-Za-z0-9]+$";
            }

            return string.Empty;
        }

        #endregion

        #region 写入db_logcenter 数据库操作日志方法

        #region public static void OperateLog(Dictionary<string, object> parameters)

        /// <summary>
        /// 记录操作日志
        /// </summary>
        /// <param name="parameters"></param>
        public static void OperateLog(Dictionary<string, object> paras, string storedProcedure)
        {
            string editUser = HttpContext.Current == null ? "" : HttpContext.Current.User.Identity.Name;
            OperateType logType = ConvertTypeByStoredProcedure(storedProcedure);
            string log_targetid = paras.ContainsKey("log_targetid") ? paras["log_targetid"].ToString() : "log_targetid not find";
            string content = DictionaryToJsonArray(paras);
            object[] operateObj = new object[] { editUser, logType, content, storedProcedure, log_targetid };
            Thread thread = new Thread(delegate(object op_para)
            {
                try
                {
                    lock (ms_LogLock)
                    {
                        object[] op_parameter = op_para as object[];

                        SqlParameter[] parameters = { 
                            new SqlParameter("@log_pkgindex", SqlDbType.TinyInt),
                            new SqlParameter("@log_type", SqlDbType.VarChar, 50),
                            new SqlParameter("@log_content", SqlDbType.NVarChar, 4000),
                            new SqlParameter("@log_operate_sp", SqlDbType.VarChar, 50),
                            new SqlParameter("@log_user", SqlDbType.VarChar, 50),
                            new SqlParameter("@log_id", SqlDbType.BigInt),
                            new SqlParameter("@log_targetid", SqlDbType.VarChar, 50),
                        };

                        int initLength = 4000;
                        string subContent = op_parameter[2].ToString();
                        int subIndex = 0;
                        int count = int.Parse(Math.Ceiling(subContent.Length / (initLength * 1d)).ToString());
                        string[] array = new string[count];

                        while (true)
                        {
                            if (subContent.Length > initLength)
                            {
                                array[subIndex] = subContent.Substring(0, initLength);
                                subContent = subContent.Substring(initLength);
                            }
                            else if (subContent.Length > 0)
                            {
                                array[subIndex] = subContent;
                                break;
                            }
                            else
                            {
                                break;
                            }

                            subIndex++;
                        }

                        using (SqlConnection connection = new SqlConnection(SqlHelper.WriteConnection))
                        {
                            if (connection.State != ConnectionState.Open) { connection.Open(); }
                            SqlTransaction trans = connection.BeginTransaction();
                            try
                            {
                                int log_id = 0;
                                for (int i = 0; i < array.Length; i++)
                                {
                                    parameters[0].Value = i;
                                    parameters[1].Value = op_parameter[1];
                                    parameters[2].Value = array[i];
                                    parameters[3].Value = op_parameter[3];
                                    parameters[4].Value = op_parameter[0];
                                    parameters[5].Value = log_id;
                                    parameters[6].Value = op_parameter[4];

                                    object obj = SqlHelper.ExecuteScalar(trans, CommandType.StoredProcedure, "P_Logs_Add", parameters);
                                    log_id = obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
                                }

                                trans.Commit();
                            }
                            catch
                            {
                                trans.Rollback();
                                throw;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogByThread(ex);
                }
            });

            thread.Start(operateObj);
        }

        /// <summary>
        /// 日志类型转换方法
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <returns></returns>
        private static OperateType ConvertTypeByStoredProcedure(string storedProcedure)
        {
            switch(storedProcedure)
            {
                case "P_Merchant_Add":
                case "P_Merchant_Delete":
                case "P_Merchant_Edit":
                case "P_Merchant_EditCredit":
                case "P_Merchant_SetBaseInfo":
                    return OperateType.Merchant;

                case "P_Merchant_SetBankInfo":
                    return OperateType.MerchantBank;

                case "P_MerchantFee_Set":
                    return OperateType.Merchantfee;

                case "P_Manager_Add":
                case "P_Manager_Edit":
                case "P_Manager_SetBaseInfo":
                    return OperateType.Manager;

                case "P_Partner_Add":
                case "P_Partner_Edit":
                case "P_Partner_SetBaseInfo":
                    return OperateType.Partner;

                case "P_Deposit_Add":
                case "P_Deposit_SetStatus":
                    return OperateType.Deposit;

                case "P_Withdraw_Add":
                case "P_Withdraw_SetState":
                    return OperateType.Withdraw;

                case "P_User_UpdatePwd":
                    return OperateType.Pwdeditlog;

                case "P_GoogleValidator_SetStatus":
                    return OperateType.Google;

                case "P_Login_LogOut":
                case "P_Login_ValidateUser":
                case "P_MgrLogin_ValidateUser":
                    return OperateType.Logintrace;
                
                case "P_Up_Merchant_Add":
                case "P_Up_Merchant_Edit":
                case "P_Up_Merchant_SetBaseInfo":
                    return OperateType.Up_Merchant;

                case "P_Up_ReceiptAct_Add":
                case "P_Up_ReceiptAct_Edit":
                case "P_Up_ReceiptAct_SetBaseInfo":
                    return OperateType.Up_ReceiptAct;

                default:
                    return OperateType.None;

            }
        }

        /// <summary>      
        /// Dictionary转换成Json格式      
        /// </summary>      
        /// <param name="dt"></param>      
        /// <returns></returns>      
        private static string DictionaryToJsonArray(Dictionary<string, object> parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            foreach (KeyValuePair<string, object> item in parameters)
            {
                sb.Append("\"");
                sb.Append(item.Key.Substring(item.Key.IndexOf('_') + 1));
                sb.Append("\":");

                if (item.Value is DataTable)
                {
                    sb.Append(DataTableToJsonArray(item.Value as DataTable));
                }
                else if (item.Value is Dictionary<string, object>)
                {
                    sb.Append(DictionaryToJsonArray(item.Value as Dictionary<string, object>));
                }
                else
                {
                    sb.Append("\"");
                    sb.Append(item.Value);
                    sb.Append("\"");
                }
                sb.Append(",");
            }

            if (sb.ToString().Length == 1)
            {
                sb.Append("\"\"");
            }
            else
            {
                sb.Remove(sb.Length - 1, 1);
            }

            sb.Append("}");

            return sb.ToString().Replace("\\\"", "\\\\\"").Replace("\\r", "\\\\r").Replace("\\n", "\\\\n");
        }

        /// <summary>      
        /// dataTable转换成Json格式      
        /// </summary>      
        /// <param name="dt"></param>      
        /// <returns></returns>      
        private static string DataTableToJsonArray(DataTable dt)
        {
            //防止线程中出错
            DataTable dt_New = dt.Copy();

            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            for (int i = 0; i < dt_New.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (int j = 0; j < dt_New.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(dt_New.Columns[j].ColumnName);
                    jsonBuilder.Append("\":\"");
                    if (dt_New.Columns[j].ColumnName.StartsWith("TransCode"))
                    {
                        //组合交易码
                        jsonBuilder.Append(dt_New.Rows[i][j + 1].ToString() + Convert.ToInt32(dt_New.Rows[i][j]).ToString("D10"));
                    }
                    else
                    {
                        jsonBuilder.Append(dt_New.Rows[i][j].ToString().Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\""));
                    }

                    jsonBuilder.Append("\",");
                }

                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            if (jsonBuilder.Length > 0)
            {
                jsonBuilder.Append("]");
            }
            else
            {
                //添加一个空的Json内容
                jsonBuilder.Append("\"\"");
            }

            return jsonBuilder.ToString();
        }

        #endregion

        #endregion
    }

    #endregion

}