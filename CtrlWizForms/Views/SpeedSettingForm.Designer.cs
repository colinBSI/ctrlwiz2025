namespace CtrlWiz.Forms
{
    partial class SpeedSettingForm
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
            this.components = new System.ComponentModel.Container();
            this.LinearTrackBar = new System.Windows.Forms.TrackBar();
            this.Speed = new System.Windows.Forms.GroupBox();
            this.LinearLabel = new System.Windows.Forms.Label();
            this.Angular = new System.Windows.Forms.GroupBox();
            this.AngularLabel = new System.Windows.Forms.Label();
            this.AngularTrackBar = new System.Windows.Forms.TrackBar();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.LinearTrackBar)).BeginInit();
            this.Speed.SuspendLayout();
            this.Angular.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AngularTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // LinearTrackBar
            // 
            this.LinearTrackBar.Location = new System.Drawing.Point(3, 21);
            this.LinearTrackBar.Maximum = 100;
            this.LinearTrackBar.Name = "LinearTrackBar";
            this.LinearTrackBar.Size = new System.Drawing.Size(438, 45);
            this.LinearTrackBar.TabIndex = 0;
            this.toolTip1.SetToolTip(this.LinearTrackBar, "123456");
            this.LinearTrackBar.Value = 20;
            // 
            // Speed
            // 
            this.Speed.Controls.Add(this.LinearLabel);
            this.Speed.Controls.Add(this.LinearTrackBar);
            this.Speed.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Speed.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.Speed.Location = new System.Drawing.Point(8, 9);
            this.Speed.Name = "Speed";
            this.Speed.Size = new System.Drawing.Size(564, 69);
            this.Speed.TabIndex = 2;
            this.Speed.TabStop = false;
            this.Speed.Text = " Linear Speed";
            // 
            // LinearLabel
            // 
            this.LinearLabel.AutoSize = true;
            this.LinearLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LinearLabel.Location = new System.Drawing.Point(463, 22);
            this.LinearLabel.Name = "LinearLabel";
            this.LinearLabel.Size = new System.Drawing.Size(50, 18);
            this.LinearLabel.TabIndex = 1;
            this.LinearLabel.Text = "2 m/s";
            // 
            // Angular
            // 
            this.Angular.Controls.Add(this.AngularLabel);
            this.Angular.Controls.Add(this.AngularTrackBar);
            this.Angular.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Angular.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.Angular.Location = new System.Drawing.Point(7, 79);
            this.Angular.Name = "Angular";
            this.Angular.Size = new System.Drawing.Size(564, 69);
            this.Angular.TabIndex = 3;
            this.Angular.TabStop = false;
            this.Angular.Text = "Angular Speed";
            // 
            // AngularLabel
            // 
            this.AngularLabel.AutoSize = true;
            this.AngularLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AngularLabel.Location = new System.Drawing.Point(461, 23);
            this.AngularLabel.Name = "AngularLabel";
            this.AngularLabel.Size = new System.Drawing.Size(72, 18);
            this.AngularLabel.TabIndex = 2;
            this.AngularLabel.Text = "45 deg/s";
            // 
            // AngularTrackBar
            // 
            this.AngularTrackBar.Location = new System.Drawing.Point(3, 21);
            this.AngularTrackBar.Maximum = 100;
            this.AngularTrackBar.Name = "AngularTrackBar";
            this.AngularTrackBar.Size = new System.Drawing.Size(440, 45);
            this.AngularTrackBar.TabIndex = 0;
            this.AngularTrackBar.Value = 45;
            // 
            // SpeedSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 156);
            this.Controls.Add(this.Angular);
            this.Controls.Add(this.Speed);
            this.Name = "SpeedSettingForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.LinearTrackBar)).EndInit();
            this.Speed.ResumeLayout(false);
            this.Speed.PerformLayout();
            this.Angular.ResumeLayout(false);
            this.Angular.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AngularTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TrackBar LinearTrackBar;
        private System.Windows.Forms.GroupBox Speed;
        private System.Windows.Forms.GroupBox Angular;
        public System.Windows.Forms.TrackBar AngularTrackBar;
        private System.Windows.Forms.Label LinearLabel;
        public System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label AngularLabel;
        private System.Windows.Forms.ToolTip toolTip2;
    }
}