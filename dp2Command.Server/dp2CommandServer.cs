using dp2Command.Server.dp2RestfulApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dp2Command.Server
{
    public class dp2CommandServer
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

        // 命令集合
        CommandContainer CmdContiner = null;

        // 当前命令
        public string CurrentCmdName = null;

        public dp2CommandServer(string strDp2Url,
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

            //命令集合
            this.CmdContiner = new CommandContainer();
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


        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="strWord"></param>
        /// <returns></returns>
        public long SearchBiblio(string strWord,
            out string strFirstPage,
            out string strError)
        {
            strFirstPage = "";
            strError = "";

            // 判断检索词
            strWord = strWord.Trim();
            if (String.IsNullOrEmpty(strWord))
            {
                strError = "检索词不能为空。";
                return -1;
            }

            long lTotoalCount = 0;
            // 从池中征用通道
            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                // -1失败
                // 0 未命令
                long lRet = channel.SearchBiblio(strWord,
                    out strError);
                if (lRet == -1 || lRet == 0)
                {
                    return lRet;
                }

                // 取出命中记录列表
                lTotoalCount = lRet;

                List<string> totalResultList = new List<string>();
                long lStart = 0;
                // 当前总共取的多少记录
                long lCurTotalCount = 0;

            REDO:
                List<string> resultPathList = null;
                long lCount = -1;
                lRet = channel.GetBiblioSearchResult(lStart,
                    lCount,
                     out resultPathList,
                     out strError);
                if (lRet == -1)
                    return -1;

                // 加到结果集中
                totalResultList.AddRange(resultPathList);

                // 检查记录是否获取完成，没取完继续取
                lCurTotalCount += lRet;
                if (lCurTotalCount < lTotoalCount)
                {
                    lStart = lCurTotalCount;
                    goto REDO;
                }


                // 检查一下，取出来的记录数，是否与返回的命中数量一致
                if (lTotoalCount != totalResultList.Count)
                {
                    strError = "内部错误，不可能结果集数量不一致";
                    return -1;
                }

                // 将检索结果信息保存到检索命令中
                SearchCommand searchCmd = (SearchCommand)this.CmdContiner.GetCommand(dp2CommandUtility.C_Command_Search);
                searchCmd.BiblioResultPathList = totalResultList;
                searchCmd.ResultNextStart = 0;

                // 获得第一页检索结果
                bool bRet = searchCmd.GetNextPage(out strFirstPage, out strError);
                if (bRet == false)
                {
                    return -1;
                }
            }
            finally
            {
                // 归还通道到池
                this.ChannelPool.ReturnChannel(channel);
            }

            return lTotoalCount;
        }

        /// <summary>
        /// 得到下页检索结果
        /// </summary>
        /// <returns></returns>
        public string GetNextPageForSearch()
        {
            string strResult = "";

            SearchCommand searchCmd = (SearchCommand)this.CmdContiner.GetCommand(dp2CommandUtility.C_Command_Search);
            if (searchCmd.IsCanNextPage == false)
            {
                strResult = "已到末页。";
                return strResult;
            }
            // 获得下页检索结果
            string strError = "";
            bool bRet = searchCmd.GetNextPage(out strResult, out strError);
            if (bRet == false)
            {
                return strError;
            }

            return strResult;
        }

        /// <summary>
        /// 根据书目序号得到详细的参考信息
        /// </summary>
        /// <param name="nIndex">书目序号，从1排序</param>
        /// <param name="strInfo"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int GetDetailBiblioInfo(int  nIndex,
            out string strBiblioInfo,
            out string strError)
        {
            strBiblioInfo = "";
            strError = "";

            SearchCommand searchCmd = (SearchCommand)this.CmdContiner.GetCommand(dp2CommandUtility.C_Command_Search);
            //检查有无超过数组界面
            if (nIndex <= 0 || searchCmd.BiblioResultPathList.Count < nIndex)
            {
                strError = "您输入的书目序号[" + nIndex.ToString() + "]越出范围。";
                return -1;
            }

            // 获取路径，注意要截取
            string strPath = searchCmd.BiblioResultPathList[nIndex - 1];
            int index = strPath.IndexOf("*");
            if (index > 0)
                strPath = strPath.Substring(0, index);
            LibraryChannel channel = this.ChannelPool.GetChannel(this.dp2Url, this.dp2UserName);
            channel.Password = this.dp2Password;
            try
            {
                long lRet1 = channel.GetBiblioDetail(strPath,
                    out strBiblioInfo,
                    out strError);
                if (lRet1 == -1)
                {
                    strError = "获取详细信息失败：" + strError;
                    return -1;
                }               
            }
            finally
            {
                this.ChannelPool.ReturnChannel(channel);
            }
            return 0;
        }
    }
}
