using dp2weixin.CookieWebClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace dp2WebToWeiXin
{
    public partial class WeiXinApi : System.Web.UI.Page
    {

        string url = "http://localhost:14153/index.aspx";//"http://localhost:6232/Index.aspx";//"http://localhost:43540/Index.aspx"; //"http://localhost/dp2weixin/Index.aspx";//
        /// <summary>
        /// 当前通道所使用的 HTTP Cookies
        /// </summary>
        public CookieContainer Cookies = new System.Net.CookieContainer();

        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                CookieAwareWebClient client = new CookieAwareWebClient(this.Cookies);
                client.Headers["Content-type"] = "application/xml; charset=utf-8";
                    

                string xml = @"<xml>
                    <ToUserName>123456789</ToUserName>
                    <FromUserName>00002</FromUserName>
                    <CreateTime>" + WeiXinDateTime.DateTimeToInt(DateTime.Now) + "</CreateTime>"
                    +"<MsgType>text</MsgType>"
                    +"<Content>"+this.txtMessage.Text+"</Content>"
                    +@"<MsgId>1234567890123456</MsgId>
                    </xml>";

                byte[] baData = Encoding.UTF8.GetBytes(xml);
                byte[] result = client.UploadData(this.url,
                    "POST",
                    baData);

                string strResult = Encoding.UTF8.GetString(result);

                this.txtResult.Text = strResult;
            }
            catch (Exception ex)
            {
                this.txtResult.Text="Exception :" + ex.Message;
            }
        }


    }

    public static class WeiXinDateTime
    {
        /// <summary>
        /// 微信的CreateTime是当前与1970-01-01 00:00:00之间的秒数
        /// </summary>
        /// <param name=“dt”></param>
        /// <returns></returns>
        public static string DateTimeToInt(this DateTime dt)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            //intResult = (time- startTime).TotalMilliseconds;
            long t = (dt.Ticks - startTime.Ticks) / 10000000;            //现在是10位，除10000调整为13位
            return t.ToString();
        }
    }
}