/*-------------------------------------*
 * 創建人:          Naker
 * 創建時間:        2012/10/22
 * 最后修改時間:    2012/10/22
 * 最后修改原因:    
 * 修改歷史:
 * 2011/10/22       Naker       創建
 *-------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web.Script.Serialization;

namespace GameBLL.Json
{
    public class FormatableObject
    {
        #region public override string ToString()
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string formatType = ConfigurationManager.AppSettings["FormatType"];

            if (formatType == "JSON")
            {
                return FormatToJson(this);
            }

            return base.ToString();
        }
        #endregion

        #region public static string FormatToJson(object input)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string FormatToJson(object input)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = Int32.MaxValue;
            return jss.Serialize(input);
        }
        #endregion

        #region public static Dictionary<string, object> FormatToDictionary(string input)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Dictionary<string, object> FormatToDictionary(string input)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.MaxJsonLength = Int32.MaxValue;
            return jss.DeserializeObject(input) as Dictionary<string, object>;
        }
        #endregion
    }
}
