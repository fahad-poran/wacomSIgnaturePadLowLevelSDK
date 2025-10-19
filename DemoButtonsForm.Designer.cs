namespace DemoButtons
{
  partial class DemoButtonsForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DemoButtonsForm));
            this.button1 = new System.Windows.Forms.Button();
            this.txtPenDataCount = new System.Windows.Forms.TextBox();
            this.lblPenDataCount = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.button1.Location = new System.Drawing.Point(143, 96);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(122, 37);
            this.button1.TabIndex = 0;
            this.button1.Text = "ADD SIGNATURE";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtPenDataCount
            // 
            this.txtPenDataCount.Location = new System.Drawing.Point(102, 155);
            this.txtPenDataCount.Name = "txtPenDataCount";
            this.txtPenDataCount.ReadOnly = true;
            this.txtPenDataCount.Size = new System.Drawing.Size(51, 20);
            this.txtPenDataCount.TabIndex = 1;
            // 
            // lblPenDataCount
            // 
            this.lblPenDataCount.AutoSize = true;
            this.lblPenDataCount.Location = new System.Drawing.Point(12, 158);
            this.lblPenDataCount.Name = "lblPenDataCount";
            this.lblPenDataCount.Size = new System.Drawing.Size(83, 13);
            this.lblPenDataCount.TabIndex = 2;
            this.lblPenDataCount.Text = "Pen data count:";
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = System.Drawing.Color.Green;
            this.lblStatus.Location = new System.Drawing.Point(12, 18);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(64, 13);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Checking....";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStatus.Click += new System.EventHandler(this.lblStatus_Click);
            // 
            // DemoButtonsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(401, 180);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblPenDataCount);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtPenDataCount);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DemoButtonsForm";
            this.Text = "Yunusco ERP Signature Application";
            this.Load += new System.EventHandler(this.DemoButtonsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
      private System.Windows.Forms.TextBox txtPenDataCount;
      private System.Windows.Forms.Label lblPenDataCount;
        private System.Windows.Forms.Label lblStatus;
    }
}

