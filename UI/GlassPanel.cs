using System.Drawing.Drawing2D;

namespace PortManager.UI;

public class GlassPanel : Panel
{
    public int Radius { get; set; } = Theme.CardRadius;
    public Color FillColor { get; set; } = Theme.CardBg;
    public Color BorderColor { get; set; } = Theme.GlassBorder;
    public bool DrawBorder { get; set; } = true;

    public GlassPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        BackColor = Color.Transparent;
    }

    private GraphicsPath GetRoundedRect(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new RectangleF(1, 1, Width - 2, Height - 2);
        using var path = GetRoundedRect(rect, Radius);

        // Fill
        using (var brush = new SolidBrush(FillColor))
            g.FillPath(brush, path);

        // Glass overlay gradient (top-to-bottom subtle highlight)
        var glassRect = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height / 2);
        using var glassPath = GetRoundedRect(rect, Radius);
        using (var glassBrush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(0, rect.Height),
            Color.FromArgb(15, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255)))
        {
            g.FillPath(glassBrush, glassPath);
        }

        // Border
        if (DrawBorder)
        {
            using var pen = new Pen(BorderColor, 1f);
            g.DrawPath(pen, path);
        }
    }
}
