// =====COPYRIGHT=====
// Code originally retrieved from http://www.vbforums.com/showthread.php?t=547778 - no license information supplied
// =====COPYRIGHT=====
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ToastNotifications
{
    public partial class Notification : Form
    {
        private static List<Notification> openNotifications = new List<Notification>();
        private bool _allowFocus;
        private readonly FormAnimator _animator;
        private IntPtr _currentForegroundWindow;
        private List<string> messages = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="body"></param>
        /// <param name="duration"></param>
        /// <param name="animation"></param>
        /// <param name="direction"></param>
        public Notification(string title, int duration, FormAnimator.AnimationMethod animation, FormAnimator.AnimationDirection direction)
        {
            InitializeComponent();

            if (duration < 0)
                duration = int.MaxValue;
            else
                duration = duration * 1000;

            lifeTimer.Interval = duration;
            labelTitle.Text = title;

            _animator = new FormAnimator(this, animation, direction, 500);

            Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, Width - 5, Height - 5, 20, 20));
        }

        #region Methods

        /// <summary>
        /// Displays the form
        /// </summary>
        /// <remarks>
        /// Required to allow the form to determine the current foreground window before being displayed
        /// </remarks>
        public new void Show()
        {
            // Determine the current foreground window so it can be reactivated each time this form tries to get the focus
            _currentForegroundWindow = NativeMethods.GetForegroundWindow();

            base.Show();
        }

        private string LimitLines(string text, int linesCount)
        {
            var graphics = CreateGraphics();
            var controlSize = new SizeF(labelBody.Width, labelBody.Height);

            var lineHeight = graphics.MeasureString(".", labelBody.Font, controlSize).Height;
            while (graphics.MeasureString(text, labelBody.Font, controlSize).Height > (lineHeight * linesCount))
            {
                text = text.Substring(0, text.Length - 1);
                text = text.Substring(0, text.Length - 3);
                text += "...";
            }

            return text;
        }

        private double CountLines(string text)
        {
            var graphics = CreateGraphics();
            var controlSize = new SizeF(labelBody.Width, labelBody.Height);

            var lineHeight = graphics.MeasureString(".", labelBody.Font, controlSize).Height;
            return Math.Round(graphics.MeasureString(text, labelBody.Font, controlSize).Height/lineHeight);
        }

        public void AddMessage(string message)
        {
            // All the existing chat messages are limited to 1 message
            for (int i = 0; i < messages.Count; i++)
            {
                messages[i] = LimitLines(messages[i], 1);
            }

            // Limit it total line count to show a bit more (because it is a new message)
            message = LimitLines(message, 3);

            // If the amount of messages is full, remove the oldest one
            while (CountLines(message) + messages.Count > 5)
            {
                messages.RemoveAt(0);
            }

            // Add the newest message
            messages.Add(message);
            
            labelBody.Text = string.Join(Environment.NewLine, messages);
        }

        #endregion // Methods

        #region Event Handlers

        private void Notification_Load(object sender, EventArgs e)
        {
            var totalOpenHeight = openNotifications.Count * Height;

            // Display the form just above the system tray.
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width,
                                      Screen.PrimaryScreen.WorkingArea.Height - Height - totalOpenHeight);

            // Move each open form upwards to make room for this one
            //foreach (Notification openForm in openNotifications)
            //{
            //    openForm.Top -= Height;
            //}

            openNotifications.Add(this);
            lifeTimer.Start();
        }

        private void Notification_Activated(object sender, EventArgs e)
        {
            // Prevent the form taking focus when it is initially shown
            if (!_allowFocus)
            {
                // Activate the window that previously had focus
                NativeMethods.SetForegroundWindow(_currentForegroundWindow);
            }
        }

        private void Notification_Shown(object sender, EventArgs e)
        {
            // Once the animation has completed the form can receive focus
            _allowFocus = true;

            // Close the form by sliding down.
            _animator.Duration = 0;
            _animator.Direction = FormAnimator.AnimationDirection.Right;
        }

        private void Notification_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Move down any open forms above this one
            var isAbove = false;
            foreach (Notification openForm in openNotifications)
            {
                if (openForm == this)
                {
                    // Remaining forms are above this one
                    isAbove = true;
                }
                if (isAbove)
                {
                    openForm.Top += Height;
                }
            }

            openNotifications.Remove(this);
        }

        private void lifeTimer_Tick(object sender, EventArgs e)
        {
            Close();
        }

        private void Notification_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void labelTitle_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void labelRO_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion // Event Handlers
    }
}