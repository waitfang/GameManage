using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GameBLL.Common; 

namespace GameBLL.Manager
{
    public class LoginManager
    {
        /// <summary>
        /// 登陆验证
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public string UserValidateLogin(string userid,string pwd)
        {
            try
            {
                #region 验证数据格式是否有效
                string userIP = ClientIp();
                Task<string>[] tooltips = new Task<string>[]{
                    Task<string>.Factory.StartNew(()=>SysUtility.ValidationData(userid, ValidateType.None, 50, "m_loginname", false)),
                    Task.Factory.StartNew(()=>SysUtility.ValidationData(pwd, ValidateType.Password, 100, "m_pwd", false)),
                    Task.Factory.StartNew(()=>SysUtility.ValidationData(userIP, ValidateType.None, 100, "m_ip", false)),
                };

                string tips = WebUtility.ComValidation<string>(tooltips);
                if (tips.Length > 0)
                {
                    //验证不正确返回异常的Json
                    return tips;
                }
                #endregion
                Dictionary<string, object> paras = new Dictionary<string, object>();
                paras["m_loginname"] = userid;
                paras["m_pwd"] = WebUtility.GetNewPwdEncryption(userid, pwd, true);
                paras["m_ip"] = userIP;
                 
                return userid;
            }
            catch (Exception ex)
            {
                SysUtility.LogByThread<Exception>(ex, null, 0L, 0);

                return "-1";
            }
        }


        public string ClientIp()
        {
            string retText = "";

            if (System.Web.HttpContext.Current == null)
            {
                retText = "127.0.0.1";
            }
            else
            {
                //按照优先级别读取IP
                string[] headerValuesList = new string[] {
                    System.Web.HttpContext.Current.Request.ServerVariables["HTTP_CF_CONNECTING_IP"],
                    System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"],
                    System.Web.HttpContext.Current.Request.Headers["X-Client-Address"],
                    System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]
                };

                foreach (string value in headerValuesList)
                {
                    retText = value;
                    if (!string.IsNullOrEmpty(retText))
                    {
                        break;
                    }
                };
            }

            if (string.IsNullOrEmpty(retText) || retText == "::1")
            {
                retText = "127.0.0.1";
            }
            return retText;
        }
    }
}
