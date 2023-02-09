using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Sql;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FacialRecognitionApp
{
    public partial class FacialRecognitionForm : Form
    {



        //Declare Variables to use them in all this project
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade faceDetected;
        Image<Bgr, Byte> Frame;
        Capture camera;
        Image<Gray, byte> result;
        Image<Gray, byte> TrainedFace = null;
        Image<Gray, byte> grayFace = null;
        public static List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();

        List<string> labels = new List<string>();
        List<string> Users = new List<string>();

        int Count, NumLables, t;
        string name, names = null;


        public FacialRecognitionForm()
        {

            InitializeComponent();
            //HaarCascade is for face detection
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");

            String Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
            string[] Labels = Labelsinf.Split(',');
            //The first label before , will be the number of faces saved.
            //Try catch skips for some reason debug and further investigate
            //Fixed 
            try
            {

                NumLables = Convert.ToInt16(Labels[0]);
                Count = NumLables;
                string FacesLoad;
                for (int i = 1; i < NumLables + 1; i++)
                {
                    FacesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/Faces/" + FacesLoad));
                    labels.Add(Labels[i]);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Database is currently empty");
            }
            camera = new Capture();
            camera.QueryFrame();
            Application.Idle += new EventHandler(FrameProcedure);
        }

        private void button2_Click(object sender, EventArgs e)
        {


            Count = Count + 1;
            grayFace = camera.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            MCvAvgComp[][] DetectedFaces = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach (MCvAvgComp f in DetectedFaces[0])
            {
                TrainedFace = Frame.Copy(f.rect).Convert<Gray, byte>();
                break;
            }
            //Where face is first detected and then added into an array 
            TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

            Random ran = new Random();
            trainingImages.Add(TrainedFace);
            User.ID = ran.Next(100);
            User.username = textBox1.Text;
            User.age = age.Text;
            User.faculty = faculty.Text;


            labels.Add(User.username);
            Data();







            //labels.Add(age.Text);
            //labels.Add(faculty.Text);
            File.WriteAllText(Application.StartupPath + "/Faces/Faces.txt", trainingImages.ToArray().Length.ToString() + ",");


            for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
            {
                trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/Faces/face" + i + ".bmp");
                File.AppendAllText(Application.StartupPath + "/Faces/Faces.txt", labels.ToArray()[i - 1] + ",");

            }
            MessageBox.Show(textBox1.Text + " Added Successfully");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            textBox1.Text = "";
            age.Text = "";
            faculty.Text = "";

            Users.Add("");
            Frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach (MCvAvgComp f in facesDetectedNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.AntiqueWhite), 3);
                //core

                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCriterias = new MCvTermCriteria(Count, 0.000001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 4000, ref termCriterias);
                    name = recognizer.Recognize(result);

                    SqlConnection con2 = new SqlConnection("Data Source = awehserverst10091865.database.windows.net; Initial Catalog = FacialRecognitionApp; User ID = jimmy; Password=4731598819Amo");
                    con2.Open();
                    //Create getters and setters for og usertxt inorder for not null

                    SqlCommand cmd2 = new SqlCommand("SELECT * FROM userdata where name = '" + name + "'", con2);
                    
                    SqlDataReader reader = cmd2.ExecuteReader();
                    if(reader.Read())
                    {
                        label5.Text = reader[0].ToString();
                        textBox1.Text = reader[1].ToString();
                        age.Text = reader[2].ToString();
                        faculty.Text = reader[3].ToString();
                    }

                    


                   

                }

                Users.Add("");
            }



        }

        
        private void FrameProcedure(object sender, EventArgs e)
        {
            Users.Add("");
            Frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach (MCvAvgComp f in facesDetectedNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.AntiqueWhite), 3);
                //core

                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCriterias = new MCvTermCriteria(Count, 0.000001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 20000, ref termCriterias);
                    name = recognizer.Recognize(result);
                    Frame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.DarkBlue));

                }
                


                Users.Add("");
            }
            imageBox1.Image = Frame;
            names = "";
            Users.Clear();

        }

        public static class User
        {
            public static Int32 ID { get; set; }
            public static string username { get; set; } = null;
            public static string age { get; set; } = null;

            public static string faculty { get; set; }

        }
        private void Data ()
        {
            SqlConnection con = new SqlConnection("Data Source = awehserverst10091865.database.windows.net; Initial Catalog = FacialRecognitionApp; User ID = jimmy; Password=4731598819Amo");
            
            con.Open();

            SqlCommand cmd = new SqlCommand("INSERT INTO userdata VALUES (@ID,@name,@age,@faculty)", con);
            //Remove as primary key and place other value as primary key too prevent duplicates

            
            cmd.Parameters.AddWithValue("@ID",User.ID);
            cmd.Parameters.AddWithValue("@name", User.username);
            cmd.Parameters.AddWithValue("@age", User.age);
            cmd.Parameters.AddWithValue("@faculty", User.faculty);
            cmd.ExecuteNonQuery();


            con.Close();
            MessageBox.Show("Saved", "UserData");
        }
    }

}
