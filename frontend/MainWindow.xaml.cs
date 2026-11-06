
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TasksWpf.Api;
using TasksWpf.Models;

namespace TasksWpf;

public partial class MainWindow : Window
{
    private readonly CalendarApiClient _api = new();

    private readonly ObservableCollection<CalendarTaskDto> _tasks = [];

    public MainWindow()
    {
        InitializeComponent();
        TasksGrid.ItemsSource = _tasks;
        FromDatePicker.SelectedDate = DateTime.Today;
        ToDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        Loaded += async (_, _) => await LoadTasksAsync();
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            if (FromDatePicker.SelectedDate == null ||
                ToDatePicker.SelectedDate == null)
            {
                throw new InvalidOperationException(
                    "Mindkét dátumot ki kell választani.");
            }

            if (!TimeSpan.TryParse(FromTimeBox.Text, out var fromTime) ||
                !TimeSpan.TryParse(ToTimeBox.Text, out var toTime))
            {
                throw new InvalidOperationException(
                    "Az időpont formátuma HH:mm legyen.");
            }

            DateTime from = FromDatePicker.SelectedDate.Value
                .Date.Add(fromTime);

            DateTime to = ToDatePicker.SelectedDate.Value
                .Date.Add(toTime);

            if (to <= from)
            {
                throw new InvalidOperationException(
                    "A záró időpontnak a kezdő időpont után kell lennie.");
            }

            var tasks = await _api.GetTasksAsync(from, to);

            _tasks.Clear();

            foreach (var task in tasks)
                _tasks.Add(task);

            StatusText.Text = $"{_tasks.Count} feladat található.";
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private CalendarTaskDto ReadForm(int id = 0)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
            throw new InvalidOperationException(
                "A feladat neve kötelező.");

        if (!DateTime.TryParseExact(
                StartBox.Text.Trim(),
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var start))
        {
            throw new InvalidOperationException(
                "A kezdés formátuma: yyyy-MM-dd HH:mm");
        }

        if (!DateTime.TryParseExact(
                EndBox.Text.Trim(),
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var end))
        {
            throw new InvalidOperationException(
                "A befejezés formátuma: yyyy-MM-dd HH:mm");
        }

        if (end <= start)
            throw new InvalidOperationException(
                "A befejezésnek a kezdés után kell lennie.");

        int? recurrenceId = null;

        // if (!string.IsNullOrWhiteSpace(RecurrenceBox.Text))
        // {
        //     if (!int.TryParse(RecurrenceBox.Text, out var parsedId)|| parsedId <= 0)
        //     {
        //         throw new InvalidOperationException(
        //             "Az ismétlődés ID-ja pozitív egész szám legyen.");
        //     }

        //     recurrenceId = parsedId;
        // }

        return new CalendarTaskDto
        {
            Id = id,
            Name = NameBox.Text.Trim(),
            StartTime = start,
            EndTime = end,
            RecurrenceId = recurrenceId
        };
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            int? recurrenceId = await GetSelectedRecurrenceIdAsync();

            var task = ReadForm();
            task.RecurrenceId = recurrenceId;

            await _api.CreateTaskAsync(task);

            ClearForm();
            await LoadTasksAsync();

            StatusText.Text = "Feladat létrehozva.";
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private async void Update_Click(object sender, RoutedEventArgs e)
    {
        if (TasksGrid.SelectedItem is not CalendarTaskDto selected)
        {
            StatusText.Text = "Előbb válassz ki egy feladatot.";
            return;
        }

        try
        {
            int? recurrenceId = await GetSelectedRecurrenceIdAsync();
            var task = ReadForm(selected.Id);
            task.RecurrenceId = recurrenceId;
            await _api.UpdateTaskAsync(task);
            await LoadTasksAsync();
            ClearForm();
            StatusText.Text = "Feladat módosítva.";
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (TasksGrid.SelectedItem is not CalendarTaskDto selected)
        {
            StatusText.Text = "Előbb válassz ki egy feladatot.";
            return;
        }

        var result = MessageBox.Show(
            $"Biztosan törlöd ezt a feladatot?\n{selected.Name}",
            "Törlés megerősítése",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _api.DeleteTaskAsync(selected.Id);
            ClearForm();
            await LoadTasksAsync();
            StatusText.Text = "Feladat törölve.";
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await LoadTasksAsync();
    }

    private void TasksGrid_SelectionChanged(
        object sender, SelectionChangedEventArgs e)
    {
        if (TasksGrid.SelectedItem is not CalendarTaskDto task)
            return;

        NameBox.Text = task.Name;
        StartBox.Text = task.StartTime.ToString("yyyy-MM-dd HH:mm");
        EndBox.Text = task.EndTime.ToString("yyyy-MM-dd HH:mm");
        // RecurrenceBox.Text = task.RecurrenceId?.ToString() ?? "";
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
        TasksGrid.SelectedItem = null;
    }

    private void ClearForm()
    {
        NameBox.Clear();
        StartBox.Clear();
        EndBox.Clear();
        RecurrenceUnitBox.SelectedIndex = 0;
        RecurrenceValueBox.Text = "2";
        CustomUnitBox.SelectedIndex = 2;
        CustomRecurrencePanel.Visibility = Visibility.Collapsed;
    }

    private void ShowError(Exception ex)
    {
        StatusText.Text = $"Hiba: {ex.Message}";

        MessageBox.Show(ex.Message,"API-hiba",MessageBoxButton.OK,MessageBoxImage.Error);
    }

    private async void Filter_Click(object sender,RoutedEventArgs e)
    {
        await LoadTasksAsync();
    }

    private async void Reset_Click(object sender,RoutedEventArgs e)
    {
        FromDatePicker.SelectedDate = DateTime.Today;
        ToDatePicker.SelectedDate = DateTime.Today.AddDays(7);

        FromTimeBox.Text = "00:00";
        ToTimeBox.Text = "23:59";

        await LoadTasksAsync();
    }


    private void RecurrenceUnitBox_SelectionChanged(object sender,SelectionChangedEventArgs e)
    {
        if (CustomRecurrencePanel == null ||
            RecurrenceUnitBox.SelectedItem is not ComboBoxItem item)
            return;

        CustomRecurrencePanel.Visibility =
            item.Tag?.ToString() == "Custom"
                ? Visibility.Visible
                : Visibility.Collapsed;
    }


    private async Task<int?> GetSelectedRecurrenceIdAsync()
    {
        if (RecurrenceUnitBox.SelectedItem is not ComboBoxItem selected)
            return null;

        string unitName = selected.Tag?.ToString() ?? "None";

        if (unitName == "None")
            return null;

        int value;
        
        if (unitName == "Custom")
        {
            if (!int.TryParse(RecurrenceValueBox.Text, out value)
                || value <= 0)
            {
                throw new InvalidOperationException(
                    "Az ismétlődés értéke pozitív egész szám legyen.");
            }

            if (CustomUnitBox.SelectedItem is not ComboBoxItem customUnit)
                throw new InvalidOperationException(
                    "Válassz időegységet!");

            unitName = customUnit.Tag?.ToString() ?? "Day";
        }
        else
        {
            value = 1;
        }

        int unit = unitName switch
        {
            "Minute" => 0,
            "Hour" => 1,
            "Day" => 2,
            "Week" => 3,
            "Month" => 4,
            "Year" => 5,
            _ => throw new InvalidOperationException(
                "Ismeretlen ismétlődési egység.")
        };

        var recurrence = await _api.CreateRecurrenceAsync(value, unit);

        return recurrence.Id;
    }
}