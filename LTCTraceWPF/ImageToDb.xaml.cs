using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;
using NpgsqlTypes;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for ImageToDb.xaml
    /// Showing the last made image in a separate window and option to upload it to the database or discard
    /// </summary>
    public partial class ImageToDb : Window
    {
        public string FilePathStr { get; set; } = "";

        public string DataMatrix { get; set; } = "";

        public string TableName { get; set; } = "";

        public int NumOfPic { get; set; } = 0;
        
        public string constr { get; set; } = @ConfigurationManager.ConnectionStrings["LTCTrace.CCDBConnectionString"].ConnectionString;


        public ImageToDb()
        {
        }
        public ImageToDb(string filePathName, string dataMatrix, string tableName, int numOfPic)
        {
            InitializeComponent();
            FilePathStr = filePathName;
            DataMatrix = dataMatrix;
            TableName = tableName;
            NumOfPic = numOfPic;
            camApp.NumOfPics = 0 ;
            ShowImage();
        }

        private void ShowImage()
        {
            image.Source = new BitmapImage(new Uri(FilePathStr));
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            InsertImageData();
        }

        private void InsertImageData()
        {
            try
            {
                if (FilePathStr != "")
                {
                    //Initialize a file stream to read the image file
                    FileStream fs = new FileStream(FilePathStr, FileMode.Open, FileAccess.Read);

                    //Initialize a byte array with size of stream
                    byte[] imgByteArr = new byte[fs.Length];

                    //Read data from the file stream and put into the byte array
                    fs.Read(imgByteArr, 0, Convert.ToInt32(fs.Length));

                    //Close a file stream
                    fs.Close();

                    using (NpgsqlConnection conn = new NpgsqlConnection(constr))
                    {
                        conn.Open();
                        string sql = "insert into picturetable(photo) values(@img)";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                        {
                            //Pass byte array into database
                            cmd.Parameters.Add(new NpgsqlParameter("img", imgByteArr));
                            int result = cmd.ExecuteNonQuery();
                            if (result == 1)
                            {
                                MessageBox.Show("Kép elmentve");
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
