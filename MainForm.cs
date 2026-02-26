using System.Drawing.Drawing2D;
using PortManager.Models;
using PortManager.UI;

namespace PortManager;

public class MainForm : Form
{
    // Title bar controls
    private readonly Panel _titleBar;
    private readonly Label _titleLabel;
    private readonly Button _minimizeBtn;
    private readonly Button _maximizeBtn;
    private readonly Button _closeBtn;

    // Toolbar
    private readonly Panel _toolbar;
    private readonly TextBox _searchBox;
    private readonly RoundedButton _refreshBtn;
    private readonly RoundedButton _addPortBtn;

    // Content
    private readonly Panel _scrollPanel;
    private readonly System.Windows.Forms.Timer _refreshTimer;

    // State
    private List<(int port, string name, bool isCustom)> _ports = new();
    private List<PortInfo> _portInfos = new();
    private readonly List<PortCardControl> _cards = new();
    private bool _isDragging;
    private Point _dragStart;

    public MainForm()
    {
        // Form setup
        Text = "Port Killer";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(560, 700);
        MinimumSize = new Size(480, 500);
        BackColor = Theme.BgDark;
        DoubleBuffered = true;

        // Load app icon from embedded resource
        var iconStream = System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("PortManager.Resources.PortKiller.ico");
        if (iconStream != null)
            Icon = new Icon(iconStream);

        // === Title Bar ===
        _titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = Theme.TitleBarHeight,
            BackColor = Theme.TitleBarBg,
        };

        _titleLabel = new Label
        {
            Text = "\u26a1 Port Killer",
            Font = Theme.TitleFont,
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(16, 10),
            BackColor = Theme.TitleBarBg,
        };

        _minimizeBtn = CreateTitleBarButton("\u2500", Theme.TitleBarButton);
        _maximizeBtn = CreateTitleBarButton("\u25a1", Theme.TitleBarButton);
        _closeBtn = CreateTitleBarButton("\u2715", Theme.TitleBarClose);

