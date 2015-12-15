using dp2CirculationWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dp2CirculationWeb.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(LoginModel model)
        {
            bool bRet = true;
            // 进行登录校验

            //进行登录

            // 存在Session中
            Session["AcountName"] = model.UserName;

            if (bRet == true)
            {
                return RedirectToAction("Index", "Home");
            }


            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            ModelState.AddModelError("", "提供的用户名或密码不正确。");
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        public ActionResult LogOff()
        {
            //WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }
    }
}