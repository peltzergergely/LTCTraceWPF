using Npgsql;
using System;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Runtime.InteropServices;


namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for HousingLeakTestWindow.xaml
    /// </summary>
    public partial class HousingLeakTestWindow : Window
    {
        public static string UserName { get; set; }

        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public HousingLeakTestWindow()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
            LoginForm();
        }

        private void LoginForm()
        {
            UserName = "user";
            //userNameTxt.Text = UserName;
        }

        private void OnKeyUpEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();

            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter && housingDmTxbx.Text.Length > 0)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
                e.Handled = true;
            }

            if (Keyboard.FocusedElement == SaveBtn)
            {
                FormValidator();
                SaveBtn_Click(sender, e);
            }
        }

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            housingDmTxbx.Text = "";
            leakTestTxbx.Text = "";
            housingDmTxbx.Focus();
        }

        private void FormValidator()
        {
            if (housingDmTxbx.Text.Length > 0 &&  float.Parse(leakTestTxbx.Text) < 5 && float.Parse(leakTestTxbx.Text) > 0)
            {
                AllFieldsValidated = true;
            }
            else
            {
                CallMessageForm("Hibás adatok");
            }
        }

        private void DbInsert(string table) //DB insert
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.CCDBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                var UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (housing_dm, leak_test_result, pc_name, username, created_on) " +
                    "VALUES(:housing_dm, :leak_test_result, :pc_name, :username, :timestamp)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", housingDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("leak_test_result", float.Parse(leakTestTxbx.Text)));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("username", UserName));
                cmd.Parameters.Add(new NpgsqlParameter("timestamp", UploadMoment));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
                CallMessageForm("Adatok feltöltve!");
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllFieldsValidated)
            {
                DbInsert("housing_leak_test_one");
            }
        }
    }
}
