using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Cache;
using System.IO;
using System.Threading;

namespace CS_HttpWebResponse_chunked_thread_Timer_show_data
{
    public partial class Form1 : Form
    {
        public static string m_StrNewData;
        public static string m_StrOldData;
        public Thread m_Thread;
        public int m_intwaitcount;
        public static void Thread_fun(object arg)
        {
            RESTfulAPI_getchunked("http://192.168.1.196:24410/syris/sydm/events?events_type=0");
        }
        public static void RESTfulAPI_getchunked(String url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                //request.SendChunked = true;
                //--
                //定義此req的緩存策略
                //https://msdn.microsoft.com/zh-tw/library/system.net.webrequest.cachepolicy(v=vs.110).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-1
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                request.CachePolicy = noCachePolicy;
                //--
                request.Method = "GET";//request.Method = "POST";
                //request.ContentType = "application/x-www-form-urlencoded";


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string encoding = response.ContentEncoding;

                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8"; //預設編碼
                }

                //---
                //add 2017/12/20
                //Reading “chunked” response with HttpWebResponse - https://stackoverflow.com/questions/16998/reading-chunked-response-with-httpwebresponse
                StringBuilder sb = new StringBuilder();
                Byte[] buf = new byte[8192];
                Stream resStream = response.GetResponseStream();
                string tmpString = null;
                int count = 0;
                do
                {
                    count = resStream.Read(buf, 0, buf.Length);
                    if (count != 0)
                    {
                        tmpString = Encoding.UTF8.GetString(buf, 0, count);
                        m_StrNewData = tmpString;
                    }
                    Thread.Sleep(10);
                } while (count >= 0);

                response.Close();
            }
            catch
            {

            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_StrNewData = "wait...";
            m_StrOldData = "";

            timer1.Interval = 100;
            timer1.Enabled = true;

            m_Thread = new Thread(Thread_fun);
            m_Thread.IsBackground = true;
            m_Thread.Start();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (m_Thread.IsAlive)
            {
                if (m_StrNewData != m_StrOldData)
                {
                    m_StrOldData = m_StrNewData;
                    richTextBox1.AppendText(m_StrOldData);
                }
                m_intwaitcount = 0;
            }
            else
            {
                m_intwaitcount++;
                if (m_intwaitcount == 600)
                {
                    m_Thread = null;
                    m_Thread = new Thread(Thread_fun);
                    m_Thread.IsBackground = true;
                    m_Thread.Start();
                    richTextBox1.Text = "restart thread..." + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                }
                else
                {
                    richTextBox1.Text = "wait..." + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_Thread.IsAlive)//關閉執行序
            {
                timer1.Enabled = false;
                m_Thread.Abort();
            }
            
        }
    }
}
