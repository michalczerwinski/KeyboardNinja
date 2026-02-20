using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace KeyboardNinja.Helpers;

/// <summary>Captures all monitors and analyzes the screenshot using flood fill to identify large color regions.</summary>
internal static class ScreenAnalyzer
{
	/// <summary>Color channel quantization step — each R/G/B channel is rounded to the nearest multiple of this value.</summary>
	private const int QuantizationStep = 8;

	/// <summary>Minimum pixel count for a region to be considered a large area.</summary>
	private const int MinRegionSize = 1000;

	/// <summary>Minimum bounding-box width in pixels for a region to receive a navigation label.</summary>
	private const int MinRegionWidth = 40;

	/// <summary>Minimum bounding-box height in pixels for a region to receive a navigation label.</summary>
	private const int MinRegionHeight = 20;

	/// <summary>Minimum bounding-box area (width × height) in pixels for a region to receive a navigation label.</summary>
	private const int MinBoundingBoxArea = 2000;

	/// <summary>Maximum number of regions to label (AA–ZZ, 26×26 two-letter pairs).</summary>
	private const int MaxLabels = 26 * 26;

	private sealed class AnalysisData
	{
		public required int Width { get; init; }
		public required int Height { get; init; }
		public required Point Origin { get; init; }
		public required int[] SrcPixels { get; init; }
		public required int[] RegionId { get; init; }
		public required List<int> RegionSizes { get; init; }
		public required int[] RegionArgb { get; init; }
		public required IReadOnlyList<ScreenRegion> Regions { get; init; }
	}

	/// <summary>Captures all monitors and returns the detected large regions with assigned letters and centers.</summary>
	public static IReadOnlyList<ScreenRegion> DetectRegions()
	{
		using var screenshot = CaptureAllScreens(out var origin);
		return Analyze(screenshot, origin).Regions;
	}

	/// <summary>
	/// Captures all monitors, runs flood-fill region detection, saves the annotated image to
	/// <paramref name="outputPath"/>, and returns the detected regions — all in a single analysis pass.
	/// </summary>
	public static IReadOnlyList<ScreenRegion> DetectRegionsAndSave(string outputPath)
	{
		using var screenshot = CaptureAllScreens(out var origin);
		var data = Analyze(screenshot, origin);
		//using var image = BuildDebugImage(data);
		//image.Save(outputPath, ImageFormat.Png);
		return data.Regions;
	}

	/// <summary>
	/// Captures all monitors, runs flood-fill region detection, and saves the annotated image to <paramref name="outputPath"/>.
	/// Large regions are colored with vivid palette colors; small regions are dimmed.
	/// </summary>
	public static void CaptureAndAnalyze(string outputPath)
	{
		using var screenshot = CaptureAllScreens(out var origin);
		var data = Analyze(screenshot, origin);
		using var image = BuildDebugImage(data);
		image.Save(outputPath, ImageFormat.Png);
	}

	private static Bitmap CaptureAllScreens(out Point origin)
	{
		var bounds = Screen.AllScreens
			.Select(s => s.Bounds)
			.Aggregate(Rectangle.Empty, Rectangle.Union);

		origin = bounds.Location;

		var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
		using var g = Graphics.FromImage(bmp);
		g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
		return bmp;
	}

