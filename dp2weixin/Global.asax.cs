﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using dp2weixin.dp2RestfulApi;
using System.Web.Configuration;
using DigitalPlatform.IO;

namespace dp2weixin
{
    public class Global : System.Web.HttpApplication
    {
        // dp2服务器地址与代理账号
        public static string dp2Url = "";//"http://dp2003.com/dp2library/rest/"; //"http://localhost:8001/dp2library/rest/";//
        public static string dp2UserName = "";//"weixin";
        public static string dp2Password = "";//"111111";

        // dp2通道池
        public static LibraryChannelPool ChannelPool = null;

        // 错误日志目录
        public static string dp2WeiXinLogDir = "";

        protected void Application_Start(object sender, EventArgs e)
        {
            // 从web config中取出url,weixin代理账号
            Global.dp2Url = WebConfigurationManager.AppSettings["dp2Url"];
            Global.dp2UserName = WebConfigurationManager.AppSettings["dp2UserName"];
            // todo 密码改为加密格式
            Global.dp2Password = WebConfigurationManager.AppSettings["dp2Password"];

            // 错误日志目录
            Global.dp2WeiXinLogDir = WebConfigurationManager.AppSettings["dp2WeiXinLogDir"];         
            PathUtil.CreateDirIfNeed(Global.dp2WeiXinLogDir);	// 确保目录创建
            
            // 通道池对象
            ChannelPool = new LibraryChannelPool();
            ChannelPool.BeforeLogin -= new BeforeLoginEventHandle(Channel_BeforeLogin);
            ChannelPool.BeforeLogin += new BeforeLoginEventHandle(Channel_BeforeLogin);            
        }

        /// <summary>
        /// 自动登录，提供密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Channel_BeforeLogin(object sender, BeforeLoginEventArgs e)
        {
            if (e.FirstTry == false)
            {
                e.Cancel = true;
                return;
            }

            // 我这里赋上通道自己的账号，而不是使用全局变量。
            // 因为从池中征用通道后，都给通道设了密码。账号密码是通道的属性。
            LibraryChannel channel = sender as LibraryChannel;
            e.LibraryServerUrl = channel.Url;
            e.UserName = channel.UserName;
            e.Password = channel.Password;            
        }
    }
}