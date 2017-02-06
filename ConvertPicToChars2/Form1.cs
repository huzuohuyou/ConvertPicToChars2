using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
namespace ConvertPicToChars2
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;
        string curFileName;
        public Form1()
        {
            InitializeComponent();
            button1.Click += open_Click;
            button2.Click += save_Click;
            button3.Click += close_Click;
        }

        void Convert(string path)
        {
            bitmap = new Bitmap(path);

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
                    Convert(curFileName);
                    //curBitmap = (Bitmap)Image.FromFile(curFileName);//使用Image.FromFile创建图像对象
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message);
                }
            }
            //使控件的整个图面无效并导致重绘控件
            Invalidate();//对窗体进行重新绘制,这将强制执行Paint事件处理程序
        }

        private void save_Click(object sender, EventArgs e)
        {
            //bitmap.Save(Environment.CurrentDirectory + "//3.jpg");
            if (bitmap == null)
            {
                return;
            }
            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Title = "保存为";
            saveDlg.OverwritePrompt = true;
            saveDlg.Filter =
                //"BMP文件 (*.bmp) | *.bmp|" +
                //"Gif文件 (*.gif) | *.gif|" +
                //"JPEG文件 (*.jpg) | *.jpg|" +
                "PNG文件 (*.png) | *.png";
            saveDlg.ShowHelp = true;
            if (saveDlg.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveDlg.FileName;
                string strFilExtn = fileName.Remove(0, fileName.Length - 3);
                switch (strFilExtn)
                {
                    //case "bmp":
                    //    bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    //    break;
                    //case "jpg":
                    //    bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    //    break;
                    //case "gif":
                    //    bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Gif);
                    //    break;
                    //case "tif":
                    //    bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Tiff);
                    //    break;
                    case "png":
                        bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    default:
                        break;
                }
            }
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 当一个应用程序需要进行绘制时，他必须通过Graphics对象来执行绘制操作
        /// 获取Graphics对象的方法有好几种，这里我们使用窗体Paint事件的PaintEventArgs属性来获取一个与窗体相关联的Graphics对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;//获取Graphics对象
            if (bitmap != null)
            {
                g.DrawImage(bitmap, 160, 20, bitmap.Width, bitmap.Height);//使用DrawImage方法绘制图像
            }
        }
        /// <summary>
        /// 提取灰度法
        /// 为了将位图的颜色设置为灰度或其他颜色，需要使用GetPixel来读取当前像素的颜色--->计算灰度值--->使用SetPixel应用新的颜色
        /// </summary>
        private void pixel_Click(object sender, EventArgs e)
        {
            if (bitmap != null)
            {
               
                Color curColor;
                int ret;
                //二维图像数组循环  
                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        //读取当前像素的RGB颜色值
                        curColor = bitmap.GetPixel(i, j);
                        //利用公式计算灰度值（加权平均法）
                        ret = (int)(curColor.R * 0.299 + curColor.G * 0.587 + curColor.B * 0.114);
                        //设置该点像素的灰度值，R=G=B=ret
                        bitmap.SetPixel(i, j, Color.FromArgb(ret, ret, ret));
                    }
                }
               
                //使控件的整个图面无效并导致重绘控件
                Invalidate();//对窗体进行重新绘制,这将强制执行Paint事件处理程序
            }
        }
    }
}