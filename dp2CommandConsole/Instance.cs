using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DigitalPlatform.Text;
using System.Net;
using DigitalPlatform.IO;
using dp2Command.Server;

namespace dp2ConsoleToWeiXin
{
    /// <summary>
    /// 一个实例
    /// </summary>
    public class Instance : IDisposable
    {
        public dp2CommandServer WeiXinServer = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Instance()
        {
            // 从config中取出url,weixin代理账号
            string strDp2Url = "http://dp2003.com/dp2library/rest/";
            string strDp2UserName = "weixin";
            // todo 密码改为加密格式
            string strDp2Password = "111111";

            // 错误日志目录
            string strDp2WeiXinLogDir = "C:\\dp2weixin_log";
            PathUtil.CreateDirIfNeed(strDp2WeiXinLogDir);	// 确保目录创建

            string strDp2WeiXinUrl = "http://dp2003.com/dp2weixin";

            // 创建一个全局的微信服务类
            this.WeiXinServer = new dp2CommandServer(strDp2Url,
                strDp2UserName,
                strDp2Password,
                strDp2WeiXinUrl,
                strDp2WeiXinLogDir);
        }


 
        // return:
        //      false   正常，继续
        //      true    退出命令
        public bool ProcessCommand(string line)
        {
            line = line.Trim().ToLower();

            if (line == "exit" || line == "quit")
                return true;

            string strError = "";
            long lRet = 0;


            // 用:号分隔命令与参数，例如：
            // search:newsearch 重新发起检索
            // search:n             显示上次命中结果集中下一页
            // search:序号         显示详细
            string strCommand = line;
            string strParam = "";
            int nIndex = line.IndexOf(':');
            if (nIndex > 0)
            {
                strCommand = line.Substring(0, nIndex);
                strParam = line.Substring(nIndex+1);
            }

            // 检索是否是命令，如果不是，则将输入认为是当前命令的参数（二级命令）
            bool bRet = dp2CommandUtility.CheckIsCommand(strCommand);
            if (bRet == false)
            {
                strCommand = "";
                if (String.IsNullOrEmpty(this.WeiXinServer.CurrentCmdName) == false)
                {
                    strCommand = this.WeiXinServer.CurrentCmdName;
                    strParam = line;
                }
                else
                {
                    Console.WriteLine("无效的命令:" + line + "");
                    return false;
                }
            }

            // 检索命令
            if (strCommand == dp2CommandUtility.C_Command_Search)
            {
                // 设置当前命令
                this.WeiXinServer.CurrentCmdName = strCommand;

                if (strParam == "")
                {
                    Console.WriteLine("请输入检索词");
                    string strWord = Console.ReadLine();

                    string strFirstPage = "";
                    lRet = this.WeiXinServer.SearchBiblio(strWord,
                        out strFirstPage,
                        out strError);
                    if (lRet == -1)
                    {
                        Console.WriteLine("检索出错：" + strError);
                    }
                    else if (lRet == 0)
                    {
                        Console.WriteLine("未命中");
                    }
                    else
                    {
                        Console.WriteLine(strFirstPage);
                    }
                    return false;
                }

                // 下一页
                if (strParam == "n")
                {
                    string strNextPage = this.WeiXinServer.GetNextPageForSearch();
                    Console.WriteLine(strNextPage);                    
                    return false;
                }

                // 试着转换为书目序号
                int nBiblioIndex = 0;
                try
                {
                    nBiblioIndex = int.Parse(strParam);
                }
                catch
                { }
                // 获取详细信息
                if (nBiblioIndex>=1)
                {
                    string strBiblioInfo = "";
                    int nRet = this.WeiXinServer.GetDetailBiblioInfo(nBiblioIndex,
                        out strBiblioInfo,
                        out strError);
                    if (nRet == -1)
                    {
                        Console.WriteLine(strError);
                        return false; 
                    }

                    // 输入详细信息
                    Console.WriteLine(strBiblioInfo);
                    return false; 
                }


                Console.WriteLine("当前是Search命令，未知的命令参数'" + strParam + "'");
                return false;
            }

            if (strCommand == "myinfo")
            { 
                Console.WriteLine("尚未绑定读者账号，请调Binding命令先绑定");
                return false;
            }


            Console.WriteLine("unknown command '" + strCommand + "'");
            return false;

        }


        static List<string> ParseParameters(string line)
        {
            // string[] parameters = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // List<string> result = new List<string>(parameters);

            List<string> result0 = StringUtil.SplitString(line,
                " ",
                new string[] { "''" },
                StringSplitOptions.RemoveEmptyEntries);

            List<string> result1 = new List<string>();
            foreach (string s in result0)
            {
                result1.Add(UnQuote(s));
            }

            // 对第一个元素修正一下。从左面开始，如果出现第一个标点符号，就认为这里应该断开
            if (result1.Count > 0)
            {
                string strText = result1[0];
                int index = strText.IndexOfAny(new char[] { '.', '/', '\\' });
                if (index != -1)
                {
                    result1[0] = strText.Substring(0, index);
                    result1.Insert(1, strText.Substring(index));
                }
            }

            return result1;
        }

        static string UnQuote(string strText)
        {
            return strText.Replace("'", "");
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        bool disposed = false;

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.

                    /*
                    this.DestoryChannel();

                    if (this.AppInfo != null)
                    {
                        AppInfo.Save();
                        AppInfo = null;	// 避免后面再用这个对象
                    }
                     */
                }

                /*
                // Call the appropriate methods to clean up 
                // unmanaged resources here.
                // If disposing is false, 
                // only the following code is executed.
                CloseHandle(handle);
                handle = IntPtr.Zero;            
                */
            }
            disposed = true;
        }
    }
}
