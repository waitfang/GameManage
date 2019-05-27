
using Dapper;
using System.Linq;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using static Dapper.SqlMapper;

namespace GameDAL
{
    public class DapperHelper
    {
        public static string WriteConnection = ConfigurationManager.ConnectionStrings["ConnString"] == null ? "" : ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

        public static string ReadConnection
        {
            get
            {
                return MethodUtility.GetClusterConfigCache(2);
            }
        }

        public static string ReportConnection
        {
            get
            {
                return MethodUtility.GetClusterConfigCache(3);
            }
        }

        /// <summary>
        /// 创建只读查询数据库连接
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetReadConnection()
        {
            SqlConnection conn = new SqlConnection(ReadConnection);
            return conn;
        }

        /// <summary>
        /// 创建只读报表数据库连接
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetReportConnection()
        {
            SqlConnection conn = new SqlConnection(ReportConnection);
            return conn;
        }

        /// <summary>
        /// 创建写库数据库连接
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetWriteConnection()
        {
            SqlConnection conn = new SqlConnection(WriteConnection);
            return conn;
        }

        /// <summary>
        /// 创建指定数据库连接
        /// </summary>
        /// <returns></returns>
        public static SqlConnection GetConnection(string connection)
        {
            SqlConnection conn = new SqlConnection(connection);
            return conn;
        }

        private static int CommandTimeout
        {
            get
            {
                int timeoutSecond = 0;

                if (int.TryParse(ConfigurationManager.AppSettings["CommandTimeout"], out timeoutSecond))
                {
                    return timeoutSecond;
                }

                return 30;
            }
        }


        #region 新增修改删除操作
        
        /// <summary>
        /// 新增操作方法
        /// </summary>
        /// <typeparam name="T">传入的T泛型</typeparam>
        /// <param name="paras">参数依泛型类型</param>
        /// <param name="storeprocedure">sp名称</param>
        /// <returns></returns>
        public static int Insert<T>(T paras, string storeprocedure)
        {
            return Execute(paras, storeprocedure);
        }

        /// <summary>
        /// 修改操作方法
        /// </summary>
        /// <typeparam name="T">传入的T泛型</typeparam>
        /// <param name="paras">参数依泛型类型</param>
        /// <param name="storeprocedure">sp名称</param>
        /// <returns></returns>
        public static int Update<T>(T paras, string storeprocedure)
        {
            return Execute(paras, storeprocedure);
        }

        /// <summary>
        /// 删除操作方法
        /// </summary>
        /// <typeparam name="T">传入的T泛型</typeparam>
        /// <param name="paras">参数依泛型类型</param>
        /// <param name="storeprocedure">sp名称</param>
        /// <returns></returns>
        public static int Delete<T>(T paras, string storeprocedure)
        {
            return Execute(paras, storeprocedure);
        }
        
        /// <summary>
        /// Execute操作方法
        /// </summary>
        /// <typeparam name="T">传入的T泛型</typeparam>
        /// <param name="paras">参数依泛型类型</param>
        /// <param name="storeprocedure">sp名称</param>
        /// <returns></returns>
        public static int Execute<T>(T paras, string storeprocedure)
        {
            return MethodUtility.InvokeMethodWithDB(delegate ()
            {
                using (IDbConnection conn = GetWriteConnection())
                {
                    return conn.Execute(storeprocedure, paras, null, null, CommandType.StoredProcedure);
                }

            }, storeprocedure, paras, true);
        }

        /// <summary>
        /// Execute操作方法-依传入链接
        /// </summary>
        /// <typeparam name="T">传入的T泛型</typeparam>
        /// <param name="paras">参数依泛型类型</param>
        /// <param name="storeprocedure">sp名称</param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static int Execute<T>(T paras, string storeprocedure, IDbConnection conn)
        {
            return MethodUtility.InvokeMethodWithDB(delegate ()
            {
                return conn.Execute(storeprocedure, paras, null, null, CommandType.StoredProcedure);

            }, storeprocedure, paras, true);
        }

        /// <summary>
        /// 事务执行方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <returns></returns>

        public static int Execute<T>(T paras, string storeprocedure, IDbConnection conn, IDbTransaction trans)
        {
            return MethodUtility.InvokeMethodWithDB(delegate ()
            {
                return conn.Execute(storeprocedure, paras, trans, null, CommandType.StoredProcedure);

            }, storeprocedure, paras, true);
        }

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// 执行新增修改、删除，并返回查询结果(主DB)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static object ExecuteScalar<T>(T paras, string storeprocedure)
        {
            return MethodUtility.InvokeMethodWithDB(delegate ()
            {
                using (IDbConnection conn = GetConnection(WriteConnection))
                {
                    return conn.ExecuteScalar(storeprocedure, paras, null, CommandTimeout, CommandType.StoredProcedure);
                }

            }, storeprocedure, paras);
        }

