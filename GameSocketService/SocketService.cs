 
using Newtonsoft.Json.Linq;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using GameSocketWeb.LocalServices;

namespace UG.SocketService
{
    partial class SocketService : ServiceBase
    {
        #region Global Variables

        private IBootstrap bootstrap = null;
        private GamingService gamingService = null;

        #endregion

        #region public SocketService()

        public SocketService()
        {
            InitializeComponent();
        }
        #endregion

        #region public void RunService()
        /// <summary>
        /// 
        /// </summary>
        public void RunService()
        { 

            bootstrap = BootstrapFactory.CreateBootstrap();

            bootstrap.Initialize();

            bootstrap.Start();

            gamingService = new GamingService();
        }
        #endregion


        #region protected override void OnStart(string[] args)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        { 
            // TODO: 在此处添加代码以启动服务。
            RunService(); 
        }
        #endregion

        #region protected override void OnStop()
        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            // TODO: 在此处添加代码以执行停止服务所需的关闭操作。
            if (bootstrap != null)
            {
                foreach (IWorkItem item in bootstrap.AppServers)
                {
                    item.Stop();
                }
            }

            if (gamingService != null)
            {
                gamingService.IsRuning = false;
            }
             
        }
        #endregion
         
       
    }

    public static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        { 
            SocketService ws = new SocketService();
            ws.RunService();

            ////Thread.Sleep(30000);
            //GamingService gamingService = new GamingService();
            //gamingService.IsRuning = true;
            //gamingService.ProcessMatchesQueue();

            while (true)
            {
                Thread.Sleep(3000);
            }
            //return;

            // 同一进程中可以运行多个用户服务。若要将
            // 另一个服务添加到此进程中，请更改下行以
            // 创建另一个服务对象。例如，
            ServiceBase[] ServicesToRun;

            ServicesToRun = new ServiceBase[] { new SocketService() };

            ServiceBase.Run(ServicesToRun);
        }
    }
}
