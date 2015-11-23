﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DigitalPlatform.Text;
using System.Net;
using DigitalPlatform.IO;
using dp2Command.Server;
using dp2Command.Server.Command;
using dp2Command.Server.dp2RestfulApi;
using System.Xml;
using DigitalPlatform.Xml;

namespace dp2ConsoleToWeiXin
{
    /// <summary>
    /// 一个实例
    /// </summary>
    public class Instance : IDisposable
    {
        public dp2CommandServer WeiXinServer = null;

        public string WeiXinId = "1234567890";
        /// <summary>
        /// 读者证条码号，如果未绑定则为空
        /// </summary>
        public string ReaderBarcode = "";

        // 命令集合
        public CommandContainer CmdContiner = null;

        // 当前命令
        public string CurrentCmdName = null;

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

            //命令集合
            this.CmdContiner = new CommandContainer();
        }
 
        // return:
        //      false   正常，继续
        //      true    退出命令
        public bool ProcessCommand(string line)
        {
            //line = line.Trim().ToLower();

            if (line == "exit" || line == "quit")
                return true;

            string strError = "";
            long lRet = 0;
            int nRet = 0;


            // 用空隔号分隔命令与参数，例如：
            // search 空 重新发起检索
            // search n             显示上次命中结果集中下一页
            // search 序号         显示详细
            // binding r0000001/111111
            string strCommand = line;
            string strParam = "";
            int nIndex = line.IndexOf(' ');
            if (nIndex > 0)
            {
                strCommand = line.Substring(0, nIndex);
                strParam = line.Substring(nIndex + 1);
            }

            // 检查是否是命令，如果不是，则将输入认为是当前命令的参数（二级命令）
            bool bRet = dp2CommandUtility.CheckIsCommand(strCommand);
            if (bRet == false)
            {
                strCommand = "";
                if (String.IsNullOrEmpty(this.CurrentCmdName) == false)
                {
                    strCommand = this.CurrentCmdName;
                    strParam = line;
                }
                else
                {
                    Console.WriteLine("无效的命令:" + line + "");
                    return false;
                }
            }

            //=========================
            // 检索命令
            if (strCommand == dp2CommandUtility.C_Command_Search)
            {
                // 设置当前命令
                this.CurrentCmdName = strCommand;

                SearchCommand searchCmd = (SearchCommand)this.CmdContiner.GetCommand(dp2CommandUtility.C_Command_Search);

                if (strParam == "")
                {
                    Console.WriteLine("请输入检索词");
                    string strWord = Console.ReadLine();

                    string strFirstPage = "";
                    lRet = this.WeiXinServer.SearchBiblio(strWord,searchCmd,
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
                    string strNextPage = "";
                    bRet = searchCmd.GetNextPage(out strNextPage, out strError);
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
                if (nBiblioIndex >= 1)
                {
                    string strBiblioInfo = "";
                    nRet = this.WeiXinServer.GetDetailBiblioInfo(searchCmd,nBiblioIndex,
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

            //=========================
            // 绑定读者账号
            if (strCommand == dp2CommandUtility.C_Command_Binding)
            {
                // 设置当前命令
                this.CurrentCmdName = strCommand;

                if (strParam == "")
                {
                    Console.WriteLine("请输入'读者证条码号'（注:您也可以同时输入'读者证条码号'和'密码'，中间以/分隔，例如:R0000001/123）。");
                    return false;
                }

                // 看看上一次输入过用户名的没有？如果已存在用户名，那么这次输入的就是密码
                BindingCommand bindingCmd = (BindingCommand)this.CmdContiner.GetCommand(strCommand);

                string readerBarcode = strParam;
                int nTempIndex = strParam.IndexOf('/');
                if (nTempIndex > 0) // 同时输入读者证条码与密码
                {
                    bindingCmd.ReaderBarcode = strParam.Substring(0, nTempIndex);
                    bindingCmd.Password = strParam.Substring(nTempIndex + 1);
                }
                else
                {
                    // 看看上一次输入过用户名的没有？如果已存在用户名，那么这次输入的就是密码
                    if (bindingCmd.ReaderBarcode == "")
                    {
                        bindingCmd.ReaderBarcode = strParam;
                        Console.WriteLine("读输入密码");
                        return false;
                    }
                    else
                    {
                        bindingCmd.Password = strParam;
                    }
                }


                string strReaderBarcode = "";
                nRet = this.WeiXinServer.Binding(bindingCmd.ReaderBarcode,
                    bindingCmd.Password,
                    this.WeiXinId,
                    out strReaderBarcode,
                    out strError);
                if (nRet == -1 || nRet == 0)
                {
                    Console.WriteLine(strError);
                }
                else if (nRet == 1)
                {
                    // 把用户名与密码清掉，以便再绑其它账号
                    bindingCmd.ReaderBarcode = "";
                    bindingCmd.Password = "";

                    // 设到当前读者变量上
                    this.ReaderBarcode = strReaderBarcode;
                    Console.WriteLine("绑定成功!");
                }

                return false;
            }

            //=========================
            // 解除绑定
            if (strCommand == dp2CommandUtility.C_Command_Unbinding)
            {
                // 设置当前命令
                this.CurrentCmdName = "";

                // 先检查有无绑定读者账号
                nRet = this.CheckIsBinding(this.WeiXinId, out strError);
                if (nRet == -1)
                {
                    Console.WriteLine(strError);
                    return false;
                }
                // 尚未绑定读者账号
                if (nRet == 0)
                {
                    Console.WriteLine("您尚未绑定读者账号，不需要解除绑定。");
                    return false;
                }

                // 解除绑定
                nRet = this.WeiXinServer.Unbinding(this.ReaderBarcode, out strError);
                if (nRet == -1 || nRet == 0)
                {
                    Console.WriteLine(strError);
                    return false;
                }

                // 置空当前读者变量上
                this.ReaderBarcode = "";
                Console.WriteLine("解除绑定成功。");
                return false;

            }

            //=========================
            // 个人信息
            if (strCommand == dp2CommandUtility.C_Command_MyInfo)            
            {
                // 置空当前命令
                this.CurrentCmdName = "";

                // 先检查有无绑定读者账号
                nRet = this.CheckIsBinding(this.WeiXinId, out strError);
                if (nRet == -1)
                {
                    Console.WriteLine(strError);
                    return false;
                }
                // 尚未绑定读者账号
                if (nRet == 0)
                {
                    Console.WriteLine("尚未绑定读者账号，请先调Binding命令先绑定");
                    return false;
                }

                // 获取读者信息
                string strMyInfo = "";
                nRet = this.WeiXinServer.GetMyInfo1(this.ReaderBarcode, out strMyInfo,
                    out strError);
                if (nRet == -1)
                {
                    Console.WriteLine(strError);
                    return false;
                }


                // 显示个人信息
                Console.WriteLine(strMyInfo);
                return false;
            }


            //=========================
            // 借书信息
            if (strCommand == dp2CommandUtility.C_Command_BorrowInfo)
            {
                // 置空当前命令
                this.CurrentCmdName = "";

                // 先检查是否绑定读者账号
                nRet = this.CheckIsBinding(this.WeiXinId, out strError);
                if (nRet == -1)
                {
                    Console.WriteLine(strError);
                    return false;
                }
                // 尚未绑定读者账号
                if (nRet == 0)
                {
                    Console.WriteLine("尚未绑定读者账号，请先调Binding命令先绑定");
                    return false;
                }

                string strBorrowInfo = "";
                nRet = this.WeiXinServer.GetBorrowInfo1(this.ReaderBarcode, out strBorrowInfo,
                    out strError);
                if (nRet == -1)
                {
                    Console.WriteLine(strError);
                    return false;
                }

                // 显示个人信息
                Console.WriteLine(strBorrowInfo);
                return false;
            }

            //=========================
            // 续借
            if (strCommand == dp2CommandUtility.C_Command_Renew)
            {
                // 设置当前命令
                this.CurrentCmdName = strCommand;

                // 先检查是否绑定读者账号
                nRet = this.CheckIsBinding(this.WeiXinId, out strError);
                if (nRet == -1)
                {
                    Console.WriteLine(strError);
                    return false;
                }
                // 尚未绑定读者账号
                if (nRet == 0)
                {
                    Console.WriteLine("尚未绑定读者账号，请先调Binding命令先绑定");
                    return false;
                }

                // 查看已借图书
                if (strParam == "" || strParam=="view")
                {
                    string strBorrowInfo = "";
                    nRet = this.WeiXinServer.GetBorrowInfo1(this.ReaderBarcode, out strBorrowInfo,
                        out strError);
                    if (nRet == -1)
                    {
                        Console.WriteLine(strError);
                        return false;
                    }
                    // 尚未绑定读者账号
                    if (nRet == 0)
                    {
                        Console.WriteLine("尚未绑定读者账号，请先调Binding命令先绑定");
                        return false;
                    }

                    // 显示个人信息
                    Console.WriteLine(strBorrowInfo);
                    Console.WriteLine("请输入续借图书编号或者册条码号");
                    return false;
                }


                // 认作册条码
                BorrowInfo borrowInfo = null;
                nRet = this.WeiXinServer.Renew1(this.ReaderBarcode,
                    strParam,
                    out borrowInfo,
                    out strError);
                if (nRet == -1)
                {
                    Console.WriteLine(strError);
                    return false;
                }

                // 显示续借成功信息信息
                if (lRet == 1)
                {
                    string returnTime = DateTimeUtil.ToLocalTime(borrowInfo.LatestReturnTime, "yyyy/MM/dd");
                    string strText = strParam + "续借成功,还书日期为：" + returnTime + "。";
                    Console.WriteLine(strText);
                    return false;
                }

            }


            Console.WriteLine("unknown command '" + strCommand + "'");
            return false;

        }

        /// <summary>
        /// 检查微信用户是否绑定读者账号
        /// </summary>
        /// <param name="strWeiXinId"></param>
        /// <param name="strXml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        private int CheckIsBinding(string strWeiXinId,  out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(this.ReaderBarcode) == true)
            {
                // 根据openid检索绑定的读者
                string strRecPath = "";
                string strXml = "";
                long lRet = this.WeiXinServer.SearchReaderByWeiXinId(strWeiXinId, out strRecPath,
                    out strXml,
                    out strError);
                if (lRet == -1)
                {
                    return -1;
                }
                // 未绑定
                if (lRet == 0)
                {
                    return 0;
                }

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);
                this.ReaderBarcode = DomUtil.GetNodeText(dom.DocumentElement.SelectSingleNode("barcode"));
            }
            return 1;
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