        /// <summary>
        /// 根据传入的链接字符串执行新增修改、删除，并返回查询结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static object ExecuteScalar<T>(T paras, string storeprocedure, IDbConnection conn)
        {
            return MethodUtility.InvokeMethodWithDB(delegate ()
            {
                return conn.ExecuteScalar(storeprocedure, paras, null, CommandTimeout, CommandType.StoredProcedure);

            }, storeprocedure, paras);
        }

        public static object ExecuteScalar<T>(T paras, string storeprocedure, IDbConnection conn, IDbTransaction trans)
        {
            return MethodUtility.InvokeMethodWithDB(delegate ()
            {
                return conn.ExecuteScalar(storeprocedure, paras, trans, CommandTimeout, CommandType.StoredProcedure);

            }, storeprocedure, paras);
        }

        /// <summary>
        /// 主db查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static T ExecuteScalar<T>(Dictionary<string, object> paras, string storeprocedure)
        {
            return ExecuteScalar<T>(paras, storeprocedure, WriteConnection);
        }

        /// <summary>
        /// 只读查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static T ExecuteScalarRead<T>(Dictionary<string, object> paras, string storeprocedure)
        {
            return ExecuteScalar<T>(paras, storeprocedure, ReadConnection);
        }

        /// <summary>
        /// 报表查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static T ExecuteScalarReport<T>(Dictionary<string, object> paras, string storeprocedure)
        {
            return ExecuteScalar<T>(paras, storeprocedure, ReportConnection);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static T ExecuteScalar<T>(Dictionary<string, object> paras, string storeprocedure, string connection)
        {
            return MethodUtility.InvokeMethodWithDB(delegate ()
            {
                using (IDbConnection conn = GetConnection(connection))
                {
                    return conn.ExecuteScalar<T>(storeprocedure, paras, null, CommandTimeout, CommandType.StoredProcedure);
                }

            }, storeprocedure, paras);
        }

        #endregion

        #region Query

        /// <summary>
        /// 主DB查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static List<T> Query<T>(Dictionary<string, object> paras, string storeprocedure)
        {
            return Query<T>(paras, storeprocedure, WriteConnection);
        }

        /// <summary>
        /// 只读查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static List<T> QueryRead<T>(Dictionary<string, object> paras, string storeprocedure)
        {
            return Query<T>(paras, storeprocedure, ReadConnection);
        }

        /// <summary>
        /// 报表查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static List<T> QueryReport<T>(Dictionary<string, object> paras, string storeprocedure)
        {
            return Query<T>(paras, storeprocedure, ReportConnection);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static List<T> Query<T>(Dictionary<string, object> paras, string storeprocedure, string connection)
        {
            return MethodUtility.InvokeMethodWithDB(delegate ()
            {
                using (IDbConnection conn = GetConnection(connection))
                {
                    return conn.Query<T>(storeprocedure, paras, null, true, CommandTimeout, CommandType.StoredProcedure).ToList();
                }

            }, storeprocedure, paras);
        }

        #endregion

        #region 多结果查询

        /// <summary>
        /// 主DB查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static GridReader QueryMultiple(Dictionary<string, object> paras, string storeprocedure)
        {
            return QueryMultiple(paras, storeprocedure, WriteConnection);
        }

        /// <summary>
        /// 只读查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static GridReader QueryMultipleRead(Dictionary<string, object> paras, string storeprocedure)
        {
            return QueryMultiple(paras, storeprocedure, ReadConnection);
        }

        /// <summary>
        /// 报表查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <returns></returns>
        public static GridReader QueryMultipleReport(Dictionary<string, object> paras, string storeprocedure)
        {
            return QueryMultiple(paras, storeprocedure, ReportConnection);
        }

        
        /// <summary>
        /// 多结果查询
        /// </summary>
        /// <param name="paras"></param>
        /// <param name="storeprocedure"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static GridReader QueryMultiple(Dictionary<string, object> paras, string storeprocedure, string connection)
        {
            return MethodUtility.InvokeMethodWithDB(delegate ()
            {
                using (IDbConnection conn = GetConnection(connection))
                {
                    return conn.QueryMultiple(storeprocedure, paras, null, CommandTimeout, CommandType.StoredProcedure);
                }

            }, storeprocedure, paras);
        }

        #endregion
    }




}
