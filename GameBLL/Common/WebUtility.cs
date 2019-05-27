using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Web.Script.Serialization;
using System.Net;
using System.Configuration;
using System.Runtime.Serialization.Json; 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GameModel;

namespace GameBLL.Common
{
    public static class WebUtility
    {
        static WebUtility()
        {
            CallbackParas[1] = new string[4];
            CallbackParas[1][0] = "partyId";
            CallbackParas[1][1] = "appType";
            CallbackParas[1][2] = "orderNo";
            CallbackParas[1][3] = "tradeNo";
        }

        private static XmlDocument xmlDoc;

        public static XmlDocument XmlDoc
        {
            get
            {
                return LoadXml();
            }
        }

        #region Properties

        #region public static string UserName

        /// <summary>
        /// 使用者帳號
        /// </summary>
        public static string UserName
        {
            get
            {
                //return "gateway_WMTT000197001";
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.User.Identity.Name.ToUpper();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #endregion

        #region public static string UserIp
        /// <summary>
        /// 使用者IP
        /// </summary>
        public static string UserIp
        {
            get
            {
                string retText = "";

                if (HttpContext.Current == null)
                {
                    retText = "127.0.0.1";
                }
                else
                {
                    //按照优先级别读取IP
                    string[] headerValuesList = new string[] {
                    HttpContext.Current.Request.ServerVariables["HTTP_CF_CONNECTING_IP"],
                    HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"],
                    HttpContext.Current.Request.Headers["X-Client-Address"],
                    HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]
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

        #endregion
 

        #region public static int PlantformPayType
        /// <summary>
        /// 目前1表示3mxta以后继续扩充
        /// </summary>
        public static int PlantformPayType = 1;
        #endregion

        #region public static Dictionary<int, string> CallbackParas
        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<int, string[]> CallbackParas = new Dictionary<int, string[]>();
        #endregion

        #endregion

        #region public static string ClientCulture
        /// <summary>
        ///  獲取客戶端選擇的語言類別
        /// </summary>
        public static string ClientCulture
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    return System.Threading.Thread.CurrentThread.CurrentCulture.ToString().ToLower();
                }

                return HttpContext.Current.Request.QueryString["ln"] == null ? System.Threading.Thread.CurrentThread.CurrentCulture.ToString().ToLower() : HttpContext.Current.Request.QueryString["ln"].ToLower();
            }
            set
            {
                try
                {
                    System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(value);
                    System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }
                catch (Exception ex)
                {
                    SysUtility.LogByThread(ex);
                }
            }
        }
        #endregion

        #region public static string RequestFileName
        /// <summary>
        /// 請求頁面名稱
        /// </summary>
        public static string RequestFileName
        {
            get
            {
                string requestPath = HttpContext.Current.Request.Path;
                return requestPath.Substring(requestPath.LastIndexOf("/") + 1);
            }
        }
        #endregion

        #region public static string DefaultNameSpace
        /// <summary>
        /// 
        /// </summary>
        public static string DefaultNameSpace
        {
            get
            {
                string defNS = System.Configuration.ConfigurationManager.AppSettings["DefNS"];

                if (defNS == null) return "";

                return defNS;
            }
        }
        #endregion

        #region public static string GetGlobalResourceByKey(string key)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetResource(string key)
        {
            object resxObj = HttpContext.GetGlobalResourceObject("Resource", key);
            if (resxObj == null)
            {
                return key;
            }

            return resxObj.ToString();
        }

        public static string GetResourceByCulture(string key)
        {
            return GetResourceByCulture(key, "zh-cn");
        }

        /// <summary>
        /// 指定语系的资源档读取
        /// </summary>
        /// <param name="key"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static string GetResourceByCulture(string key, string culture)
        {
            System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(culture);
            object resxObj = HttpContext.GetGlobalResourceObject("Resource", key, cultureInfo);
            if (resxObj == null)
            {
                return key;
            }

            return resxObj.ToString();
        }

        #endregion

        #region public static SortedDictionary<string, string> DictonarySort(Dictionary<string, string> dicObject)

        /// <summary>
        /// 字典排序方法【按照指定类型进行指定排序】
        /// </summary>
        /// <param name="dicObject">字典对象</param>
        /// <param name="sortBy">排序方式，1按照key排序，2按照value排序</param>
        /// <param name="sortType">排序类型，1升序，2降序</param>
        public static SortedDictionary<string, object> DictonarySort(Dictionary<string, object> dicObject)
        {
            SortedDictionary<string, object> sortedObj = new SortedDictionary<string, object>();

            foreach (string key in dicObject.Keys)
            {
                sortedObj[key] = dicObject[key];
            }

            return sortedObj;
        }

        #endregion

        #region public static Dictionary<string, object> ReversalJson(string result)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReversalJson(string result)
        {
            try
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.MaxJsonLength = Int32.MaxValue;
                Dictionary<string, object> resultObj = jss.DeserializeObject(result) as Dictionary<string, object>;
                return resultObj;
            }
            catch (Exception ex)
            {
                SysUtility.RequestLog(ex.ToString() + "|" + result);
            }

            return null;
        }

        #endregion

        #region public static object InvokeMethod(object targetObject, string methodName, object[] args)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object InvokeMethod(object targetObject, string methodName, object[] args)
        {
            try
            {
                if (methodName.IndexOf(".") != -1)
                {
                    methodName = methodName.Split('.')[0];
                }

                MethodInfo methodObject = targetObject.GetType().GetMethod(methodName.ToLower());

                return methodObject.Invoke(targetObject, args);
            }
            catch (Exception ex)
            {
                SysUtility.LogByThread(ex);

                if (ex.InnerException != null)
                {
                    SysUtility.LogByThread(ex.InnerException);
                }

                result_base resultObj = new result_base();
                error_base errObj = new error_base();

                errObj.errorCode = "";
                errObj.error = "请求方法" + targetObject.ToString() + "[" + methodName + "]不存在";
                errObj.errorDetail = "Method [" + methodName + "] not exist in object " + targetObject.ToString();

                resultObj.errorInfo.Add(errObj);

                resultObj.result = false;

                return resultObj.ToString();
            }
        }
        #endregion

        #region Json转换为字典
        /// <summary>
        /// Json转换为字典
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static JObject JsonToDictionary(string result)
        {
            return JsonConvert.DeserializeObject<JObject>(result);
        }
        #endregion



        #region public static string GetRequestData(string key)

        /// <summary>
        /// 获取提交数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetRequestData(string key)
        {
            return GetRequestData(key, string.Empty);
        }

        /// <summary>
        /// 获取提交数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static string GetRequestData(string key, string defaultVal)
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                return HttpContext.Current.Request[key] == null || HttpContext.Current.Request[key].Trim() == "" ? defaultVal : HttpContext.Current.Request[key].Trim();
            }
            else
            {
                return defaultVal;
            }
        }

