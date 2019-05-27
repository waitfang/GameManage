using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBLL.Json
{
    public class sgresult_base : FormatableObject
    {
        private bool _result;
        private List<errorInfo> _errorInfo = new List<errorInfo>();

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

        public List<errorInfo> errorInfo
        {
            get
            {
                return _errorInfo;
            }
        }
    }
}
