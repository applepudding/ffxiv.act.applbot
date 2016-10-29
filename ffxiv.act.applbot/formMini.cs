using System;
using System.Drawing;
using System.Windows.Forms;

namespace ffxiv.act.applbot
{
    public partial class formMini : Form
    {
        public formMini()
        {
            InitializeComponent();
        }

        public Label uxlbl_eventCurrent = new Label();
        public Label uxlbl_eventNext = new Label();
        public Label uxlbl_eventCountdown = new Label();

        private void scaleFont(Label lab)
        {
            if (lab.Text.Length > 0)
            {
                Image fakeImage = new Bitmap(1, 1); //As we cannot use CreateGraphics() in a class library, so the fake image is used to load the Graphics.
                Graphics graphics = Graphics.FromImage(fakeImage);

                SizeF extent = graphics.MeasureString(lab.Text, lab.Font);

                float hRatio = lab.Height / extent.Height;
                float wRatio = lab.Width / extent.Width;
                float ratio = (hRatio < wRatio) ? hRatio : wRatio;

                float newSize = lab.Font.Size * ratio;

                lab.Font = new Font(lab.Font.FontFamily, newSize, lab.Font.Style);
            }
        }

        private void formMini_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void formMini_Shown(object sender, EventArgs e)
        {
            uxlbl_eventCurrent.Show();
            uxlbl_eventNext.Show();
            uxlbl_eventCountdown.Show();
            scaleFont(uxlbl_eventCurrent);
            scaleFont(uxlbl_eventNext);
            scaleFont(uxlbl_eventCountdown);
        }

        private void lbl_eventCurrent_SizeChanged(object sender, EventArgs e)
        {
            scaleFont(uxlbl_eventCurrent);
        }

        private void lbl_eventNext_SizeChanged(object sender, EventArgs e)
        {
            scaleFont(uxlbl_eventNext);
        }

        private void lbl_eventCountdown_SizeChanged(object sender, EventArgs e)
        {
            scaleFont(uxlbl_eventCountdown);
        }

        private void lbl_eventCurrent_TextChanged(object sender, EventArgs e)
        {
            scaleFont(uxlbl_eventCurrent);
        }

        private void lbl_eventNext_TextChanged(object sender, EventArgs e)
        {
            scaleFont(uxlbl_eventNext);
        }

        private void formMini_Load(object sender, EventArgs e)
        {
            uxlbl_eventCurrent = this.lbl_eventCurrent;
            uxlbl_eventNext = this.lbl_eventNext;
            uxlbl_eventCountdown = this.lbl_eventCountdown;
            this.lbl_eventCurrent.Hide();
            this.lbl_eventNext.Hide();
            this.lbl_eventCountdown.Hide();
        }
    }
}
