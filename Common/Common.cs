using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace Crypto
{
    public class Common
    {
        public static string SubstringBetween(string text, string start, string end)
        {
            int startIndex = text.IndexOf(start) + start.Length;
            return text.Substring(startIndex, text.IndexOf(end) - startIndex);
        }

        //Chrome 59 on Windows 10
        public const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
        public static string WebRequest(string url, string userAgent, int timeout)
        {
            //
            System.Net.HttpWebRequest httpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            httpWebRequest.Timeout = timeout;
            httpWebRequest.UserAgent = userAgent;
            System.Net.HttpWebResponse httpWebResponse = (System.Net.HttpWebResponse)httpWebRequest.GetResponse();

            //
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            System.IO.StreamReader streamReader = new System.IO.StreamReader(httpWebResponse.GetResponseStream(), encoding);
            string content = streamReader.ReadToEnd();
            httpWebResponse.Close();
            streamReader.Close();
            content = content.Trim();
            
            //
            return content;
        }

        public static string FormatJson(string serialized)
        {
            serialized = serialized.Replace("[\n", "[");
            serialized = serialized.Replace("[    ", "[ ");
            serialized = serialized.Replace("[   ", "[ ");
            serialized = serialized.Replace("[  ", "[ ");
            serialized = serialized.Replace("\n        ],", " ],\n");
            serialized = serialized.Replace("\n      },", " },");
            serialized = serialized.Replace("\n\n", "\n");
            serialized = serialized.Replace("[    ", "[ ");
            return serialized;
        }

        //private static void ThreadEntryPoint()
        public static System.Threading.Thread StartThread(System.Threading.ThreadStart entryPoint)
        {
            System.Threading.Thread thread = new System.Threading.Thread(entryPoint);
            thread.IsBackground = true;
            thread.Start();
            //thread.Join();
            return thread;
        }

        public static string Shell(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            
            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            if (Configuration.IsLinux())
            {
                process.StartInfo.Environment["JAVA_HOME"] = "/usr/lib/jvm/default-java";
                process.StartInfo.EnvironmentVariables["JAVA_HOME"] = "/usr/lib/jvm/default-java";
            }

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Dispose();
            return result;
        }

        /*
            private static void MyHandler(object o, EventArgs e) {}
            ShellAsync("ps aux", MyHandler);
         */
        public static void ShellAsync(string cmd, EventHandler eventHandler)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            
            var process = new System.Diagnostics.Process()
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            if (Configuration.IsLinux())
            {
                process.StartInfo.Environment["JAVA_HOME"] = "/usr/lib/jvm/default-java";
                process.StartInfo.EnvironmentVariables["JAVA_HOME"] = "/usr/lib/jvm/default-java";
            }

            //http://community.bartdesmet.net/blogs/bart/archive/2006/08/30/4366.aspx
            process.Start();
            process.Exited += eventHandler;
        }

        public static string LoadFile(string path)
        {
            try
            {
                System.IO.StreamReader streamReader = new System.IO.StreamReader(path);
                string contents = streamReader.ReadToEnd();
                streamReader.Close();
                return contents;
            } catch {}

            return "";
        }

        /// <summary>
        /// Get the true IP of the client
        /// </summary>
        public static string GetClientIP(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            string result = "";

            //Get IP
            try
            {
                result = httpContext.Connection.RemoteIpAddress.ToString();
            }
            catch { }
            return result;
        }

        public static void SaveToFile(string filePath, string text)
        {
            try
            {
                bool append = false;
                System.IO.TextWriter textWriter = new System.IO.StreamWriter(filePath, append);
                textWriter.Write(text);
                textWriter.Close();
            }
            catch (Exception e)
            {
                Exception(e);
            }
        }

        public static void LogToFile(string filePath, string text)
        {
            try
            {
                bool append = true;
                System.IO.TextWriter textWriter = new System.IO.StreamWriter(filePath, append);
                textWriter.Write(text);
                textWriter.Close();
            }
            catch {}
        }

        private static bool LogEnabled = true;

        private static string LogFile
        {
            get
            {
                string defaultPath = @"/opt/LogFile.txt";
                string configPath = defaultPath;
                if (configPath != null && configPath.Length > 0)
                {
                    defaultPath = configPath;
                }
                string dateSuffix = System.DateTime.Today.ToString("yyyyMMdd");
                return defaultPath.Replace(".txt", dateSuffix + ".txt");
            }
        }
        private static string NewLine = Environment.NewLine;

        public static void Log(string log)
        {
            Console.WriteLine(log);
        }

        public static void Info(string info)
        {
            appendText(getTimeStamp() + " Info: " + info + " (" + getParentMethodName(1) + ")" + NewLine);
        }
        public static void Debug(string debug)
        {
            appendText(getTimeStamp() + " Debug: " + debug + " (" + getParentMethodName(1) + ")" + NewLine);
        }
        public static void Trace(string debug)
        {
            appendText(getTimeStamp() + " Trace: " + debug + " (" + getParentMethodName(1) + ")" + NewLine);
        }
        public static void Error(string error)
        {
            appendText(getTimeStamp() + " Error: " + error + " (" + getParentMethodName(1) + ")" + NewLine);
        }
        public static void Exception(System.Exception e)
        {
            appendText(getTimeStamp() + " Exception: " + "(" + getParentMethodName(1) + ") " + e.GetType().Name + " " + e.Message + NewLine + e.TargetSite + NewLine + e.Source + NewLine + e.StackTrace + NewLine);
            if (e.InnerException != null)
            {
                recursiveException(e.InnerException);
            }
        }

        private static void recursiveException(System.Exception e)
        {
            appendText("InnerException: " + e.Message + NewLine + e.TargetSite + NewLine + e.Source + NewLine + e.StackTrace + NewLine);
            if (e.InnerException != null)
            {
                recursiveException(e.InnerException);
            }
        }

        private static void appendText(string text)
        {
            if (!LogEnabled) return;

            try
            {
                bool append = true;
                System.IO.TextWriter textWriter = new System.IO.StreamWriter(LogFile, append);
                textWriter.Write(text);
                textWriter.Close();
            }
            catch (System.Exception e) { }
        }

        private static string getTimeStamp()
        {
            return System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private static string getParentMethodName(int level)
        {
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame stackFrame = stackTrace.GetFrame(level + 1);
            System.Reflection.MethodBase methodBase = stackFrame.GetMethod();
            string parentMethodName = methodBase.Name;
            return parentMethodName;
        }
    }
}
