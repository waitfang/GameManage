using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace GameModel
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
                JavaScriptSerializer jss = new JavaScriptSerializer();
                jss.MaxJsonLength = Int32.MaxValue;
                return jss.Serialize(this);
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
