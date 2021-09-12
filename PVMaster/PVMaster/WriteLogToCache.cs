using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVMaster
{
    class WriteLogToCache
    {
        private static WriteLogToCache instance;
        public static WriteLogToCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WriteLogToCache();
                }
                return WriteLogToCache.instance;
            }

            private set { WriteLogToCache.instance = value; }
        }
        public void WriteToCache(string txt, string localID)
        {
            try
            {
                string Filepath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "//inuseCache//" + localID + System.Windows.Forms.SystemInformation.ComputerName + ".txt";
                if (File.Exists(Filepath)) // If file exists
                {
                    using (StreamWriter sw = File.AppendText(Filepath))
                    {
                        sw.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " " + txt); // Write text to .txt file
                    }
                }               
            }
            catch (Exception)
            {

            }
        }      

    }
}
