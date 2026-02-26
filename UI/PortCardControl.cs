using System.Drawing.Drawing2D;
using PortManager.Models;

namespace PortManager.UI;

public class PortCardControl : Control
{
    private PortInfo _portInfo = new();
    private bool _hovered;
    private readonly RoundedButton _killButton;

    public event EventHandler? KillClicked;

    public PortInfo PortInfo
    {
        get => _portInfo;
        set
        {
            _portInfo = value;
            UpdateLayout();
            Invalidate();
        }
    }

    public PortCardControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Height = Theme.CardHeight;
        Cursor = Cursors.Default;

        _killButton = new RoundedButton
        {
            Text = "Kill",
            Size = new Size(64, 30),
            NormalBackColor = Theme.KillBg,
            HoverBackColor = Theme.KillBgHover,
            NormalForeColor = Theme.KillText,
            HoverForeColor = Theme.KillTextHover,
            Visible = false,
            Radius = 8,
        };
        _killButton.Click += (_, _) => KillClicked?.Invoke(this, EventArgs.Empty);
        Controls.Add(_killButton);
    }

    private void UpdateLayout()
    {
        Height = _portInfo.InUse ? Theme.CardHeightInUse : Theme.CardHeight;
        _killButton.Visible = _portInfo.InUse;
        if (_portInfo.InUse)
        {
            _killButton.Location = new Point(Width - 80, Height - 40);
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_portInfo.InUse)
            _killButton.Location = new Point(Width - 80, Height - 40);
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
        using var path = GetRoundedRect(rect, Theme.CardRadius);

        // Card background
        var bgColor = _hovered ? Theme.CardBgHover : Theme.CardBg;
        using (var brush = new SolidBrush(bgColor))
            g.FillPath(brush, path);

        // Glass overlay
        using (var glassBrush = new LinearGradientBrush(
            new PointF(0, 0), new PointF(0, Height),
            Color.FromArgb(10, 255, 255, 255),
            Color.FromArgb(0, 255, 255, 255)))
        {
            g.FillPath(glassBrush, path);
        }

        // Border
        using (var pen = new Pen(_hovered ? Theme.CardBorder : Color.FromArgb(40, 255, 255, 255), 1f))
            g.DrawPath(pen, path);

        // Hover glow
        if (_hovered)
        {
            using var glowPen = new Pen(Color.FromArgb(20, Theme.Accent.R, Theme.Accent.G, Theme.Accent.B), 2f);
            g.DrawPath(glowPen, path);
        }

        // Port number
        var portText = $":{_portInfo.Port}";
        using (var brush = new SolidBrush(Theme.TextPrimary))
            g.DrawString(portText, Theme.PortFont, brush, 16, 14);

        // Port name
        var portSize = g.MeasureString(portText, Theme.PortFont);
        using (var brush = new SolidBrush(Theme.TextSecondary))
            g.DrawString(_portInfo.Name, Theme.PortNameFont, brush, 20 + portSize.Width, 20);

        // Status dot
        var dotX = Width - 36;
        var dotY = 24;
        var dotColor = _portInfo.InUse ? Theme.StatusInUse : Theme.StatusFree;

        // Glow behind dot
        using (var glowBrush = new SolidBrush(Color.FromArgb(40, dotColor.R, dotColor.G, dotColor.B)))
            g.FillEllipse(glowBrush, dotX - 4, dotY - 4, 20, 20);
        using (var dotBrush = new SolidBrush(dotColor))
            g.FillEllipse(dotBrush, dotX, dotY, 12, 12);

        // Process info (when in use)
        if (_portInfo.InUse)
        {
            var procText = $"{_portInfo.ProcessName} (PID {_portInfo.Pid})";
            using var brush = new SolidBrush(Theme.TextMuted);
            g.DrawString(procText, Theme.ProcessFont, brush, 20, 54);
        }
    }

    protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e)
    {
        if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
        {
            _hovered = false;
            Invalidate();
        }
        base.OnMouseLeave(e);
    }
}
