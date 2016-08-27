using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Emgu.CV.Structure;
using Emgu.CV;
#if !(__IOS__ || NETFX_CORE)
using Emgu.CV.Cuda;
#endif

using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;


namespace FaceDetection
{
    public partial class OcrForm : Form
    {
        Image<Bgr, byte> gimg;
        Image<Gray, byte> rimg;

        string filename;

        Tesseract Tocr;

        public OcrForm()
        {
            InitializeComponent();
            try
            {
                Tocr = new Tesseract("", "eng", OcrEngineMode.TesseractOnly);
                
                
            }
            catch (Exception exp)
            {
                MessageBox.Show("Error loading Tessract Data ");
            }
        }

        private void OcrForm_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            DialogResult res = op.ShowDialog();

            if (res == DialogResult.OK)
            {
                filename = op.FileName;
                Image img = Image.FromFile(filename);


                Bitmap bimg = (Bitmap)img;
                Image<Gray, byte> grayimg;
                Bgr color = new Bgr(Color.Red);

                gimg = new Image<Bgr, byte>(bimg);

                try
                {
                    grayimg = gimg.Convert<Gray, Byte>();
                    Image<Gray, byte> bimimg = grayimg.ThresholdBinary(new Gray(126), new Gray(255));

                    


                    Tocr.Recognize(bimimg);

                    Tesseract.Character[] chararcters = Tocr.GetCharacters();
                    foreach (Tesseract.Character c in chararcters)
                    {
                        gimg.Draw(c.Region, color, 1);
                        

                    }


                    pictureBox1.Image = gimg.Resize(pictureBox1.Width, pictureBox1.Height, Emgu.CV.CvEnum.Inter.Linear).Bitmap;

                    string txt = Tocr.GetText();
                    textBox1.Text = txt;

                }
                catch (Exception ex)
                {

                }

            }
        }
    }
}
