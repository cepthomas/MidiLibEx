#if _XXX


using Ephemera.MidiLib;

namespace Ephemera.MidiLibEx.Test
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnOpen = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            btnLogMidi = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            btnKillMidi = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            btnPlay = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            btnRewind = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            btnExportCsv = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            btnExportMidi = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            cmbDrumChannel1 = new System.Windows.Forms.ToolStripComboBox();
            toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            cmbDrumChannel2 = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            btnStuff = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            btnDocs = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            btnSettings = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            txtViewer = new Ephemera.NBagOfUis.TextViewer();
            lbPatterns = new System.Windows.Forms.CheckedListBox();
            btnAll = new System.Windows.Forms.Button();
            btnNone = new System.Windows.Forms.Button();
            //nudTempo = new System.Windows.Forms.NumericUpDown();
            sldTempo = new NBagOfUis.Slider();
            label1 = new System.Windows.Forms.Label();
            sldVolume = new Ephemera.NBagOfUis.Slider();
            timeBar = new TimeBar();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sldTempo).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnOpen, toolStripSeparator12, btnLogMidi, toolStripSeparator3, btnKillMidi, toolStripSeparator4, btnPlay, toolStripSeparator5, btnRewind, toolStripSeparator6, btnExportCsv, toolStripSeparator8, btnExportMidi, toolStripSeparator9, toolStripLabel1, cmbDrumChannel1, toolStripLabel2, cmbDrumChannel2, toolStripSeparator10, btnStuff, toolStripSeparator1, btnDocs, toolStripSeparator2, btnSettings, toolStripSeparator11 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1062, 27);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnOpen
            // 
            btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new System.Drawing.Size(44, 24);
            btnOpen.Text = "open";
            btnOpen.Click += Open_Click;
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new System.Drawing.Size(6, 27);
            // 
            // btnLogMidi
            // 
            btnLogMidi.CheckOnClick = true;
            btnLogMidi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnLogMidi.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnLogMidi.Name = "btnLogMidi";
            btnLogMidi.Size = new System.Drawing.Size(62, 24);
            btnLogMidi.Text = "log midi";
            btnLogMidi.ToolTipText = "Enable logging midi events";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // btnKillMidi
            // 
            btnKillMidi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnKillMidi.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnKillMidi.Name = "btnKillMidi";
            btnKillMidi.Size = new System.Drawing.Size(29, 24);
            btnKillMidi.Text = "kill";
            btnKillMidi.ToolTipText = "Kill all midi channels";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // btnPlay
            // 
            btnPlay.CheckOnClick = true;
            btnPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new System.Drawing.Size(38, 24);
            btnPlay.Text = "play";
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 27);
            // 
            // btnRewind
            // 
            btnRewind.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRewind.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRewind.Name = "btnRewind";
            btnRewind.Size = new System.Drawing.Size(54, 24);
            btnRewind.Text = "rewind";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(6, 27);
            // 
            // btnExportCsv
            // 
            btnExportCsv.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnExportCsv.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.Size = new System.Drawing.Size(75, 24);
            btnExportCsv.Text = "export csv";
            btnExportCsv.Click += Export_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(6, 27);
            // 
            // btnExportMidi
            // 
            btnExportMidi.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnExportMidi.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnExportMidi.Name = "btnExportMidi";
            btnExportMidi.Size = new System.Drawing.Size(82, 24);
            btnExportMidi.Text = "export midi";
            btnExportMidi.Click += Export_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new System.Drawing.Size(35, 24);
            toolStripLabel1.Text = "Dr1:";
            // 
            // cmbDrumChannel1
            // 
            cmbDrumChannel1.AutoSize = false;
            cmbDrumChannel1.Name = "cmbDrumChannel1";
            cmbDrumChannel1.Size = new System.Drawing.Size(50, 27);
            cmbDrumChannel1.SelectedIndexChanged += DrumChannel_SelectedIndexChanged;
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new System.Drawing.Size(35, 24);
            toolStripLabel2.Text = "Dr2:";
            // 
            // cmbDrumChannel2
            // 
            cmbDrumChannel2.AutoSize = false;
            cmbDrumChannel2.Name = "cmbDrumChannel2";
            cmbDrumChannel2.Size = new System.Drawing.Size(50, 27);
            cmbDrumChannel2.SelectedIndexChanged += DrumChannel_SelectedIndexChanged;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(6, 27);
            // 
            // btnStuff
            // 
            btnStuff.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnStuff.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnStuff.Name = "btnStuff";
            btnStuff.Size = new System.Drawing.Size(40, 24);
            btnStuff.Text = "stuff";
            btnStuff.Click += Stuff_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // btnDocs
            // 
            btnDocs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnDocs.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnDocs.Name = "btnDocs";
            btnDocs.Size = new System.Drawing.Size(41, 24);
            btnDocs.Text = "docs";
            btnDocs.Click += Docs_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // btnSettings
            // 
            btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new System.Drawing.Size(61, 24);
            btnSettings.Text = "settings";
            btnSettings.Click += Settings_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new System.Drawing.Size(6, 27);
            // 
            // txtViewer
            // 
            txtViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtViewer.Location = new System.Drawing.Point(633, 130);
            txtViewer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            txtViewer.MaxText = 5000;
            txtViewer.Name = "txtViewer";
            txtViewer.Prompt = "";
            txtViewer.Size = new System.Drawing.Size(417, 401);
            txtViewer.TabIndex = 58;
            txtViewer.WordWrap = true;
            // 
            // lbPatterns
            // 
            lbPatterns.BackColor = System.Drawing.SystemColors.Control;
            lbPatterns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbPatterns.FormattingEnabled = true;
            lbPatterns.Location = new System.Drawing.Point(8, 130);
            lbPatterns.Name = "lbPatterns";
            lbPatterns.Size = new System.Drawing.Size(217, 401);
            lbPatterns.TabIndex = 89;
            lbPatterns.SelectedIndexChanged += Patterns_SelectedIndexChanged;
            // 
            // btnAll
            // 
            btnAll.Location = new System.Drawing.Point(8, 94);
            btnAll.Name = "btnAll";
            btnAll.Size = new System.Drawing.Size(50, 28);
            btnAll.TabIndex = 90;
            btnAll.Text = "all";
            btnAll.UseVisualStyleBackColor = true;
            btnAll.Click += AllOrNone_Click;
            // 
            // btnNone
            // 
            btnNone.Location = new System.Drawing.Point(66, 94);
            btnNone.Name = "btnNone";
            btnNone.Size = new System.Drawing.Size(50, 28);
            btnNone.TabIndex = 91;
            btnNone.Text = "none";
            btnNone.UseVisualStyleBackColor = true;
            btnNone.Click += AllOrNone_Click;
            // 
            // sldTempo
            // 
            //nudTempo.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            //nudTempo.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            //nudTempo.Minimum = new decimal(new int[] { 40, 0, 0, 0 });
            //nudTempo.Value = new decimal(new int[] { 40, 0, 0, 0 });

            sldTempo.Location = new System.Drawing.Point(159, 60);
            sldTempo.Name = "sldTempo";
            sldTempo.Size = new System.Drawing.Size(58, 26);
            sldTempo.TabIndex = 96;

            sldTempo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            sldTempo.DrawColor = System.Drawing.Color.White;
            sldTempo.Label = "";
            sldTempo.Location = new System.Drawing.Point(8, 38);
            sldTempo.Maximum = 240D;
            sldTempo.Minimum = 40D;
            sldTempo.Name = "nudTempo";
            sldTempo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            sldTempo.Resolution = 1D;
            sldTempo.Value = 100D;

            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(159, 38);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 19);
            label1.TabIndex = 98;
            label1.Text = "BPM";
            // 
            // sldVolume
            // 
            sldVolume.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            sldVolume.DrawColor = System.Drawing.Color.White;
            sldVolume.Label = "";
            sldVolume.Location = new System.Drawing.Point(8, 38);
            sldVolume.Maximum = 10D;
            sldVolume.Minimum = 0D;
            sldVolume.Name = "sldVolume";
            sldVolume.Orientation = System.Windows.Forms.Orientation.Horizontal;
            sldVolume.Resolution = 0.1D;
            sldVolume.Size = new System.Drawing.Size(138, 48);
            sldVolume.TabIndex = 99;
            sldVolume.Value = 5D;
            // 
            // barBar
            // 
            timeBar.Name = "barBar";
            timeBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            timeBar.FontLarge = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            timeBar.FontSmall = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            timeBar.Location = new System.Drawing.Point(244, 38);
            timeBar.DrawColor = System.Drawing.Color.Black;
            timeBar.SelectedColor = System.Drawing.Color.White;
            timeBar.Size = new System.Drawing.Size(701, 48);
            timeBar.TabIndex = 100;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1062, 542);
            Controls.Add(timeBar);
            Controls.Add(sldVolume);
            Controls.Add(label1);
            Controls.Add(sldTempo);
            Controls.Add(btnNone);
            Controls.Add(btnAll);
            Controls.Add(lbPatterns);
            Controls.Add(txtViewer);
            Controls.Add(toolStrip1);
            Location = new System.Drawing.Point(300, 50);
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "Midi Lib";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)sldTempo).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnLogMidi;
        private System.Windows.Forms.ToolStripButton btnKillMidi;
        private System.Windows.Forms.ToolStripButton btnPlay;
        private System.Windows.Forms.ToolStripButton btnRewind;
        private System.Windows.Forms.ToolStripButton btnExportCsv;
        private System.Windows.Forms.ToolStripButton btnExportMidi;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox cmbDrumChannel1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox cmbDrumChannel2;

        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;

        private System.Windows.Forms.ToolStripButton btnStuff;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnDocs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;

        private System.Windows.Forms.CheckedListBox lbPatterns;
        private System.Windows.Forms.Button btnAll;
        private System.Windows.Forms.Button btnNone;
        //private System.Windows.Forms.NumericUpDown nudTempo;
        private NBagOfUis.Slider sldTempo;
        private System.Windows.Forms.Label label1;
        private TimeBar timeBar;

        private NBagOfUis.Slider sldVolume;
        private NBagOfUis.TextViewer txtViewer;
    }
}

#endif