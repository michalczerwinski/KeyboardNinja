using System.Drawing.Drawing2D;

namespace KeyboardNinja.Helpers;

internal static class IconHelper
{
	public static Icon CreateNinjaIcon(int size = 32)
	{
		using var bitmap = new Bitmap(size, size);
		using var g = Graphics.FromImage(bitmap);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.Clear(Color.Gray);

		float s = size;

		// Head (black circle)
		using var headBrush = new SolidBrush(Color.FromArgb(20, 20, 20));
		g.FillEllipse(headBrush, s * 0.15f, s * 0.05f, s * 0.7f, s * 0.7f);

		// Mask band across eyes (dark gray)
		using var maskBrush = new SolidBrush(Color.FromArgb(40, 40, 40));
		g.FillRectangle(maskBrush, s * 0.1f, s * 0.25f, s * 0.8f, s * 0.2f);

		// Eyes (white slits)
		using var eyeBrush = new SolidBrush(Color.White);
		g.FillEllipse(eyeBrush, s * 0.25f, s * 0.30f, s * 0.15f, s * 0.08f);
		g.FillEllipse(eyeBrush, s * 0.60f, s * 0.30f, s * 0.15f, s * 0.08f);

		// Red ribbon tails flowing to the right
		using var ribbonPen = new Pen(Color.FromArgb(200, 30, 30), s * 0.06f)
		{
			StartCap = LineCap.Round,
			EndCap = LineCap.Round
		};

		// Upper ribbon tail
		g.DrawBezier(ribbonPen,
			s * 0.75f, s * 0.30f,
			s * 0.85f, s * 0.20f,
			s * 0.90f, s * 0.45f,
			s * 0.95f, s * 0.55f);

		// Lower ribbon tail
		g.DrawBezier(ribbonPen,
			s * 0.75f, s * 0.38f,
			s * 0.82f, s * 0.50f,
			s * 0.88f, s * 0.55f,
			s * 0.95f, s * 0.70f);

		return Icon.FromHandle(bitmap.GetHicon());
	}
}
