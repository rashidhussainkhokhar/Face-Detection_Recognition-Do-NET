using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;


using Emgu.CV.Structure;
using Emgu.CV;
#if !(__IOS__ || NETFX_CORE)
using Emgu.CV.Cuda;
#endif

using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Face;


namespace FaceDetection
{
    public partial class Form1 : Form
    {
        private Capture Cap;
        Image<Bgr, byte> rimg;
        Image<Bgr, byte> gimg;

        Rectangle[] facesDetected;
        CascadeClassifier face = new CascadeClassifier("haarcascade_frontalface_alt2.xml");

        FaceRecognizer recognizer;



        List<Image<Bgr, byte>> ExtractedFaces;
        int efacecount = 0;
        int imboxcount = 0;


        int TotalFrames;
        int FPS;
        int CurrentFrame;
        bool videoloader = false;

        

        SqlConnection connection = new SqlConnection();
        
      
        int TotalRows;
        int rownum;

        int row_id;

        Image<Gray, byte>[] TrainedFaces;
        string[] FaceLabels;
        int[] ids;

        bool isREc = false;
        bool startDetec = false;

        List<String> RecName = new List<string>();

        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                

                ExtractedFaces = new List<Image<Bgr, byte>>();
            }
            catch (Exception exp)
            {

            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            DialogResult res = op.ShowDialog();

            if (res == DialogResult.OK)
            {
                try
                {
                    string filename = op.FileName;
                    Cap = new Capture(filename);

                    TotalFrames = (int)Cap.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                    totalF.Text = Convert.ToString(TotalFrames);

                    CurrentFrame = (int)Cap.GetCaptureProperty(CapProp.PosFrames);
                    CurrentF.Text = Convert.ToString(CurrentFrame);

                    FPS = (int)Cap.GetCaptureProperty(CapProp.Fps);
                    Frate.Text = Convert.ToString(FPS);
                    timer2.Interval = 5000 / FPS;



                    label4.Text = filename;

                    videoloader = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Loading Video");
                }

            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DetectFace();
        }
        public void DetectFace()
        {
            try
            {
                IImage gim = captureImageBox.Image;

                facesDetected = face.DetectMultiScale(
                        gim,
                        1.1,
                        4,
                        new Size(20, 20));


                ExtractedFaces.Clear();
                RecName.Clear();
                Bitmap bimg = gim.Bitmap;
                int ind=-1;
                
                Image<Bgr, byte> bgrtemp = new Image<Bgr, byte>(bimg);


                if (facesDetected != null)
                {
                    if (facesDetected.Length > 0)
                    {
                        foreach (Rectangle f in facesDetected)
                        {

                            Image<Bgr, byte> Eface = new Image<Bgr, byte>(bimg);

                            Eface.ROI = f;
                            ExtractedFaces.Add(Eface);

                            Image<Gray, byte> EfaceGray = Eface.Convert<Gray, byte>();
                            EfaceGray = EfaceGray.Resize(105, 105, Inter.Cubic);
                            EfaceGray._EqualizeHist();
                            if (isREc)
                            {
                                FaceRecognizer.PredictionResult Elabel = recognizer.Predict(EfaceGray);
                                
                                if (Elabel.Label == -1)
                                {
                                    RecName.Add("Unknown");
                                }
                                else
                                {
                                    RecName.Add(FaceLabels[Elabel.Label]);

                                }
                            }



                        }
                        
                        
                        foreach (Rectangle f in facesDetected)
                        {

                            CvInvoke.Rectangle(bgrtemp, f, new Bgr(Color.Red).MCvScalar, 2);

                            if (isREc)
                            {
                                CvInvoke.PutText(bgrtemp, RecName[++ind], new Point(f.X - 2, f.Y - 2), FontFace.HersheyComplex, 1.0, new Bgr(Color.LightBlue).MCvScalar);


                            }
                        }
                        extractedf.Image = ExtractedFaces[0].ToBitmap();
                    }
                }

                captureImageBox.Image = bgrtemp;
            }
            catch (Exception ep)
            {
                MessageBox.Show("Error Detecting ");
            }

            
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (ExtractedFaces != null)
            {
                imboxcount++;
                int k = ExtractedFaces.Count;
                if (imboxcount < k)
                {
                    extractedf.Image = ExtractedFaces[imboxcount].ToBitmap();
                }
                else
                {
                    imboxcount = 0;
                    if (imboxcount < k)
                    {
                        extractedf.Image = ExtractedFaces[imboxcount].ToBitmap();
                    }
                }
            }
        }

        private void activeCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {


                //c = new Capture();
                //c.ImageGrabbed += ProcessFrame;


                //c.Start();
            }
            catch (Exception exp)
            {

            }
        }

        private void oCRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void button4_Click_1(object sender, EventArgs e)
        {

            try
            {
                if (videoloader)
                {
                    Mat img;
                    img = Cap.QueryFrame();
                    Image<Bgr, byte> frame = img.ToImage<Bgr, byte>();
                    CurrentFrame = (int)Cap.GetCaptureProperty(CapProp.PosFrames);

                    captureImageBox.Image = frame;
                    CurrentF.Text = Convert.ToString(CurrentFrame);
                }

            }
            catch (Exception exp)
            {
                MessageBox.Show("Error getting next frame");
            }

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            if (videoloader)
            {
                timer2.Start();
            }

        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            if (videoloader)
            {
                timer2.Stop();
            }
        }

  

