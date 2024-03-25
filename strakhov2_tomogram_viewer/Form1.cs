using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace strakhov2_tomogram_viewer
{
    public partial class Form1 : Form
    {
        bool needReload = false;
        int frameCount;
        DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);
        void displayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                FpsLabel.Text = String.Format("FPS: {0}", frameCount);
                NextFPSUpdate = DateTime.Now.AddSeconds(1);
                frameCount = 0;
            }
            frameCount++;
        }
        int currentLayer = 0;
        bool frameLoaded = false;
        View view = new View();
        Bin bin = new Bin();
        public Form1()
        {
            InitializeComponent();

        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (frameLoaded)
            {
                
            
                if (needReload)
                {
                    if (radioButtonTex.Checked)
                    {

                        view.generateTextureImage(currentLayer);
                        view.Load2DTexture();
                        view.DrawTexture();
                    }
                    if (radioButtonQuads.Checked){ view.DrawQuads(currentLayer); }
                    if (radioButton1.Checked) { view.DrawQuadstrip(currentLayer); }

                    needReload = false;
                    glControl1.SwapBuffers();
                }
                
              
            }
        }

        private void glControl1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            trackBar1.Value = 0;
            currentLayer = 0;
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string str = dialog.FileName;
                bin.readBin(str);
                trackBar1.Maximum = Bin.Z-1;
                label2.Text = trackBar1.Maximum.ToString();
                view.setupView(glControl1.Width, glControl1.Height);
                frameLoaded = true;
                glControl1.Invalidate();
                needReload = true;

            }

        }



        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            currentLayer = trackBar1.Value;
            needReload = true;
            toolTip1.SetToolTip(trackBar1, trackBar1.Value.ToString());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Application.Idle += Application_Idle;
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
                displayFPS();
                glControl1.Invalidate();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog4 = new SaveFileDialog();

            saveFileDialog4.Filter = "BIN Files(*.bin;)|*.bin;";

            if (saveFileDialog4.ShowDialog() == DialogResult.OK)
            {
                BinaryWriter f = new BinaryWriter(System.IO.File.Open(saveFileDialog4.FileName.ToString(), FileMode.CreateNew));
                for (int i =0; i < numericUpDown1.Value; i++)
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.Title = "Выберите слой # "+i.ToString();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        
                        string str = dialog.FileName;
                        if (File.Exists(str))
                        {
                            Bitmap bmap = new Bitmap(str);
                            
                            int size = bmap.Width*bmap.Height;

                            if (i == 0)
                            {
                                f.Write((Int32)(bmap.Width));
                                f.Write((Int32)(bmap.Height));
                                f.Write((Int32)numericUpDown1.Value);
                            }
                            Color clr = new Color();
                          for(int k = 0; k < size; k++)
                            {
                                clr = bmap.GetPixel(k % bmap.Width, k / bmap.Height);

                                f.Write((Int16)View.Clamp((clr.R + clr.G + clr.B) * 3, 0, 2000));
                            }

                        }

                    }
                }
                f.Close();
                

            }
            // public void testBin()
            //  {
            // FileStream fs = File.Create(@"E:\Downloads\tomographtester.bin");
           
        }
    

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            needReload = true;
        }
        private void radioButtonTex_CheckedChanged(object sender, EventArgs e)
        {
            needReload = true;

        }
        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
            
        }

        private void trackBar1_MouseHover(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(trackBar1, trackBar1.Value.ToString());
        }
        int width = 2000;
        private void widthBar_Scroll(object sender, EventArgs e)
        {
          
        }

        private void minimumBar_Scroll(object sender, EventArgs e)
        {
         
        }

        private void minimumBar_ValueChanged(object sender, EventArgs e)
        {
            minLabel.Text = minimumBar.Value.ToString();
            view.min = minimumBar.Value;
            int newWidth = View.Clamp(width, 1, 2000 - view.min);
            view.max = view.min + newWidth;
            widthLabel.Text = newWidth.ToString();
            widthBar.Maximum = 2000-view.min;
            needReload = true;
        }

        private void widthBar_ValueChanged(object sender, EventArgs e)
        {
            width = widthBar.Value;
            view.max = view.min + width;

            widthLabel.Text = width.ToString();
            needReload = true;
        }
    }
}