	private static AnalysisData Analyze(Bitmap source, Point origin)
	{
		int width = source.Width;
		int height = source.Height;
		int total = width * height;

		var srcData = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		var srcPixels = new int[total];
		Marshal.Copy(srcData.Scan0, srcPixels, 0, total);
		source.UnlockBits(srcData);

		// Quantize each pixel: round each channel to the nearest QuantizationStep bucket
		var quantized = new int[total];
		for (int i = 0; i < total; i++)
		{
			int argb = srcPixels[i];
			int r = ((argb >> 16) & 0xFF) / QuantizationStep * QuantizationStep;
			int g = ((argb >> 8) & 0xFF) / QuantizationStep * QuantizationStep;
			int b = (argb & 0xFF) / QuantizationStep * QuantizationStep;
			quantized[i] = (r << 16) | (g << 8) | b;
		}

		// BFS flood fill: assign a region ID to every pixel
		var regionId = new int[total];
		Array.Fill(regionId, -1);
		var regionSizes = new List<int>();
		var queue = new Queue<int>(4096);

		for (int startIdx = 0; startIdx < total; startIdx++)
		{
			if (regionId[startIdx] != -1) continue;

			int id = regionSizes.Count;
			regionSizes.Add(0);
			int color = quantized[startIdx];

			queue.Enqueue(startIdx);
			regionId[startIdx] = id;

			while (queue.Count > 0)
			{
				int cur = queue.Dequeue();
				regionSizes[id]++;
				int cx = cur % width;
				int cy = cur / width;

				if (cx > 0) TryEnqueue(cur - 1, id, color, quantized, regionId, queue);
				if (cx < width - 1) TryEnqueue(cur + 1, id, color, quantized, regionId, queue);
				if (cy > 0) TryEnqueue(cur - width, id, color, quantized, regionId, queue);
				if (cy < height - 1) TryEnqueue(cur + width, id, color, quantized, regionId, queue);
			}
		}

		// Compute bounding boxes and centroids for large regions only
		var largeBounds = new Dictionary<int, (int minX, int minY, int maxX, int maxY, long sumX, long sumY)>();
		for (int id = 0; id < regionSizes.Count; id++)
			if (regionSizes[id] >= MinRegionSize)
				largeBounds[id] = (int.MaxValue, int.MaxValue, int.MinValue, int.MinValue, 0L, 0L);

		for (int i = 0; i < total; i++)
		{
			int id = regionId[i];
			if (!largeBounds.TryGetValue(id, out var b)) continue;
			int cx = i % width;
			int cy = i / width;
			largeBounds[id] = (Math.Min(b.minX, cx), Math.Min(b.minY, cy), Math.Max(b.maxX, cx), Math.Max(b.maxY, cy), b.sumX + cx, b.sumY + cy);
		}

		// Assign two-letter labels to the largest regions (up to MaxLabels), ordered by descending size
		var palette = BuildPalette();
		var regionArgb = new int[regionSizes.Count];
		var regions = new List<ScreenRegion>(MaxLabels);
		int paletteIndex = 0;
		int labelIndex = 0;

		foreach (var (id, b) in largeBounds.OrderByDescending(kv => regionSizes[kv.Key]))
		{
			if (labelIndex >= MaxLabels) break;
			int bbWidth = b.maxX - b.minX + 1;
			int bbHeight = b.maxY - b.minY + 1;
			if (bbWidth < MinRegionWidth || bbHeight < MinRegionHeight || bbWidth * bbHeight < MinBoundingBoxArea) continue;
			string label = IndexToLabel(labelIndex++);
			var color = palette[paletteIndex++ % palette.Length];
			regionArgb[id] = color.ToArgb();
			int centroidX = (int)(b.sumX / regionSizes[id]);
			int centroidY = (int)(b.sumY / regionSizes[id]);
			var bitmapCenter = FindPixelOnRegion(id, centroidX, centroidY, width, height, regionId);
			var center = new Point(origin.X + bitmapCenter.X, origin.Y + bitmapCenter.Y);
			regions.Add(new ScreenRegion(label, center, color));
		}

		return new AnalysisData
		{
			Width = width,
			Height = height,
			Origin = origin,
			SrcPixels = srcPixels,
			RegionId = regionId,
			RegionSizes = regionSizes,
			RegionArgb = regionArgb,
			Regions = regions,
		};
	}

