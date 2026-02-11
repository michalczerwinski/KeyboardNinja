using Windows.Win32;
using Windows.Win32.Foundation;

namespace KeyboardNinja.Helpers;

internal class FormHelper
{
	private static Rectangle GetActiveWindowRectangle()
	{
		var activeWindow = PInvoke.GetForegroundWindow();
		RECT rect;
		PInvoke.GetWindowRect(activeWindow, out rect);

		return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
	}

	public static void ToggleForm<TForm>(Action<TForm>? init = null) where TForm : Form, new()
	{
		var form = Application.OpenForms.OfType<TForm>().FirstOrDefault();
		if (form == null)
		{
			var position = GetActiveWindowRectangle();

			form = new TForm();
			form.HandleCreated += (s, e) =>
			{
				if (position != Rectangle.Empty)
				{
					TryToMoveWindow(form, position);
				}

				init?.Invoke(form);
			};
			form.ShowDialog();
		}
		else
		{
			form.Invoke(() =>
			{
				if (form.Visible)
				{
					form.Hide();
				}
			});
		}
	}

	private static void TryToMoveWindow<TForm>(TForm form, Rectangle position) where TForm : Form, new()
	{
		// Compute intended top-left based on the center of the position rect
		var middle = new Point(position.X + position.Width / 2, position.Y + position.Height / 2);
		var destX = middle.X - form.Width / 2;
		var destY = middle.Y - form.Height / 2;
		var topLeft = new Point(destX, destY);
		var bottomRight = new Point(destX + form.Width - 1, destY + form.Height - 1);
		try
		{
			var s1 = Screen.FromPoint(topLeft);
			var s2 = Screen.FromPoint(bottomRight);
			if (s1.DeviceName == s2.DeviceName)
			{
				PInvoke.MoveWindow((HWND)form.Handle, destX, destY, form.Width, form.Height, true);
			}
			else
			{
				// fallback to primary screen center
				var screen = Screen.PrimaryScreen ?? Screen.AllScreens.First();
				var wa = screen.WorkingArea;
				var cx = wa.Left + (wa.Width - form.Width) / 2;
				var cy = wa.Top + (wa.Height - form.Height) / 2;
				PInvoke.MoveWindow((HWND)form.Handle, cx, cy, form.Width, form.Height, true);
			}
		}
		catch
		{
			var screen = Screen.PrimaryScreen ?? Screen.AllScreens.First();
			var wa = screen.WorkingArea;
			var cx = wa.Left + (wa.Width - form.Width) / 2;
			var cy = wa.Top + (wa.Height - form.Height) / 2;
			PInvoke.MoveWindow((HWND)form.Handle, cx, cy, form.Width, form.Height, true);
		}
	}
}
