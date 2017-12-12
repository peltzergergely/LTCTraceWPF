using Npgsql;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for MbHsAssyWindow.xaml
    /// Data Input validation: check if barcode, checkboxes
    /// Database Validation: backcheck if heatsinks are there
    /// </summary>
    public partial class MbHsAssyWindow : Window
    {
        public MbHsAssyWindow()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            InitializeComponent();
        }

        private void FocusNext(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool DmValidation()
        {
            if (string.IsNullOrWhiteSpace(MbDm.Text))
                return false;
            else if (string.IsNullOrWhiteSpace(HsDm0.Text))
                return false;
            else if (string.IsNullOrWhiteSpace(HsDm1.Text))
                return false;
            else if (string.IsNullOrWhiteSpace(HsDm2.Text))
                return false;
            else if (string.IsNullOrWhiteSpace(HsDm3.Text))
                return false;
            else
                return true;
        }

        private void ValidationMsg(bool isValid)
        {
            if (!isValid)
                MessageBox.Show("HIÁNYOS KITÖLTÉS!");
        }

        //do a query the items and check if heatsink is already in the hspreassy db
        private bool InterlockCheck(string table)
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.CCDBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring); // Making connection
                conn.Open();
                var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM " + table + " WHERE hs_dm_0 = :hs_dm_0", conn);
                cmd.Parameters.Add(new NpgsqlParameter("hs_dm_0", HsDm0.Text));
                Int32 countProd = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                if (countProd == 1)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
                return false;
            }
        }

        private void InterlockMsg(bool interlockResult)
        {
            if (!interlockResult)
                MessageBox.Show("Interlock! A termék nem szerepelt a korábbi munkaállomáson!");
        }

        //adatbázis kapcsolat és adatok feltöltése az adatábisba
        private void DbInsert(string dbTableName) //DB insert
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.CCDBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + dbTableName +
                    " (mb_dm, hs_dm_0, hs_dm_1, hs_dm_2, hs_dm_3, created_on, username, station)" +
                    " VALUES(:mb_dm, :hs_dm_0, :hs_dm_1, :hs_dm_2, :hs_dm_3, :created_on, :username, :station)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("mb_dm", MbDm.Text));
                cmd.Parameters.Add(new NpgsqlParameter("hs_dm_0", HsDm0.Text));
                cmd.Parameters.Add(new NpgsqlParameter("hs_dm_1", HsDm1.Text));
                cmd.Parameters.Add(new NpgsqlParameter("hs_dm_2", HsDm2.Text));
                cmd.Parameters.Add(new NpgsqlParameter("hs_dm_3", HsDm3.Text));
                cmd.Parameters.Add(new NpgsqlParameter("created_on", DateTime.Now));
                cmd.Parameters.Add(new NpgsqlParameter("username", "PG"));
                cmd.Parameters.Add(new NpgsqlParameter("station", System.Environment.MachineName));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
                MessageBox.Show("Adatok feltöltve!");
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DmValidation())
            {
                if (InterlockCheck("hspreassy"))
                {
                    DbInsert("mbhsassy");
                }else
                    InterlockMsg(InterlockCheck("hspreassy"));
            }
            else
                ValidationMsg(DmValidation());
        }
    }
}
