using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UG.DAL;
using UG.Model;
using UG.BLL.Common;
using System.Threading;

namespace UG.BLL
{
    public class PlayerQueue
    {
        /// <summary>
        /// 新增数据拉去client 端的本地资料
        /// </summary>
        /// <param name="paras"></param>
        /// <returns></returns>
        public string Save(tb_playerqueue paras)
        {
            try
            {
                int ret = DALCore.GetInstance().PlayerQueue.Add(paras);
                return ret.ToString();
            }
            catch (Exception ex)
            {
                MethodUtility.LogByThread(ex);
                return ex.ToString();
            }
        }
        
        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public List<tb_playerqueue> QryList(string[] input)
        {
            List<tb_playerqueue> list = new List<tb_playerqueue>();

            try
            {
                Dictionary<string, object> paras = new Dictionary<string, object>();
                paras["p_state"] = input[0];
                list = DALCore.GetInstance().PlayerQueue.QryList(paras);
            }
            catch (Exception ex)
            {
                MethodUtility.LogByThread(ex);
            }

            return list;
        }

        /// <summary>
        /// 根据房间编号确认当前匹配到的队列结果，如果返回null表示仍然在匹配队列中
        /// </summary>
        /// <param name="input">
        /// 0 房间编号
        /// </param>
        /// <returns></returns>
        public List<tb_playerqueue> GetMathcingResult(string[] input)
        {
            return null;
        }

        /// <summary>
        /// 执行游戏排队人员的匹配逻辑
        /// </summary>
        public void GameMatching()
        {
            //匹配提交时间最近的两个团队，需要匹配一下几个类型
            //1两队人数（平台注册的人数）必须对等
            //2选择的匹配类型模式一致，如3v3，4v4，5v5
            //3所在游戏大区一致
            //4相同金额
            while (true)
            {
                //查询所有匹配队列中的匹配
                //排序一下几个关键字段
                //比赛大区，比赛类型1v1，2v2等，金额，只有相同条件的这些信息才允许配对


                Thread.Sleep(2000);
            }
        }
    }
}
