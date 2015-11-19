using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

//using Ionic.Zip;

//using DigitalPlatform;
//using DigitalPlatform.CirculationClient;
//using DigitalPlatform.IO;
//using DigitalPlatform.Range;
//using DigitalPlatform.Text;
//using DigitalPlatform.Xml;
//using DigitalPlatform.CirculationClient.localhost;
using System.Diagnostics;
using DigitalPlatform.Text;

namespace dp2ConsoleToWeiXin
{
    /// <summary>
    /// 一个实例
    /// </summary>
    public class Instance : IDisposable
    {
       

        /// <summary>
        /// 构造函数
        /// </summary>
        public Instance()
        {
            
        }


 
        // return:
        //      false   正常，继续
        //      true    退出命令
        public bool ProcessCommand(string line)
        {
            if (line == "exit" || line == "quit")
                return true;

            string strError = "";
            int nRet = 0;

            List<string> parameters = ParseParameters(line);
            if (parameters.Count == 0)
                return false;

            if (parameters[0] == "search")
            {
                Console.WriteLine("search");
                return false;
            }
            if (parameters[0] == "myinfo")
            {                

                Console.WriteLine("myinfo");


                return false;
            }


            Console.WriteLine("unknown command '" + line + "'");
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
