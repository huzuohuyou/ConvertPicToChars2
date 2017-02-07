using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace ConvertPicToChars2
{
    public partial class MainForm : Form
    {
        Bitmap bitmap;
        string curFileName;
        int step = 5;
        int max = 0;
        int level = 0;
        string imagesDir = Application.StartupPath + "\\frames";
        char[] array =
            //{ '#','M','X', 'B','A',
            //             'G','H','A', 'R','K',
            //             'N','S','Q', 'd','h',
            //             'p','b','x', 's','r',
            //             'o','i',';', ':','.',
            //             ' ' };
         { '#','&','$', '*','o','!',';',' ' };

        public MainForm()
        {
            InitializeComponent();
            button1.Click += open_Click;
            button2.Click += convert_Click;
            button3.Click += close_Click;
            button4.Click += output_Click;
            button5.Click += gif_Click;
            button6.Click += run_Click;
            button7.Click += show_Click;
        }

        public delegate void InitItemInvoke(string str);

        void run_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(new ThreadStart(DoWord));
            thread.Start();
        }

        private void DoWord()
        {
            Thread.Sleep(41);
            InitItemInvoke mi = new InitItemInvoke(SetImage);
            DirectoryInfo di = new DirectoryInfo(imagesDir);
            FileInfo[] fi = di.GetFiles();
            foreach (FileInfo item in fi)
            {
                BeginInvoke(mi, new object[] { item.FullName });
            }
            
        }

        private void SetImage(string str)
        {
            pbconverted.Image = null;
            Bitmap bitmap = new Bitmap(str);
            GetMax(bitmap);
            pbconverted.Image = bitmap;
            bitmap = Convert2(bitmap);
            richTextBox1.Text = GetContent(bitmap);
        }

        void show_Click(object sender, EventArgs e) {
            richTextBox1.Text = GetContent(this.bitmap);
        }
        void gif_Click(object sender, EventArgs e)
        {
            OpenFileDialog opnDlg = new OpenFileDialog();//创建OpenFileDialog对象
            //为图像选择一个筛选器
            opnDlg.Filter = "gif|*.gif";
            opnDlg.Title = "打开gif文件";
            opnDlg.ShowHelp = true;//启动帮助按钮
            if (opnDlg.ShowDialog() == DialogResult.OK)
            {
                curFileName = opnDlg.FileName;
                bitmap = new Bitmap(curFileName);
                pb_origin.Image = bitmap;
                ImageHelper.GetFrames(curFileName, imagesDir);
            }
        }

        void Convert(string path)
        {
            bitmap = new Bitmap(path);
            Convert2(bitmap);
        }

        Bitmap Convert2(Bitmap bitmap)
        {
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    //读取当前像素的RGB颜色值
                    Color curColor = bitmap.GetPixel(i, j);
                    //利用公式计算灰度值（加权平均法）
                    int ret = (int)(curColor.R * 0.299 + curColor.G * 0.587 + curColor.B * 0.114);
                    //设置该点像素的灰度值，R=G=B=ret
                    bitmap.SetPixel(i, j, Color.FromArgb(ret, ret, ret));
                }
            }
            return bitmap;
        }

        void GetMax(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new Exception("no bitmap exist!");
            }
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color c = bitmap.GetPixel(i, j);

                    if (((c.R + c.G + c.B) / 3) > max)
                    {
                        max = ((c.R + c.G + c.B) / 3);
                    }
                }
            }
            GetLevel();
        }

        void GetMax()
        {
            GetMax(bitmap);
        }

        void GetLevel()
        {
            level = max / array.Length;
            // level = 10;
        }

        char GetChar(int b)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (b >= i * level && (i + 1) * level > b)
                {
                    return array[i];
                }
            }
            return ' ';
        }

        int GetAvg(int x, int y)
        {
            if (bitmap == null)
            {
                throw new Exception("no bitmap exist!");
            }
            int sum = 0;
            for (int i = 0; i < step; i++)
            {
                for (int j = 0; j < step; j++)
                {
                    if (x + i >= bitmap.Width || y + j >= bitmap.Height)
                    {
                        continue;
                    }
                    else
                    {
                        Color c = bitmap.GetPixel(x + i, y + j);
                        sum += (c.R + c.G + c.B) / 3;
                    }
                }
            }
            return sum / (step*step);
        }

        public void Write(string content)
        {
            FileStream fs = new FileStream(string.Format("{0}\\temp.txt", Application.StartupPath), FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.WriteLine(content);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }

        string GetContent(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new Exception("no bitmap exist!");
            }
            string content = string.Empty;
            for (int j = 0; j < bitmap.Height; j += step)
            {
                for (int i = 0; i < bitmap.Width; i += step)
                {
                    char x = GetChar(GetAvg(i, j));
                    content += x + " ";
                }
                content += "\r\n";
            }
            return content;
        }

        private void output_Click(object sender, EventArgs e)
        {
            Write(GetContent(this.bitmap));
            Process.Start(string.Format("{0}\\temp.txt", Application.StartupPath));
        }

        void Open() {
            OpenFileDialog opnDlg = new OpenFileDialog();//创建OpenFileDialog对象
            //为图像选择一个筛选器
            opnDlg.Filter = "所有图像文件 | *.bmp; *.pcx; *.png; *.jpg; *.gif;" +
                "*.tif; *.ico; *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf|" +
                "位图( *.bmp; *.jpg; *.png;...) | *.bmp; *.pcx; *.png; *.jpg; *.gif; *.tif; *.ico|" +
                "矢量图( *.wmf; *.eps; *.emf;...) | *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf";
            opnDlg.Title = "打开图像文件";
            opnDlg.ShowHelp = true;//启动帮助按钮
            if (opnDlg.ShowDialog() == DialogResult.OK)
            {
                curFileName = opnDlg.FileName;
                try
                {
                    bitmap = new Bitmap(curFileName);
                    pb_origin.Image = bitmap;// new Bitmap(curFileName); ;
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }
            }
        }

        //打开图像
        private void open_Click(object sender, EventArgs e)
        {

            Open();
        }

        private void convert_Click(object sender, EventArgs e)
        {
            if (bitmap == null)
            {
                return;
            }
            Convert2(bitmap);
            string fileName = Application.StartupPath + "\\temp.Png";
            string strFilExtn = fileName.Remove(0, fileName.Length - 3);
            switch (strFilExtn)
            {
                case "png":
                    bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    break;
                default:
                    break;
            }
            pbconverted.Image = bitmap;
            GetMax();
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       
    }
}