using System.IO;
using DrawingIcon = System.Drawing.Icon;
using Forms = System.Windows.Forms;

namespace Pompom.Services;

internal sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ToolStripMenuItem _startItem;
    private readonly Forms.ToolStripMenuItem _stopItem;
    private bool _disposed;

    public TrayIconService()
    {
        string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Pompom.ico");
        var icon = new DrawingIcon(iconPath);

        var showItem = new Forms.ToolStripMenuItem("Show Pompom");
        showItem.Font = new System.Drawing.Font(showItem.Font, System.Drawing.FontStyle.Bold);
        showItem.Click += (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty);

        _startItem = new Forms.ToolStripMenuItem("Start");
        _startItem.Click += (_, _) => StartRequested?.Invoke(this, EventArgs.Empty);

        _stopItem = new Forms.ToolStripMenuItem("Stop");
        _stopItem.Click += (_, _) => StopRequested?.Invoke(this, EventArgs.Empty);

        var skipItem = new Forms.ToolStripMenuItem("Skip");
        skipItem.Click += (_, _) => SkipRequested?.Invoke(this, EventArgs.Empty);

        var settingsItem = new Forms.ToolStripMenuItem("Settings");
        settingsItem.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);

        var exitItem = new Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        var menu = new Forms.ContextMenuStrip();
        menu.Items.AddRange(
        [
            showItem,
            new Forms.ToolStripSeparator(),
            _startItem,
            _stopItem,
            skipItem,
            new Forms.ToolStripSeparator(),
            settingsItem,
            new Forms.ToolStripSeparator(),
            exitItem,
        ]);

        _notifyIcon = new Forms.NotifyIcon
        {
            ContextMenuStrip = menu,
            Icon = icon,
            Text = "Pompom",
            Visible = true,
        };
        _notifyIcon.DoubleClick += (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty);
        _notifyIcon.BalloonTipClicked += (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ShowRequested;

    public event EventHandler? StartRequested;

    public event EventHandler? StopRequested;

    public event EventHandler? SkipRequested;

    public event EventHandler? SettingsRequested;

    public event EventHandler? ExitRequested;

    public void Update(string sessionName, string remaining, bool isRunning)
    {
        string tooltip = $"Pompom — {sessionName} {remaining}";
        _notifyIcon.Text = tooltip.Length <= 63 ? tooltip : tooltip[..63];
        _startItem.Enabled = !isRunning;
        _stopItem.Enabled = isRunning;
    }

    public void ShowBalloon(string title, string body)
    {
        _notifyIcon.ShowBalloonTip(5000, title, body, Forms.ToolTipIcon.Info);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Icon?.Dispose();
        _notifyIcon.Dispose();
        _disposed = true;
    }
}
