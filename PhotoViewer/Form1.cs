using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace PhotoViewer
{
    public partial class Form1 : Form
    {
        delegate void sevent(object s, EventArgs e);
        PhotoControl pc;
        public delegate void sendVoid();
        Point dragPoint = new Point(-1,-1);

        public Form1()
        {
            InitializeComponent();
            pc = new PhotoControl();
            this.DoubleBuffered = true;
            
        }

        public void OnImageFrameChanged(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                sevent se = new sevent(OnImageFrameChanged);
                Invoke(se, sender, e);
            }
            else
            {
                if (sender != null)
                {
                    Image img = (Image)sender;
                    this.Size = new Size(pc.now_width, pc.now_height);
                    pictureBox1.Image = img;
                    pictureBox1.Refresh();
                }
            }

            
        }
        
        

        private void Form1_Load(object sender, EventArgs e)
        {
            
            var cmd = Environment.GetCommandLineArgs();
            string now_file = "";
            if (cmd.Length >= 2)
            {
                now_file = cmd[1];
            }
            else
            {
                Environment.Exit(0);
            }
            pc.OnFrameChanged += OnImageFrameChanged;
            pc.LoadImageList(now_file);
            this.Location = new Point(0, 0);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Up:
                case Keys.PageUp:
                    pc.LastImage();
                    break;
                case Keys.Right:
                case Keys.Down:
                case Keys.PageDown:
                case Keys.Space:
                case Keys.Enter:
                    pc.NextImage();
                    break;
                case Keys.Escape:
                    Environment.Exit(0);
                    break;
                case Keys.Delete:
                    try
                    {
                        pc.DeleteImage();
                    }
                    catch
                    {

                    }
                    break;
                default:
                    break;
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragPoint = MousePosition;

            }
            else if (e.Button == MouseButtons.Right)
            {
                // reset size
                pc.now_scale = 1.0;
                pc.now_x = 0;
                pc.now_y = 0;
                pc.ResizeImage();
            }
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            dragPoint = new Point(-1, -1);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragPoint.X >= 0)
            {
                int dx = MousePosition.X - dragPoint.X;
                int dy = MousePosition.Y - dragPoint.Y;
                dragPoint = MousePosition;
                if (dragPoint.Y-Location.Y > 30)
                {
                    pc.now_x -= (int)(dx / pc.now_scale);
                    pc.now_y -= (int)(dy / pc.now_scale);
                    pc.ResizeImage();
                }
                else
                {
                    Location = new Point(Location.X + dx, Location.Y + dy);
                }
                
            }

        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragPoint = new Point(-1, -1);
            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            dragPoint = new Point(-1, -1);
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {

            if (e.Delta > 0)
                pc.now_scale -= e.Delta * 0.0003;
            else
                pc.now_scale -= e.Delta * 0.0003;
            pc.ResizeImage();
        }
    }
}
