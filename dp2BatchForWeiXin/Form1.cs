using DigitalPlatform.IO;
using dp2Command.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2BatchForWeiXin
{
    public partial class Form1 : Form
    {
        //public dp2CommandServer _cmdServer = null;
        // 命令集合
        public CommandContainer CmdContiner = null;

        public Form1()
        {
            InitializeComponent();

            // 从config中取出url,weixin代理账号
            string strDp2Url = "http://dp2003.com/dp2library/rest/";//http://localhost/dp2library/xe/rest";//"
            string strDp2UserName = "weixin";// "supervisor";//
            // todo 密码改为加密格式
            string strDp2Password = "111111";// "";//

            // 错误日志目录
            string strLogDir = "C:\\dp2BatchForWeiXin_log";
            PathUtil.CreateDirIfNeed(strLogDir);	// 确保目录创建

            string strDp2WeiXinUrl = "http://dp2003.com/dp2weixin";

            // 将命令服务类改为单一实例方式 20151215
            dp2CommandServer.Instance.Init(strDp2Url,
                strDp2UserName,
                strDp2Password,
                strDp2WeiXinUrl,
                strLogDir);

            //命令集合
            this.CmdContiner = new CommandContainer();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SearchCommand searchCmd = (SearchCommand)this.CmdContiner.GetCommand(dp2CommandUtility.C_Command_Search);
            string strFirstPage = "";
            string strError = "";
            long nRet = dp2CommandServer.Instance.SearchBiblioByPublishtime("1900-04-01 00:00:00Z~2015-10-30 00:00:00Z",//"1900-04-01 00:00:00Z~2015-10-30 00:00:00Z",//2003-04-01 00:00:00Z~2015-12-30 00:00:00Z", 
                
                searchCmd,
                out strFirstPage,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, "出错：" + strError);
                return;
            }

            if (nRet == 0)
            {
                MessageBox.Show(this, "未命中"); 
                return;
            }
            MessageBox.Show(strFirstPage);
        }
    }
}
