namespace ffxiv.act.applbot
{
    partial class formMini
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
            this.lbl_eventCurrent = new System.Windows.Forms.Label();
            this.lbl_eventCountdown = new System.Windows.Forms.Label();
            this.lbl_eventNext = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_eventCurrent
            // 
            this.lbl_eventCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_eventCurrent.BackColor = System.Drawing.Color.Black;
            this.lbl_eventCurrent.ForeColor = System.Drawing.Color.Yellow;
            this.lbl_eventCurrent.Location = new System.Drawing.Point(12, 9);
            this.lbl_eventCurrent.Name = "lbl_eventCurrent";
            this.lbl_eventCurrent.Size = new System.Drawing.Size(148, 38);
            this.lbl_eventCurrent.TabIndex = 1;
            this.lbl_eventCurrent.Text = "Dragoon Death 20 of 100 :D";
            this.lbl_eventCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_eventCurrent.SizeChanged += new System.EventHandler(this.lbl_eventCurrent_SizeChanged);
            this.lbl_eventCurrent.TextChanged += new System.EventHandler(this.lbl_eventCurrent_TextChanged);
            // 
            // lbl_eventCountdown
            // 
            this.lbl_eventCountdown.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_eventCountdown.Location = new System.Drawing.Point(3, 9);
            this.lbl_eventCountdown.Name = "lbl_eventCountdown";
            this.lbl_eventCountdown.Size = new System.Drawing.Size(53, 74);
            this.lbl_eventCountdown.TabIndex = 2;
            this.lbl_eventCountdown.Text = "25";
            this.lbl_eventCountdown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_eventCountdown.SizeChanged += new System.EventHandler(this.lbl_eventCountdown_SizeChanged);
            // 
            // lbl_eventNext
            // 
            this.lbl_eventNext.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_eventNext.ForeColor = System.Drawing.Color.DimGray;
            this.lbl_eventNext.Location = new System.Drawing.Point(9, 10);
            this.lbl_eventNext.Name = "lbl_eventNext";
            this.lbl_eventNext.Size = new System.Drawing.Size(151, 11);
            this.lbl_eventNext.TabIndex = 3;
            this.lbl_eventNext.Text = "Dragoon Death 21 of 100";
            this.lbl_eventNext.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbl_eventNext.SizeChanged += new System.EventHandler(this.lbl_eventNext_SizeChanged);
            this.lbl_eventNext.TextChanged += new System.EventHandler(this.lbl_eventNext_TextChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(12, 12);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lbl_eventCountdown);
            this.splitContainer1.Size = new System.Drawing.Size(235, 94);
            this.splitContainer1.SplitterDistance = 172;
            this.splitContainer1.TabIndex = 6;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.lbl_eventCurrent);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.lbl_eventNext);
            this.splitContainer2.Size = new System.Drawing.Size(172, 94);
            this.splitContainer2.SplitterDistance = 58;
            this.splitContainer2.TabIndex = 0;
            // 
            // formMini
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(259, 116);
            this.Controls.Add(this.splitContainer1);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(275, 125);
            this.Name = "formMini";
            this.Text = "minibot";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formMini_FormClosing);
            this.Load += new System.EventHandler(this.formMini_Load);
            this.Shown += new System.EventHandler(this.formMini_Shown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lbl_eventCurrent;
        private System.Windows.Forms.Label lbl_eventCountdown;
        private System.Windows.Forms.Label lbl_eventNext;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}