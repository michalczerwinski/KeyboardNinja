namespace KeyboardNinja.Helpers;

public static class ToastNotificationHelper
{
	private sealed class PopupForm : Form
	{
		private readonly Label messageLabel;

		public PopupForm(string message)
		{
			FormBorderStyle = FormBorderStyle.None;
			StartPosition = FormStartPosition.Manual;
			ShowInTaskbar = false;
			TopMost = true;
			BackColor = Color.FromArgb(30, 30, 30);
			ForeColor = Color.White;
			Padding = new Padding(20);

			messageLabel = new Label
			{
				Text = message,
				AutoSize = true,
				TextAlign = ContentAlignment.MiddleCenter,
				ForeColor = ForeColor,
				BackColor = BackColor
			};

			Controls.Add(messageLabel);
			ClientSize = new Size(messageLabel.Width + Padding.Horizontal, messageLabel.Height + Padding.Vertical);
			messageLabel.AutoSize = false;
			messageLabel.Dock = DockStyle.Fill;
		}

		protected override bool ShowWithoutActivation => true;
		protected override CreateParams CreateParams
		{
			get
			{
				var cp = base.CreateParams;
				cp.ExStyle |= 0x08000000;
				return cp;
			}
		}
	}

	public static void Show(string message, int durationMs = 2000)
	{
		using var form = new PopupForm(message);
		var workingArea = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 800, 600);
		form.Load += (_, _) =>
		{
			form.Location = new Point(workingArea.Right - form.Width - 16, workingArea.Bottom - form.Height - 16);
		};
		var timer = new System.Windows.Forms.Timer { Interval = durationMs };
		timer.Tick += (_, _) =>
		{
			timer.Stop();
			form.Close();
		};
		timer.Start();
		form.Show();
		Application.DoEvents();
		while (form.Visible)
		{
			Thread.Sleep(50);
			Application.DoEvents();
		}
	}
}
