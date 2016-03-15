using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraEditors;

namespace SuperDownloader2
{
    public partial class Form2 : Form
    {
        //用来保存输出目录和链接地址数据;
        private DataTable mytable = new DataTable();
        private string timeStr = "";

        public Form2()
        {
            InitializeComponent();
        }


        private void Form2_Load(object sender, EventArgs e)
        {
            if (File.Exists("TimeSetting.txt"))
            {
                timeStr = ReadTimeSetting("TimeSetting.txt");
                //初始化group2
                for (int i = 0; i < timeStr.Length; i++)
                {
                    if (timeStr[i] == '1')
                        ((CheckEdit)(groupControl2.Controls[groupControl2.Controls.Count - 1 - i])).Checked = true;
                    else if (timeStr[i] == '0')
                        ((CheckEdit)(groupControl2.Controls[groupControl2.Controls.Count - 1 - i])).Checked = false;
                }
            }

            gridView1.OptionsView.NewItemRowPosition = DevExpress.XtraGrid.Views.Grid.NewItemRowPosition.Bottom;
            if (File.Exists(Application.StartupPath+"\\SaveSetting.txt"))
                mytable = ReadSaveSetting(Application.StartupPath+"\\SaveSetting.txt");

            gridControl1.DataSource = mytable;
            gridView1.PopulateColumns();//使数据显示

            //显示列头图标和名称
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            gridView1.Columns[0].Caption = "保存目录";
            gridView1.Columns[0].Image = ((System.Drawing.Image)(resources.GetObject("savePath.Image")));
            gridView1.Columns[1].Caption = "下载地址";
            gridView1.Columns[1].Image = ((System.Drawing.Image)(resources.GetObject("address.Image")));

        }

        //显示行头序号
        private void gridView1_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            e.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            if (e.Info.IsRowIndicator)
            {
                if (e.RowHandle >= 0)
                {
                    e.Info.DisplayText = (e.RowHandle + 1).ToString();
                }
                else if (e.RowHandle < 0 && e.RowHandle > -1000)
                {
                    e.Info.Appearance.BackColor = System.Drawing.Color.AntiqueWhite;
                    e.Info.DisplayText = "G" + e.RowHandle.ToString();
                }
            }
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

        //检查数据
        private string CheckInput()
        {
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
                            try
                            {
                                Directory.CreateDirectory(mytable.Rows[i][j].ToString());
                            }
                            catch
                            {
                                return "行" + (i + 1) + "列" + (j + 1) + mytable.Rows[i][i].ToString() + "：该目录不存在";
                            }
                        }
                    }
                }
            }

            return "OK";
        }


        //保存数据
        private void SaveData(DataTable myDT, string dataFile, string timestring, string timeFile)
        {
            //保存路径设置数据
            StreamWriter sw_data = new StreamWriter(dataFile);
            for (int i = 0; i < myDT.Rows.Count; i++)
                sw_data.WriteLine(myDT.Rows[i][0] + "\t" + myDT.Rows[i][1]);
            sw_data.Close();
            
            //保存下载时间设置数据
            StreamWriter sw_time = new StreamWriter(timeFile);
            sw_time.WriteLine(timestring);
            sw_time.Close();
        }

        //确定
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //检查数据
            string checkResult = CheckInput() ;
            if (checkResult == "OK")
            {
                //保存
                timeStr = "";
                for (int i = 0; i < groupControl2.Controls.Count; i++)
                {
                    CheckEdit myCheck = (CheckEdit)(groupControl2.Controls[groupControl2.Controls.Count - 1 - i]);
                    if (myCheck.Checked)
                        timeStr += "1";
                    else
                        timeStr += "0";
                }
                SaveData(mytable, Application.StartupPath + "\\SaveSetting.txt", timeStr, Application.StartupPath + "\\TimeSetting.txt");
                try
                {
                    File.Copy(Application.StartupPath + "\\SaveSetting.txt", "C:\\Windows\\System32\\SaveSetting.txt");
                    File.Copy(Application.StartupPath + "\\TimeSetting.txt", "C:\\Windows\\System32\\TimeSetting.txt");
                }
                catch
                {
                    ;
                }

                globalPrams.SettingReset = true;
                MessageBox.Show("保存成功！");
                this.Close();

            }
            else
                MessageBox.Show(checkResult);
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
