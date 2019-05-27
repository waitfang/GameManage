using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBLL.Json
{
    public class errorInfo : FormatableObject
    {
        private string _errorCode = string.Empty;
        private string _error;
        private string _errorDetail;

        public string errorCode
        {
            get
            {
                return _errorCode;
            }
            set
            {
                _errorCode = value;
            }
        }

        public string error
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
            }
        }

        public string errorDetail
        {
            get
            {
                return _errorDetail;
            }
            set
            {
                _errorDetail = value;
            }
        }
    }
}
