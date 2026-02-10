using Microsoft.Win32;
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
					var middle = new Point(position.X + position.Width / 2, position.Y + position.Height / 2);
					PInvoke.MoveWindow((HWND)form.Handle, middle.X - form.Width / 2, middle.Y - form.Height / 2, form.Width, form.Height, true);
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

	public static IReadOnlyList<(Guid DesktopId, string DesktopName)> GetWindowDesktops()
	{
		var list = new List<(Guid, string)>();

		using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops", writable: false);
		if (key != null)
		{
			if (key.GetValue("VirtualDesktopIDs") is byte[] ids)
			{
				const int GuidSize = 16;
				var span = ids.AsSpan();
				while (span.Length >= GuidSize)
				{
					var guid = new Guid(span.Slice(0, GuidSize));
					string? name = null;
					using (var keyName = key.OpenSubKey($@"Desktops\{guid:B}", writable: false))
					{
						name = keyName?.GetValue("Name") as string;
					}

					name ??= "Desktop " + (list.Count + 1);
					list.Add((guid, name));

					span = span.Slice(GuidSize);
				}
			}
		}

		return list;
	}
}
