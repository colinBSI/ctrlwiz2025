namespace CtrlWiz.Forms.Views
{
    partial class HelpFormRVT
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.BLeft = new System.Windows.Forms.Button();
            this.BRight = new System.Windows.Forms.Button();
            this.BMid = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::CtrlWiz.Forms.Properties.Resources.MainHelpRVT;
            this.pictureBox1.Location = new System.Drawing.Point(-1, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(760, 487);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.BRight_Click);
            // 
            // BLeft
            // 
            this.BLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BLeft.BackColor = System.Drawing.Color.MediumAquamarine;
            this.BLeft.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BLeft.FlatAppearance.BorderColor = System.Drawing.Color.MediumAquamarine;
            this.BLeft.FlatAppearance.BorderSize = 0;
            this.BLeft.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.BLeft.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.BLeft.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BLeft.Location = new System.Drawing.Point(-3, 428);
            this.BLeft.Name = "BLeft";
            this.BLeft.Size = new System.Drawing.Size(190, 35);
            this.BLeft.TabIndex = 6;
            this.BLeft.UseVisualStyleBackColor = false;
            this.BLeft.Click += new System.EventHandler(this.BLeft_Click);
            // 
            // BRight
            // 
            this.BRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BRight.BackColor = System.Drawing.Color.MediumAquamarine;
            this.BRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BRight.FlatAppearance.BorderColor = System.Drawing.Color.MediumAquamarine;
            this.BRight.FlatAppearance.BorderSize = 0;
            this.BRight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.BRight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.BRight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BRight.Location = new System.Drawing.Point(551, 429);
            this.BRight.Name = "BRight";
            this.BRight.Size = new System.Drawing.Size(194, 35);
            this.BRight.TabIndex = 6;
            this.BRight.UseVisualStyleBackColor = false;
            this.BRight.Click += new System.EventHandler(this.BRight_Click);
            // 
            // BMid
            // 
            this.BMid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BMid.BackColor = System.Drawing.Color.MediumAquamarine;
            this.BMid.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BMid.FlatAppearance.BorderColor = System.Drawing.Color.MediumAquamarine;
            this.BMid.FlatAppearance.BorderSize = 0;
            this.BMid.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.BMid.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.BMid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BMid.Location = new System.Drawing.Point(313, 447);
            this.BMid.Name = "BMid";
            this.BMid.Size = new System.Drawing.Size(93, 25);
            this.BMid.TabIndex = 6;
            this.BMid.UseVisualStyleBackColor = false;
            this.BMid.Click += new System.EventHandler(this.BMid_Click);
            // 
            // HelpFormRVT
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 486);
            this.Controls.Add(this.BRight);
            this.Controls.Add(this.BMid);
            this.Controls.Add(this.BLeft);
            this.Controls.Add(this.pictureBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "HelpFormRVT";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Controller Map";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button BLeft;
        private System.Windows.Forms.Button BRight;
        private System.Windows.Forms.Button BMid;
    }
}