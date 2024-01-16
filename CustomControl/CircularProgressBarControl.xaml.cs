

using Syncfusion.Maui.ProgressBar;
using System;


namespace loadshedding.CustomControl;

public partial class CircularProgressBarControl : ContentView
{
    private SfCircularProgressBar circularProgressBar;

    public DateTime EventStartTime { get; set; }
    public DateTime EventEndTime { get; set; }
    public DateTime secEventStartTime { get; set; }

    public CircularProgressBarControl()
    {
        InitializeComponent();
    }

    public void UpdateProgressBar()
    {
        DateTime currentTime = DateTime.Now;

        if (currentTime > EventStartTime && currentTime < EventEndTime)
        {
            ActiveLoadSheddingUI(currentTime, EventStartTime, EventEndTime);
        }
        else
        {
            DateTime futureEventStartTime = EventStartTime;
            DateTime futureEventBeforeStart = EventStartTime;

            if(DateTime.Now > DateTime.MinValue && futureEventBeforeStart > DateTime.Now )
            {
                futureEventBeforeStart = EventStartTime.AddHours(-2);
            }
            else if (futureEventBeforeStart > DateTime.MinValue)
            {
                futureEventBeforeStart = EventStartTime;
            }
            else
            {
                futureEventBeforeStart = EventStartTime;
            }

            DateTime newEventStartTime = futureEventBeforeStart;
            DateTime newEventEndTime = futureEventStartTime;

            NotActiveLoadSheddingUI(currentTime, newEventStartTime, newEventEndTime);
        }
    }

    private void ActiveLoadSheddingUI(DateTime currentTime, DateTime EventStartTime, DateTime EventEndTime)
    {
        TimeSpan totalDuration = EventEndTime - EventStartTime;
        TimeSpan elapsedTime = currentTime - EventStartTime;

        double progress = (elapsedTime.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100;

        progress = Math.Max(0, Math.Min(100, progress));

        circularProgressBar = new SfCircularProgressBar();
        Content = circularProgressBar;

        double remainingProgress = 100 - progress;

        circularProgressBar.Progress = remainingProgress;

        if (remainingProgress == 100)
        {
            circularProgressBar.ProgressFill = new SolidColorBrush(Color.FromArgb("#c20c08"));//SOLID RED
        }
        else
        {
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#c20c08"), Value = 99.9 });//RED
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#c20c08"), Value = 25 });//RED
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#Fffa00"), Value = 15 });//YELLOW
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#61c208"), Value = 05 });//GREEN
        }

        circularProgressBar.TrackFill = new SolidColorBrush(Color.FromArgb("#c20c08"));//RED Track
        circularProgressBar.HeightRequest = 300;
        circularProgressBar.TrackRadiusFactor = 0.8;
        circularProgressBar.ProgressRadiusFactor = 0.75;
        circularProgressBar.TrackThickness = 0.01;
        circularProgressBar.ProgressThickness = 0.075;
        circularProgressBar.ThicknessUnit = SizeUnit.Factor;
        circularProgressBar.ProgressCornerStyle = CornerStyle.EndCurve;

        Grid grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());

        Label textTopLabel = new Label();
        textTopLabel.Text = "POWER";
        textTopLabel.FontFamily = "Nexa-Light";
        textTopLabel.HorizontalTextAlignment = TextAlignment.Center;
        textTopLabel.VerticalOptions = LayoutOptions.End;
        textTopLabel.TextColor = Color.FromArgb("#c20c08");
        Grid.SetRow(textTopLabel, 0);
        grid.Children.Add(textTopLabel);

        Label textBottomLabel = new Label();
        textBottomLabel.Text = "OFF";
        textBottomLabel.FontFamily = "Nexa-Heavy";
        textBottomLabel.FontAttributes = FontAttributes.Bold;
        textBottomLabel.FontSize = 40;
        textBottomLabel.HorizontalTextAlignment = TextAlignment.Center;
        textBottomLabel.VerticalOptions = LayoutOptions.Start;
        textBottomLabel.TextColor = Color.FromArgb("#c20c08");
        Grid.SetRow(textBottomLabel, 1);
        grid.Children.Add(textBottomLabel);
        circularProgressBar.Content = grid;
    }
    private void NotActiveLoadSheddingUI(DateTime currentTime, DateTime newEventStartTime, DateTime newEventEndTime)
    {
        TimeSpan totalDuration = newEventEndTime - newEventStartTime;
        TimeSpan elapsedTime = currentTime - newEventStartTime;

        TimeSpan positiveElapsedTime = elapsedTime.Duration();

        double progress = (positiveElapsedTime.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100;

        progress = Math.Max(0, Math.Min(100, progress));

        circularProgressBar = new SfCircularProgressBar();
        Content = circularProgressBar;

        double remainingProgress = 100 - progress;

        circularProgressBar.Progress = remainingProgress;

        if (remainingProgress == 0)
        {
            circularProgressBar.ProgressFill = new SolidColorBrush(Color.FromArgb("#61c208"));//SOLID GREEN
        }
        else
        {
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#61c208"), Value = 100 });//GREEN
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#61c208"), Value = 25 });//GREEN
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#Fffa00"), Value = 15 });//YELLOW
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#c20c08"), Value = 05 });//ORANGE
        }

        circularProgressBar.TrackFill = new SolidColorBrush(Color.FromArgb("#61c208"));//GREEN Track
        circularProgressBar.HeightRequest = 300;
        circularProgressBar.TrackRadiusFactor = 0.8;
        circularProgressBar.ProgressRadiusFactor = 0.75;
        circularProgressBar.TrackThickness = 0.01;
        circularProgressBar.ProgressThickness = 0.075;
        circularProgressBar.ThicknessUnit = SizeUnit.Factor;
        circularProgressBar.ProgressCornerStyle = CornerStyle.EndCurve;

        Grid grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition());
        grid.RowDefinitions.Add(new RowDefinition());

        Label textTopLabel = new Label();
        textTopLabel.Text = "POWER";
        textTopLabel.FontFamily = "Nexa-Light";
        textTopLabel.HorizontalTextAlignment = TextAlignment.Center;
        textTopLabel.VerticalOptions = LayoutOptions.End;
        textTopLabel.TextColor = Color.FromArgb("#61c208");
        Grid.SetRow(textTopLabel, 0);
        grid.Children.Add(textTopLabel);

        Label textBottomLabel = new Label();
        textBottomLabel.Text = "ON";
        textBottomLabel.FontAttributes = FontAttributes.Bold;
        textBottomLabel.FontSize = 40;
        textBottomLabel.FontFamily = "Nexa-Heavy";
        textBottomLabel.HorizontalTextAlignment = TextAlignment.Center;
        textBottomLabel.VerticalOptions = LayoutOptions.Start;
        textBottomLabel.TextColor = Color.FromArgb("#61c208");
        Grid.SetRow(textBottomLabel, 1);
        grid.Children.Add(textBottomLabel);
        circularProgressBar.Content = grid;
    }
}

