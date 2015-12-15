using dp2CirculationWeb.Models;
using ilovelibrary.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2CirculationWeb.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            string strError = "";
            string strRight = "";
            //登录dp2library服务器
            bool bRet = ilovelibraryServer.Instance.Login(model.UserName,
                model.Password,
                out strRight,
                out strError);
            if (bRet == true)
            {
                // 存在Session中
                SessionInfo sessionInfo = new SessionInfo();
                sessionInfo.UserName = model.UserName;
                sessionInfo.Password = model.Password;
                sessionInfo.Rights = strRight;
                Session[SessionInfo.C_Session_sessioninfo] = sessionInfo;

                // 返回来源界面
                if (String.IsNullOrEmpty(returnUrl) == false)
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }

            }


            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            ModelState.AddModelError("", strError);//"提供的用户名或密码不正确。");

            // 继续跟上登录成功返回的url
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        public ActionResult Logout()
        {
            // 将session置空
            Session[SessionInfo.C_Session_sessioninfo] = null;

            return RedirectToAction("Index", "Home");
        }
    }
}