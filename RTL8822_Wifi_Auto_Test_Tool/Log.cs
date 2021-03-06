using RTKModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RTL8822_Wifi_Auto_Test_Tool
{
    public class Log : ILog
    {
        private StringBuilder sb;

        public Log(int defaultBuffer = 1024 * 1024 * 1)
        {
            sb = new StringBuilder(defaultBuffer);    
        }

        public string Read()
        {
            lock (sb)
                return sb.ToString();
        }

        public void Write(string text)
        {
            lock (sb)
                sb.Append(text);
        }

        public void WriteLine(string text)
        {
            lock (sb)
                sb.AppendLine(text);
        }

        public void Clear()
        {
            lock (sb)
                sb.Length = 0;
        }

        public void AppendFile(string path, string content)
        {
            
        }

        public void Save(string path)
        {
            lock (sb)
                File.WriteAllText(path, sb.ToString());
        }
    }
}