        public static string GetRequestFormData(string key)
        {
            return GetRequestFormData(key, string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetRequestFormData(string key, string defaultVal)
        {
            if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Form.Count > 0)
            {
                return HttpContext.Current.Request.Form[key] == null || HttpContext.Current.Request.Form[key].Trim() == "" ? defaultVal : HttpContext.Current.Request.Form[key].Trim();
            }
            else
            {
                return defaultVal;
            }
        }

        #endregion

        #region public static string ComValidation<T>(Task<T>[] tooltips)
        /// <summary>
        /// 组合验证的消息内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tooltips"></param>
        /// <returns></returns>
        public static string ComValidation<T>(Task<T>[] tooltips)
        {
            StringBuilder tips = new StringBuilder();
            for (int i = 0; i < tooltips.Length; i++)
            {
                if (tooltips[i].Result.ToString().Length > 0)
                {
                    tips.Append(tooltips[i].Result).Append("|");
                }
            }

            if (tips.ToString().Length > 0)
            {
                //验证不正确返回异常的Json
                return WebUtility.ErrorToJson(tips.ToString());
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion

        #region 加密相关方法

        #region MD5 加密方法
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetMd5Hash(string input)
        {
            MD5 ms_MD5Object = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = ms_MD5Object.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        #endregion

        #region RSA 加密解密

        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publickey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string publickey, string content)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(publickey);
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(content), false);

            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="privatekey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSADecrypt(string privatekey, string content)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(privatekey);
            cipherbytes = rsa.Decrypt(Convert.FromBase64String(content), false);

            return Encoding.UTF8.GetString(cipherbytes);
        }

        /// <summary>
        /// RSA 签名
        /// </summary>
        /// <param name="str_DataToSign"></param>
        /// <param name="str_Private_Key"></param>
        /// <returns></returns>
        public static string RSAHashAndSign(string privatekey, string content)
        {
            byte[] dataToSign = Encoding.UTF8.GetBytes(content);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privatekey);
            byte[] signedData = rsa.SignData(dataToSign, "SHA1");
            string str_SignedData = Convert.ToBase64String(signedData);
            return str_SignedData;
        }

        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="content">字符串内容</param>
        /// <param name="hashAndSign">签名值</param>
        /// <param name="publickey">公钥</param>
        /// <returns></returns>
        public static bool RSAVerifySignedHash(string content, string hashAndSign, string publickey)
        {
            byte[] signedData = Convert.FromBase64String(hashAndSign);
            byte[] DataToVerify = Encoding.UTF8.GetBytes(content);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publickey);

