using dp2weixin.dp2RestfulApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2weixin.Server
{
    public class dp2WeiXinServer
    {
        // dp2服务器地址与代理账号
        public string dp2Url = "";//"http://dp2003.com/dp2library/rest/"; //"http://localhost:8001/dp2library/rest/";//
        public string dp2UserName = "";//"weixin";
        public string dp2Password = "";//"111111";

        // dp2weixin url
        public string dp2WeiXinUrl = "http://dp2003.com/dp2weixin";
        public string dp2WeiXinLogDir = "";

        // dp2通道池
        public LibraryChannelPool ChannelPool = null;

        public dp2WeiXinServer(string strDp2Url,
            string strDp2UserName,
            string strDp2Password,
            string strDp2WeiXinUrl,
            string strDp2WeiXinLogDir)
        {
            this.dp2Url = strDp2Url;
            this.dp2UserName = strDp2UserName;
            this.dp2Password = strDp2Password;
            this.dp2WeiXinUrl = strDp2WeiXinUrl;
            this.dp2WeiXinLogDir = strDp2WeiXinLogDir;

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
