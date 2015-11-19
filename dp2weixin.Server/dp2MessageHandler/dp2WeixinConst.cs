using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dp2weixin
{
    public class dp2WeiXinConst
    {
        public const string ACTION_Binding = "binding";
        public const string ACTION_Unbinding = "unbinding";
        public const string ACTION_MyInfo = "myinfo";
        public const string ACTION_BorrowInfo = "borrowinfo";
        public const string ACTION_Renew = "renew";
        public const string ACTION_Search = "search";
        public const string ACTION_SearchDetail = "search-detail";

        // 图书消息
        public const string ACTION_BookRecommend = "bookrecommend";
        public const string ACTION_Notice = "notice";//Notice

        public const int C_VIEW_COUNT = 50;
    }
}