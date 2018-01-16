using Npgsql;
using System;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for FbEmcWindow.xaml
    /// </summary>
    public partial class FbEmcWindow : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public bool IsCameraLaunched { get; set; } = false;

        public FbEmcWindow()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

            if (Keyboard.FocusedElement == FbDmTxbx && FbDmTxbx.Text.Length > 0) //calls the validator for the field in focus
            {
                FbDmValidator();
            }
        }

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void FbDmValidator()
        {
            if (RegexValidation(FbDmTxbx.Text, "FbDmRegEx"))
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            IsCameraLaunched = false;
            FbDmTxbx.Text = "";
            FbDmTxbx.Focus();
        }

        private void WebCamLaunchClick(object sender, RoutedEventArgs e)
        {
            SaveBtn.Focus();
            IsCameraLaunched = true;
            var webCam = new camApp();
            webCam.Show();
        }

        private void ValidateAll()
        {
            if (IsDmValidated == true || IsCameraLaunched == true)
                AllFieldsValidated = true;
            else
                CallMessageForm("Hibás kitöltés");
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllFieldsValidated) { };
                //UploadToDb();
        }
    }
}
