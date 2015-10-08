using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;

namespace SuperDownloader2
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private DataTable mytable = new DataTable();//保存下载设置
        private string timeStr = "";//保存下载时间节点设置
        private int hour_now = -1;//当前小时
        private int hour_pre = -1;
        bool SettingOK = false;//下载设置是否完成
        Thread myThread;
        


        bool[] DownLoadMark = new bool[100];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer2.Start();
            hour_now = DateTime.Now.Hour;
            Control.CheckForIllegalCrossThreadCalls = false;　
            myThread = new Thread(new ThreadStart(DownData));
        }

        //检查数据
        private string CheckInput()
        {
            if (mytable.Rows.Count == 0)
                return "无资料下载地址及保存目录设置！";

            for (int i = 0; i < mytable.Rows.Count; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (!(mytable.Rows[i][j].ToString().Length > 0))//如果有单元格值为空
                    {
                        return "行" + (i + 1) + "列" + (j + 1) + "：未输入";
                    }

                    if (j == 0)
                    {
                        if (!Directory.Exists(mytable.Rows[i][j].ToString()))
                        {
                            return "行" + (i + 1) + "列" + (j + 1) + mytable.Rows[i][i].ToString() + "：该目录不存在";
                        }
                    }
                }
            }

            return "OK";
        }

        //读取数据1
        private DataTable ReadSaveSetting(string saveFile)
        {
            StreamReader sr = new StreamReader(saveFile);//,Encoding.GetEncoding("gb2312"));
            DataTable myDt = new DataTable();
            myDt.Columns.Add();
            myDt.Columns.Add();

            string str;
            string[] str_;

            while (true)
            {
                str = sr.ReadLine();
                if (str == null)
                    break;
                str_ = str.Split('\t');
                if (str_.Length >= 2)
                    myDt.Rows.Add(str_[0], str_[1]);
            }
            sr.Close();

            return myDt;
        }

        //读取数据2
        private string ReadTimeSetting(string saveFile)
        {
            StreamReader sr = new StreamReader(saveFile);
            string str;
            str = sr.ReadLine();
            sr.Close();
            return str;
        }

        //初始设置
        private bool init()
        {            
            if (File.Exists("SaveSetting.txt"))
            {
                mytable = ReadSaveSetting("SaveSetting.txt");
                for (int i = 0; i < mytable.Rows.Count; i++)
                    DownLoadMark[i] = false;
            }
            else
            {
                MessageBox.Show("请设置文件链接及保存目录！");
                return false;
            }


            if (File.Exists("TimeSetting.txt"))
                timeStr = ReadTimeSetting("TimeSetting.txt");
            else
            {
                MessageBox.Show("请设置下载时间！");
                return false;
            }

            //检查设置数据
            string Check_result = CheckInput();
            if (Check_result == "OK")
            {
                SettingOK = true;
                return true;
            }
            else
            {
                MessageBox.Show(Check_result + "\t\n请检查设置！");
                return false;
            }
        }


        //下载链接
        private string DownloadURL(string address, string path, string filename)
        {
            WebClient client = new WebClient();
            string URLAddress = address;
            string receivePath = path;
            try
            {
                if (!File.Exists(receivePath + "\\" + filename))
                {
                    client.DownloadFile(URLAddress, receivePath + "\\" + filename);
                    return "OK";
                }
                else
                {
                    return "OK";
                }
            }
            catch (Exception errorMSG)
            {
                return errorMSG + "\n";
            }

        }

        //计算倒计时
        private string GetTimeRemain()
        {
            string result;
            int timeNext = -1;

            for (int i = hour_now + 1; i < timeStr.Length; i++)
            {
                if (timeStr[i] == '1')
                {
                    timeNext = i;
                    break;
                }
            }

            if (timeNext == -1)
            {
                for (int i = 0; i < hour_now; i++)
                {
                    if (timeStr[i] == '1')
                    {
                        timeNext = i + 24;
                        break;
                    }
                }
            }

            int now = DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second;
            int next = timeNext * 3600;
            int dur = next - now;

            int hour_remain = dur / 3600;
            int min_remain = (dur % 3600) / 60;
            int sec_remain = (dur % 3600) % 60;
            result = hour_remain + "时" + min_remain + "分" + sec_remain + "秒";

            return result;
        }

        //下载
        private void DownData()
        {
            bool flag = false;
            for (int i = 0; i < mytable.Rows.Count; i++)
            {
                if (!DownLoadMark[i])
                {
                    flag = true;
                    richTextBox1.Text += "正在下载" + DateTime.Now.ToLongDateString() + " " + hour_now + "时次：" + Path.GetFileName(mytable.Rows[i][0].ToString()) + "···\n";
                    string fileNAM = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + DateTime.Now.Hour.ToString("00") + ".png";
                    string result = DownloadURL(mytable.Rows[i][1].ToString(), mytable.Rows[i][0].ToString(), fileNAM);

                    //显示下载信息
                    if (result == "OK")
                    {
                        FileInfo inf = new FileInfo(mytable.Rows[i][0].ToString() + "\\" + fileNAM);
                        double filelength = inf.Length / 1024.0;
                        if (filelength > 20)//如果文件过小，则认为文件出错
                        {
                            richTextBox1.Text += "下载完成！\n";
                            DownLoadMark[i] = true;
                        }
                        else
                        {
                            File.Delete(mytable.Rows[i][0].ToString() + "\\" + fileNAM);
                            richTextBox1.Text += "下载文件错误，10分钟后再次尝试下载！\n";
                        }
                    }
                    else
                        richTextBox1.Text += "下载失败,请检查链接或网络是否可用！\n" + result;
                    
                    richTextBox1.Select(richTextBox1.TextLength, 0);
                    richTextBox1.ScrollToCaret();
                    richTextBox1.Focus();
                }
                else
                    continue;
            }
            if (flag)
                richTextBox1.Text += "本次下载结束！如有失败下载，10分钟后会再次尝试下载\n--------------------------------------------------------\n";
            
            richTextBox1.Select(richTextBox1.TextLength, 0);
            richTextBox1.ScrollToCaret();
            richTextBox1.Focus();
        }



        //启动
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //timer2.Start();
            richTextBox1.Text += "开始……\n" + DateTime.Now.ToLongTimeString() + "\n--------------------------------------------------------\n";
            if (init())
            {
                //开始下载
                timer1.Start();

                simpleButton1.Enabled = false;
                simpleButton2.Enabled = true;
            }
        }

        //停止
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            simpleButton2.Enabled = false;
            simpleButton1.Enabled = true;

            timer1.Stop();
            timer1.Interval = 100;
            if (myThread.IsAlive)
                myThread.Abort();
            richTextBox1.Text += "下载已停止，如继续下载请点击开始按钮\n";
        }

        //设置
        private void simpleButton3_Click(object sender, EventArgs e)
        {
            Form2 Frm_setting = new Form2();
            Frm_setting.ShowDialog();
            if (globalPrams.SettingReset == true)
            {
                init();
                globalPrams.SettingReset = false;
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //下载
            if (timeStr[hour_now] == '1')
            {
                for (int i = 0; i < mytable.Rows.Count; i++)
                {
                    if (DownLoadMark[i] == false)
                    {
                        myThread = new Thread(new ThreadStart(DownData));
                        myThread.Start();
                        break;
                    }
                }
            }
            timer1.Interval = 60000;
        }

        //更新当前时间和倒计时
        private void timer2_Tick(object sender, EventArgs e)
        {
            hour_now = DateTime.Now.Hour;
            //状态栏时间提示
            toolStripStatusLabel1.Text = "时间：" + DateTime.Now.ToLongTimeString();

            if (SettingOK)
            {
                //状态栏下载倒计时
                toolStripStatusLabel2.Text = "距下次下载还有：" + GetTimeRemain();

                //当到达指定下载时间点后，将已下载标记组（DownLoadMark）全部归零（false）
                if (timeStr[hour_now] == '1')
                {
                    if (hour_pre != hour_now)
                    {
                        for (int i = 0; i < mytable.Rows.Count; i++)
                            DownLoadMark[i] = false;
                        hour_pre = hour_now;
                        timer1.Interval = 100;
                        timer1.Start();
                    }
                }
            }
        }






    }
}