	private static Bitmap BuildDebugImage(AnalysisData d)
	{
		int total = d.Width * d.Height;
		var outPixels = new int[total];

		for (int i = 0; i < total; i++)
		{
			int id = d.RegionId[i];
			int paletteArgb = id >= 0 ? d.RegionArgb[id] : 0;
			if (paletteArgb != 0)
			{
				outPixels[i] = paletteArgb;
			}
			else
			{
				int argb = d.SrcPixels[i];
				int r = ((argb >> 16) & 0xFF) / 4;
				int g = ((argb >> 8) & 0xFF) / 4;
				int b = (argb & 0xFF) / 4;
				outPixels[i] = unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
			}
		}

		var output = new Bitmap(d.Width, d.Height, PixelFormat.Format32bppArgb);
		var outData = output.LockBits(new Rectangle(0, 0, d.Width, d.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
		Marshal.Copy(outPixels, 0, outData.Scan0, total);
		output.UnlockBits(outData);

		DrawDebugLabels(output, d);

		return output;
	}

	private static void DrawDebugLabels(Bitmap output, AnalysisData d)
	{
		const int pillWidth = 64;
		const int pillHeight = 40;
		const int cornerRadius = 10;

		using var g = Graphics.FromImage(output);
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

		using var font = new Font("Segoe UI", 15f, FontStyle.Bold, GraphicsUnit.Point);
		using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
		using var textBrush = new SolidBrush(Color.White);
		using var borderPen = new Pen(Color.White, 1.5f);

		foreach (var region in d.Regions)
		{
			var bitmapCenter = new Point(region.Center.X - d.Origin.X, region.Center.Y - d.Origin.Y);
			var rect = new Rectangle(bitmapCenter.X - pillWidth / 2, bitmapCenter.Y - pillHeight / 2, pillWidth, pillHeight);

			// Drop shadow
			var shadowRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
			using var shadowPath = CreateRoundedRectPath(shadowRect, cornerRadius);
			using var shadowBrush = new SolidBrush(Color.FromArgb(140, 0, 0, 0));
			g.FillPath(shadowBrush, shadowPath);

			// Badge fill
			using var bgPath = CreateRoundedRectPath(rect, cornerRadius);
			using var bgBrush = new SolidBrush(region.BadgeColor);
			g.FillPath(bgBrush, bgPath);

			// Badge border
			g.DrawPath(borderPen, bgPath);

			// Label
			g.DrawString(region.Label, font, textBrush, rect, fmt);
		}
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

	private static string IndexToLabel(int index)
		=> new string([(char)('A' + index / 26), (char)('A' + index % 26)]);

	private static Point FindPixelOnRegion(int targetId, int cx, int cy, int width, int height, int[] regionIdMap)
	{
		// Fast path: centroid is already on the region (true for all convex regions)
		if ((uint)cx < (uint)width && (uint)cy < (uint)height && regionIdMap[cy * width + cx] == targetId)
			return new Point(cx, cy);

		// Expand outward in concentric squares until a region pixel is found
		for (int r = 1; r <= 200; r++)
		{
			// Top and bottom rows
			for (int dx = -r; dx <= r; dx++)
			{
				int nx = cx + dx;
				if ((uint)nx >= (uint)width) continue;
				int ny1 = cy - r, ny2 = cy + r;
				if ((uint)ny1 < (uint)height && regionIdMap[ny1 * width + nx] == targetId) return new Point(nx, ny1);
				if ((uint)ny2 < (uint)height && regionIdMap[ny2 * width + nx] == targetId) return new Point(nx, ny2);
			}
			// Left and right columns (corners already covered above)
			for (int dy = -r + 1; dy < r; dy++)
			{
				int ny = cy + dy;
				if ((uint)ny >= (uint)height) continue;
				int nx1 = cx - r, nx2 = cx + r;
				if ((uint)nx1 < (uint)width && regionIdMap[ny * width + nx1] == targetId) return new Point(nx1, ny);
				if ((uint)nx2 < (uint)width && regionIdMap[ny * width + nx2] == targetId) return new Point(nx2, ny);
			}
		}
		return new Point(cx, cy);
	}

	private static void TryEnqueue(int nIdx, int id, int color, int[] quantized, int[] regionId, Queue<int> queue)
	{
		if (regionId[nIdx] != -1 || quantized[nIdx] != color) return;
		regionId[nIdx] = id;
		queue.Enqueue(nIdx);
	}

	private static Color[] BuildPalette() =>
	[
		Color.FromArgb(220,  80,  80),  // red
		Color.FromArgb( 80, 200,  80),  // green
		Color.FromArgb( 80, 120, 220),  // blue
		Color.FromArgb(220, 200,  60),  // yellow
		Color.FromArgb(200,  80, 200),  // magenta
		Color.FromArgb( 60, 200, 200),  // cyan
		Color.FromArgb(220, 140,  60),  // orange
		Color.FromArgb(140,  80, 220),  // purple
		Color.FromArgb( 80, 200, 140),  // teal
		Color.FromArgb(220,  80, 140),  // pink
		Color.FromArgb(160, 220,  80),  // lime
		Color.FromArgb( 80, 160, 220),  // sky blue
		Color.FromArgb(220, 160, 140),  // salmon
		Color.FromArgb(140, 220, 160),  // mint
		Color.FromArgb(160, 140, 220),  // lavender
		Color.FromArgb(220, 220, 140),  // cream
	];
}
