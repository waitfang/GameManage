using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameBLL.Json;
using GameDAL;

namespace GameBLL.Common
{
    public static class APIUtility
    {
        #region Global Variables
        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<string, object> handlerCache = new Dictionary<string, object>();
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

                if (methodObject == null)
                {
                    sgresult_base resultObj = new sgresult_base();

                    //表示方法不存在，不需要写入日志
                    errorInfo errObj = new errorInfo();

                    errObj.errorCode = "";
                    errObj.error = "请求方法" + targetObject.ToString() + "[" + methodName + "]不存在";
                    errObj.errorDetail = "Method [" + methodName + "] not exist in object " + targetObject.ToString();

                    resultObj.result = false;

                    resultObj.errorInfo.Add(errObj);

                    return resultObj.ToString();
                }
                else
                {
                    return methodObject.Invoke(targetObject, args);
                }
            }
            catch (Exception ex)
            {
                MethodUtility.LogByThread<Exception>(ex, null, 0L, 0);

                if (ex.InnerException != null)
                {
                    MethodUtility.LogByThread<Exception>(ex.InnerException, null, 0L, 0);
                }

                sgresult_base resultObj = new sgresult_base();

                errorInfo errObj = new errorInfo();

                errObj.errorCode = "";
                errObj.error = "请求方法" + targetObject.ToString() + "[" + methodName + "]不存在";
                errObj.errorDetail = "Method [" + methodName + "] not exist in object " + targetObject.ToString();

                if (ex.InnerException != null)
                {
                    errObj.errorDetail += Environment.NewLine + ex.InnerException.Message + Environment.NewLine + ex.InnerException.StackTrace;
                }

                resultObj.errorInfo.Add(errObj);

                resultObj.result = false;

                return resultObj.ToString();
            }
        }
        #endregion

        #region public static object CreateSGInstance(string className)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static object CreateSGInstance(string className)
        {
            string cacheKey = "GameBLL.GameUI." + className.ToLower();
            if (!handlerCache.ContainsKey(cacheKey) || handlerCache[cacheKey] == null)
            {
                lock (typeof(APIUtility))
                {
                    handlerCache[cacheKey] = DALCore.LoadAssamblyType<object>(cacheKey);
                }
            }

            return handlerCache[cacheKey];
        }
        #endregion

        #region public static void DisposeLoad()

        public static void DisposeLoad()
        {
            lock (typeof(APIUtility))
            {
                handlerCache.Clear();
            }
        }
        #endregion


        public static bool IsNumeric(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
    }
}
