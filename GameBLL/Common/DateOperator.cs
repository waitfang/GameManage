using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBLL.Common
{
/// <summary>
/// 日期操作类
/// </summary>
   public class DateOperator
    {
        #region 日期分割相关函数

        /// <summary>
        /// 按，天、周、月 分割时间的公用方法
        /// </summary>
        /// <param name="strsDate">开始时间</param>
        /// <param name="streDate">结束时间</param>
        /// <param name="splitType">分割类型1,按天，2按周，3按月，0其他</param>
        /// <param name="tips">错误提示</param>
        /// <returns></returns>
        public static Dictionary<string, string> SpiltDate(string strsDate, string streDate, string splitType, ref string tips)
        {
            //验证通过，则根据类型来分割日期区间
            DateTime sDate = Convert.ToDateTime(strsDate); //起始日期
            DateTime eDate = Convert.ToDateTime(streDate); //结束日期
            Dictionary<string, string> dic_date = new Dictionary<string, string>();//日期分割字典
            int index = 0;

            switch (splitType)
            {
                case "1": //日期按天分割

                    #region 日期按天分割

                    while (true)
                    {
                        if (sDate.AddDays(index) > eDate)
                        {
                            break;
                        }

                        dic_date.Add("Day_" + index, sDate.AddDays(index).ToShortDateString() + "|" + sDate.AddDays(index).ToShortDateString());
                        index++;

                        if (index > 100) { break; }
                    }

                    if (dic_date.Count == 0)
                    {
                        //表示日期不合格
                        tips = "开始结束日期区间跨度至少1天！";
                    }

                    #endregion

                    break;

                case "2": //日期按周分割

                    #region 日期按周分割

                    //获取今天星期几
                    int sWeek = (int)sDate.DayOfWeek;
                    DateTime wDate = DateTime.MinValue;
                    //如果不是星期1则找到下一个星期1
                    while (true)
                    {
                        if (sWeek != 1)
                        {
                            sWeek = (int)sDate.AddDays(index).DayOfWeek;
                            index++;
                        }
                        else
                        {
                            wDate = sDate.AddDays(index > 0 ? index - 1 : 0); //日期内的周一
                            //符合条件的周加入字典
                            if (wDate < eDate && wDate.AddDays(6) <= eDate)
                            {
                                dic_date.Add("Week_" + index, wDate.ToShortDateString() + "|" + wDate.AddDays(6).ToShortDateString());
                            }
                            else
                            {
                                break;
                            }

                            sWeek = 0;
                            index += 6;
                        }

                        if (index > 600) { break; }
                    }

                    if (dic_date.Count == 0)
                    {
                        //周一大于等于结束日期，表示日期不合格
                        tips = "请选择周一至周五的时间区间！";
                    }

                    #endregion

                    break;
                case "3": //日期按月分割

                    #region 日期按月分割

                    //当前月的第一天的日期
                    DateTime firstDate = new System.DateTime(sDate.Year, sDate.Month, 1, 0, 0, 0, 0);
                    int fWeek = (int)firstDate.DayOfWeek;
                    DateTime mDate = DateTime.MinValue; //第一个周一的日期
                    DateTime nmDate = DateTime.MinValue;//下月周1前一天的日期
                    while (true)
                    {
                        if (fWeek != 1)
                        {
                            fWeek = (int)firstDate.AddDays(index).DayOfWeek;
                            index++;

                            if (index > 100) { break; }
                        }
                        else
                        {
                            mDate = firstDate.AddDays(index > 0 ? index - 1 : 0); //当前月的第一个周一

                            //获取下个月的第一个周一的前一天
                            DateTime nextDate = firstDate.AddMonths(1);
                            int mIndex = 0;
                            int nWeek = (int)nextDate.DayOfWeek;
                            while (true)
                            {
                                if (nWeek != 1)
                                {
                                    nWeek = (int)nextDate.AddDays(mIndex).DayOfWeek;
                                    mIndex++;
                                }
                                else
                                {
                                    nmDate = nextDate.AddDays(mIndex == 0 ? -1 : mIndex - 2);//取前一天
                                    break;
                                }

                                if (mIndex > 100) { break; }
                            }

                            if (mDate < eDate && nmDate <= eDate)
                            {
                                dic_date.Add("Month_" + mDate.Year + "_" + mDate.Month + "_" + index, mDate.ToShortDateString() + "|" + nmDate.ToShortDateString());
                                fWeek = 0;
                                index = 0;
                                firstDate = firstDate.AddMonths(1);
                            }
                            else
                            {
                                break;
                            }
                        }

                    }

                    if (dic_date.Count == 0)
                    {
                        //表示日期不合格
                        tips = "日期区间应为本月第一个周一开始且结束跨度至少一个月！";
                    }

                    #endregion

                    break;
                default:
                    dic_date.Add("Default", sDate.ToShortDateString() + "|" + eDate.ToShortDateString());
                    break;
            }

            if (dic_date.Count == 0 && tips.Length == 0)
            {
                tips = "日期区间不正确！";
            }

            return dic_date;
        }

        /// <summary>
        /// 用于切割报表查询的时间，起始时间和结束时间如果带有时分秒则切开，中间日期部分查询压缩数据.
        /// 数据返回格式List Item = "1&sdt|edt&sdt|edt", "2&sdt|edt&sdt|edt" 1表示查询压缩数据，2表示查询api [WG，DQ，XQ，MG没有时分秒查询需要独立的日期格式] 
        /// </summary>
        /// <returns></returns>
        public static List<string> SplitReportDate(string sdt, string edt)
        {
            List<string> datelist = new List<string>();
            if (sdt.Length == 8)//20190102
            {
                sdt = sdt.Insert(4,"/").Insert(7,"/");
            }
            if (edt.Length == 8) {
                edt=edt.Insert(4, "/").Insert(7, "/");
            }
            DateTime sdate = Convert.ToDateTime(sdt);
            DateTime edate = Convert.ToDateTime(edt);
            DateTime tmpdate = DateTime.Now.Date.AddDays(-1);
            if (DateTime.Now.Hour < 16)
            {
                tmpdate = tmpdate.AddDays(-1);
            }

            //开始时间为当天则直接查询api
            if (sdate.Date >= tmpdate)
            {
                datelist.Add("2&" + sdt + "|" + edt + "&" + sdt + "|" + edt);
            }
            else if (sdate == sdate.Date && edate == edate.Date)
            {
                #region 传入的是日期情况分割

                if (edate > tmpdate)
                {
                    datelist.Add("2&" + tmpdate + "|" + edate + "&" + tmpdate + "|" + edate);//13-14
                    datelist.Add("1&" + sdt + "|" + tmpdate.AddDays(-1) + "&" + sdt + "|" + tmpdate.AddDays(-1)); //2-12
                }
                else
                {
                    datelist.Add("1&" + sdt + "|" + edt + "&" + sdt + "|" + edt);
                }

                #endregion
            }
            else
            {
                #region 传入的日期带有时间情况分割

                if (sdate.Date == edate.Date || sdate.Date == edate.Date.AddDays(-1))
                {
                    //判断起始日期和结束日期在同一天或者相差一天直接查询api
                    datelist.Add("2&" + sdate + "|" + edate + "&" + sdate + "|" + edate);
                }
                else
                {
                    //判断起始日期 [WG,MG,DQ,XQ产品的起始日期没有时分秒，所以不用切割开始日期]
                    DateTime tmpsdt = sdate;
                    if (sdate != sdate.Date)
                    {
                        if (sdate.Hour < 12 || (sdate.Hour == 12 && sdate.Minute == 0 && sdate.Second == 0))
                        {
                            tmpsdt = sdate.Date.AddHours(12);
                        }
                        else
                        {
                            tmpsdt = sdate.AddDays(1).Date.AddHours(12);
                        }

                        datelist.Add("2&" + sdate + "|" + tmpsdt + "&");//[WG,MG,DQ,XQ]日期给空
                    }

                    //判断结束日期
                    DateTime tmpedt = edate;
                    DateTime otedt = edate; //wg,mg,dq,xq时间
                    if (tmpedt != edate.Date)
                    {
                        if (edate >= tmpdate)
                        {
                            tmpedt = tmpdate.AddHours(12);
                            otedt = tmpdate.AddHours(12);
                        }
                        else if (edate.Hour < 12 || (edate.Hour == 12 && edate.Minute == 0 && edate.Second == 0))
                        {
                            tmpedt = edate.Date.AddDays(-1).AddHours(12);
                        }
                        else
                        {
                            tmpedt = edate.Date.AddHours(12);
                        }

                        if (otedt == edate)
                        {
                            datelist.Add("2&" + tmpedt + "|" + edate + "&");
                        }
                        else
                        {
                            datelist.Add("2&" + tmpedt + "|" + edate + "&" + otedt + "|" + edate);
                        }
                    }
                    else
                    {
                        if (edate > tmpdate)
                        {
                            tmpedt = tmpdate.AddHours(12);
                            otedt = tmpdate.AddHours(12);
                            datelist.Add("2&" + tmpedt + "|" + edate + "&" + otedt + "|" + edate);
                        }
                    }
                    if (otedt == edate)
                    {
                        datelist.Add("1&" + tmpsdt.Date + "|" + tmpedt.AddDays(-1).Date + "&" + sdate.Date + "|" + otedt.Date);
                    }
                    else
                    {
                        datelist.Add("1&" + tmpsdt.Date + "|" + tmpedt.AddDays(-1).Date + "&" + sdate.Date + "|" + otedt.AddDays(-1).Date);
                    }
                }

                #endregion
            }

            return datelist;
        }

        /// <summary>
        /// 根据传入的时间区间按天分割出来
        /// </summary>
        /// <param name="sdate"></param>
        /// <param name="edate"></param>
        /// <returns></returns>
        public static List<string> GetDayByDateRange(string sdt, string edt)
        {
            List<string> date_range = new List<string>();
            DateTime sdate = Convert.ToDateTime(sdt);
            DateTime edate = Convert.ToDateTime(edt);

            while (true)
            {
                if (sdate <= edate)
                {
                    date_range.Add(sdate.ToShortDateString());
                }
                else
                {
                    break;
                }

                //如果开始时间+1天还比结束日期小的话就加入一天的数据
                sdate = sdate.AddDays(1d);
            }

            return date_range;
        }

        /// <summary>
        /// 根据传入的日期返回一期的2个日期区间
        /// </summary>
        /// <param name="sdt"></param>
        /// <returns></returns>
        public static List<string> GetDateByMonth(string sdt)
        {
            DateTime tmpdate = Convert.ToDateTime(sdt);
            DateTime startDate = Convert.ToDateTime(tmpdate.ToString("yyyy-MM") + "-01");
            DateTime endDate = Convert.ToDateTime(tmpdate.ToString("yyyy-MM") + "-01").AddMonths(1);

            //得到是星期几
            int sDays = startDate.DayOfWeek.GetHashCode();
            int eDays = endDate.DayOfWeek.GetHashCode();
            int sday = 0, eday = 0;
            int sMonday = sDays != 0 ? sDays - 1 : 6; // 当前日期与本周一相差的天数
            int eMonday = eDays != 0 ? eDays - 1 : 6; // 当前日期与本周一相差的天数

            //表示当前日期就是本月的第一个星期一
            if (sMonday == 1)
            {
                sday = -sMonday;
            }
            else
            {
                sday = -sMonday + 7;
            }

            if (eDays == 1)
            {
                eday = -eMonday;
            }
            else
            {
                eday = -eMonday + 7;
            }

            startDate = startDate.AddDays(sday);
            endDate = endDate.AddDays(eday - 1);//这里改成下个月的第一个星期一前一天的时间

            //如果当前期的起始时间比传入的时间大则表示要查询的数据是上期的不是本期
            if (tmpdate < startDate)
            {
                return GetDateByMonth(tmpdate.AddMonths(-1).ToString("yyyy-MM") + "-28");
            }

            List<string> list = new List<string>();
            list.Add(startDate.ToShortDateString());
            list.Add(endDate.ToShortDateString());

            return list;
        }

        #endregion
    }
}
