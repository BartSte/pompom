using System.Globalization;
using System.Windows;
using Pompom.Core;
using Pompom.Services;

namespace Pompom;

public partial class SettingsWindow : Window
{
    private readonly Action _sendTestNotification;

    internal SettingsWindow(PompomSettings settings, Action sendTestNotification)
    {
        _sendTestNotification = sendTestNotification;
        InitializeComponent();
        WindowAppearance.UseThemeTitleBar(this);

        WorkMinutesTextBox.Text = settings.WorkMinutes.ToString(CultureInfo.InvariantCulture);
        ShortBreakMinutesTextBox.Text = settings.ShortBreakMinutes.ToString(CultureInfo.InvariantCulture);
        LongBreakMinutesTextBox.Text = settings.LongBreakMinutes.ToString(CultureInfo.InvariantCulture);
        LongBreakEnabledCheckBox.IsChecked = settings.LongBreakEnabled;
        AutoStartBreaksCheckBox.IsChecked = settings.AutoStartBreaks;
        AutoStartWorkCheckBox.IsChecked = settings.AutoStartWork;
        NotifyAfterWorkCheckBox.IsChecked = settings.NotifyAfterWork;
        NotifyAfterBreakCheckBox.IsChecked = settings.NotifyAfterBreak;
        DarkThemeCheckBox.IsChecked = settings.DarkTheme;
        VersionText.Text = BuildInfo.Version;
        GitCommitText.Text = BuildInfo.GitCommit;
        UpdateLongBreakInput();
    }

    internal PompomSettings? SavedSettings { get; private set; }

    private void SaveButton_Click(object sender, RoutedEventArgs eventArgs)
    {
        if (!TryReadMinutes(WorkMinutesTextBox, "Work time", out int workMinutes)
            || !TryReadMinutes(
                ShortBreakMinutesTextBox,
                "Short break",
                out int shortBreakMinutes)
            || !TryReadMinutes(
                LongBreakMinutesTextBox,
                "Long break",
                out int longBreakMinutes))
        {
            return;
        }

        SavedSettings = new PompomSettings
        {
            WorkMinutes = workMinutes,
            ShortBreakMinutes = shortBreakMinutes,
            LongBreakMinutes = longBreakMinutes,
            LongBreakEnabled = LongBreakEnabledCheckBox.IsChecked == true,
            AutoStartBreaks = AutoStartBreaksCheckBox.IsChecked == true,
            AutoStartWork = AutoStartWorkCheckBox.IsChecked == true,
            NotifyAfterWork = NotifyAfterWorkCheckBox.IsChecked == true,
            NotifyAfterBreak = NotifyAfterBreakCheckBox.IsChecked == true,
            DarkTheme = DarkThemeCheckBox.IsChecked == true,
        };

        DialogResult = true;
    }

    private void TestNotificationButton_Click(object sender, RoutedEventArgs eventArgs)
    {
        _sendTestNotification();
    }

    private void LongBreakEnabled_Changed(object sender, RoutedEventArgs eventArgs)
    {
        UpdateLongBreakInput();
    }

    private bool TryReadMinutes(
        System.Windows.Controls.TextBox textBox,
        string fieldName,
        out int minutes)
    {
        bool valid = int.TryParse(
            textBox.Text,
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out minutes)
            && minutes is >= PompomSettings.MinimumMinutes
                and <= PompomSettings.MaximumMinutes;

        if (valid)
        {
            return true;
        }

        ValidationMessageText.Text =
            $"{fieldName} must be a whole number from "
            + $"{PompomSettings.MinimumMinutes} through {PompomSettings.MaximumMinutes}.";
        textBox.Focus();
        textBox.SelectAll();
        return false;
    }

    private void UpdateLongBreakInput()
    {
        if (LongBreakMinutesTextBox is not null)
        {
            LongBreakMinutesTextBox.IsEnabled = LongBreakEnabledCheckBox.IsChecked == true;
        }
    }
}
