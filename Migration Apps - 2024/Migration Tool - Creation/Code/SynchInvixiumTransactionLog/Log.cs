using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SynchInvixiumTransactionLog
{
    public class Log
    {
        public bool IsInitialized;
        public bool Logfile;
        private string _filename = string.Empty;

        public void Error(Exception exception)
        {
            if (!IsInitialized) return;
            if (!Logfile) return;
            try
            {
                var fs = new FileStream(_filename, FileMode.Append, FileAccess.Write);
                var sr = new StreamWriter(fs);
                string ndate = string.Format("[ERROR] {0:yyyy-MM-dd HH:mm:ss.ff}", DateTime.Now);
                sr.WriteLine(ndate + " : " + GetFormattedException(exception, true));
                sr.Flush();
                sr.Close();
            }
            catch (Exception)
            {
                IsInitialized = false;
            }
        }

        private string GetFormattedException(Exception ex, bool isRecursive)
        {
            StringBuilder exceptionString = new StringBuilder();

            if (isRecursive)
                exceptionString.AppendFormat("************ Inner Exception************{0}", Environment.NewLine);
            else
                exceptionString.AppendFormat("*************** Exception***************{0}", Environment.NewLine);

            exceptionString.AppendFormat("Exception Message:{0}{1}{2}", Environment.NewLine, ex.Message, Environment.NewLine);
            exceptionString.AppendFormat("Stack Trace:{0}{1}{2}", Environment.NewLine, ex.StackTrace, Environment.NewLine);
            exceptionString.AppendFormat("Source:{0}{1}{2}", Environment.NewLine, ex.Source, Environment.NewLine);

            //recurse into inner exceptions
            if (ex.InnerException != null)
            {
                exceptionString.Append(String.Format("{0}{1}", GetFormattedException(ex.InnerException, true), Environment.NewLine));
            }
            return exceptionString.ToString();
        }

        public void Error(string strLog)
        {
            if (!IsInitialized) return;
            if (!Logfile) return;
            try
            {
                var fs = new FileStream(_filename, FileMode.Append, FileAccess.Write);
                var sr = new StreamWriter(fs);
                string ndate = string.Format("[ERROR] {0:yyyy-MM-dd HH:mm:ss.ff}", DateTime.Now);
                sr.WriteLine(ndate + " : " + strLog);
                sr.Flush();
                sr.Close();
            }
            catch (Exception)
            {
                IsInitialized = false;
            }
        }

        public void Info(string strLog)
        {
            if (!IsInitialized) return;
            if (!Logfile) return;
            try
            {
                var fs = new FileStream(_filename, FileMode.Append, FileAccess.Write);
                var sr = new StreamWriter(fs);
                string ndate = string.Format("[INFO] {0:yyyy-MM-dd HH:mm:ss.ff}", DateTime.Now);
                sr.WriteLine(ndate + " : " + strLog);
                sr.Flush();
                sr.Close();
            }
            catch (Exception)
            {
                IsInitialized = false;
            }
        }

        public void InitializeLog()
        {
            // this.Logfile = true;
            var currentassembly = System.Reflection.Assembly.GetEntryAssembly();
            this.InitializeLog(currentassembly.ManifestModule.ScopeName);
        }

        public void InitializeLog(string activityname)
        {
            if (IsInitialized) return;
            if (!Logfile) return;
            try
            {
                _filename = string.Format(@"C:\logs\" + activityname + "_{0:yyyy-MM-dd HH-mm-ss-ff}.log", DateTime.Now);

                var fs = new FileStream(_filename, FileMode.Create, FileAccess.Write);
                var sr = new StreamWriter(fs);
                sr.Flush();
                sr.Close();
                IsInitialized = true;
            }
            catch (Exception) { }
        }
    }
}
