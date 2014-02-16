using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XBoxPresenterLibrary;

namespace PresenterGUI
{
    public partial class PresenterGUI : Form
    {

        private XBoxPresenter _presenter;
        private Thread _presenterThread;

        public PresenterGUI()
        {
            InitializeComponent();
            _presenter = new XBoxPresenter();
            _presenterThread = new Thread(_presenter.UpdateGameControllerStatus);            
            _presenter.XBoxControllerAButtonPressed += (x, y) => { SendKeys.SendWait("A"); };
            _presenter.XBoxControllerBackPressed += (x, y) => { SendKeys.SendWait("{ESC}"); };
            _presenter.XBoxControllerLeftTriggerPressed += (x, y) => { SendKeys.SendWait("{LEFT}"); };
            _presenter.XBoxControllerRightTriggerPressed += (x, y) => { SendKeys.SendWait("{RIGHT}"); };
            _presenter.XBoxControllerStartPressed += (x, y) => { SendKeys.SendWait("+{F5}"); };
            _presenterThread.Start();
           
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)  
            {         
                ShowInTaskbar = !(xboxNotifyIcon.Visible = true);
                xboxNotifyIcon.ShowBalloonTip(500);
            }
        }

        private void xboxNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            WindowState = FormWindowState.Normal;
            xboxNotifyIcon.Visible = !(ShowInTaskbar = true);
        }

        private void PresenterGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            _presenterThread.Abort();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int value = (int)numericUpDown1.Value;
            if (value >= 1)
            {
                shakyTimer.Interval = value * 1000 * 60;
                shakyTimer.Stop();
                shakyTimer.Start();
            }
            else
                shakyTimer.Stop();
        }

        private void MakeItVibrate()
        {
            _presenter.MakeItShake();
            var stopTimer = new System.Windows.Forms.Timer();
            stopTimer.Interval = 2000;
            stopTimer.Tick += (x, y) => { _presenter.MakeItStop(); stopTimer.Stop(); };
            stopTimer.Start();
        }

        private void shakyTimer_Tick(object sender, EventArgs e)
        {
            MakeItVibrate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutXBoxPresenterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutXBoxPresenter().ShowDialog();
        }

    }
}
