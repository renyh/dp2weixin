using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
//using dp2weixin.dp2RestfulApi;
using System.Web.Configuration;
using DigitalPlatform.IO;
using dp2weixin.Server;

namespace dp2weixin
{
    public class Global : System.Web.HttpApplication
    {
        public static dp2WeiXinServer GlobalWeiXinServer = null;

        protected void Application_Start(object sender, EventArgs e)
        {        
            // 从web config中取出url,weixin代理账号
            string strDp2Url = WebConfigurationManager.AppSettings["dp2Url"];
            string strDp2UserName = WebConfigurationManager.AppSettings["dp2UserName"];
            // todo 密码改为加密格式
            string strDp2Password = WebConfigurationManager.AppSettings["dp2Password"];

            // 错误日志目录
            string strDp2WeiXinLogDir = WebConfigurationManager.AppSettings["dp2WeiXinLogDir"];
            PathUtil.CreateDirIfNeed(strDp2WeiXinLogDir);	// 确保目录创建

            string strDp2WeiXinUrl = "http://dp2003.com/dp2weixin";
            
            // 创建一个全局的微信服务类
            GlobalWeiXinServer = new dp2WeiXinServer(strDp2Url,
                strDp2UserName,
                strDp2Password,
                strDp2WeiXinUrl,
                strDp2WeiXinLogDir);
        }

    }
}