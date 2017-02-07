using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvertPicToChars2
{
    public static class ImageHelper
    {
        /// <summary>
     /// 获取图片中的各帧
     /// </summary>
     /// <param name="pPath">图片路径</param>
     /// <param name="pSavePath">保存路径</param>
        public static void GetFrames(string pPath, string pSavedPath)
        {
            try
            {
                if (!Directory.Exists(pSavedPath))
                {
                    Directory.CreateDirectory(pSavedPath);
                }
                Image gif = Image.FromFile(pPath);
                FrameDimension fd = new FrameDimension(gif.FrameDimensionsList[0]);

                //获取帧数(gif图片可能包含多帧，其它格式图片一般仅一帧)
                int count = gif.GetFrameCount(fd);

                //以Jpeg格式保存各帧
                for (int i = 0; i < count; i++)
                {
                    gif.SelectActiveFrame(fd, i);
                    gif.Save(pSavedPath + "\\frame_" + i + ".jpg", ImageFormat.Jpeg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           
        }
    }
}