        _closeBtn.Click += (_, _) => Close();
        _minimizeBtn.Click += (_, _) => WindowState = FormWindowState.Minimized;
        _maximizeBtn.Click += (_, _) =>
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        };

        _titleBar.Controls.AddRange([_titleLabel, _minimizeBtn, _maximizeBtn, _closeBtn]);
        _titleBar.Resize += (_, _) => LayoutTitleBarButtons();

        // Drag to move
        _titleBar.MouseDown += TitleBar_MouseDown;
        _titleBar.MouseMove += TitleBar_MouseMove;
        _titleBar.MouseUp += TitleBar_MouseUp;
        _titleLabel.MouseDown += TitleBar_MouseDown;
        _titleLabel.MouseMove += TitleBar_MouseMove;
        _titleLabel.MouseUp += TitleBar_MouseUp;

        // === Toolbar ===
        _toolbar = new DoubleBufferedPanel
        {
            Dock = DockStyle.Top,
            Height = Theme.ToolbarHeight,
            BackColor = Theme.BgDark,
        };

        _searchBox = new TextBox
        {
            Font = Theme.SearchFont,
            ForeColor = Theme.TextPrimary,
            BackColor = Theme.SearchBg,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Search ports...",
            Size = new Size(280, 32),
        };
        _searchBox.TextChanged += (_, _) => FilterCards();

        _refreshBtn = new RoundedButton
        {
            Text = "\u21bb Refresh",
            Size = new Size(100, 34),
            NormalBackColor = Theme.Accent,
            HoverBackColor = Theme.AccentHover,
        };
        _refreshBtn.Click += async (_, _) => await RefreshPortsAsync();

        _addPortBtn = new RoundedButton
        {
            Text = "+ Add Port",
            Size = new Size(115, 34),
            NormalBackColor = Color.FromArgb(50, 50, 80),
            HoverBackColor = Color.FromArgb(65, 65, 100),
            NormalForeColor = Theme.TextSecondary,
            HoverForeColor = Theme.TextPrimary,
        };
        _addPortBtn.Click += (_, _) => ShowAddPortDialog();

        _toolbar.Controls.AddRange([_searchBox, _refreshBtn, _addPortBtn]);
        _toolbar.Resize += (_, _) => LayoutToolbar();

        // === Scroll Panel ===
        _scrollPanel = new DoubleBufferedPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Theme.BgDark,
        };

        // WinForms Dock order: LAST added with DockStyle.Top goes on TOP.
        // So add in REVERSE visual order: fill first, then toolbar, then title bar.
        Controls.Add(_scrollPanel);
        Controls.Add(_toolbar);
        Controls.Add(_titleBar);

        // === Timer ===
        _refreshTimer = new System.Windows.Forms.Timer { Interval = 5000 };
        _refreshTimer.Tick += async (_, _) => await RefreshPortsAsync();

        // Initialize ports
        InitializePorts();

        Resize += (_, _) => Invalidate();
    }

    private void InitializePorts()
    {
        var commonPorts = PortService.GetCommonPorts();
        _ports = commonPorts.Select(kv => (kv.Key, kv.Value, false)).OrderBy(p => p.Key).ToList();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        LayoutTitleBarButtons();
        LayoutToolbar();
        await RefreshPortsAsync();
        _refreshTimer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        // Background gradient
        if (ClientRectangle.Width > 0 && ClientRectangle.Height > 0)
        {
            using var bgBrush = new LinearGradientBrush(
                ClientRectangle,
                Theme.BgDark, Theme.BgLight,
                LinearGradientMode.Vertical);
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        // Thin border
        using var borderPen = new Pen(Theme.GlassBorder, 1f);
        g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
    }

    // --- Port Refresh ---
    private async Task RefreshPortsAsync()
    {
        _refreshBtn.Enabled = false;
        _refreshBtn.Text = "...";

        try
        {
            _portInfos = await PortService.GetPortInfosAsync(_ports);
            RebuildCards();
        }
        finally
        {
            _refreshBtn.Text = "\u21bb Refresh";
            _refreshBtn.Enabled = true;
        }
    }

    private void RebuildCards()
    {
        _scrollPanel.SuspendLayout();

        foreach (var card in _cards)
        {
            _scrollPanel.Controls.Remove(card);
            card.Dispose();
        }
        _cards.Clear();

        var filter = _searchBox.Text.Trim().ToLower();
        var filtered = _portInfos.Where(p =>
            string.IsNullOrEmpty(filter) ||
            p.Port.ToString().Contains(filter) ||
            p.Name.ToLower().Contains(filter) ||
            p.ProcessName.ToLower().Contains(filter)
        ).ToList();

        // Reset scroll before rebuilding
        _scrollPanel.AutoScrollPosition = Point.Empty;
        _scrollPanel.AutoScrollMinSize = Size.Empty;

        var cardWidth = _scrollPanel.ClientSize.Width - 32; // 16px padding each side
        var y = 8;
        foreach (var info in filtered)
        {
            var card = new PortCardControl
            {
                PortInfo = info,
                Width = cardWidth,
                Left = 16,
                Top = y,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
            };
            card.KillClicked += async (sender, _) =>
            {
                if (sender is PortCardControl c)
                {
                    var pid = c.PortInfo.Pid;
                    var port = c.PortInfo.Port;
                    var result = MessageBox.Show(
                        $"Kill {c.PortInfo.ProcessName} (PID {pid}) on port {port}?",
                        "Confirm Kill",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        if (PortService.KillProcess(pid))
                        {
                            await Task.Delay(500);
                            await RefreshPortsAsync();
                        }
                        else
                        {
                            MessageBox.Show($"Failed to kill process {pid}.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            };

            _cards.Add(card);
            _scrollPanel.Controls.Add(card);
            y += card.Height + Theme.CardSpacing;
        }

        // Set scroll range to exactly fit the content (+ small bottom padding)
        _scrollPanel.AutoScrollMinSize = new Size(0, y + 8);

        _scrollPanel.ResumeLayout();
    }

    private void FilterCards()
    {
        RebuildCards();
    }

    // --- Add Custom Port ---
    private void ShowAddPortDialog()
    {
        using var dialog = new Form
        {
            Text = "Add Custom Port",
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(340, 200),
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Theme.BgDark,
            ForeColor = Theme.TextPrimary,
        };

        var portLabel = new Label { Text = "Port:", Location = new Point(20, 20), AutoSize = true, ForeColor = Theme.TextPrimary };
        var portBox = new TextBox { Location = new Point(100, 18), Size = new Size(200, 28), BackColor = Theme.SearchBg, ForeColor = Theme.TextPrimary, BorderStyle = BorderStyle.FixedSingle };

        var nameLabel = new Label { Text = "Name:", Location = new Point(20, 60), AutoSize = true, ForeColor = Theme.TextPrimary };
        var nameBox = new TextBox { Location = new Point(100, 58), Size = new Size(200, 28), BackColor = Theme.SearchBg, ForeColor = Theme.TextPrimary, BorderStyle = BorderStyle.FixedSingle };

        var okBtn = new Button
        {
            Text = "Add",
            DialogResult = DialogResult.OK,
            Location = new Point(100, 110),
            Size = new Size(90, 32),
            BackColor = Theme.Accent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
        };
        okBtn.FlatAppearance.BorderSize = 0;

        var cancelBtn = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(200, 110),
            Size = new Size(90, 32),
            BackColor = Color.FromArgb(60, 60, 90),
            ForeColor = Theme.TextSecondary,
            FlatStyle = FlatStyle.Flat,
        };
        cancelBtn.FlatAppearance.BorderSize = 0;

        dialog.Controls.AddRange([portLabel, portBox, nameLabel, nameBox, okBtn, cancelBtn]);
        dialog.AcceptButton = okBtn;
        dialog.CancelButton = cancelBtn;

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            if (int.TryParse(portBox.Text.Trim(), out var port) && port > 0 && port <= 65535)
            {
                var name = string.IsNullOrWhiteSpace(nameBox.Text) ? "Custom" : nameBox.Text.Trim();
                if (!_ports.Any(p => p.port == port))
                {
                    _ports.Add((port, name, true));
                    _ports = _ports.OrderBy(p => p.port).ToList();
                    _ = RefreshPortsAsync();
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Invalid Port",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    // --- Title Bar ---
    private Button CreateTitleBarButton(string text, Color hoverColor)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(44, Theme.TitleBarHeight),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Theme.TitleBarButton,
            BackColor = Theme.TitleBarBg,
            Font = new Font("Segoe UI", 10f),
            Cursor = Cursors.Hand,
            TabStop = false,
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, hoverColor.R, hoverColor.G, hoverColor.B);
        return btn;
    }

    private void LayoutTitleBarButtons()
    {
        var x = _titleBar.Width;
        _closeBtn.Location = new Point(x -= 44, 0);
        _maximizeBtn.Location = new Point(x -= 44, 0);
        _minimizeBtn.Location = new Point(x -= 44, 0);
    }

    private void LayoutToolbar()
    {
        var w = _toolbar.ClientSize.Width;
        var pad = 16;
        var btnGap = 10;
        var vCenter = (_toolbar.Height - 34) / 2;

        // Buttons on the right
        _addPortBtn.Location = new Point(w - pad - _addPortBtn.Width, vCenter);
        _refreshBtn.Location = new Point(_addPortBtn.Left - btnGap - _refreshBtn.Width, vCenter);

        // Search box fills remaining space on the left
        _searchBox.Location = new Point(pad, vCenter + 2);
        _searchBox.Width = _refreshBtn.Left - btnGap - pad;
    }

    // --- Drag to Move ---
    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = true;
            _dragStart = e.Location;
        }
    }

    private void TitleBar_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            var screen = (sender as Control)!.PointToScreen(e.Location);
            Location = new Point(screen.X - _dragStart.X, screen.Y - _dragStart.Y);
        }
    }

    private void TitleBar_MouseUp(object? sender, MouseEventArgs e)
    {
        _isDragging = false;
    }

    // --- Resize Grip ---
    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST = 0x84;
        const int HTBOTTOMRIGHT = 17;
        const int HTRIGHT = 11;
        const int HTBOTTOM = 15;
        const int HTLEFT = 10;
        const int HTBOTTOMLEFT = 16;

        if (m.Msg == WM_NCHITTEST)
        {
            base.WndProc(ref m);
            var cursor = PointToClient(Cursor.Position);
            const int grip = 8;

            if (cursor.X >= Width - grip && cursor.Y >= Height - grip)
                m.Result = HTBOTTOMRIGHT;
            else if (cursor.X >= Width - grip)
                m.Result = HTRIGHT;
            else if (cursor.Y >= Height - grip)
                m.Result = HTBOTTOM;
            else if (cursor.X <= grip && cursor.Y >= Height - grip)
                m.Result = HTBOTTOMLEFT;
            else if (cursor.X <= grip)
                m.Result = HTLEFT;
            return;
        }

        base.WndProc(ref m);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _refreshTimer.Stop();
        _refreshTimer.Dispose();
        base.OnFormClosed(e);
    }
}

// Helper: Panel with double-buffering to prevent white flicker
internal class DoubleBufferedPanel : Panel
{
    public DoubleBufferedPanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);
    }
}
