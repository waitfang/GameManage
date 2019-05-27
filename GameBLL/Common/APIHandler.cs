using GameDAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace GameBLL.Common
{
    public class APIHandler : IHttpAsyncHandler, IRequiresSessionState
    {
        #region Global Variables
        const string RESP_CALLBACK_SCRIPT = "<script type='text/javascript'>parent.{0}({1});</script>";
        #endregion

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private void ProcessRequestInternal(object stateObj)
        {
            HttpContext context = stateObj as HttpContext;
            HttpContext.Current = context;
            ProcessRequest(context);
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            AsyncOperation arObj = new AsyncOperation(context, cb, extraData);
            arObj.ProcessInternal = ProcessRequestInternal;
            arObj.StartAsyncWork();
            return arObj;
        }

        public void EndProcessRequest(IAsyncResult result)
        {

        }

        public void ProcessRequest(HttpContext context)
        {
            #region 完善IIS日志

            string postData = string.Empty;
            using (StreamReader reader = new StreamReader(context.Request.InputStream))
            {
                if (reader.BaseStream.Length > 0)
                {
                    postData = reader.ReadToEnd();
                }
            }
            if (postData.Length > 0)
            {
                int QueryStringLength = 0;
                if (0 < HttpContext.Current.Request.QueryString.Count)
                {
                    QueryStringLength = HttpContext.Current.Request.ServerVariables["QUERY_STRING"].Length;
                    HttpContext.Current.Response.AppendToLog("&");
                }

                if (4100 > (QueryStringLength + postData.Length))
                {
                    HttpContext.Current.Response.AppendToLog(postData);
                }
                else
                {
                    // append only the first 4090 the limit is a total of 4100 char.
                    HttpContext.Current.Response.AppendToLog(postData.Substring(0, (4090 - QueryStringLength)));
                    // indicate buffer exceeded
                    HttpContext.Current.Response.AppendToLog(".........");
                    // TODO: if s.Length >; 4000 then log to separate file

                    MethodUtility.LogByThread<string>(HttpContext.Current.Request.Url + HttpContext.Current.Request.ServerVariables["QUERY_STRING"] + postData, null, 0L, 3);
                }
            }

            #endregion

            string[] segments = context.Request.Url.Segments;
            if (segments.Length <= 3)
            {
                return;
            }

            int pathOffSet = 0;
            if (segments[1].ToLower() == "interface/" || segments[1].ToLower() == "interface2/")
            {
                pathOffSet = 3;
            }
            else
            {
                pathOffSet = 2;
            }
            if (segments[1] == "Web.Client/")
            {
                pathOffSet++;
            }

            //../Interface/[1,zh-cn,en-us,zh-tw]/account/pm_unread_count.json
            string clientCulture = segments[pathOffSet - 1].TrimEnd('/');
            string className = segments[pathOffSet].TrimEnd('/');
            string methodName = segments[pathOffSet + 1].TrimEnd('/');

            object executeObj = APIUtility.CreateSGInstance(className);

            if (executeObj == null) return;
            string respText = APIUtility.InvokeMethod(executeObj, methodName, null).ToString();

            if (respText.Length != 0)
            {
                if (context.Request["callback"] == null)
                {
                    ResponseText(respText);
                }
                else
                {
                    ResponseText(string.Format(RESP_CALLBACK_SCRIPT, context.Request["callback"], respText));
                }
            }
        }

        private void ResponseText(string respData)
        {
            lock (this)
            {
                HttpContext.Current.Response.Write(respData);
            }
        }

        #region Async Areas

        public class AsyncOperation : IAsyncResult
        {
            HttpContext _context; //保存context的引用 
            AsyncCallback _cb;//保存回调委托的引用 
            object _state;//保存额外的信息 
            bool _iscomplate;//保存异步操作是否完成

            public WaitCallback ProcessInternal
            {
                get;
                set;
            }

            /// <summary> 
            /// 构造函数，将AsyncHttpHandler的参数全部传递进来 
            /// </summary> 
            /// <param name="context"></param> 
            /// <param name="cb"></param> //该回调不可被重写，否则将会出现客户端永久等待的状态 
            /// <param name="state"></param> //构造时该值可以传递任意自己需要的数据 
            public AsyncOperation(HttpContext context, AsyncCallback cb, object state)
            {
                _context = context;
                _cb = cb;
                _state = state;
                _iscomplate = false; //表明当前异步操作未完成 
            }

            public void SetComplete()
            {
                _iscomplate = true;
            }

            /// <summary> 
            /// 实现获得当前异步处理的状态 
            /// </summary> 
            bool IAsyncResult.IsCompleted
            {
                get
                {
                    return _iscomplate;
                }
            }

            /// <summary> 
            /// 返回 false 即可 
            /// </summary> 
            bool IAsyncResult.CompletedSynchronously
            {
                get
                {
                    return false;
                }
            }

            /// <summary> 
            /// 将返回额外的信息 
            /// </summary> 
            object IAsyncResult.AsyncState
            {
                get
                {
                    return _state;
                }
            }

            /// <summary> 
            /// 为空 
            /// </summary> 
            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get
                {
                    return null;
                }
            }
            /// <summary> 
            /// 表明开始异步处理的主函数（方法名可以改，但上面的调用也需要一起改） 
            /// </summary> 
            public void StartAsyncWork()
            {
                ThreadPool.QueueUserWorkItem(StartAsyncTask, null);
            }

            public void StartAsyncTask(Object workItemState)
            {
                if (ProcessInternal != null)
                {
                    ProcessInternal.Invoke(_context);
                }
                _iscomplate = true;
                _cb(this);
            }
        }

        #endregion
    }
}
