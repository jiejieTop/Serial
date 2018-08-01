using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Ports;
using System.IO;

using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.DirectoryServices;
using System.Net.NetworkInformation;

using System.Text.RegularExpressions;  //提取IP时的正则
using NATUPNPLib;                      //Windows UPnP COM组件

using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace 恒温系统上位机
{
    public partial class Form1 : Form
    {
        public int PortName;
       
       public StringBuilder Note_StringBuilder;
 
        //---定义  
        private delegate void ShowReceiveMessageCallBack(string text);
        //---声明一个委托  
     
       public static Socket socket;

        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//必须手动添加事件处理程序
           
        }
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)//串口数据接收事件
        {
            try
            {
                if (!radioButton6.Checked)//如果接收模式为字符模式
                {
                    int ilen = serialPort1.BytesToRead;
                    byte[] bytes = new byte[ilen];
                    serialPort1.Read(bytes, 0, ilen);
                    string str = System.Text.Encoding.Default.GetString(bytes); //xx="中文";
                    if (time_flag == true)
                    {
                        textBox1.AppendText(GetTimeStamp() + " 温度: " + str + '\r' + '\n');//添加内容
                        textBox15.AppendText(str);
                    }
                    else
                        textBox1.AppendText(str + '\r' + '\n');//添加内容
                       
                }
                else
                { //如果接收模式为数值接收
                    byte data;
                    data = (byte)serialPort1.ReadByte();//此处需要强制类型转换，将(int)类型数据转换为(byte类型数据，不必考虑是否会丢失数据
                    string str = Convert.ToString(data, 16).ToUpper();//转换为大写十六进制字符串
                    textBox1.AppendText("0x" + (str.Length == 1 ? "0" + str : str) + " " + '\r'+'\n');//空位补“0”   
                }
            }
            catch
            {
                textBox1.AppendText("串口数据接收出错，请检查!\r\n");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "连接")
            {
                try
                {
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.Open();
                    button1.Text = "断开连接";
                    button2.Enabled = false;
                    panel2.Enabled = false;
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    textBox1.AppendText("串口已连接\r\n");
                }
                catch
                {
                    if (serialPort1.IsOpen)
                        serialPort1.Close();

                    button1.Text = "连接";
                    button2.Enabled = true;
                    comboBox1.Enabled = true;
                    panel2.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    textBox1.AppendText("请检查串口连接\r\n");
                }
            }
            else if (button1.Text == "断开连接")
            {
                try
                {
                    serialPort1.Close();
                    button1.Text = "连接";
                    button2.Enabled = true;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    panel2.Enabled = true;
                    comboBox4.Enabled = true;
                    textBox1.AppendText("串口已断开\r\n");
                }
                catch { }
            }
        }

  
        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.AppendText("开始自动配置串口\r\n");//出错提示
            textBox1.AppendText("串口扫描\r\n");//出错提示
          
            textBox1.AppendText("端口扫描完毕\r\n");//出错提示
            textBox1.AppendText("正在配置波特率\r\n");//出错提示
            comboBox2.Text = comboBox4.Text;
            serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
          
            textBox1.AppendText("自动配置完成\r\n");//出错提示
            button1_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox3.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
           // string[] STR_DATA1;
          //  STR_DATA1 = System.IO.File.ReadAllLines(@"F:\Visual_Studio\恒温控制系统上位机\恒温系统上位机\数据保存\接收数据保存.txt");
          //  string myStr = string.Join("", STR_DATA1);
          //     textBox1.AppendText(myStr);
            System.IO.File.WriteAllText(@"F:\Visual_Studio\恒温控制系统上位机\恒温系统上位机\数据保存\接收数据保存.txt", textBox1.Text);
            System.IO.File.WriteAllText(@"F:\Visual_Studio\恒温控制系统上位机\恒温系统上位机\数据保存\发送数据保存.txt", textBox3.Text);
            textBox2.AppendText(" 数据保存完成!!! \r\n");
          
        }

        private void button6_Click(object sender, EventArgs e)
        {
            byte[] Data = new byte[1];//作用同上集
            if (serialPort1.IsOpen)//判断串口是否打开，如果打开执行下一步操作
            {
                try
                {
                    if (textBox3.Text != "")
                    {
                       // if(textBox3.Text != comboBox7.Text)         //与设置温度不等
                       //     textBox3.Text = comboBox7.Text;
                         if (!radioButton6.Checked)//如果发送模式是字符模式
                          {
                              try
                              {//实现串口发送汉字
                                  Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
                                  byte[] bytes = gb.GetBytes(textBox3.Text);
                                  serialPort1.Write(bytes, 0, bytes.Length);
                                textBox2.Text = "你发送的数据为：" + textBox3.Text;
                              }
                              catch
                              {
                                  textBox1.AppendText("串口数据写入错误\r\n");//出错提示
                                  serialPort1.Close();
                                  button1_Click(sender, e);
                             }
                         }
                          else
                            {
                        for (int i = 0; i < (textBox3.Text.Length - textBox3.Text.Length % 2) / 2; i++)//取余3运算作用是防止用户输入的字符为奇数个
                            {
                                Data[0] = Convert.ToByte(textBox3.Text.Substring(i * 2, 2), 16);
                                serialPort1.Write(Data, 0, 1);  //循环发送（如果输入字符为0A0BB,则只发送0A,0B）
                            }
                            if (textBox3.Text.Length % 2 != 0)//剩下一位单独处理
                            {
                                Data[0] = Convert.ToByte(textBox3.Text.Substring(textBox3.Text.Length - 1, 1), 16);//单独发送B（0B）
                                serialPort1.Write(Data, 0, 1);//发送
                            }
                            textBox2.Text = "你发送的数据为：" + textBox3.Text;
                        }
                    }
                   // else
                 //   {
                  //      textBox3.Text = comboBox7.Text;
                     //   
                    //    for (int i = 0; i < (textBox3.Text.Length - textBox3.Text.Length % 2) / 2; i++)//取余3运算作用是防止用户输入的字符为奇数个
                    //    {
                   //         Data[0] = Convert.ToByte(textBox3.Text.Substring(i * 2, 2), 16);
                   //         serialPort1.Write(Data, 0, 1);//循环发送（如果输入字符为0A0BB,则只发送0A,0B）
                   //     }
                   //     if (textBox3.Text.Length % 2 != 0)//剩下一位单独处理
                   //     {
                    //        Data[0] = Convert.ToByte(textBox3.Text.Substring(textBox3.Text.Length - 1, 1), 16);//单独发送B（0B）
                   //         serialPort1.Write(Data, 0, 1);//发送
                   //     }
                  //  }


                }
                catch
                {
                    textBox1.AppendText("串口数据写入错误\r\n");//出错提示
                }
            }

            else
            {
                textBox1.AppendText("请检查串口连接是否错误\r\n");//出错提示
            }


        }

      
      
        //---添加接受消息到列表的委托的方法  
        private void AddMessageToList(string text)
        {
            textBox1.AppendText(text);
        }

        //监听数据
 
      
     
    
     
      
        private void button7_Click(object sender, EventArgs e)
        {
        }

        private void sortProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            textBox1.AppendText("\r\n");
            if (!String.IsNullOrEmpty(e.Data))
            {
                this.BeginInvoke(new Action(() => { textBox1.AppendText("\r\n" + e.Data + "\r\n"); }));
            }
            textBox1.AppendText("\r\n");
        }
        private void ExcuteDosCommand(string cmd)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = "cmd";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.OutputDataReceived += new DataReceivedEventHandler(sortProcess_OutputDataReceived);
                p.Start();
                StreamWriter cmdWriter = p.StandardInput;
                p.BeginOutputReadLine();
                if (!String.IsNullOrEmpty(cmd))
                {
                    cmdWriter.WriteLine(cmd);
                }
                cmdWriter.Close();
                p.WaitForExit();
                p.Close();
            }
            catch
            {
                MessageBox.Show("执行命令失败，请检查输入的命令是否正确！");
            }
        }


      
      
    
        private void button13_Click(object sender, EventArgs e)
        {
            if (button13.Text == "开启自动")
            {
                button13.Text = "关闭自动";
                timer1.Start();
                time_flag = true;
            }
            else
            {
                button13.Text = "开启自动";
                timer1.Stop();
                time_flag = false;
            }
        }

        int time_out=0;
        int[] myIntArray4 = new int[20];
        bool time_flag=false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (time_out == 0)
            {
                time_out = Convert.ToInt16(comboBox5.Text);

                if (radioButton8.Checked)
                {
                    try
                    {//实现串口发送汉字
                        textBox15.Clear();
                        Encoding gb = System.Text.Encoding.GetEncoding("gb2312");
                        //byte[] bytes = gb.GetBytes("%");
                        //serialPort1.Write(bytes, 0, bytes.Length);
                    }
                    catch
                    {
                        textBox1.AppendText("串口数据写入错误\r\n");//出错提示
                        serialPort1.Close();
                        button1_Click(sender, e);
                    }
                }
            }
            if (time_out == 1)
            {
                time_out --;
                chart1.Series.Clear();
                chart1.ChartAreas[0].AxisX.Maximum = 20;//设定x轴的最大值
                chart1.ChartAreas[0].AxisY.Maximum = 100;//设定y轴的最大值

                chart1.ChartAreas[0].AxisX.Minimum = 0;//设定x轴的最小值
                chart1.ChartAreas[0].AxisY.Minimum =-10;//设定y轴的最小值

                //第一条数据
                Series series = new Series("实时温度");
                series.ChartType = SeriesChartType.Spline;  //设置为曲线模式
                series.BorderWidth = 1;
                series.ShadowOffset = 1;             // Populate new series with data    

                for (int i = 0; i < 19; i++)
                {
                    myIntArray4[i] = myIntArray4[i + 1];
                         series.Points.AddY(myIntArray4[i]);//画线
                }

                try
                {
                    string s =textBox15.Text;
                    string str = s.Remove(0, 3);
                    str = str.Remove(2, str.Length - 2);
                    // textBox13.Text = textBox15.Text.Substring(textBox15.Text.IndexOf("温度：") + 2, textBox15.Text.IndexOf("温度：") + 4);
                    textBox13.Text = str;
                    myIntArray4[19] = Convert.ToInt32(textBox13.Text);
                }
                catch
                {
                    myIntArray4[19] = 0;
                }
                series.Points.AddY(myIntArray4[19]);

                chart1.Series.Add(series);
            }
            else time_out--;
        }
        private void button7_Click_1(object sender, EventArgs e)
        {
            if (button7.Text == "开启自动")
            {
                button7.Text = "关闭自动";
                timer2.Start();
            }
            else
            {
                button7.Text = "开启自动";
                timer2.Stop();
                time_1ms = 0;
               // time_num = 0;
            }
        }
        float time_1ms = 0;
      //  int time_num = 0;
        private void timer2_Tick(object sender, EventArgs e)
        {

            if (time_1ms == (Convert.ToSingle(comboBox6.Text) * 10))
            {
               // time_1ms = 0;
                System.IO.File.WriteAllText(@"F:\Visual_Studio\恒温控制系统上位机\恒温系统上位机\数据保存\接收数据保存.txt", textBox1.Text);
                System.IO.File.WriteAllText(@"F:\Visual_Studio\恒温控制系统上位机\恒温系统上位机\数据保存\发送数据保存.txt", textBox3.Text);
                textBox2.Clear();
                textBox2.AppendText(" 数据保存完成!!! \r\n");

                //   time_num++;
                //   textBox1.Text = Convert.ToString(time_num);
            }
            else
            {
                time_1ms++;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*DialogResult result;
             result = MessageBox.Show("确定退出吗？", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
             if (result == DialogResult.OK)
             {

                 Application.ExitThread();
             }
             else
             {
                 e.Cancel = true;
             }*/
            Process.GetCurrentProcess().Kill();//彻底关闭软件
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        public static string GetTimeStamp()
        {
           // TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
          //  return Convert.ToInt64(ts.TotalSeconds).ToString();

            //   DateTime.Now.ToString();            // 2008-9-4 20:02:10
            return DateTime.Now.ToLocalTime().ToString();        // 2008-9-4 20:12:12
        }
        private void button8_Click(object sender, EventArgs e)
        {
            byte[] Data = new byte[1];//作用同上集
            if (serialPort1.IsOpen)//判断串口是否打开，如果打开执行下一步操作
            {
                try
                {
                    
                    //设置温度十位
                    for (int j = 0; j < (comboBox7.Text.Length - comboBox7.Text.Length % 2) / 2; j++)//取余3运算作用是防止用户输入的字符为奇数个
                    {
                        Data[0] = Convert.ToByte(comboBox7.Text.Substring(j * 2, 2), 16);
                        serialPort1.Write(Data, 0, 1);  //循环发送（如果输入字符为0A0BB,则只发送0A,0B）
                    }
                    if (comboBox7.Text.Length % 2 != 0)//剩下一位单独处理
                    {
                        Data[0] = Convert.ToByte(comboBox7.Text.Substring(comboBox7.Text.Length - 1, 1), 16);//单独发送B（0B）
                        serialPort1.Write(Data, 0, 1);//发送
                    }
                    //设置温度个位
                    for (int j = 0; j < (comboBox8.Text.Length - comboBox8.Text.Length % 2) / 2; j++)//取余3运算作用是防止用户输入的字符为奇数个
                    {
                        Data[0] = Convert.ToByte(comboBox8.Text.Substring(j * 2, 2), 16);
                        serialPort1.Write(Data, 0, 1);  //循环发送（如果输入字符为0A0BB,则只发送0A,0B）
                    }
                    if (comboBox8.Text.Length % 2 != 0)//剩下一位单独处理
                    {
                        Data[0] = Convert.ToByte(comboBox8.Text.Substring(comboBox8.Text.Length - 1, 1), 16);//单独发送B（0B）
                        serialPort1.Write(Data, 0, 1);//发送
                    }

                    textBox2.Text = "你设置的温度为：" + comboBox7.Text + comboBox8.Text +"度!!";
                }
                catch
                {
                    textBox1.AppendText("串口数据写入错误\r\n");//出错提示
                }
            }
            else
            {
                textBox1.AppendText("请检查串口连接是否错误\r\n");//出错提示
            }
        }

    }

}
