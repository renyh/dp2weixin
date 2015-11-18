using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Web.Configuration;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Helpers;
using System.Xml;
using DigitalPlatform.Xml;
using System.Globalization;
using dp2weixin.dp2RestfulApi;
using Senparc.Weixin.Context;
using Senparc.Weixin.MP.Entities.Request;
using DigitalPlatform.IO;
using System.Diagnostics;
using DigitalPlatform;
using DigitalPlatform.Text;

namespace dp2weixin
{
    /// <summary>
    /// 自定义MessageHandler
    /// 把MessageHandler作为基类，重写对应请求的处理方法
    /// </summary>
    public partial class dp2MessageHandler : MessageHandler<dp2MessageContext>
    {
        public string ServerBaseUrl = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="maxRecordCount"></param>
        public dp2MessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalWeixinContext.ExpireMinutes = 3。
            WeixinContext.ExpireMinutes = 3;
        }

        /// <summary>
        /// 执行时，用于过滤黑名单
        /// </summary>
        public override void OnExecuting()
        {                     
            base.OnExecuting();
        }

        /// <summary>
        /// 执行后
        /// </summary>
        public override void OnExecuted()
        {
            base.OnExecuted();
        }

        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            try
            {
                //===================
                // 一些第二步操作的放在前面

                // 检查是否是输入next环境,且输入N/n
                if (this.CurrentMessageContext.IsCanNextBrowse == true
                    && requestMessage.Content.ToLower()=="n")
                {
                    return this.GetNextBrowse();
                }
                //清掉next环境
                this.CurrentMessageContext.IsCanNextBrowse = false;

                // 判断是否是续借，输入的册条码
                if (this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_Renew)
                {
                    return this.Renew(requestMessage.Content);
                }
                //判断是否是查询输入的检索词步骤
                if (this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_Search)
                {
                    return this.SearchBiblio(requestMessage.Content);
                }
                // 获取详细的书目信息
                if (this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_SearchDetail)
                {
                    return this.GetDetailBiblioInfo(requestMessage.Content);
                }



                //================

                //============
                // 关于绑定
                if (this.CurrentMessageContext.BindingStep == 0)
                {
                    string strContent = requestMessage.Content.Trim();
                    int nIndex = strContent.IndexOf('/');
                    if (nIndex > 0)
                    {
                        // 同时输入读者证条码与密码
                        // 先恢复一下状态
                        this.CurrentMessageContext.BindingStep = -1;
                        string barcode = strContent.Substring(0, nIndex);
                        string password = strContent.Substring(nIndex + 1);

                        return this.Binding(barcode, password, requestMessage.FromUserName);
                    }
                    else
                    {
                        this.CurrentMessageContext.BindingStep = 1;
                        var responseMessage = CreateResponseMessage<ResponseMessageText>();
                        responseMessage.Content = "读输入密码";
                        return responseMessage;
                    }
                }
                else if (this.CurrentMessageContext.BindingStep == 1)
                {
                    // 已输入密码
                    // 先恢复一下状态
                    this.CurrentMessageContext.BindingStep = -1;
                    string strBarcode = "";
                    string strPassword = requestMessage.Content;
                    var historyMessage = this.CurrentMessageContext.RequestMessages[this.CurrentMessageContext.RequestMessages.Count - 2];//是否是后进先出
                    if (historyMessage is RequestMessageText)
                        strBarcode = ((RequestMessageText)historyMessage).Content;

                    // 绑定用户
                    return this.Binding(strBarcode, strPassword, requestMessage.FromUserName);
                }

                // 先恢复一下绑定步骤
                this.CurrentMessageContext.BindingStep = -1;

                //=============

                // 把操作步骤清掉
                this.CurrentMessageContext.CurrentAction = "";

                if (requestMessage.Content.ToLower() == dp2WeiXinConst.ACTION_Search)
                {
                    return this.WaitForSearchWordMessage();
                }
                else if (requestMessage.Content.ToLower() == dp2WeiXinConst.ACTION_Binding)
                {
                    this.CurrentMessageContext.CurrentAction = dp2WeiXinConst.ACTION_Binding;
                    return this.ReplyMyMessage(requestMessage.FromUserName);
                }
                else if (requestMessage.Content.ToLower() == dp2WeiXinConst.ACTION_Unbinding)
                {
                    return this.ReplyUnbindingMessage(requestMessage.FromUserName);
                }
                else if (requestMessage.Content.ToLower() == dp2WeiXinConst.ACTION_MyInfo)
                {
                    this.CurrentMessageContext.CurrentAction = dp2WeiXinConst.ACTION_MyInfo;
                    return this.ReplyMyMessage(requestMessage.FromUserName);
                }
                else if (requestMessage.Content.ToLower() == dp2WeiXinConst.ACTION_BorrowInfo)
                {
                    this.CurrentMessageContext.CurrentAction = dp2WeiXinConst.ACTION_BorrowInfo;
                    return this.ReplyMyMessage(requestMessage.FromUserName);
                }
                else if (requestMessage.Content.ToLower() == dp2WeiXinConst.ACTION_Renew)
                {
                    this.CurrentMessageContext.CurrentAction = dp2WeiXinConst.ACTION_Renew;
                    return this.ReplyMyMessage(requestMessage.FromUserName);
                }
                //BookRecommend
                else if (requestMessage.Content.ToLower() == dp2WeiXinConst.ACTION_BookRecommend)
                {
                    return this.ReplyNewBooks();
                }
                else if (requestMessage.Content.ToLower() == dp2WeiXinConst.ACTION_Notice)
                {
                    return this.ReplyNotice();
                }
                else 
                {   
                    return this.ReplyCommonTextMessage(requestMessage);
                }
            }
            catch (Exception ex)
            {
                var responseMessage = CreateResponseMessage<ResponseMessageText>();
                responseMessage.Content = "抛出异常："+ ex.Message;
                return responseMessage;
            }
        }



        #region 检索

        /// <summary>
        /// 用户点击检索菜单，或者输入检索命令
        /// </summary>
        /// <returns></returns>
        private IResponseMessageBase WaitForSearchWordMessage()
        {
            this.CurrentMessageContext.CurrentAction = dp2WeiXinConst.ACTION_Search;
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "请输入检索词";
            return responseMessage;
        }

        /// <summary>
        /// 检索书目
        /// </summary>
        /// <param name="strWord"></param>
        /// <returns></returns>
        private IResponseMessageBase SearchBiblio(string strWord)
        {
            // 先把检索状态清掉
            this.CurrentMessageContext.CurrentAction = dp2WeiXinConst.ACTION_SearchDetail;

            var responseMessage = CreateResponseMessage<ResponseMessageText>();

            // 判断检索词
            strWord = strWord.Trim();
            if (String.IsNullOrEmpty(strWord))
            {
                responseMessage.Content = "检索词不能为空。";
                return responseMessage;
            }

            // 从池中征用通道
            LibraryChannel channel = Global.ChannelPool.GetChannel(Global.dp2Url, Global.dp2UserName);
            channel.Password = Global.dp2Password;
            try
            {
                string strError = "";
                long lRet1 = channel.SearchBiblio(strWord,
                    out strError);
                if (lRet1 == -1)
                {
                    responseMessage.Content = "检索失败：" + strError;
                    return responseMessage;
                }
                else if (lRet1 == 0)
                {
                    responseMessage.Content = "您输入的检索词[" +strWord+ "]未命中书目记录。";
                    return responseMessage;
                }
                else if (lRet1 >= 1)
                {
                    long ltotoalCount = lRet1;

                    this.CurrentMessageContext.BiblioResultPathList = new List<string>();
                    long lStart = 0;
                    // 当前总共取的多少记录
                    long lCurTotalCount = 0;

                    //
                REDO:
                    List<string> resultPathList = null;
                    long lCount = -1;
                    long lRet = channel.GetBiblioSearchResult(lStart,
                        lCount,
                         out resultPathList,
                         out strError);
                    if (lRet == -1)
                    {
                        responseMessage.Content = strError;
                        return responseMessage;
                    }
                    else
                    {
                        // 加到内存结果集中
                        this.CurrentMessageContext.BiblioResultPathList.AddRange(resultPathList);

                        // 检查记录是否获取完成，没取完继续取
                        lCurTotalCount += lRet;
                        if (lCurTotalCount < ltotoalCount)
                        {
                            lStart = lCurTotalCount;
                            goto REDO;
                        }
                    }



                    // 检查一下，取出来的记录数，是否与返回的命中数量一致
                    if (ltotoalCount != this.CurrentMessageContext.BiblioResultPathList.Count)
                    {
                        responseMessage.Content = "内部错误，不可能结果集数量不一致";
                        return responseMessage;
                    }

                    // 开始索引,显示一页书目记录
                    this.CurrentMessageContext.ResultNextStart = 0;
                    return this.GetNextBrowse();
                }
            }
            finally
            {
                // 归还通道到池
                Global.ChannelPool.ReturnChannel(channel);
            }

            return responseMessage;
        }

        /// <summary>
        /// 获取下一页检索结果
        /// </summary>
        /// <returns></returns>
        private IResponseMessageBase GetNextBrowse()
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();

            long nRealTotalCount = this.CurrentMessageContext.BiblioResultPathList.Count;
            if (this.CurrentMessageContext.ResultNextStart >= nRealTotalCount)
            {
                responseMessage.Content = "内部错误，下页起始序号>=总记录数了";
                return responseMessage;
            }

            // 本页显示的最大序号
            long nMaxIndex = this.CurrentMessageContext.ResultNextStart + dp2WeiXinConst.C_VIEW_COUNT;
            if (nMaxIndex > nRealTotalCount)
            {
                nMaxIndex = nRealTotalCount;                
            }

            string strPreMessage = "";
            if (nMaxIndex < dp2WeiXinConst.C_VIEW_COUNT
                || (this.CurrentMessageContext.ResultNextStart==0 && nMaxIndex== nRealTotalCount))
            {
                // 没有下页了
                this.CurrentMessageContext.IsCanNextBrowse = false;
                strPreMessage = "命中'" + nRealTotalCount + "'条书目记录。您可以回复序列查看详细信息。\r\n";
            }
            else if (nMaxIndex < nRealTotalCount)
            {
                // 有下页
                this.CurrentMessageContext.IsCanNextBrowse = true;
                strPreMessage = "命中'" + nRealTotalCount + "'条书目记录。本次显示第" + (this.CurrentMessageContext.ResultNextStart + 1).ToString() + "-" + nMaxIndex + "条，您可以回复N继续显示下一页，或者回复序列查看详细信息。\r\n";
            }
            else if (nMaxIndex == nRealTotalCount)
            {
                //无下页
                this.CurrentMessageContext.IsCanNextBrowse = false;
                strPreMessage = "命中'" + nRealTotalCount + "'条书目记录。本次显示第" + (this.CurrentMessageContext.ResultNextStart + 1).ToString() + "-" + nMaxIndex + "条，已到末页。您可以回复序列查看详细信息。\r\n";
            }

            string strBrowse = "";
            for (long i = this.CurrentMessageContext.ResultNextStart; i < nMaxIndex; i++)
            {
                if (strBrowse != "")
                    strBrowse += "\n";

                string text = this.CurrentMessageContext.BiblioResultPathList[(int)i];
                int index = text.IndexOf("*");
                if (index >= 0)
                    text = text.Substring(index + 1);
                strBrowse += (i + 1).ToString().PadRight(5, ' ') + text;
            }

            // 设置下页索引
            this.CurrentMessageContext.ResultNextStart = nMaxIndex;

            //返回结果
            responseMessage.Content = strPreMessage + strBrowse;

            return responseMessage;
        }



        /// <summary>
        /// 根据书目序号得到详细的参考信息
        /// </summary>
        /// <param name="strIndex">书目序号，从1排序</param>
        /// <returns>
        /// 文本消息
        /// </returns>
        private IResponseMessageBase GetDetailBiblioInfo(string strIndex)
        {
            // 设置可以继续看其它书目详细信息
            this.CurrentMessageContext.CurrentAction = dp2WeiXinConst.ACTION_SearchDetail;
            //设回可以next
            this.CurrentMessageContext.IsCanNextBrowse = true;

            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            int nIndex = 0;
            try
            {
                nIndex = int.Parse(strIndex);
            }
            catch
            {
                // 这里设置结束看详细信息的语境
                this.CurrentMessageContext.CurrentAction = "";

                responseMessage.Content = "您输入的书目序号[" + strIndex + "]不正确。";
                return responseMessage;
            }

            //检查有无超过数组界面
            if (nIndex <= 0 || this.CurrentMessageContext.BiblioResultPathList.Count < nIndex)
            {
                responseMessage.Content = "您输入的书目序号[" + strIndex + "]越出范围。";
                return responseMessage;
            }

            // 获取路径，注意要截取
            string strPath = this.CurrentMessageContext.BiblioResultPathList[nIndex - 1];
            int index = strPath.IndexOf("*");
            if (index > 0)
                strPath = strPath.Substring(0, index);
            LibraryChannel channel = Global.ChannelPool.GetChannel(Global.dp2Url, Global.dp2UserName);
            channel.Password = Global.dp2Password;
            try
            {
                string strError = "";
                string strBrowse = "";
                long lRet1 = channel.GetBiblioDetail(strPath,
                    out strBrowse,
                    out strError);
                if (lRet1 == -1)
                {
                    responseMessage.Content = "获取详细信息失败：" + strError;
                    return responseMessage;
                }
                else if (lRet1 == 0)
                {
                    //返回结果
                    responseMessage.Content = strBrowse;
                    return responseMessage;
                }
            }
            finally
            {
                Global.ChannelPool.ReturnChannel(channel);
            }
            return responseMessage;
        }

        #endregion

        #region 绑定/解绑

        /// <summary>
        /// 微信用户绑定读者证身份，绑定完成后，会根据读者意图显示需要的信息
        /// </summary>
        /// <param name="strBarcode"></param>
        /// <param name="strPassword"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        public IResponseMessageBase Binding(string strBarcode, string strPassword, string openid)
        {
            LibraryChannel channel = Global.ChannelPool.GetChannel(Global.dp2Url, Global.dp2UserName);
            channel.Password = Global.dp2Password;
            try
            {
                // 检验用户名与密码
                string strError = "";
                long lRet = channel.VerifyReaderPassword(strBarcode,
                   strPassword,
                    out strError);
                if (lRet == -1)
                {
                    this.CurrentMessageContext.BindingStep = 0;
                    var responseMessage = CreateResponseMessage<ResponseMessageText>();
                    responseMessage.Content = "读者证条码号或密码不正确。\n请重新输入'读者证条码号'（注:您也可以同时输入'读者证条码号'和'密码'，中间以/分隔，例如:R0000001/123）";
                    return responseMessage;
                }
                else if (lRet == 0)
                {
                    this.CurrentMessageContext.BindingStep = 0;
                    var responseMessage = CreateResponseMessage<ResponseMessageText>();
                    responseMessage.Content = "读者证条码号或密码不正确。\n请重新输入'读者证条码号'（注:您也可以同时输入'读者证条码号'和'密码'，中间以/分隔，例如:R0000001/123）";
                    return responseMessage;
                }
                else if (lRet == 1)
                {
                    // 进行绑定
                    // 先根据barcode检索出来,得到原记录与时间戳
                    GetReaderInfoResponse response = channel.GetReaderInfo(strBarcode, 
                        "xml,advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary");
                    if (response.GetReaderInfoResult.Value != 1)
                    {
                        var responseMessage = CreateResponseMessage<ResponseMessageText>();
                        responseMessage.Content = "根据读者证条码号得到读者记录异常：" + response.GetReaderInfoResult.ErrorInfo;
                        return responseMessage;
                    }
                    string strRecPath = response.strRecPath;
                    string strTimestamp = StringUtil.GetHexTimeStampString(response.baTimestamp);
                    string strXml = response.results[0];
                    string strAdvanceXml = response.results[1];

                    // 改读者的email字段
                    XmlDocument readerDom = new XmlDocument();
                    readerDom.LoadXml(strXml);
                    XmlNode emailNode = readerDom.SelectSingleNode("//email");
                    if (emailNode == null)
                    {
                        emailNode = readerDom.CreateElement("email");
                        readerDom.DocumentElement.AppendChild(emailNode);
                    }
                    
                    emailNode.InnerText = this.joinEmail(emailNode.InnerText,openid);
                    string strNewXml = dp2MessageHandler.ConvertXmlToString(readerDom);

                    // 更新到读者库
                    lRet = channel.SetReaderInfoForWeiXin(strRecPath,
                        strNewXml,
                        strTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        var responseMessage = CreateResponseMessage<ResponseMessageText>();
                        responseMessage.Content = "绑定出错：" + strError;
                        return responseMessage;
                    }
                    else
                    {
                        // 绑定成功，把读者证条码记下来，用于续借 2015/11/7
                        this.CurrentMessageContext.ReaderBarcode = strBarcode;

                        if (this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_Binding)
                        {
                            this.CurrentMessageContext.CurrentAction="";
                            // 返回读者信息
                            var responseMessage = CreateResponseMessage<ResponseMessageText>();
                            responseMessage.Content = "绑定读者账号成功。";
                            return responseMessage;
                        }
                        else
                        {
                            // 拼出各种消息的不同样式
                            return GetMessageDetailStyleByAction(strAdvanceXml);
                        }
                    }
                }
                else
                {
                    var responseMessage = CreateResponseMessage<ResponseMessageText>();
                    responseMessage.Content = "校验读者账号返回未知情况，返回值：" + lRet.ToString() + "-" + strError;
                    return responseMessage;
                }
            }
            finally
            {
                Global.ChannelPool.ReturnChannel(channel);
            }
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <param name="openid"></param>
        /// <returns></returns>
        public ResponseMessageText ReplyUnbindingMessage(string openid)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();

            // 根据openid检索是否已经绑定的读者
            string strRecPath = "";
            string strXml = "";
            string strError = "";
            long lRet = this.SearchReaderByOpenId(openid, out strRecPath,
                out strXml,
                out strError);
            if (lRet == -1)
            {
                responseMessage.Content = strError;
                return responseMessage;
            }
            else if (lRet == 0) // 未绑定
            {
                this.CurrentMessageContext.BindingStep = 0;
                responseMessage.Content = "您尚未绑定读者账号，不需要解绑。";
                return responseMessage;
            }
            else if (lRet == 1)
            {
                LibraryChannel channel = Global.ChannelPool.GetChannel(Global.dp2Url, Global.dp2UserName);
                channel.Password = Global.dp2Password;
                try
                {
                    // 进行解绑工作
                    string strPath = "";
                    lRet = channel.GetSearchResultForWeiXinUser(out strPath,
                         out strXml,
                         out strError);
                    if (lRet == 1)
                    {
                        // 先根据barcode检索出来,得到原记录与时间戳
                        string barcode = "@path:" + strPath;
                        GetReaderInfoResponse response = channel.GetReaderInfo(barcode, "xml");
                        if (response.GetReaderInfoResult.Value != 1)
                        {
                            responseMessage = CreateResponseMessage<ResponseMessageText>();
                            responseMessage.Content = "根据路径得到读者记录异常：" + response.GetReaderInfoResult.ErrorInfo;
                            return responseMessage;
                        }
                        strRecPath = response.strRecPath;
                        string strTimestamp = StringUtil.GetHexTimeStampString(response.baTimestamp);
                        strXml = response.results[0];

                        // 修改xml中的email字段，去掉weixin:***
                        // 改为读者的email字段
                        XmlDocument readerDom = new XmlDocument();
                        readerDom.LoadXml(strXml);
                        XmlNode emailNode = readerDom.SelectSingleNode("//email");
                        string email = emailNode.InnerText.Trim();
                        string strEmailLeft = email;
                        string strEmailLRight = "";
                        int nIndex = email.IndexOf("weixinid:");
                        if (nIndex >= 0)
                        {
                            strEmailLeft = email.Substring(0, nIndex);
                            string strOldWeixinId = email.Substring(nIndex);
                            nIndex = strOldWeixinId.IndexOf(',');
                            if (nIndex > 0)
                            {
                                strEmailLRight = strOldWeixinId.Substring(nIndex);
                                strOldWeixinId = strOldWeixinId.Substring(0, nIndex);
                            }
                            strEmailLeft = dp2MessageHandler.TrimComma(strEmailLeft);
                            strEmailLRight = dp2MessageHandler.TrimComma(strEmailLRight);
                        }
                        email = strEmailLeft;
                        if (strEmailLRight != "")
                        {
                            if (email != "")
                                email += ",";
                            email += strEmailLRight;
                        }
                        emailNode.InnerText = email;
                        string strNewXml = dp2MessageHandler.ConvertXmlToString(readerDom);

                        // 更新到读者库
                        lRet = channel.SetReaderInfoForWeiXin(strPath,
                            strNewXml,
                            strTimestamp,
                            out strError);
                        if (lRet == -1)
                        {
                            responseMessage = CreateResponseMessage<ResponseMessageText>();
                            responseMessage.Content = "解除绑定出错：" + strError;
                            return responseMessage;
                        }
                        else
                        {
                            // 返回读者信息
                            responseMessage = CreateResponseMessage<ResponseMessageText>();
                            responseMessage.Content = "解除绑定成功";
                            return responseMessage;
                        }
                    }
                    else
                    {
                        responseMessage.Content = "获取结果集异常:" + strError;
                        return responseMessage;
                    }
                }
                finally
                {
                    Global.ChannelPool.ReturnChannel(channel);
                }
            }
            else
            {
                responseMessage.Content = "解绑，应该不会走到这里。";
                return responseMessage;
            }
        }

        #endregion

        #region 我的各类信息

        /// <summary>
        /// 回复用户信息Binding，MyInfo，BorrowInfo，Renew
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public IResponseMessageBase ReplyMyMessage(string openid)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();

            // 根据openid检索是否已经绑定的读者
            string strRecPath = "";
            string strXml = "";
            string strError = "";
            long lRet = this.SearchReaderByOpenId(openid, out strRecPath,
                out strXml,
                out strError);
            if (lRet == -1)
            {
                responseMessage.Content = strError;
                return responseMessage;
            }
            else if (lRet == 0) // 未绑定
            {
                this.CurrentMessageContext.BindingStep = 0;
                string text = "请输入'读者证条码号'（注:您也可以同时输入'读者证条码号'和'密码'，中间以/分隔，例如:R0000001/123）。<font color='red'>请点左下角的小键盘图标，切换输入框方式。</font>";
                if (this.CurrentMessageContext.CurrentAction != "Binding")
                    text = "您尚未绑定读者账号，" + text;
                responseMessage.Content = text;
                return responseMessage;
            }
            else if (lRet == 1)
            {
                if (this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_Binding)
                {
                    this.CurrentMessageContext.CurrentAction = "";
                    responseMessage.Content = "您已绑定读者账号。";
                    return responseMessage;
                }
                else if (this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_MyInfo
                        || this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_BorrowInfo
                        || this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_Renew)
                {
                    LibraryChannel channel = Global.ChannelPool.GetChannel(Global.dp2Url, Global.dp2UserName);
                    channel.Password = Global.dp2Password;
                    try
                    {
                        // 对于已绑定的用户，取出Barcode，用于续借
                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml(strXml);
                        XmlNode barcodeNode = dom.SelectSingleNode("/root/barcode");
                        this.CurrentMessageContext.ReaderBarcode = barcodeNode.InnerText;

                        // 先根据barcode检索出来,得到原记录与时间戳
                        GetReaderInfoResponse response = channel.GetReaderInfo("@path:" + strRecPath,
                            "advancexml,advancexml_borrow_bibliosummary,advancexml_overdue_bibliosummary");
                        if (response.GetReaderInfoResult.Value != 1)
                        {
                            responseMessage = CreateResponseMessage<ResponseMessageText>();
                            responseMessage.Content = "根据读者证条码号得到读者记录异常：" + response.GetReaderInfoResult.ErrorInfo;
                            return responseMessage;
                        }
                        string strTimestamp = StringUtil.GetHexTimeStampString(response.baTimestamp);
                        strXml = response.results[0];

                        // 拼出各种消息的不同样式
                        return GetMessageDetailStyleByAction(strXml);
                    }
                    finally
                    {
                        Global.ChannelPool.ReturnChannel(channel);
                    }
                }
                else
                {
                    this.CurrentMessageContext.CurrentAction = "";
                    var strongResponseMessage = CreateResponseMessage<ResponseMessageText>();
                    strongResponseMessage.Content = this.CurrentMessageContext.CurrentAction + "功能正在开发中...";
                    return strongResponseMessage;
                }
            }
            else
            {
                responseMessage.Content = "ReplyMyinfoTextMessage函数，不应该走到这里。";
                return responseMessage;
            }
        }

        /// <summary>
        /// 拼出各种消息的不同样式
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private IResponseMessageBase GetMessageDetailStyleByAction(string strXml)
        {
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(strXml);
            if (this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_MyInfo)
            {
                this.CurrentMessageContext.CurrentAction = "";
                return this.GetReaderInfoMessage(dom);
            }
            else if (this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_BorrowInfo)
            {
                this.CurrentMessageContext.CurrentAction = "";
                bool bHasBorrow = false;
                return this.GetBorrowsMessage(dom, out bHasBorrow);
            }
            else if (this.CurrentMessageContext.CurrentAction == dp2WeiXinConst.ACTION_Renew)
            {
                // 这里需要把状态设为renew，后面继续输入条码续借
                this.CurrentMessageContext.CurrentAction = dp2WeiXinConst.ACTION_Renew;


                bool bHasBorrow = false;
                IResponseMessageBase message = this.GetBorrowsMessage(dom, out bHasBorrow);
                if (bHasBorrow == true)
                {
                    ResponseMessageNews message1 = (ResponseMessageNews)message;
                    message1.Articles.Add(new Article()
                    {
                        Title = "您有上列已借图书，请输入要续借图书的编号或者册条码号。",
                        Description = "",
                        PicUrl = "",
                        Url = ""
                    });
                    return message1;
                }
                else
                {
                    return message;
                }
            }
            else
            {
                var responseMessage = CreateResponseMessage<ResponseMessageText>();
                responseMessage.Content = "不可能走到这里1";
                return responseMessage;
            }
        }

        /// <summary>
        /// 输出个人信息
        /// </summary>
        /// <param name="dom"></param>
        /// <returns></returns>
        private IResponseMessageBase GetReaderInfoMessage(XmlDocument dom)
        {
            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,"barcode");
            string strName = DomUtil.GetElementText(dom.DocumentElement, "name");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,"department");
            string strState = DomUtil.GetElementText(dom.DocumentElement,    "state");
            string strCreateDate = DateTimeUtil.ToLocalTime(DomUtil.GetElementText(dom.DocumentElement,
                "createDate"), "yyyy/MM/dd");
            string strExpireDate = DateTimeUtil.ToLocalTime(DomUtil.GetElementText(dom.DocumentElement,
                "expireDate"), "yyyy/MM/dd");
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
                "readerType");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");

            string strText = "姓       名：" + strName + "\n"
                + "证条码号：" + strReaderBarcode + "\n"
                + "部       门：" + strDepartment + "\n"
                + "联系方式：\n" + GetContactString(dom) + "\n"
                + "状       态：" + strState + "\n"
                + "有  效  期：" + strCreateDate + "~" + strExpireDate + "\n"
                + "读者类别：" + strReaderType + "\n"
                + "注       释：" + strComment;

            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            responseMessage.Articles.Add(new Article()
            {
                Title = "个人信息",
                Description ="",
                PicUrl = "",
                Url = ""
            });

             responseMessage.Articles.Add(new Article()
            {
                Title = strText,
                Description ="",
                PicUrl = "",
                Url = ""
            });

            return responseMessage;
        }

        /// <summary>
        /// 输出借阅信息
        /// </summary>
        /// <param name="dom"></param>
        /// <param name="bHasBorrow"></param>
        /// <returns></returns>
        private IResponseMessageBase GetBorrowsMessage(XmlDocument dom,out bool bHasBorrow)
        {
            bHasBorrow = false;
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            if (nodes.Count == 0)
            {
                var responseMessage1 = CreateResponseMessage<ResponseMessageText>();
                responseMessage1.Content = "无借阅记录";
                return responseMessage1;
            }

            bHasBorrow = true;
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            responseMessage.Articles.Add(new Article()
            {
                Title = "借阅信息",
                Description = "",
                PicUrl = "",
                Url = ""
            });

            Dictionary<string, string> borrowLit = new Dictionary<string, string>();

            int index = 1;
            foreach (XmlElement borrow in nodes)
            {        
                string overdueText = "";
                string strIsOverdue = DomUtil.GetAttr(borrow, "isOverdue");
                if (strIsOverdue == "yes")
                   overdueText = DomUtil.GetAttr(borrow, "overdueInfo1");
                else
                    overdueText = "未超期";

               
                string itemBarcode = DomUtil.GetAttr(borrow, "barcode");
                borrowLit[index.ToString()] = itemBarcode; // 设到字典点，已变续借


                string bookName = DomUtil.GetAttr(borrow, "summary");//borrow.GetAttribute("summary")
                int tempIndex = bookName.IndexOf('/');
                if (tempIndex>0)
                {
                    bookName = bookName.Substring(0,tempIndex);
                }

                string title = "编号：" + index.ToString() + "\n"
                    + "册条码号：" +itemBarcode+"\n"
                    +"书       名："+ bookName+"\n"
                    + "借阅时间：" + DateTimeUtil.ToLocalTime(borrow.GetAttribute("borrowDate"), "yyyy-MM-dd HH:mm") + "\n"
                    + "借       期：" + DateTimeUtil.GetDisplayTimePeriodString(borrow.GetAttribute("borrowPeriod")) + "\n"
                    + "应还时间：" + DateTimeUtil.ToLocalTime(borrow.GetAttribute("returningDate"), "yyyy-MM-dd") + "\n"
                    +"是否超期："+overdueText;

                responseMessage.Articles.Add(new Article()
                {
                    Title = title,
                    Description = "",
                    PicUrl = "",
                    Url = ""
                });

                index++; //编号+1
            }

            // 设到用户上下文
            this.CurrentMessageContext.BorrowDict = borrowLit;

            return responseMessage;
           
        }

        /// <summary>
        /// 续借
        /// </summary>
        /// <param name="strItemBarcode">册条码号</param>
        /// <returns></returns>
        private IResponseMessageBase Renew(string strItemBarcode)
        {
            //先将续借状态清掉
            this.CurrentMessageContext.CurrentAction = "";

            if (strItemBarcode == null)
                strItemBarcode = "";
            strItemBarcode = strItemBarcode.Trim();

            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "内部异常：未给消息内容赋值";

            //先把续借状态清掉
            this.CurrentMessageContext.CurrentAction = "";

            if (strItemBarcode == "")
            {
                responseMessage.Content = "续借失败：您输入的续借图书编号或者册条码号为空。";
                return responseMessage;
            }

            if (this.CurrentMessageContext.ReaderBarcode == null || this.CurrentMessageContext.ReaderBarcode == "")
            {
                responseMessage.Content = "续借失败：内部错误，读者证条码号为空。";
                return responseMessage;
            }

            // 优先从序号字典中找下
            if (this.CurrentMessageContext.BorrowDict.ContainsKey(strItemBarcode))
            {
                string temp = this.CurrentMessageContext.BorrowDict[strItemBarcode];
                if (temp != null && temp != "")
                    strItemBarcode = temp;
            }


            LibraryChannel channel = Global.ChannelPool.GetChannel(Global.dp2Url, Global.dp2UserName);
            channel.Password = Global.dp2Password;
            try
            {
                string strError = "";
                BorrowInfo borrowInfo = null;
                long lRet = channel.Renew(this.CurrentMessageContext.ReaderBarcode,
                    strItemBarcode,
                    out borrowInfo,
                    out strError);
                if (lRet == -1)
                {
                    responseMessage.Content = "续借失败：" + strError;
                    return responseMessage;
                }
                else if (lRet == 0)
                {
                    string returnTime = DateTimeUtil.ToLocalTime(borrowInfo.LatestReturnTime, "yyyy/MM/dd");
                    responseMessage.Content = strItemBarcode+"续借成功,还书日期为：" + returnTime + "。";
                    return responseMessage;
                }
            }
            finally
            {
                Global.ChannelPool.ReturnChannel(channel);
            }


            return responseMessage;
        }


        #endregion

        #region 有用函数

        /// <summary>
        /// 根据微信id查找读者
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="strRecPath"></param>
        /// <param name="strXml"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        private long SearchReaderByOpenId(string openid, out string strRecPath, out string strXml,
            out string strError)
        {
            strError = "";
            strRecPath = "";
            strXml = "";

            long lRet = 0;

            LibraryChannel channel = Global.ChannelPool.GetChannel(Global.dp2Url, Global.dp2UserName);
            channel.Password = Global.dp2Password;
            try
            {
                string strWeiXinId = "weixinid:" + openid;
                lRet = channel.SearchReaderByWeiXinId(strWeiXinId, out strError);
                if (lRet == -1)
                {
                    strError = "检索微信用户对应的读者出错:" + strError;
                    return -1;
                }
                else if (lRet > 1)
                {
                    strError = "检索微信用户对应的读者异常，得到" + lRet.ToString() + "条读者记录";
                    return -1;
                }
                else if (lRet == 0)
                {
                    strError = "根据微信id未找到对应读者。";
                    return 0;
                }
                else if (lRet == 1)
                {
                    lRet = channel.GetSearchResultForWeiXinUser(out strRecPath,
                         out strXml,
                         out strError);
                    if (lRet != 1)
                    {
                        strError = "获取结果集异常:" + strError;
                        return -1;
                    }
                }
            }
            finally
            {
                Global.ChannelPool.ReturnChannel(channel);
            }

            return 1;
        }

        /// <summary>
        /// 拼email
        /// </summary>
        /// <param name="oldEmail"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        private string joinEmail(string oldEmail, string openid)
        {
            string email = oldEmail.Trim();
            string strEmailLeft = email;
            string strEmailLRight = "";
            int nIndex = email.IndexOf("weixinid:");
            if (nIndex > 0)
            {
                strEmailLeft = email.Substring(0, nIndex);
                string strOldWeixinId = email.Substring(nIndex);
                nIndex = strOldWeixinId.IndexOf(',');
                if (nIndex > 0)
                {
                    strEmailLRight = strOldWeixinId.Substring(nIndex);
                    strOldWeixinId = strOldWeixinId.Substring(0, nIndex);
                }
                strEmailLeft = dp2MessageHandler.TrimComma(strEmailLeft);
                strEmailLRight = dp2MessageHandler.TrimComma(strEmailLRight);
            }
            email = strEmailLeft;
            if (strEmailLRight != "")
            {
                if (email != "")
                    email += ",";
                email += strEmailLRight;
            }

            if (openid != null && openid != "")
            {
                if (email != "")
                    email += ",";
                email += "weixinid:" + openid;
            }

            return email;
        }

        /// <summary>
        /// 去掉前后逗号
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public static string TrimComma(string strText)
        {
            if (strText == null || strText.Length == 0)
                return strText;

            int nIndex = strText.LastIndexOf(',');
            if (nIndex > 0)
                strText = strText.Substring(0, nIndex);

            nIndex = strText.IndexOf(',');
            if (nIndex > 0)
                strText = strText.Substring(nIndex + 1);

            return strText;
        }

        /// <summary>
        /// 将XmlDocument转化为string
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public static string ConvertXmlToString(XmlDocument xmlDoc)
        {
            MemoryStream stream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            xmlDoc.Save(writer);

            StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8);
            stream.Position = 0;
            string xmlString = sr.ReadToEnd();
            sr.Close();
            stream.Close();

            return xmlString;
        }

        /// <summary>
        /// 得到的读者的联系方式
        /// </summary>
        /// <param name="dom"></param>
        /// <returns></returns>
        private string GetContactString(XmlDocument dom)
        {
            string strTel = DomUtil.GetElementText(dom.DocumentElement, "tel");
            string strEmail = DomUtil.GetElementText(dom.DocumentElement, "email");
            string strAddress = DomUtil.GetElementText(dom.DocumentElement, "address");
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(strTel) == false)
                list.Add(strTel);
            if (string.IsNullOrEmpty(strEmail) == false)
            {
                strEmail = this.joinEmail(strEmail, "");
                list.Add(strEmail);
            }
            if (string.IsNullOrEmpty(strAddress) == false)
                list.Add(strAddress);
            return StringUtil.MakePathList(list, "; ");
        }

        /// <summary>
        /// 日志锁
        /// </summary>
        static object logSyncRoot = new object();        

        /// <summary>
        /// 写错误日志
        /// </summary>
        /// <param name="strText"></param>
        public static void WriteErrorLog(string strText)
        {
            try
            {
                lock (logSyncRoot)
                {
                    DateTime now = DateTime.Now;
                    // 每天一个日志文件
                    string strFilename = PathUtil.MergePath(Global.dp2WeiXinLogDir, "log_" + DateTimeUtil.DateTimeToString8(now) + ".txt");
                    string strTime = now.ToString();
                    StreamUtil.WriteText(strFilename,
                        strTime + " " + strText + "\r\n");
                }
            }
            catch (Exception ex)
            {
                EventLog Log = new EventLog();
                Log.Source = "dp2opac";
                Log.WriteEntry("因为原本要写入日志文件的操作发生异常， 所以不得不改为写入Windows系统日志(见后一条)。异常信息如下：'" + ExceptionUtil.GetDebugText(ex) + "'", EventLogEntryType.Error);
                Log.WriteEntry(strText, EventLogEntryType.Error);
            }
        }

        #endregion


        #region 新书推荐

        /// <summary>
        /// 图书推荐
        /// </summary>
        /// <returns></returns>
        private IResponseMessageBase ReplyNewBooks()
        {
            string fileName = this.ServerBaseUrl + "/newbooks.xml";
            if (File.Exists(fileName) == false)
            {
                var textResponseMessage = CreateResponseMessage<ResponseMessageText>();
                textResponseMessage.Content = "当前没有推荐图书。";
                return textResponseMessage;
            }
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            XmlDocument dom = new XmlDocument();
            dom.Load(fileName);
            XmlNodeList itemList = dom.DocumentElement.SelectNodes("item");
            foreach (XmlNode node in itemList)
            {
                responseMessage.Articles.Add(new Article()
                {
                    Title = DomUtil.GetNodeText(node.SelectSingleNode("Title")),
                    Description = DomUtil.GetNodeText(node.SelectSingleNode("Description")),
                    PicUrl = Global.dp2WeiXinUrl + DomUtil.GetNodeText(node.SelectSingleNode("PicUrl")),
                    Url = DomUtil.GetNodeText(node.SelectSingleNode("Url"))
                });
            }
            return responseMessage;
        }

        /// <summary>
        /// 近期通告
        /// </summary>
        /// <returns></returns>
        private IResponseMessageBase ReplyNotice()
        {
            string fileName = this.ServerBaseUrl + "/notice.xml";
            if (File.Exists(fileName) == false)
            {
                var textResponseMessage = CreateResponseMessage<ResponseMessageText>();
                textResponseMessage.Content = "暂无通告";
                return textResponseMessage;
            }
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            XmlDocument dom = new XmlDocument();
            dom.Load(fileName);
            XmlNodeList itemList = dom.DocumentElement.SelectNodes("item");
            foreach (XmlNode node in itemList)
            {
                Article article = new Article();
                article.Title = DomUtil.GetNodeText(node.SelectSingleNode("Title"));
                article.Description = DomUtil.GetNodeText(node.SelectSingleNode("Description"));
                string picUrl = DomUtil.GetNodeText(node.SelectSingleNode("PicUrl"));
                if (String.IsNullOrEmpty(picUrl) == false)
                    article.PicUrl = Global.dp2WeiXinUrl + picUrl;
                article.Url = DomUtil.GetNodeText(node.SelectSingleNode("Url"));
                responseMessage.Articles.Add(article);
            }
            return responseMessage;
        }

        #endregion


        #region 返回通用文本消息
        /// <summary>
        /// 返回通用处理
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public ResponseMessageText ReplyCommonTextMessage(RequestMessageText requestMessage)
        {
            //注意：下面泛型ResponseMessageText即返回给客户端的类型，可以根据自己的需要填写ResponseMessageNews等不同类型。
            var responseMessage = CreateResponseMessage<ResponseMessageText>();

            var result = new StringBuilder();
            result.AppendFormat("您刚才发送了文字信息：{0}\r\n\r\n", requestMessage.Content);

            if (CurrentMessageContext.RequestMessages.Count > 1)
            {
                result.AppendFormat("您刚才还发送了如下消息（{0}）：\r\n", CurrentMessageContext.RequestMessages.Count);

                for (int i = CurrentMessageContext.RequestMessages.Count - 2; i >= 0; i--)
                {
                    var historyMessage = CurrentMessageContext.RequestMessages[i];
                    result.AppendFormat("{0} 【{1}】{2}\r\n",
                                        historyMessage.CreateTime.ToShortTimeString(),
                                        historyMessage.MsgType.ToString(),
                                        (historyMessage is RequestMessageText)
                                            ? (historyMessage as RequestMessageText).Content
                                            : "[非文字类型]"
                        );
                }
                result.AppendLine("\r\n");
            }
            result.AppendFormat("如果您在{0}分钟内连续发送消息，记录将被自动保留（当前设置：最多记录{1}条）。过期后记录将会自动清除。\r\n", WeixinContext.ExpireMinutes, WeixinContext.MaxRecordCount);
            result.AppendLine("\r\n");
            result.AppendLine("您还可以发送【位置】【图片】【语音】【视频】等类型的信息（注意是这几种类型，不是这几个文字），查看不同格式的回复。");
            responseMessage.Content = result.ToString();
            return responseMessage;
        }



        #endregion

        /// <summary>
        /// 处理位置请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = string.Format("您刚才发送了地理位置信息。Location_X：{0}，Location_Y：{1}，Scale：{2}，标签：{3}",
                              requestMessage.Location_X, requestMessage.Location_Y,
                              requestMessage.Scale, requestMessage.Label);
            return responseMessage;
        }
        /// <summary>
        /// 处理图片请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageNews>();
            responseMessage.Articles.Add(new Article()
            {
                Title = "您刚才发送了图片信息",
                Description = "您发送的图片将会显示在边上",
                PicUrl = requestMessage.PicUrl,
                Url = "http://www.qxuninfo.com"
            });
            responseMessage.Articles.Add(new Article()
            {
                Title = "第二条",
                Description = "第二条带连接的内容",
                PicUrl = requestMessage.PicUrl,
                Url = "http://www.qxuninfo.com"
            });
            return responseMessage;
        }
        /// <summary>
        /// 处理语音请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVoiceRequest(RequestMessageVoice requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageMusic>();
            responseMessage.Music.MusicUrl = "http://www.qxuninfo.com/music.mp3";
            responseMessage.Music.Title = "这里是一条音乐消息";
            responseMessage.Music.Description = "时间都去哪儿了";
            return responseMessage;
        }
        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您发送了一条视频信息，ID：" + requestMessage.MediaId;
            return responseMessage;
        }
        /// <summary>
        /// 处理链接消息请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        {
            var responseMessage = CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = string.Format(@"您发送了一条连接信息：
Title：{0}
Description:{1}
Url:{2}", requestMessage.Title, requestMessage.Description, requestMessage.Url);
            return responseMessage;
        }
        /// <summary>
        /// 处理事件请求（这个方法一般不用重写，这里仅作为示例出现。除非需要在判断具体Event类型以外对Event信息进行统一操作
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnEventRequest(IRequestMessageEventBase requestMessage)
        {
            var eventResponseMessage = base.OnEventRequest(requestMessage);//对于Event下属分类的重写方法，见：CustomerMessageHandler_Events.cs
            return eventResponseMessage;
        }
        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {     
            //所有没有被处理的消息会默认返回这里的结果
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "这条消息来自DefaultResponseMessage。";
            return responseMessage;
        }
    }
}
