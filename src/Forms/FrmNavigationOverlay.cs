using KeyboardNinja.Helpers;
using SharpHook;
using SharpHook.Native;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace KeyboardNinja.Forms;

/// <summary>
/// Transparent topmost overlay that displays a two-letter badge at the center of each detected screen region. Keys are
/// received via <see cref="HandleKeyFromHook"/> from the global SharpHook hook rather than through form focus, so the
/// overlay never steals keyboard focus from the active window. <para> Navigation is a two-step sequence: the first
/// keypress filters badges to those sharing that prefix (showing only the second character); the second keypress
/// confirms and navigates. Escape cancels the current step (or closes the overlay when at the first step). </para>
/// </summary>
internal sealed class FrmNavigationOverlay : Form
{
	private readonly IReadOnlyList<ScreenRegion> _regions;
	private volatile ScreenRegion? _pendingNavigation;
	private volatile char _firstKey;

	private readonly Font _labelFont;
	private readonly Font _filteredFont;

	// Fixed pill badge dimensions
	private const int BadgePillWidth = 64;
	private const int BadgePillHeight = 40;
	private const int BadgeCornerRadius = 10;

	private static readonly Dictionary<KeyCode, char> s_letterKeys = new()
	{
		{ KeyCode.VcA, 'A' }, { KeyCode.VcB, 'B' }, { KeyCode.VcC, 'C' },
		{ KeyCode.VcD, 'D' }, { KeyCode.VcE, 'E' }, { KeyCode.VcF, 'F' },
		{ KeyCode.VcG, 'G' }, { KeyCode.VcH, 'H' }, { KeyCode.VcI, 'I' },
		{ KeyCode.VcJ, 'J' }, { KeyCode.VcK, 'K' }, { KeyCode.VcL, 'L' },
		{ KeyCode.VcM, 'M' }, { KeyCode.VcN, 'N' }, { KeyCode.VcO, 'O' },
		{ KeyCode.VcP, 'P' }, { KeyCode.VcQ, 'Q' }, { KeyCode.VcR, 'R' },
		{ KeyCode.VcS, 'S' }, { KeyCode.VcT, 'T' }, { KeyCode.VcU, 'U' },
		{ KeyCode.VcV, 'V' }, { KeyCode.VcW, 'W' }, { KeyCode.VcX, 'X' },
		{ KeyCode.VcY, 'Y' }, { KeyCode.VcZ, 'Z' },
	};

	/// <summary>The currently visible navigation overlay, or <see langword="null"/> if none is shown.</summary>
	public static volatile FrmNavigationOverlay? ActiveOverlay;

	/// <summary>The region the user selected, populated when the overlay closes via a two-letter sequence.</summary>
	public ScreenRegion? PendingNavigation => _pendingNavigation;

	private FrmNavigationOverlay(IReadOnlyList<ScreenRegion> regions)
	{
		_regions = regions;
		_labelFont = new Font("Segoe UI", 15f, FontStyle.Bold, GraphicsUnit.Point);
		_filteredFont = new Font("Segoe UI", 20f, FontStyle.Bold, GraphicsUnit.Point);

		var bounds = Screen.AllScreens
			.Select(s => s.Bounds)
			.Aggregate(Rectangle.Empty, Rectangle.Union);

		FormBorderStyle = FormBorderStyle.None;
		ShowInTaskbar = false;
		TopMost = true;
		StartPosition = FormStartPosition.Manual;
		Bounds = bounds;
		BackColor = Color.Black;
		TransparencyKey = Color.Black;
	}

