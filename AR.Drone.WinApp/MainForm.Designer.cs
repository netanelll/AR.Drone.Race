namespace AR.Drone.WinApp
{
    partial class MainForm
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
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.pbVideo = new System.Windows.Forms.PictureBox();
            this.btnFlatTrim = new System.Windows.Forms.Button();
            this.btnEmergency = new System.Windows.Forms.Button();
            this.tmrStateUpdate = new System.Windows.Forms.Timer(this.components);
            this.btnSwitchCam = new System.Windows.Forms.Button();
            this.tvInfo = new System.Windows.Forms.TreeView();
            this.tmrVideoUpdate = new System.Windows.Forms.Timer(this.components);
            this.btnReset = new System.Windows.Forms.Button();
            this.btnReadConfig = new System.Windows.Forms.Button();
            this.btnSendConfig = new System.Windows.Forms.Button();
            this.btnStartRecording = new System.Windows.Forms.Button();
            this.btnStopRecording = new System.Windows.Forms.Button();
            this.btnReplay = new System.Windows.Forms.Button();
            this.LedsShow = new System.Windows.Forms.Button();
            this.tmrChangeQuadLocation = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.tbBattery = new System.Windows.Forms.TextBox();
            this.tbMin = new System.Windows.Forms.Label();
            this.tbSec = new System.Windows.Forms.Label();
            this.tblScores = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.tbNewScore = new System.Windows.Forms.TextBox();
            this.btnNewScore = new System.Windows.Forms.Button();
            this.name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.score = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.pbVideo)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Activate";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(93, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Deactivate";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // pbVideo
            // 
            this.pbVideo.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pbVideo.Location = new System.Drawing.Point(12, 41);
            this.pbVideo.Name = "pbVideo";
            this.pbVideo.Size = new System.Drawing.Size(640, 360);
            this.pbVideo.TabIndex = 2;
            this.pbVideo.TabStop = false;
            // 
            // btnFlatTrim
            // 
            this.btnFlatTrim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnFlatTrim.Location = new System.Drawing.Point(563, 510);
            this.btnFlatTrim.Name = "btnFlatTrim";
            this.btnFlatTrim.Size = new System.Drawing.Size(75, 23);
            this.btnFlatTrim.TabIndex = 3;
            this.btnFlatTrim.Text = "Flat Trim";
            this.btnFlatTrim.UseVisualStyleBackColor = true;
            this.btnFlatTrim.Click += new System.EventHandler(this.btnFlatTrim_Click);
            // 
            // btnEmergency
            // 
            this.btnEmergency.Location = new System.Drawing.Point(569, 12);
            this.btnEmergency.Name = "btnEmergency";
            this.btnEmergency.Size = new System.Drawing.Size(83, 23);
            this.btnEmergency.TabIndex = 6;
            this.btnEmergency.Text = "Emergency";
            this.btnEmergency.UseVisualStyleBackColor = true;
            this.btnEmergency.Click += new System.EventHandler(this.btnEmergency_Click);
            // 
            // tmrStateUpdate
            // 
            this.tmrStateUpdate.Interval = 500;
            this.tmrStateUpdate.Tick += new System.EventHandler(this.tmrStateUpdate_Tick);
            // 
            // btnSwitchCam
            // 
            this.btnSwitchCam.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSwitchCam.Location = new System.Drawing.Point(563, 416);
            this.btnSwitchCam.Name = "btnSwitchCam";
            this.btnSwitchCam.Size = new System.Drawing.Size(89, 23);
            this.btnSwitchCam.TabIndex = 8;
            this.btnSwitchCam.Text = "Video Channel";
            this.btnSwitchCam.UseVisualStyleBackColor = true;
            this.btnSwitchCam.Click += new System.EventHandler(this.btnSwitchCam_Click);
            // 
            // tvInfo
            // 
            this.tvInfo.Location = new System.Drawing.Point(1002, 32);
            this.tvInfo.Name = "tvInfo";
            this.tvInfo.Size = new System.Drawing.Size(227, 192);
            this.tvInfo.TabIndex = 18;
            // 
            // tmrVideoUpdate
            // 
            this.tmrVideoUpdate.Interval = 20;
            this.tmrVideoUpdate.Tick += new System.EventHandler(this.tmrVideoUpdate_Tick);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(480, 12);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(83, 23);
            this.btnReset.TabIndex = 19;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnReadConfig
            // 
            this.btnReadConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReadConfig.Location = new System.Drawing.Point(563, 451);
            this.btnReadConfig.Name = "btnReadConfig";
            this.btnReadConfig.Size = new System.Drawing.Size(89, 23);
            this.btnReadConfig.TabIndex = 20;
            this.btnReadConfig.Text = "Read Config";
            this.btnReadConfig.UseVisualStyleBackColor = true;
            this.btnReadConfig.Click += new System.EventHandler(this.btnReadConfig_Click);
            // 
            // btnSendConfig
            // 
            this.btnSendConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSendConfig.Location = new System.Drawing.Point(563, 480);
            this.btnSendConfig.Name = "btnSendConfig";
            this.btnSendConfig.Size = new System.Drawing.Size(89, 23);
            this.btnSendConfig.TabIndex = 21;
            this.btnSendConfig.Text = "Send Config";
            this.btnSendConfig.UseVisualStyleBackColor = true;
            this.btnSendConfig.Click += new System.EventHandler(this.btnSendConfig_Click);
            // 
            // btnStartRecording
            // 
            this.btnStartRecording.Location = new System.Drawing.Point(174, 12);
            this.btnStartRecording.Name = "btnStartRecording";
            this.btnStartRecording.Size = new System.Drawing.Size(75, 23);
            this.btnStartRecording.TabIndex = 22;
            this.btnStartRecording.Text = "Start Rec.";
            this.btnStartRecording.UseVisualStyleBackColor = true;
            this.btnStartRecording.Click += new System.EventHandler(this.btnStartRecording_Click);
            // 
            // btnStopRecording
            // 
            this.btnStopRecording.Location = new System.Drawing.Point(255, 12);
            this.btnStopRecording.Name = "btnStopRecording";
            this.btnStopRecording.Size = new System.Drawing.Size(75, 23);
            this.btnStopRecording.TabIndex = 23;
            this.btnStopRecording.Text = "Stop Rec.";
            this.btnStopRecording.UseVisualStyleBackColor = true;
            this.btnStopRecording.Click += new System.EventHandler(this.btnStopRecording_Click);
            // 
            // btnReplay
            // 
            this.btnReplay.Location = new System.Drawing.Point(336, 12);
            this.btnReplay.Name = "btnReplay";
            this.btnReplay.Size = new System.Drawing.Size(75, 23);
            this.btnReplay.TabIndex = 24;
            this.btnReplay.Text = "Replay";
            this.btnReplay.UseVisualStyleBackColor = true;
            this.btnReplay.Click += new System.EventHandler(this.btnReplay_Click);
            // 
            // LedsShow
            // 
            this.LedsShow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LedsShow.Location = new System.Drawing.Point(563, 539);
            this.LedsShow.Name = "LedsShow";
            this.LedsShow.Size = new System.Drawing.Size(75, 23);
            this.LedsShow.TabIndex = 28;
            this.LedsShow.Text = "Leds show";
            this.LedsShow.UseVisualStyleBackColor = true;
            this.LedsShow.Click += new System.EventHandler(this.LedsShow_Click);
            // 
            // tmrChangeQuadLocation
            // 
            this.tmrChangeQuadLocation.Tick += new System.EventHandler(this.ChangeQuadLocation_Tick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(697, 416);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 29;
            this.button1.Text = "Clean map";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnCleanMap_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(778, 416);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 30;
            this.button4.Text = "Draw Track";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.btnDrawTrack_Click);
            // 
            // tbBattery
            // 
            this.tbBattery.Location = new System.Drawing.Point(659, 14);
            this.tbBattery.Name = "tbBattery";
            this.tbBattery.ReadOnly = true;
            this.tbBattery.Size = new System.Drawing.Size(100, 20);
            this.tbBattery.TabIndex = 31;
            // 
            // tbMin
            // 
            this.tbMin.AutoSize = true;
            this.tbMin.Location = new System.Drawing.Point(679, 388);
            this.tbMin.Name = "tbMin";
            this.tbMin.Size = new System.Drawing.Size(13, 13);
            this.tbMin.TabIndex = 32;
            this.tbMin.Text = "0";
            // 
            // tbSec
            // 
            this.tbSec.AutoSize = true;
            this.tbSec.Location = new System.Drawing.Point(724, 388);
            this.tbSec.Name = "tbSec";
            this.tbSec.Size = new System.Drawing.Size(13, 13);
            this.tbSec.TabIndex = 33;
            this.tbSec.Text = "0";
            // 
            // tblScores
            // 
            this.tblScores.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.name,
            this.score});
            this.tblScores.GridLines = true;
            this.tblScores.Location = new System.Drawing.Point(255, 480);
            this.tblScores.Name = "tblScores";
            this.tblScores.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.tblScores.RightToLeftLayout = true;
            this.tblScores.Size = new System.Drawing.Size(156, 97);
            this.tblScores.TabIndex = 34;
            this.tblScores.UseCompatibleStateImageBehavior = false;
            this.tblScores.View = System.Windows.Forms.View.Details;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(333, 456);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 35;
            this.label1.Text = "טבלת שיאים";
            // 
            // tbNewScore
            // 
            this.tbNewScore.Location = new System.Drawing.Point(137, 480);
            this.tbNewScore.Name = "tbNewScore";
            this.tbNewScore.Size = new System.Drawing.Size(112, 20);
            this.tbNewScore.TabIndex = 36;
            // 
            // btnNewScore
            // 
            this.btnNewScore.Location = new System.Drawing.Point(162, 510);
            this.btnNewScore.Name = "btnNewScore";
            this.btnNewScore.Size = new System.Drawing.Size(87, 23);
            this.btnNewScore.TabIndex = 37;
            this.btnNewScore.Text = "הכנס לטבלה";
            this.btnNewScore.UseVisualStyleBackColor = true;
            this.btnNewScore.Click += new System.EventHandler(this.btnNewScore_Click);
            // 
            // name
            // 
            this.name.Text = "שם";
            // 
            // score
            // 
            this.score.Text = "תוצאה";
            this.score.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1241, 676);
            this.Controls.Add(this.btnNewScore);
            this.Controls.Add(this.tbNewScore);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tblScores);
            this.Controls.Add(this.tbSec);
            this.Controls.Add(this.tbMin);
            this.Controls.Add(this.tbBattery);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.LedsShow);
            this.Controls.Add(this.btnReplay);
            this.Controls.Add(this.btnStopRecording);
            this.Controls.Add(this.btnStartRecording);
            this.Controls.Add(this.btnSendConfig);
            this.Controls.Add(this.btnReadConfig);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.tvInfo);
            this.Controls.Add(this.btnSwitchCam);
            this.Controls.Add(this.btnEmergency);
            this.Controls.Add(this.btnFlatTrim);
            this.Controls.Add(this.pbVideo);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.Text = "AR.Drone Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.pbVideo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.PictureBox pbVideo;
        private System.Windows.Forms.Button btnFlatTrim;
        private System.Windows.Forms.Button btnEmergency;
        private System.Windows.Forms.Timer tmrStateUpdate;
        private System.Windows.Forms.Button btnSwitchCam;
        private System.Windows.Forms.TreeView tvInfo;
        private System.Windows.Forms.Timer tmrVideoUpdate;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnReadConfig;
        private System.Windows.Forms.Button btnSendConfig;
        private System.Windows.Forms.Button btnStartRecording;
        private System.Windows.Forms.Button btnStopRecording;
        private System.Windows.Forms.Button btnReplay;
        private System.Windows.Forms.Button LedsShow;
        private System.Windows.Forms.Timer tmrChangeQuadLocation;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox tbBattery;
        private System.Windows.Forms.Label tbMin;
        private System.Windows.Forms.Label tbSec;
        private System.Windows.Forms.ListView tblScores;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbNewScore;
        private System.Windows.Forms.Button btnNewScore;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.ColumnHeader score;
    }
}

