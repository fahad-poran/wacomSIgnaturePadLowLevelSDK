/*
 DemoButtonsForm.cs - Full working example for Wacom STU-430
 Captures signature, draws smooth ink, and saves as PNG
 Requires: wgssSTU.dll from Wacom STU SDK
 Target Framework: .NET 4.8
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

using DemoButtons.Properties;
using DemoButtons.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace DemoButtons
{
    enum PenDataOptionMode
    {
        PenDataOptionMode_None,
        PenDataOptionMode_TimeCount,
        PenDataOptionMode_SequenceNumber,
        PenDataOptionMode_TimeCountSequence
    }

    public partial class DemoButtonsForm : Form
    {
        string dbConn = Settings.Default.ConnectionString;
        private UIHelper uiHelper;
        private string currentEmployeeId = "";

        public DemoButtonsForm()
        {
            InitializeComponent();
            uiHelper = new UIHelper(this, dbConn, lblStatus);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // First, ask for employee ID card number and get full name
            string employeeFullName = ValidateEmployeeId();

            if (!string.IsNullOrEmpty(employeeFullName))
            {
                // If employee exists and we got the full name, proceed with signature capture
                CaptureSignature(employeeFullName);
            }
        }

        private string ValidateEmployeeId()
        {
            using (var inputForm = new Form())
            {
                inputForm.Text = "Employee Verification";
                inputForm.Size = new Size(300, 150);
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                var lblInstruction = new Label()
                {
                    Text = "Enter Employee ID Card Number:",
                    Location = new Point(10, 10),
                    Size = new Size(250, 20),
                    Font = new Font("Arial", 9)
                };

                var txtIdCard = new TextBox()
                {
                    Location = new Point(10, 35),
                    Size = new Size(250, 20),
                    Font = new Font("Arial", 10)
                };

                var btnOk = new Button()
                {
                    Text = "OK",
                    Location = new Point(100, 65),
                    Size = new Size(75, 25),
                    DialogResult = DialogResult.OK
                };

                var btnCancel = new Button()
                {
                    Text = "Cancel",
                    Location = new Point(180, 65),
                    Size = new Size(75, 25),
                    DialogResult = DialogResult.Cancel
                };

                inputForm.Controls.AddRange(new Control[] { lblInstruction, txtIdCard, btnOk, btnCancel });
                inputForm.AcceptButton = btnOk;
                inputForm.CancelButton = btnCancel;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(txtIdCard.Text))
                    {
                        MessageBox.Show("Please enter Employee ID Card Number.");
                        return null;
                    }

                    currentEmployeeId = txtIdCard.Text.Trim();
                    return CheckEmployeeInDatabase(currentEmployeeId);
                }
            }
            return null;
        }

        private string CheckEmployeeInDatabase(string idCardNo)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbConn))
                {
                    conn.Open();
                    string query = "SELECT FullName FROM HrEmployee WHERE IdCardNo = @IdCardNo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdCardNo", idCardNo);
                        var result = cmd.ExecuteScalar();

                        if (result != null && !string.IsNullOrEmpty(result.ToString()))
                        {
                            string fullName = result.ToString();
                            //MessageBox.Show($"Employee Found: {fullName}", "Success",MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return fullName;
                        }
                        else
                        {
                            MessageBox.Show("Employee ID Card Number not found in database.");
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}");
                return null;
            }
        }

        private void CaptureSignature(string employeeFullName)
        {
            int penDataType;
            List<wgssSTU.IPenDataTimeCountSequence> penTimeData = null;
            List<wgssSTU.IPenData> penData = null;

            wgssSTU.UsbDevices usbDevices = new wgssSTU.UsbDevices();
            if (usbDevices.Count == 0)
            {
                MessageBox.Show("No STU devices attached");
                return;
            }

            try
            {
                wgssSTU.IUsbDevice usbDevice = usbDevices[0];

                // You can show the employee name in the signature form if needed
                SignatureForm demo = new SignatureForm(usbDevice, false); // false = no encryption
                demo.Text = $"Signature Capture - {employeeFullName}"; // Optional: Show name in title
                demo.ShowDialog();

                penDataType = demo.penDataType;

                if (penDataType == (int)PenDataOptionMode.PenDataOptionMode_TimeCountSequence)
                    penTimeData = demo.getPenTimeData();
                else
                    penData = demo.getPenData();

                if (penData != null || penTimeData != null)
                {
                    // Display pen count
                    txtPenDataCount.Text = (penData != null ? penData.Count : penTimeData.Count).ToString();

                    // Draw smooth signature and save
                    string savedFilePath = SaveSignatureAsImage(penData, penTimeData, demo.getCapability());

                    // Update database with signature path
                    if (!string.IsNullOrEmpty(savedFilePath))
                    {
                        UpdateSignaturePathInDatabase(currentEmployeeId, savedFilePath);
                    }
                }

                demo.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string SaveSignatureAsImage(List<wgssSTU.IPenData> penData,
                                          List<wgssSTU.IPenDataTimeCountSequence> penTimeData,
                                          wgssSTU.ICapability capability)
        {
            // First, check if any signature data exists
            bool hasSignatureData = false;

            if (penData != null)
            {
                // Check if any pen data has sw != 0 (pen down)
                hasSignatureData = penData.Any(pd => pd.sw != 0);
            }
            else if (penTimeData != null)
            {
                // Check if any pen time data has sw != 0 (pen down)
                hasSignatureData = penTimeData.Any(pd => pd.sw != 0);
            }

            // If no signature data, return null and show message
            if (!hasSignatureData)
            {
                MessageBox.Show("No signature detected. Please draw your signature before saving.",
                              "No Signature", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            float scale = 0.1f; // smaller = higher resolution
            int width = (int)(capability.tabletMaxX * scale);
            int height = (int)(capability.tabletMaxY * scale);

            // Create bitmap with 32-bit ARGB format for transparency
            using (Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // Clear with transparent background instead of white
                gfx.Clear(Color.Transparent);

                using (Pen pen = new Pen(Color.Black, 2f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

                    if (penData != null)
                    {
                        for (int i = 1; i < penData.Count; i++)
                        {
                            var pd0 = penData[i - 1];
                            var pd1 = penData[i];

                            if (pd0.sw != 0 && pd1.sw != 0)
                            {
                                float x0 = pd0.x * scale;
                                float y0 = pd0.y * scale;
                                float x1 = pd1.x * scale;
                                float y1 = pd1.y * scale;
                                gfx.DrawLine(pen, x0, y0, x1, y1);
                            }
                        }
                    }
                    else if (penTimeData != null)
                    {
                        for (int i = 1; i < penTimeData.Count; i++)
                        {
                            var pd0 = penTimeData[i - 1];
                            var pd1 = penTimeData[i];

                            if (pd0.sw != 0 && pd1.sw != 0)
                            {
                                float x0 = pd0.x * scale;
                                float y0 = pd0.y * scale;
                                float x1 = pd1.x * scale;
                                float y1 = pd1.y * scale;
                                gfx.DrawLine(pen, x0, y0, x1, y1);
                            }
                        }
                    }
                }

                // Create Employee_Signatures folder on Desktop if it doesn't exist
                //string signaturesFolder = @"D:\Employee_Signatures";
                string signaturesFolder = @"\\192.168.15.6\d$\YunuscoERP\App_Data\HR\TemporarySignature";

                if (!System.IO.Directory.Exists(signaturesFolder))
                    System.IO.Directory.CreateDirectory(signaturesFolder);

                // Save with employee ID in filename
                string fileName = $"{currentEmployeeId}";
                string filePath = System.IO.Path.Combine(signaturesFolder, fileName);

                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                //return filePath;
                return $"\\App_Data\\HR\\TemporarySignature\\{currentEmployeeId}.png";
            }
        }

        private async void UpdateSignaturePathInDatabase(string idCardNo, string signaturePath)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbConn))
                {
                    conn.Open();
                    string query = @"UPDATE HrEmployee 
                            SET SignatureImg = @SignatureImg, 
                                ModifiedDate = @ModifiedDate 
                            WHERE IdCardNo = @IdCardNo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SignatureImg", signaturePath);
                        cmd.Parameters.AddWithValue("@ModifiedDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@IdCardNo", idCardNo);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        string message = rowsAffected > 0
                            ? "Signature path updated successfully in desktop folder!"
                            : "Failed to update signature path in database.";

                        // Show message and auto-close after 2 seconds
                        await ShowTimedMessageBox(message);
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowTimedMessageBox($"Error updating database: {ex.Message}");
            }
        }

        private async Task ShowTimedMessageBox(string message)
        {
            var messageForm = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Information",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label()
            {
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(10)
            };
            messageForm.Controls.Add(label);

            messageForm.Show();
            await Task.Delay(2000);
            messageForm.Close();
        }

        // Optional: Add a method to verify the update
        private void VerifySignatureUpdate(string idCardNo)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(dbConn))
                {
                    conn.Open();
                    string query = "SELECT SignatureImg FROM HrEmployee WHERE IdCardNo = @IdCardNo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdCardNo", idCardNo);
                        var result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            MessageBox.Show($"Signature path in database: {result}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Verification error: {ex.Message}");
            }
        }

        private void DemoButtonsForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Initialize UI
                uiHelper.InitializeUI(button1, txtPenDataCount);
                uiHelper.SetVersionInfo();

                // Update status immediately to test connection
                uiHelper.UpdateStatus("Initializing...", Color.Blue);

                // Check system status with delays to see updates
                System.Threading.Thread.Sleep(1000); // So you can see the status change

                bool dbConnected = uiHelper.CheckDatabaseConnection();

                System.Threading.Thread.Sleep(1000); // So you can see the status change

                bool deviceConnected = uiHelper.CheckWacomDevice();

                // Enable button only if both checks pass
                button1.Enabled = dbConnected && deviceConnected;

                if (dbConnected && deviceConnected)
                {
                    uiHelper.UpdateStatus("✓ System Ready - Click 'Capture Signature' to start", Color.Green);
                }
                else
                {
                    uiHelper.UpdateStatus("✗ System Not Ready - Check connections", Color.Red);
                }
            }
            catch (Exception ex)
            {
                uiHelper.UpdateStatus($"✗ Error: {ex.Message}", Color.Red);
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void chkUseEncryption_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {
            // Refresh system status when user clicks the status label
            bool dbConnected = uiHelper.CheckDatabaseConnection();
            bool deviceConnected = uiHelper.CheckWacomDevice();

            if (dbConnected && deviceConnected)
            {
                uiHelper.UpdateStatus("✓ System Ready", Color.Green);
            }
            else
            {
                uiHelper.UpdateStatus("✗ System Issues - Some checks failed", Color.Red);
            }

            button1.Enabled = dbConnected && deviceConnected;
        }
    }
}