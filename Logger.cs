using System;
using System.IO;

namespace GothicSaveEditor
{
    static class Logger
    {
        static readonly object locker = new object();
        static StreamWriter sw;

        static int loggedCount = 1;

        static Logger()
        {
            try
            {
                sw = new StreamWriter(@"SaveEditor.log", false);
            }
            catch { }
        }

        public static void Log(Exception ex)
        {
            try
            {
                lock (locker)
                {
                    sw.WriteLine($"=====message {loggedCount}=====");
                    while (ex != null)
                    {
                        sw.WriteLine(DateTime.Now.ToString());
                        sw.WriteLine(ex.Message);
                        sw.WriteLine(ex.StackTrace);
                        ex = ex.InnerException;
                    }
                    sw.WriteLine($"=====message {loggedCount++}=====");
                    sw.WriteLine();
                    sw.Flush();
                }
            }
            catch { }
        }

        public static void Log(string msg)
        {
            try
            {
                lock (locker)
                {
                    sw.WriteLine($"=====message {loggedCount}=====");
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(msg);
                    sw.WriteLine();
                    sw.Flush();
                    sw.WriteLine($"=====message {loggedCount++}=====");
                }
            }
            catch { }
        }
    }
}
