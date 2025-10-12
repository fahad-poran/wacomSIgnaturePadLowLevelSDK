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
        public DemoButtonsForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
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
                SignatureForm demo = new SignatureForm(usbDevice, false); // false = no encryption
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
                    SaveSignatureAsImage(penData, penTimeData, demo.getCapability());
                }

                demo.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveSignatureAsImage(List<wgssSTU.IPenData> penData,
                                          List<wgssSTU.IPenDataTimeCountSequence> penTimeData,
                                          wgssSTU.ICapability capability)
        {
            float scale = 0.1f; // smaller = higher resolution
            int width = (int)(capability.tabletMaxX * scale);
            int height = (int)(capability.tabletMaxY * scale);

            using (Bitmap bmp = new Bitmap(width, height))
            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                gfx.Clear(Color.White);

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

                string filePath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "SignatureSmooth_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");

                bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show("Smooth signature saved at:\n" + filePath);
            }
        }
    }
}
