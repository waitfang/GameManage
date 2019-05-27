using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBLL.Common
{
    /// <summary>
    /// 数据验证的类型枚举
    /// </summary>
    public enum ValidateType
    {
        /// <summary>
        /// 用户名，只能输入由数字和26个英文字母组成的字符串
        /// </summary>
        UserName = 1,
        /// <summary>
        /// 密码， 长度在4~18之间，只能包含字符、数字和下划线
        /// </summary>
        Password = 2,
        /// <summary>
        /// 电话号码， 支持分机和固话，以及手机
        /// </summary>
        Tel = 3,
        /// <summary>
        /// 邮箱
        /// </summary>
        Email = 4,
        /// <summary>
        /// 身份证 
        /// </summary>
        IDCard = 5,
        /// <summary>
        /// 金额，数字或者小数2位，最大长度[int:10，bigint:20, smallint:5,tinyint:3]
        /// </summary>
        Number = 6,
        /// <summary>
        /// 正整数，最大长度[int:10，bigint:20, smallint:5,tinyint:3]
        /// </summary>
        Integer = 7,
        /// <summary>
        /// 短日期格式 yyyy-MM-dd
        /// </summary>
        Date = 8,
        /// <summary>
        /// 日期时间格式 yyyy-MM-dd HH:mm:ss
        /// </summary>
        DateTime = 9,
        /// <summary>
        /// bool值，true或者false
        /// </summary>
        Boolean = 10,
        /// <summary>
        /// 站台编号，只能是字母和数字的组合
        /// </summary>
        SiteNumber = 11,
        /// <summary>
        /// 交易码组合，只能是数字组合长度18位
        /// </summary>
        TransCode = 12,
        /// <summary>
        /// 银行卡号，只给50位以内的数字
        /// </summary>
        BankCard = 13,
        /// <summary>
        /// 会员注册帐号时，必须是字母和数字的组合
        /// </summary>
        RegisterAccountRole = 14,
        /// <summary>
        /// 手机号码验证规则，11位，以1开头
        /// </summary>
        TelPhone = 15,
        /// <summary>
        /// 中文名称或者英文名称
        /// </summary>
        RealName = 16,
        /// <summary>
        /// 台湾手机号码验证规则
        /// </summary>
        TelPhone_TW = 17,
        /// <summary>
        /// 会员注册帐号时，必须是字母和数字的组合
        /// </summary>
        KBGHRegisterAccountRole = 18,
        /// <summary>
        /// ApiKey,必须是字母和数字的组合，不能有中文和空格
        /// </summary>
        ApiKey = 19,
        /// <summary>
        /// 表示不需要验证正则
        /// </summary>
        None = 100

    }

    /// <summary>
    /// 日志枚举
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 错误日志
        /// </summary>
        Unhandled = 0,
        /// <summary>
        /// 操作流水账
        /// </summary>
        Journal = 1,
        /// <summary>
        /// Seeesion日志
        /// </summary>
        Session = 2
    }
     
    /// <summary>
    /// 数据库操作日志类
    /// </summary>
    public enum OperateType
    {
        /// <summary>
        /// 
        /// </summary>
        Merchant = 1,
        /// <summary>
        /// 
        /// </summary>
        Manager = 2,
        /// <summary>
        /// 
        /// </summary>
        Logintrace = 3,
        /// <summary>
        /// 
        /// </summary>
        MerchantBank = 4,
        /// <summary>
        /// 
        /// </summary>
        Merchantfee = 5,
        /// <summary>
        /// 
        /// </summary>
        Partner = 6,
        /// <summary>
        /// 
        /// </summary>
        Pwdeditlog = 7,
        /// <summary>
        /// 
        /// </summary>
        Deposit = 8,
        /// <summary>
        /// 
        /// </summary>
        Withdraw = 9,
        /// <summary>
        /// 
        /// </summary>
        Google = 10,
        /// <summary>
        /// 
        /// </summary>
        Up_Merchant =11,
        /// <summary>
        /// 
        /// </summary>
        Up_ReceiptAct = 12,
        /// <summary>
        /// 
        /// </summary>
        None  = 100
    }
     

}
