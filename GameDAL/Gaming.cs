using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameIDAL;

namespace GameDAL
{
    public class Gaming : IGaming
    {

        public List<UserInfo> UserLogin(UserInfo ParmUserInfo)
        {
            List<UserInfo> ListUserInfo = new List<UserInfo>();
            Dictionary<string, object> paras = new Dictionary<string, object>();
            paras["USERID"] = ParmUserInfo.USERID;
            ListUserInfo = DapperHelper.Query<UserInfo>(paras, "SP_GetUserInfo");
            return ListUserInfo;
        }
          
    }
}