            return rsa.VerifyData(DataToVerify, "SHA1", signedData);
        }

        /// <summary>
        /// 生成RSA key
        /// </summary>
        public static void RSAKey()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
            using (StreamWriter writer = new StreamWriter("PrivateKey.xml"))  //这个文件要保密...
            {

                writer.WriteLine(rsa.ToXmlString(true));

            }
            using (StreamWriter writer = new StreamWriter("PublicKey.xml"))
            {

                writer.WriteLine(rsa.ToXmlString(false));

            }
        }

        #endregion

        #region AES 加密解密

        /// <summary>
        /// 随机生成密钥
        /// </summary>
        /// <returns></returns>
        public static string GetAesKey(int n)
        {
            char[] arrChar = new char[]{
           'a','b','d','c','e','f','g','h','i','j','k','l','m','n','p','r','q','s','t','u','v','w','z','y','x',
           '0','1','2','3','4','5','6','7','8','9',
           'A','B','C','D','E','F','G','H','I','J','K','L','M','N','Q','P','R','T','S','V','U','W','X','Y','Z'
          };

            StringBuilder num = new StringBuilder();

            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < n; i++)
            {
                num.Append(arrChar[rnd.Next(0, arrChar.Length)].ToString());

            }

            return num.ToString();
        }

        ///<summary>
        /// 有密码的AES加密 
        /// </summary>
        /// <param name="text">加密字符</param>
        /// <param name="aesKey">加密的密码</param>
        /// <param name="iv">密钥</param>
        /// <returns></returns>
        public static string AesEncrypt(string text, string aesKey, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(aesKey);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(text);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;//加密模式
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="aesKey"></param>
        /// <returns></returns>
        public static string AesEncrypt(string str, string aesKey)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(aesKey),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="aesKey"></param>
        /// <returns></returns>
        public static string AesDecrypt(string str, string aesKey)
        {
            if (string.IsNullOrEmpty(str)) return null;
            Byte[] toEncryptArray = Convert.FromBase64String(str);

            System.Security.Cryptography.RijndaelManaged rm = new System.Security.Cryptography.RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(aesKey),
                Mode = System.Security.Cryptography.CipherMode.ECB,
                Padding = System.Security.Cryptography.PaddingMode.PKCS7
            };

            System.Security.Cryptography.ICryptoTransform cTransform = rm.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        /// aes解密
        /// </summary>
        /// <param name="str"></param>
        /// <param name="aesKey"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static string AesDecrypt(string str, string aesKey, string iv)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(aesKey);
            byte[] ivArray = UTF8Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = Convert.FromBase64String(str);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        #endregion

        #region HMAC 加密算法

        /// <summary>
        /// HMAC256加密方法
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Hmac256Encrypt(string strText, string key)
        {
            HMACSHA256 hmacsha256 = new HMACSHA256(Encoding.Default.GetBytes(key));
            hmacsha256.ComputeHash(Encoding.Default.GetBytes(strText));

            StringBuilder displayString = new StringBuilder();
            foreach (byte test in hmacsha256.Hash)
            {
                displayString.Append(test.ToString("X2"));
            }

            return displayString.ToString().ToLower();
        }

        #endregion

        #region SHA256 加密
        /// <summary>
        /// SHA256 加密
        /// </summary>
        /// <param name="strText">待加密字符串</param>
        /// <returns></returns>
        public static string GetSHA256Encrypt(string strText)
        {
            byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(strText);
            try
            {
                SHA256 sha256 = new SHA256CryptoServiceProvider();
                byte[] retVal = sha256.ComputeHash(bytValue);
                StringBuilder displayString = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    displayString.Append(retVal[i].ToString("x2"));
                }
                return displayString.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetSHA256HashFromString() fail,error:" + ex.Message);
            }
        }
        #endregion

        #region 密码加密

        /// <summary>
        /// 获取新加密规则的密码
        /// </summary>
        /// <param name="strPartyID"></param>
        /// <param name="strUID"></param>
        /// <param name="strPwd"></param>
        /// <param name="IsBack">True:后台调用,False:商号调用</param>
        /// <returns></returns>
        public static string GetNewPwdEncryption(string strUID, string strPwd, bool IsBack)
        {
            string strEncryptionPwd = "";
            //if (IsBack)
            //{
            //    //留给后台备用
            //}
            string strKey = strUID + strPwd + ConfigurationManager.AppSettings["PwdKey"];
            strEncryptionPwd = GetMd5Hash(strKey.ToUpper());
            return strEncryptionPwd.ToUpper();
        }

        #endregion

        #endregion

        #region API加密以及验签方法

        /// <summary>
        /// 参数提交验证方法
        /// </summary>
        /// <param name="merchantKey">商户密钥</param>
        /// <param name="parasText">返回提交的参数</param>
        /// <returns></returns>
        public static bool VerifySignedHash(string merchantKey, out string parasText)
        {
            bool retVal = false;
            parasText = string.Empty;
            try
            {
                if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.Form.Count > 0)
                {
                    Dictionary<string, object> paras = new Dictionary<string, object>();
                    string sign = string.Empty;
                    for (int i = 0; i < HttpContext.Current.Request.Form.Count; i++)
                    {
                        if (HttpContext.Current.Request.Form.Keys[i].ToLower() != "sign" && HttpContext.Current.Request.Form.Keys[i].ToLower() != "submit")
                        {
                            paras[HttpContext.Current.Request.Form.Keys[i]] = HttpContext.Current.Request.Form[i];
                        }

                        if (HttpContext.Current.Request.Form.Keys[i].ToLower() == "sign")
                        {
                            sign = HttpContext.Current.Request.Form[i];
                        }
                    }

                    SortedDictionary<string, object> sortedObj = DictonarySort(paras);
                    StringBuilder sb = new StringBuilder();
                    foreach (KeyValuePair<string, object> item in sortedObj)
                    {
                        sb.Append(item.Key);
                        sb.Append("=");
                        sb.Append(item.Value.ToString());
                        sb.Append("&");
                    }

                    if (sb.ToString().Length > 0)
                    {
                        sb.Append(merchantKey);
                    }

                    string localSign = GetMd5Hash(sb.ToString());
                    if (localSign.ToLower() == sign.ToLower())
                    {
                        retVal = true;
                    }

                    parasText = sb.ToString();
                    SysUtility.RequestLog(parasText);//写入请求日志
                }
                else if (HttpContext.Current != null && HttpContext.Current.Request != null && HttpContext.Current.Request.QueryString.Count > 0)
                {
                    Dictionary<string, object> paras = new Dictionary<string, object>();
                    string sign = string.Empty;
                    for (int i = 0; i < HttpContext.Current.Request.QueryString.Count; i++)
                    {
                        if (HttpContext.Current.Request.QueryString.Keys[i].ToLower() != "sign")
                        {
                            paras[HttpContext.Current.Request.QueryString.Keys[i]] = HttpContext.Current.Request.QueryString[i];
                        }

                        if (HttpContext.Current.Request.QueryString.Keys[i].ToLower() == "sign")
                        {
                            sign = HttpContext.Current.Request.QueryString[i];
                        }
                    }

                    SortedDictionary<string, object> sortedObj = DictonarySort(paras);
                    StringBuilder sb = new StringBuilder();
                    foreach (KeyValuePair<string, object> item in paras)
                    {
                        sb.Append(item.Key);
                        sb.Append("=");
                        sb.Append(item.Value.ToString());
                        sb.Append("&");
                    }

                    if (sb.ToString().Length > 0)
                    {
                        sb.Append(merchantKey);
                    }

                    string localSign = GetMd5Hash(sb.ToString());
                    if (localSign == sign)
                    {
                        retVal = true;
                    }

                    parasText = sb.ToString();
                    SysUtility.RequestLog(parasText);//写入请求日志
                }
                else
                {
                    if (HttpContext.Current == null)
                    {
                        parasText = "HttpContext.Current 对象为NULL！";
                    }
                    else if (HttpContext.Current.Request == null)
                    {
                        parasText = "HttpContext.Current.Request 对象为NULL！";
                    }
                    else if (HttpContext.Current.Request.Form.Count == 0)
                    {
                        parasText = "HttpContext.Current.Request.Form.Count 笔数为0 ！";
                    }
                }
            }
            catch (Exception ex)
            {
                SysUtility.LogByThread(ex);
            }

            return retVal;
        }

        #endregion

        #region 转换数据为Json字符串

        /// <summary>
        /// 当出现异常以及数据验证不正确时，返回一个异常Json
        /// </summary>
        /// <param name="errorDetails"></param>
        /// <returns></returns>
        public static string ErrorToJson(string errorDetails)
        {
            return ErrorToJson("", errorDetails);
        }

        public static string ErrorToJson(string errorCode, string errorDetails)
        {
            Dictionary<string, string> errorObj = new Dictionary<string, string>();

            errorObj["ErrorCode"] = errorCode;

            if (!string.IsNullOrEmpty(errorDetails))
            {
                errorObj["ErrorDetail"] = errorDetails;
            }

            return FormatableObject.FormatToJson(errorObj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static string DataSetToJson(DataSet ds)
        {
            return DataSetToJson(ds, -1);
        }

        /// <summary>
        /// 将多个DataSet结果组成Json数组
        /// </summary>
        /// <returns></returns>
        public static string DataSetToJsonArray(DataSet ds)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (DataTable dt in ds.Tables)
            {
                sb.Append(DataSetToJson(dt, -1)).Append(",");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");

            return sb.ToString();
        }

        /// <summary>
        /// 将多个DataSet结果组成Json数组
        /// </summary>
        /// <returns></returns>
        public static string DataSetToJsonArray(DataSet ds, int total, int totalIndex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                if (i == totalIndex)
                {
                    sb.Append(DataSetToJson(ds.Tables[i], total)).Append(",");
                }
                else
                {
                    sb.Append(DataSetToJson(ds.Tables[i], -1)).Append(",");
                }
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append("]");

            return sb.ToString();
        }

        /// <summary>
        /// 转换DataSet为Json字符串
        /// </summary>
        /// <param name="ds">数据源</param>
        /// <param name="total">数据总数 -1表示不设定total</param>
        /// <returns></returns>
        public static string DataSetToJson(DataSet ds, int total)
        {
            //if (ds == null) 
            //{
            //    return string.Empty;
            //}

            //if (ds.Tables[0].Rows.Count == 0)
            //{
            //    return string.Empty;
            //}

            return DataSetToJson(ds.Tables[0], total);
        }

        /// <summary>
        /// 转换DataTable为Json字符串， 带分页的数据
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="total">数据总数 -1表示不设定total</param>
        /// <returns></returns>
        public static string DataSetToJson(DataTable dt, int total)
        {
            StringBuilder jsonBuilder = new StringBuilder();

            jsonBuilder.Append("{");

            if (total > -1)
            {
                jsonBuilder.Append("\"total\":").Append(total).Append(",");
            }

            jsonBuilder.Append("\"rows\":");

            jsonBuilder.Append(DataTableToJsonArray(dt));

            jsonBuilder.Append("}");

            return jsonBuilder.ToString();
        }

        /// <summary>      
        /// dataTable转换成Json格式      
        /// </summary>      
        /// <param name="dt"></param>      
        /// <returns></returns>      
        public static string DataTableToJsonArray(DataTable dt)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = Int32.MaxValue;
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(dt.Columns[j].ColumnName);
                    jsonBuilder.Append("\":");
                    if (dt.Columns[j].ColumnName.StartsWith("TransCode"))
                    {
                        jsonBuilder.Append("\"");
                        //组合交易码
                        string c_id = dt.Rows[i][j].ToString().Length == 0 ? "" : Convert.ToInt32(dt.Rows[i][j]).ToString("D10");
                        jsonBuilder.Append(dt.Rows[i][j + 1].ToString() + c_id);
                        jsonBuilder.Append("\"");
                    }
                    else
                    {
                        object jsonText = jss.Serialize(dt.Rows[i][j].ToString());
                        //jsonBuilder.Append(dt.Rows[i][j].ToString().Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"").Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\0", "\\0").Replace("\b", "\\b"));
                        jsonBuilder.Append(jsonText.ToString());
                    }

                    jsonBuilder.Append(",");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string DictionaryToUrl(Dictionary<string, object> inputs)
        {
            StringBuilder requestUrl = new StringBuilder();

            foreach (string key in inputs.Keys)
            {
                requestUrl.Append(key).Append("=").Append(inputs[key]).Append("&");
            }

            requestUrl.Length--;

            return requestUrl.ToString();
        }

        /// <summary>
        /// 把字符串转换为Json
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StringToJson(Dictionary<string, object> inputs)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = Int32.MaxValue;
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            foreach (KeyValuePair<string, object> item in inputs)
            {
                jsonBuilder.Append("{");
                jsonBuilder.Append("\"");
                jsonBuilder.Append(item.Key);
                jsonBuilder.Append("\":");
                string objText = jss.Serialize(item.Value.ToString());
                jsonBuilder.Append(objText);
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

        public static string StringToJson(Dictionary<string, object> inputs, bool isNeedReplace)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            foreach (KeyValuePair<string, object> item in inputs)
            {
                jsonBuilder.Append("{");
                jsonBuilder.Append("\"");
                jsonBuilder.Append(item.Key);
                jsonBuilder.Append("\":\"");
                if (isNeedReplace)
                {
                    jsonBuilder.Append(item.Value.ToString().Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"").Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\0", "\\0").Replace("\b", "\\b"));
                }
                else
                {
                    jsonBuilder.Append(item.Value.ToString().Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"").Replace("\t", "\\t").Replace("\0", "\\0").Replace("\b", "\\b"));
                }
                jsonBuilder.Append("\"");
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

        /// <summary>
        /// 将字典类型转换为Json
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public static string DictionaryToJsonArray(Dictionary<string, string> inputs)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            foreach (KeyValuePair<string, string> item in inputs)
            {
                jsonBuilder.Append("{");
                jsonBuilder.Append("\"");
                jsonBuilder.Append("GameName");
                jsonBuilder.Append("\":");
                jsonBuilder.Append("\"");
                jsonBuilder.Append(item.Key);
                jsonBuilder.Append("\"");
                jsonBuilder.Append(",");
                jsonBuilder.Append("\"Content\"");
                jsonBuilder.Append(":");
                jsonBuilder.Append(item.Value == "" ? "null" : item.Value);
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

        #region List转json
        /// <summary>
        /// List转json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ListToJson<T>(T t)
        {
            return JsonConvert.SerializeObject(t);
        }
        #endregion

        /// <summary>
        /// Json转换为对象 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static JObject JsonToObj(string json)
        {
            return JsonConvert.DeserializeObject<JObject>(json);
        }

        #endregion

        #region 转换BankCode

        #region public static XmlDocument LoadXml()

        /// <summary>
        /// 加载XML文件的内容
        /// </summary>
        /// <returns></returns>
        public static XmlDocument LoadXml()
        {
            if (xmlDoc == null)
            {
                XmlDocument doc = new XmlDocument();
                StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("FG.Payment.Common.BankCode.xml"));
                string xmlSource = reader.ReadToEnd();
                doc.LoadXml(xmlSource);

                xmlDoc = doc;
            }

            return xmlDoc;
        }

        #endregion

        #region public static Dictionary<string, string> GetAvailableBankList()
        /// <summary>
        /// 获取有效银行列表
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetAvailableBankList()
        {
            XmlDocument doc = LoadXml();

            XmlNodeList bankItems = doc.GetElementsByTagName("subitem");

            Dictionary<string, string> retList = new Dictionary<string, string>();

            foreach (XmlNode item in bankItems)
            {
                retList[item.Attributes["key"].Value] = item.Attributes["name_cn"].Value;
            }

            return retList;
        }
        #endregion

        /// <summary>
        /// 转换Code
        /// </summary>
        /// <param name="strItemsID">ItemsID(支付类型)</param>
        /// <param name="strKey">Key</param>
        /// <param name="strType">获取需要的属性值</param>
        public static string ChangeCode(string strItemsID, string strKey, string strType)
        {
            XmlNode node = XmlDoc.SelectSingleNode(string.Format("//Items[@id='{0}']/subitem[@key='{1}']", strItemsID, strKey));
            string strBankCode = string.Empty;
            if (node != null)
            {
                return node.Attributes[strType].InnerText;
            }
            return strBankCode;
        }

        #endregion

        #region public static long ConvertDataTimeLong()
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long ConvertDataTimeLong()
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan toNow = DateTime.Now.Subtract(dtStart);
            long timeStamp = toNow.Ticks;
            timeStamp = long.Parse(timeStamp.ToString().Substring(0, timeStamp.ToString().Length - 4));
            return timeStamp;
        }
        #endregion

        #region public static long ConvertDateTimeInt()
        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ConvertDateTimeInt()
        {
            DateTime time = DateTime.Now;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 8, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;//除10000调整为13位
            return t;
        }
        #endregion
         

        #region Post HttpWebRequest数据

        /// <summary>
        /// 返回request对象
        /// </summary>
        /// <param name="requestUrl">地址</param>
        /// <returns></returns>
        public static HttpWebRequest CreateCustomRequest(string requestUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl.ToString());
            request.Timeout = 20000;
            request.ReadWriteTimeout = 20000;
            request.ServicePoint.ConnectionLimit = int.MaxValue;
            request.ServicePoint.Expect100Continue = false;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.AllowWriteStreamBuffering = false;
            request.KeepAlive = false;
            request.Proxy = null;
            return request;
        }

        /// <summary>
        /// 模拟Post请求
        /// </summary>
        /// <param name="reqText">请求数据</param>
        /// <param name="requestUrl">请求地址</param>
        /// <param name="contentType">请求类型</param>
        /// <param name="logPath">log地址</param>
        /// <param name="fileName">log文件命名</param>
        /// <returns></returns>
        public static string MockPostReqeust(string reqText, string requestUrl, string contentType, string logPath, string fileName)
        {
            string result = string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(reqText);
            HttpWebRequest request = CreateCustomRequest(requestUrl);
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)";
            request.Method = "POST";
            if (!string.IsNullOrEmpty(contentType))
            {
                request.ContentType = contentType;
            }
            request.ContentLength = bytes.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                    sr.Close();
                }

                SysUtility.RequestLog("请求参数：" + reqText + ",地址：" + requestUrl + "，结果：" + result, fileName);
            }
            catch (WebException we)
            {
                HttpWebResponse httpResp = (HttpWebResponse)we.Response;
                try
                {
                    if (httpResp != null && (httpResp.StatusCode == HttpStatusCode.InternalServerError || httpResp.StatusCode == HttpStatusCode.NotFound))
                    {
                        using (Stream stream = we.Response.GetResponseStream())
                        {
                            StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                            result = sr.ReadToEnd().Trim();
                            SysUtility.RequestLog(reqText + "====" + result, fileName);
                        }
                    }
                    else
                    {
                        SysUtility.RequestLog(reqText + "====" + we.ToString(), fileName);
                    }
                }
                catch
                {
                    SysUtility.RequestLog(reqText + "====" + we.ToString(), fileName);
                }
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }
                request = null;
            }
            return result;
        }
        #endregion

    

        #region 验签加密

        /// <summary>
        /// 验签加密
        /// </summary>
        /// <param name="paras">参数</param>
        /// <param name="merchantKey">商户密钥</param>
        /// <returns></returns>
        public static string SignedHash(Dictionary<string, object> paras, string merchantKey)
        {
            SortedDictionary<string, object> sortedObj = WebUtility.DictonarySort(paras);
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> item in sortedObj)
            {
                sb.Append(item.Key);
                sb.Append("=");
                sb.Append(item.Value.ToString());
                sb.Append("&");
            }

            if (sb.ToString().Length > 0)
            {
                sb.Append(merchantKey);
            }

            return WebUtility.GetMd5Hash(sb.ToString());
        }

        #endregion
    }
}
