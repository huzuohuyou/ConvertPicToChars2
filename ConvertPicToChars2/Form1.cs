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
using System.Windows.Forms;
namespace ConvertPicToChars2
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;
        string curFileName;
        int step = 5;
        int max = 0;
        int level = 0;
        char[] array = { '#','@','X', 'B','A',
                         'G','5','A', 'R','9',
                         '3','S','2', 'd','h',
                         'i','&','x', 's','r',
                         'm','z',';', ':','.',
                         ' ' };
        private int i;

        public Form1()
        {
            InitializeComponent();
            button1.Click += open_Click;
            button2.Click += convert_Click;
            button3.Click += close_Click;
            button4.Click += output_Click;
        }

        void Convert(string path)
        {
            bitmap = new Bitmap(path);
            Convert(bitmap);
        }

        void Convert(Bitmap bitmap)
        {
            //定义锁定bitmap的rect的指定范围区域
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            //加锁区域像素
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            //位图的首地址
            var ptr = bitmapData.Scan0;
            //stride：扫描行
            int len = bitmapData.Stride * bitmap.Height;
            var bytes = new byte[len];
            //锁定区域的像素值copy到byte数组中
            Marshal.Copy(ptr, bytes, 0, len);
            for (int i = 0; i < bitmap.Height; i++)
            {
                for (int j = 0; j < bitmap.Width * 3; j = j + 3)
                {
                    var color = bytes[i * bitmapData.Stride + j + 2] * 0.299
                          + bytes[i * bitmapData.Stride + j + 1] * 0.597
                          + bytes[i * bitmapData.Stride + j] * 0.114;

                    bytes[i * bitmapData.Stride + j]
                         = bytes[i * bitmapData.Stride + j + 1]
                         = bytes[i * bitmapData.Stride + j + 2] = (byte)color;
                }
            }

            //copy回位图
            Marshal.Copy(bytes, 0, ptr, len);

            //解锁
            bitmap.UnlockBits(bitmapData);
        }

        void Convert2(Bitmap bitmap)
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
        }

        void GetMax() {
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

        void GetLevel()
        {
            level = max / 26;
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
            FileStream fs = new FileStream(string.Format("{0}\\temp.txt", Application.StartupPath), FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.WriteLine(content);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }

        private void output_Click(object sender, EventArgs e)
        {
            string content = string.Empty;
            for (int j = 0; j < bitmap.Height; j += step)
            {
                for (int i = 0; i < bitmap.Width; i += step)
                {
                    char x = GetChar(GetAvg(i, j));
                    content += x;
                }
                content += "\r\n";
            }
            Write(content);
            Process.Start(string.Format("{0}\\temp.txt", Application.StartupPath));
        }

        //打开图像
        private void open_Click(object sender, EventArgs e)
        {
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
            //使控件的整个图面无效并导致重绘控件
            Invalidate();//对窗体进行重新绘制,这将强制执行Paint事件处理程序
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