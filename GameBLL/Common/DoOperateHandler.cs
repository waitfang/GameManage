using System;
using System.IO;
using System.Web;
using System.Web.SessionState; 

namespace GameBLL.Common
{
    /// <summary>
    /// DoOperateHandler 的摘要描述
    /// </summary>
    public class DoOperateHandler : IHttpHandler, IRequiresSessionState
    {
        #region Global Variables

        const string QRYSRE_TYPECODE = "typecode";
        const string QRYSRE_PORTCODE = "portcode";
        const string QRYSRE_ACTIONTYPE = "actiontype";
        const string QRYSRE_PARACOUNT = "parascount";
        const string QRYSTR_PARA = "para";
        const string QRYSTR_ISPOSTBACK = "ispostback";
        const string QRYSTR_PAGECODE = "pagecode";

        //const string RESP_LOGOUT_DATA = "{ 'islogout' : true, 'result' : false, 'ErrorDetail' : '" + APIUtility.GetResource("LoginError4") + "' }";//当前帐号已经被登出或者登入超时!
        const string RESP_CALLBACK_SCRIPT = "<script type='text/javascript'>parent.{0}({1});</script>";
        const string FORMAT_JS_VERSION = "[\"{0}\", {1}]";
        #endregion

        #region public DoOperateHandler()

        public DoOperateHandler()
        {
            //
            // TODO: 在此加入建構函式的程式碼
            //
        }

        #endregion

        #region public bool IsReusable

        public bool IsReusable
        {
            get { return true; }
        }

        #endregion

        #region public void ProcessRequest(HttpContext context)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            InitCulture();
             

            bool pageHandlerFlag = true;

            bool isPost = string.Compare("POST", context.Request.RequestType) == 0;

            if (isPost)
            {
                #region POST验证，预留开发

                using (StreamReader inputStream = new StreamReader(context.Request.InputStream, System.Text.Encoding.UTF8))
                {
                    string postData = inputStream.ReadToEnd();

                    if (VerifyPostRequest(postData))
                    {
                        string requestPath = HttpContext.Current.Request.Path;
                        requestPath = requestPath.Substring(requestPath.LastIndexOf("/") + 1);

                        if (!string.IsNullOrEmpty(context.Request["typecode"]))
                        {
                            if (HttpContext.Current.User.Identity.IsAuthenticated)//提交了typecode以及登錄的情況下處理
                            {
                                pageHandlerFlag = true;
                            }
                        }
                    }
                }

                #endregion
            }

            if (pageHandlerFlag)
            {
                #region 新功能請求，如ajax開發的新功能都統一請求到當前Handler中

                int typeCode = 0, paraCount = 0, portCode = -1;
                string actionType = string.Empty, callbackMethod = string.Empty, pageCode = string.Empty;
                string[] paras;
                bool isPostBack = false;

                if (!VerifyGetRequest(ref typeCode, ref actionType, ref paraCount, out paras, ref callbackMethod, isPost, ref portCode, ref isPostBack, ref pageCode))
                {
                    ResponseScript("{ \"islogout\" : true, \"result\" : false, \"ErrorDetail\" : \"" + WebUtility.GetResource("LoginError4") + "\" }");//当前帐号已经被登出或者登入超时!
                    return;
                }
                 

                #endregion
            }
        }

        #endregion

        #region private void ResponseScript(string output)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        private void ResponseScript(string output)
        {
            ResponseScript(output, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="output"></param>
        /// <param name="formatJS"></param>
        private void ResponseScript(string output, bool formatJS)
        {
            if (string.IsNullOrEmpty(output))
            {
                return;
            }

            if (formatJS)
            {
                if (HttpContext.Current.Request["callback"] == null)
                {
                    HttpContext.Current.Response.Write(string.Format(FORMAT_JS_VERSION, "1.0", output));
                }
                else
                {
                    HttpContext.Current.Response.Write(string.Format(FORMAT_JS_VERSION, "1.0", string.Format(RESP_CALLBACK_SCRIPT, HttpContext.Current.Request["callback"], output)));
                }
            }
            else
            {
                if (HttpContext.Current.Request["callback"] == null)
                {
                    HttpContext.Current.Response.Write(output);
                }
                else
                {
                    HttpContext.Current.Response.Write(string.Format(RESP_CALLBACK_SCRIPT, HttpContext.Current.Request["callback"], output));
                }
            }
        }
        #endregion

        #region private bool VerifyPostRequest(string postData)

        private bool VerifyPostRequest(string postData)
        {
            //暫時驗證安全性功能沒有實作
            return true;
        }

        #endregion

        #region private bool VerifyGetRequest(ref int typeCode, ref string actionType, ref int paraCount, out string[] paras, ref string callbackMethod, bool isPost)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="actionType"></param>
        /// <param name="paraCount"></param>
        /// <param name="paras"></param>
        /// <param name="callbackMethod"></param>
        /// <param name="isPost"></param>
        /// <param name="portCode"></param>
        /// <returns></returns>
        private bool VerifyGetRequest(ref int typeCode, ref string actionType, ref int paraCount, out string[] paras, ref string callbackMethod, bool isPost, ref int portCode, ref bool isPostBack, ref string pageCode)
        {
            paras = null;

            HttpRequest request = HttpContext.Current.Request;

            //驗證類別代碼是否為數字
            string validText = request[QRYSRE_TYPECODE];
            if (string.IsNullOrEmpty(validText))
            {
                return false;
            }

            if (!int.TryParse(validText, out typeCode))
            {
                return false;
            }

            //獲取端口號,不是必須提交數據,如果沒有提交默認是會員端
            validText = request[QRYSRE_PORTCODE];
            if (!int.TryParse(validText, out portCode))
            {
                portCode = -1;
            }

            //驗證動作類型是否為數字
            validText = request[QRYSRE_ACTIONTYPE];
            if (string.IsNullOrEmpty(validText))
            {
                return false;
            }
            actionType = validText;

            validText = request[QRYSTR_PAGECODE];
            if (!string.IsNullOrEmpty(validText))
            {
                pageCode = validText;
            }

            //驗證提交的參數是否完整
            if (!int.TryParse(request[QRYSRE_PARACOUNT], out paraCount))
            {
                return false;
            }

            paras = new string[paraCount];

            for (int i = 0; i < paraCount; i++)
            {
                if (request[QRYSTR_PARA + i.ToString()] == null)
                {
                    return false;
                }

                paras[i] = request[QRYSTR_PARA + i.ToString()];
            }

            return true;
        }

        #endregion

     
        #region private void InitCulture()
        /// <summary>
        /// 
        /// </summary>
        private void InitCulture()
        {
            foreach (string seg in HttpContext.Current.Request.Url.Segments)
            {
                if (seg.IndexOf("-") != -1)
                {
                    WebUtility.ClientCulture = seg.TrimEnd('/').TrimEnd('2');
                    break;
                }
            }
        }
        #endregion
    }
}