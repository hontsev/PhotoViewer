using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing.Drawing2D;
using Microsoft.VisualBasic.FileIO;

namespace PhotoViewer
{
    class PhotoControl
    {
        Image show_img;
        AnimateImage now_img;
        public event EventHandler<EventArgs> OnFrameChanged;

        int max_height = 800;
        int max_width = 600;

        public int now_x;
        public int now_y;
        public int now_height;
        public int now_width;
        public double now_scale;

        string[] pics;
        int now_pic;

        Bitmap b;
        Graphics g;

        public PhotoControl()
        {
            now_x = 0;
            now_y = 0;
            now_scale = 1.0;           
        }

        public void LoadImageList(string pic)
        {
            try
            {
                string path = Path.GetDirectoryName(pic);

                //string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                //Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                //path = r.Replace(path, "");
                var searchPattern = new Regex(@"$(?<=\.(jpg|jpeg|png|bmp|gif))", RegexOptions.IgnoreCase);
                pics = Directory.GetFiles(path, "*.*").Where(f => searchPattern.IsMatch(f)).ToArray();
                for (int i = 0; i < pics.Length; i++)
                {
                    if (pics[i] == pic)
                    {
                        now_pic = i;
                        break;
                    }
                }

                ShowImage();
            }
            catch
            {
                Environment.Exit(0);
            }
            
        }

        public void NextImage()
        {
            CloseImage();
            int new_now_pic = (pics.Length + now_pic + 1) % pics.Length;
            while (new_now_pic != now_pic)
            {
                if(File.Exists(pics[new_now_pic]))
                {
                    now_pic = new_now_pic;
                    ShowImage();
                    break;
                }
                else
                {
                    new_now_pic = (pics.Length + new_now_pic + 1) % pics.Length;
                }
            }
        }

        public void LastImage()
        {
            CloseImage();
            int new_now_pic = (pics.Length + now_pic - 1) % pics.Length;
            while (new_now_pic != now_pic)
            {
                if (File.Exists(pics[new_now_pic]))
                {
                    now_pic = new_now_pic;
                    ShowImage();
                    break;
                }
                else
                {
                    new_now_pic = (pics.Length + new_now_pic - 1) % pics.Length;
                }
            }
        }

        public void OnImageFrameChanged(object sender, EventArgs e)
        {
            ResizeImage();
            
        }

        public void CloseImage()
        {
            if (now_img != null) now_img.Dispose();
            //now_img = null;
        }

        public void ShowImage()
        {
            if (now_img != null) now_img.Dispose();
            Image this_img = Image.FromFile(pics[now_pic]);
            if(now_img==null &&(this_img.Width>max_width || this_img.Height > max_height))
            {
                // 第一次加载，则调整适应大小
                now_scale = Math.Min((double)(max_width) / this_img.Width, (double)(max_height) / this_img.Height);
            }
            now_height = max_height;
            now_width = max_width;
            now_x = this_img.Width / 2;
            now_y = this_img.Height / 2;
            now_img = new AnimateImage(this_img);
            now_img.OnFrameChanged += OnImageFrameChanged;
            OnImageFrameChanged(null, null);
            b = new Bitmap(now_width, now_height);
            g = Graphics.FromImage(b);
            
            //now_img.Play();
            ResizeImage();
        }

        public void DeleteImage()
        {
            string f = pics[now_pic];
            NextImage();
            FileSystem.DeleteFile(f, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            
        }

        public void ResizeImage()
        {
            try
            {
                if (g == null) return ;
                lock (g)
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.High;
                    g.Clear(Color.Black);
                    if (now_scale <= 0.1) now_scale = 0.1;
                    else if (now_scale > 6) now_scale = 6;
                    int nw = (int)(now_width / now_scale);
                    int nh = (int)(now_height / now_scale);
                    int nx = (int)(now_x - nw / 2);
                    int ny = (int)(now_y - nh / 2);
                    g.DrawImage(
                        now_img.Image,
                        new Rectangle(0, 0, now_width, now_height),
                        new Rectangle(nx, ny, nw, nh),
                        GraphicsUnit.Pixel);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(50, 50, 50, 50)), new Rectangle(0, 0, now_width, 30));
                }
                   
                //g.Dispose();
                show_img = b;
                OnFrameChanged(show_img, null);
            }
            catch
            {

            }
        }
    }
}
