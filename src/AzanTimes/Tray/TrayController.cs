using System.Drawing;
using System.Windows.Forms;

namespace AzanTimes.Tray;

internal sealed class TrayController : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly Icon _ownedIcon;

    public TrayController()
    {
        var menu = new ContextMenuStrip
        {
            RightToLeft = RightToLeft.Yes,
            Font = new Font("Segoe UI", 10f)
        };
        menu.Items.Add("نمایش ویجت", null, (_, _) => ShowWidgetRequested?.Invoke());
        menu.Items.Add("تنظیمات", null, (_, _) => SettingsRequested?.Invoke());
        menu.Items.Add("پاپ‌آپ آزمایشی", null, (_, _) => SamplePopupRequested?.Invoke());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("خروج", null, (_, _) => ExitRequested?.Invoke());

        _ownedIcon = LoadIcon();
        _icon = new NotifyIcon
        {
            Icon = _ownedIcon,
            Visible = true,
            Text = "AzanTimes",
            ContextMenuStrip = menu
        };
        _icon.DoubleClick += (_, _) => ShowWidgetRequested?.Invoke();
    }

    public event Action? ShowWidgetRequested;

    public event Action? SettingsRequested;

    public event Action? SamplePopupRequested;

    public event Action? ExitRequested;

    public void SetTooltip(string text)
    {
        _icon.Text = text.Length <= 63 ? text : text[..63];
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
        _ownedIcon.Dispose();
    }

    private static Icon LoadIcon()
    {
        var path = Environment.ProcessPath;
        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                var extracted = Icon.ExtractAssociatedIcon(path);
                if (extracted is not null)
                    return extracted;
            }
            catch (ArgumentException)
            {
            }
        }

        return DrawFallback();
    }

    private static Icon DrawFallback()
    {
        using var bitmap = new Bitmap(32, 32);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);
            using var background = new SolidBrush(Color.FromArgb(23, 79, 68));
            graphics.FillEllipse(background, 1, 1, 30, 30);
            using var gold = new SolidBrush(Color.FromArgb(224, 177, 90));
            graphics.FillEllipse(gold, 9, 6, 14, 18);
            graphics.FillEllipse(background, 13, 6, 14, 18);
            graphics.FillEllipse(gold, 20, 8, 4, 4);
        }

        return Icon.FromHandle(bitmap.GetHicon());
    }
}