	// Do not steal focus from the previously active window
	protected override bool ShowWithoutActivation => true;

	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		PInvoke.SetWindowPos((HWND)Handle, (HWND)(-1), 0, 0, 0, 0,
			SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOSIZE |
			SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW);
	}

	protected override CreateParams CreateParams
	{
		get
		{
			var cp = base.CreateParams;
			cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
			return cp;
		}
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		base.OnFormClosed(e);
		ActiveOverlay = null;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_labelFont.Dispose();
			_filteredFont.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		var g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

		char firstKey = _firstKey;
		foreach (var region in _regions)
		{
			if (firstKey == '\0' || region.Label[0] == firstKey)
				DrawBadge(g, region, firstKey != '\0');
		}
		g.DrawRectangle(Pens.Red, 0, 0, Width - 1, Height - 1);
	}

	private void DrawBadge(Graphics g, ScreenRegion region, bool filtered)
	{
		var font = filtered ? _filteredFont : _labelFont;
		string text = filtered ? region.Label[1].ToString() : region.Label;

		var clientPt = PointToClient(region.Center);
		var rect = new Rectangle(
			clientPt.X - BadgePillWidth / 2,
			clientPt.Y - BadgePillHeight / 2,
			BadgePillWidth,
			BadgePillHeight);

		// Drop shadow
		var shadowRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
		using var shadowPath = CreateRoundedRectPath(shadowRect, BadgeCornerRadius);
		using var shadowBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0));
		g.FillPath(shadowBrush, shadowPath);

		// Badge fill
		using var bgPath = CreateRoundedRectPath(rect, BadgeCornerRadius);
		using var bgBrush = new SolidBrush(region.BadgeColor);
		g.FillPath(bgBrush, bgPath);

		// Badge border
		using var borderPen = new Pen(Color.White, 1.5f);
		g.DrawPath(borderPen, bgPath);

		// Label text
		using var textBrush = new SolidBrush(Color.White);
		using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		g.DrawString(text, font, textBrush, rect, fmt);
	}

	private static GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
	{
		int d = radius * 2;
		var path = new GraphicsPath();
		path.AddArc(rect.X, rect.Y, d, d, 180, 90);
		path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
		path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
		path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
		path.CloseFigure();
		return path;
	}

	/// <summary>
	/// Called from the global SharpHook hook thread. First keypress filters to matching prefix; second keypress confirms
	/// navigation. Escape steps back (or closes when at the first step).
	/// </summary>
	public void HandleKeyFromHook(KeyCode keyCode)
	{
		if (_pendingNavigation != null)
			return;

		if (keyCode == KeyCode.VcEscape)
		{
			if (_firstKey != '\0')
			{
				_firstKey = '\0';
				BeginInvoke(Invalidate);
			}
			else
			{
				BeginInvoke(Close);
			}
			return;
		}

		if (!s_letterKeys.TryGetValue(keyCode, out char letter))
			return;

		if (_firstKey == '\0')
		{
			// First step: accept the key if at least one region starts with it
			if (_regions.Any(r => r.Label[0] == letter))
			{
				_firstKey = letter;
				BeginInvoke(Invalidate);
			}
		}
		else
		{
			// Second step: find the exact two-letter match
			string label = new([_firstKey, letter]);
			var region = _regions.FirstOrDefault(r => r.Label == label);
			if (region != null)
			{
				_pendingNavigation = region;
				BeginInvoke(Close);
			}
			else
			{
				// No match for this combination: reset and treat the new key as a fresh first step
				_firstKey = '\0';
				if (_regions.Any(r => r.Label[0] == letter))
				{
					_firstKey = letter;
				}
				BeginInvoke(Invalidate);
			}
		}
	}

	/// <summary>
	/// Shows the overlay on a dedicated STA thread and, after the user completes a two-letter sequence, moves the mouse
	/// cursor to the selected region and clicks.
	/// </summary>
	public static void ShowAndNavigate(IReadOnlyList<ScreenRegion> regions)
	{
		ScreenRegion? target = null;

		var thread = new Thread(() =>
		{
			using var form = new FrmNavigationOverlay(regions);
			ActiveOverlay = form;
			form.ShowDialog();
			target = form.PendingNavigation;
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.IsBackground = true;
		thread.Start();
		thread.Join();

		if (target != null)
			NavigateTo(target.Center);
	}

	private static void NavigateTo(Point center)
	{
		Cursor.Position = center;
		var sim = new EventSimulator();
		sim.SimulateMousePress((short)center.X, (short)center.Y, MouseButton.Button1);
		sim.SimulateMouseRelease((short)center.X, (short)center.Y, MouseButton.Button1);
	}
}


