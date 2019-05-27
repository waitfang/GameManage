using GameDAL;
using GameIDAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 

namespace GameBLL.Common
{
    public class DALCore
    {
        private static DALCore singleInstance;

        private DALCore()
        {

        } 
 
        #region  IGaming gaming

        /// <summary>
        /// 
        /// </summary>
        private static IGaming gaming;
        /// <summary>
        /// 
        /// </summary>
        public IGaming Gaming
        {
            get
            {
                if (gaming == null)
                {
                    gaming = LoadAssamblyType<IGaming>("GameDAL.Gaming");
                }

                return gaming;
            }
        }
        #endregion

    

        public static DALCore GetInstance()
        {
            if (singleInstance == null)
            {
                singleInstance = new DALCore();
            }

            return singleInstance;
        }

        public static T LoadAssamblyType<T>(string fullType) where T : class
        {
            string assemblyName = GetDLLByTypeName(fullType);
            System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(assemblyName);
            if (assembly != null)
            {
                Type loadType = assembly.GetType(fullType);

                if (loadType != null)
                {
                    try
                    {
                        return Activator.CreateInstance(loadType) as T;
                    }
                    catch (Exception ex)
                    {
                        MethodUtility.LogByThread(ex);
                    }
                }
            }

            return default(T);
        }

        private static string GetDLLByTypeName(string typeName)
        {
            string dllFileName = "";

            if (typeName.IndexOf(".GameBLL.") != -1)
            {
                dllFileName = "GameBLL";
            }
            else if (typeName.IndexOf("GameDAL") != -1)
            {
                dllFileName = "GameDAL";
            }

            return dllFileName;
        }
    }
}