        private void button7_Click_1(object sender, EventArgs e)
        {
            try
            {
                timer2.Stop();
                Cap.SetCaptureProperty(CapProp.PosFrames, 0);
                CurrentFrame = (int)Cap.GetCaptureProperty(CapProp.PosFrames);
                CurrentF.Text = Convert.ToString(CurrentFrame);
                captureImageBox.Image = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Stopping Video");
            }

        }

        public void connectToDb()
        {
            if (connection.State == ConnectionState.Closed)
            {

                connection.ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=\"C:\\Users\\Rashid Khokhar\\documents\\visual studio 2010\\Projects\\FaceDetection\\FaceDetection\\FDB.mdf\";Integrated Security=True;Connect Timeout=30;User Instance=True";
                connection.Open();

                String q = "select * from Table1";
                SqlCommand cmd = new SqlCommand(q, connection);
                SqlDataAdapter dap = new SqlDataAdapter(cmd);
                DataSet dd = new DataSet();
                dap.Fill(dd);
                int count = dd.Tables[0].Rows.Count;
                row_id = count;
               // MessageBox.Show(Convert.ToString(count));
            }

        }

        private void button10_Click_1(object sender, EventArgs e)
        {
           
            connectToDb();
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    String query = "select * from Table1";
                    SqlCommand com = new SqlCommand(query, connection);

                    SqlDataAdapter dp = new SqlDataAdapter(com);
                    DataSet ds = new DataSet();

                    dp.Fill(ds);

                    TotalRows = ds.Tables[0].Rows.Count;
                    int[] indexs = new int[TotalRows];
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        TrainedFaces = new Image<Gray, byte>[TotalRows];
                        FaceLabels = new string[TotalRows];
                        ids = new int[TotalRows];
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            DataRow dr;
                            dr = ds.Tables[0].Rows[i];

                            byte[] pdata = (byte[])dr["pic"];
                            MemoryStream ms = new MemoryStream(pdata);
                            Image resimg = Image.FromStream(ms);
                            Bitmap bimgg=(Bitmap)resimg;

                            TrainedFaces[i]=new Image<Gray, byte>(bimgg);
                            string name = (String)dr["name"];
                            FaceLabels[i]=name;
                            int id = (int)dr["id"];
                            ids[i]=id;

                            indexs[i] = i;

                            //MessageBox.Show("done getting data");

                            //extractedf.Image = resimg;

                            
                        }

                        try
                        {
                            //MCvTermCriteria cret = new MCvTermCriteria(TotalRows, 0.001);
                        recognizer = new LBPHFaceRecognizer(1,8,8,8,150);
                            

                           
                            recognizer.Train(TrainedFaces, indexs);
                           

                            isREc = true;

                            MessageBox.Show("Recognizer Trained");


                       }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to Train Recognizer");
                        }
                        
                    }

                  
                }
            }
            catch (Exception ex)
            {
               isREc = false;
            }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                connectToDb();

                if (nametb.Text.ToString() != "" && extractedf.Image != null)
                {
                    MemoryStream ms = new MemoryStream();
                    Image img = extractedf.Image;
                    Bitmap b = (Bitmap)img;
                    Image<Gray, byte> fac = new Image<Gray, byte>(b);
                    Image<Gray, byte> res = fac.Resize(105, 105, Inter.Cubic);
                    res._EqualizeHist();
                    img = res.ToBitmap();
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] pic = ms.ToArray();
                   
                    row_id++;
                    String query = "insert into Table1 (id,name,pic) values(" + row_id + ",'" +nametb.Text.ToString()+ "',@pic)";
                    SqlCommand com = new SqlCommand(query, connection);

                    com.Parameters.AddWithValue("@pic", pic);
                    try
                    {
                        int roweffect=com.ExecuteNonQuery();
                        //MessageBox.Show(roweffect + " row added to db");
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("Error in inserting the image");
                    }

                }
                else
                {
                    MessageBox.Show("Name or Image is missing");
                }

                




            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong");
            }




        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                if ((int)Cap.GetCaptureProperty(CapProp.PosFrames) <= TotalFrames)
                {
                    Mat img;
                    img = Cap.QueryFrame();
                    Image<Bgr, byte> frame = img.ToImage<Bgr, byte>();
                    CurrentFrame = (int)Cap.GetCaptureProperty(CapProp.PosFrames);

                    captureImageBox.Image = frame;
                    CurrentF.Text = Convert.ToString(CurrentFrame);
                    if (startDetec == true)
                    {
                        DetectFace();
                    }
                    
                }
                if ((int)Cap.GetCaptureProperty(CapProp.PosFrames) == TotalFrames)
                {
                    timer1.Stop();
                    MessageBox.Show("timer stopped");
                }



            }
            catch (Exception exp)
            {

            }
        }

        private void oCRToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OcrForm form = new OcrForm();
            form.ShowDialog();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            isREc = true;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            startDetec = true;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            startDetec = false;
        }

        private void button14_Click(object sender, EventArgs e)
        {

            if (textBox1.Text.ToString() != "")
            {
                Cap.SetCaptureProperty(CapProp.PosFrames, Convert.ToInt32(textBox1.Text.ToString()));
            }
        }
      

    }
}
