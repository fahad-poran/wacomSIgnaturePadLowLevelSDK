using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Reflection;
using wgssSTU;
using DemoButtons.Properties;

namespace DemoButtons.Utilities
{
    public class UIHelper
    {
        private Form mainForm;
        private string dbConnectionString;
        private Label statusLabel;

        public UIHelper(Form form, string connectionString, Label statusLabelControl)
        {
            mainForm = form;
            dbConnectionString = connectionString;
            statusLabel = statusLabelControl;
        }

        public void InitializeUI(Button captureButton, TextBox penDataTextBox)
        {
            // Set up better button appearance
            if (captureButton != null)
            {
                captureButton.Text = "Capture Signature";
                //captureButton.BackColor = Color.DeepSkyBlue;
                captureButton.ForeColor = Color.White;
                captureButton.FlatStyle = FlatStyle.Flat;
                captureButton.FlatAppearance.BorderSize = 0;
                captureButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                captureButton.Height = 40;
            }

            
            // Set up label for pen data count
            if (penDataTextBox != null)
            {
                penDataTextBox.BackColor = Color.White;
                penDataTextBox.BorderStyle = BorderStyle.FixedSingle;
                penDataTextBox.Font = new Font("Consolas", 9);
                penDataTextBox.ForeColor = Color.DarkBlue;
            }

            // Set form background
            mainForm.BackColor = Color.WhiteSmoke;
        }

        // Simple Wacom device check
        public bool CheckWacomDevice()
        {
            try
            {
                wgssSTU.UsbDevices usbDevices = new wgssSTU.UsbDevices();
                bool deviceFound = usbDevices.Count > 0;

                if (deviceFound)
                {
                    UpdateStatus("✓ Wacom STU-430/G Connected", Color.Green);
                }
                else
                {
                    UpdateStatus("✗ Wacom STU-430/G Not Found", Color.Red);
                }

                return deviceFound;
            }
            catch (Exception ex)
            {
                UpdateStatus($"✗ Device Error: {ex.Message}", Color.Red);
                return false;
            }
        }

        // Simple database check
        public bool CheckDatabaseConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbConnectionString))
                {
                    conn.Open();
                    UpdateStatus("✓ Database Connected", Color.Green);
                    return true;
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"✗ Database Error: {ex.Message}", Color.Red);
                return false;
            }
        }

        public void SetVersionInfo()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            mainForm.Text += $" v{version.Major}.{version.Minor}";
        }

        public void UpdateStatus(string message, Color color)
        {
            if (statusLabel != null && !statusLabel.IsDisposed)
            {
                if (statusLabel.InvokeRequired)
                {
                    statusLabel.Invoke((MethodInvoker)(() =>
                    {
                        statusLabel.Text = message;
                        statusLabel.ForeColor = color;
                    }));
                }
                else
                {
                    statusLabel.Text = message;
                    statusLabel.ForeColor = color;
                }
            }
        }
    }
}