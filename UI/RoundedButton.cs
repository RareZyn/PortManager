using System.Drawing.Drawing2D;

namespace PortManager.UI;

public class RoundedButton : Control
{
    private bool _hovered;
    private bool _pressed;

    public Color NormalBackColor { get; set; } = Theme.Accent;
    public Color HoverBackColor { get; set; } = Theme.AccentHover;
    public Color PressBackColor { get; set; } = Theme.Accent;
    public Color NormalForeColor { get; set; } = Color.White;
    public Color HoverForeColor { get; set; } = Color.White;
    public int Radius { get; set; } = Theme.ButtonRadius;

    public RoundedButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Font = Theme.ButtonFont;
        Cursor = Cursors.Hand;
        Size = new Size(80, 32);
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
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = new RectangleF(0, 0, Width, Height);
        using var path = GetRoundedRect(rect, Radius);

        var bgColor = _pressed ? PressBackColor : _hovered ? HoverBackColor : NormalBackColor;
        var fgColor = _hovered ? HoverForeColor : NormalForeColor;

        using (var brush = new SolidBrush(bgColor))
            g.FillPath(brush, path);

        // Text centered
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using (var brush = new SolidBrush(fgColor))
            g.DrawString(Text, Font, brush, rect, sf);
    }

    protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
}
