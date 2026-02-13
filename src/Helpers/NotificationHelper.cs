using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace KeyboardNinja.Helpers;

public static class NotificationHelper
{
	private static Rectangle GetTargetWorkingArea()
	{
		try
		{
			// Prefer an open visible form from this application
			var appForm = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f.Visible);
			if (appForm != null)
			{
				return Screen.FromControl(appForm).WorkingArea;
			}

			// Otherwise try the foreground window's screen
			var fg = PInvoke.GetForegroundWindow();
			if ((IntPtr)fg != IntPtr.Zero)
			{
				return Screen.FromHandle((IntPtr)fg).WorkingArea;
			}
		}
		catch { }

		return Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 800, 600);
	}
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

	public static Form ShowToast(string message, int? durationMs)
	{
		var form = new PopupForm(message);
		var workingArea = GetTargetWorkingArea();
		form.Load += (_, _) =>
		{
			form.Location = new Point(workingArea.Right - form.Width - 16, workingArea.Bottom - form.Height - 16);
		};


		if (durationMs.HasValue)
		{
			System.Windows.Forms.Timer? timer = new() { Interval = durationMs.Value };
			timer.Tick += (_, _) =>
			{
				timer.Stop();
				form.Close();
				form.Dispose();
			};
			timer.Start();
		}

		form.Show();
		PInvoke.SetWindowPos((HWND)form.Handle, (HWND)(-1), 0, 0, 0, 0, SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW);

		if (durationMs.HasValue)
		{
			Application.DoEvents();
			while (form.Visible)
			{
				Thread.Sleep(50);
				Application.DoEvents();
			}
		}

		return form;
	}
}
