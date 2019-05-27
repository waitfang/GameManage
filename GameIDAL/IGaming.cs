using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameIDAL
{
    public interface IGaming
    { 
        List<UserInfo> UserLogin(UserInfo ParmUserInfo); 
    }
}
