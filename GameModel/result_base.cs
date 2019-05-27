using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace GameModel
{
    /// <summary>
    ///result_base 的摘要说明
    /// </summary>
    public class result_base : FormatableObject
    {
        private bool _result;
        private string _data = string.Empty;
        private string _errorCode = string.Empty;
        private string _errorMsg = string.Empty;
        private List<error_base> _errorInfo = new List<error_base>();

        /// <summary>
        /// 返回结果
        /// true:成功
        /// false:失败
        /// </summary>
        public bool result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
            }
        }

        /// <summary>
        /// 返回数据
        /// </summary>
        public string data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string errorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string errorMsg
        {
            get { return _errorMsg; }
            set { _errorMsg = value; }
        }

        /// <summary>
        /// 错误集合
        /// </summary>
        public List<error_base> errorInfo
        {
            get
            {
                return _errorInfo;
            }
        }        
    }
}
