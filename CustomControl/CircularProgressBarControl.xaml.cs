

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

        StackLayout stackLayout = Content.FindByName<StackLayout>("Circular");
        if (stackLayout != null)
        {
            stackLayout.Children.Clear();
            stackLayout.Children.Add(circularProgressBar);
        }

        circularProgressBar.Progress = progress;

        if (progress == 100)
        {
            circularProgressBar.ProgressFill = new SolidColorBrush(Color.FromArgb("#Ff0000"));//SOLID RED
        }
        else
        {
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#Ff0000"), Value = 0 });//RED
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#Ff0000"), Value = 75 });//RED
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#Fffa00"), Value = 85 });//YELLOW
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#0dff00"), Value = 95 });//GREEN
        }

        circularProgressBar.TrackFill = new SolidColorBrush(Color.FromArgb("#Ff0000"));//RED Track
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

        Label textToplabel = new Label();
        textToplabel.Text = "POWER";
        textToplabel.HorizontalTextAlignment = TextAlignment.Center;
        textToplabel.VerticalOptions = LayoutOptions.End;
        textToplabel.TextColor = Color.FromArgb("#d61717");
        Grid.SetRow(textToplabel, 0);
        grid.Children.Add(textToplabel);

        Label textBottomLabel = new Label();
        textBottomLabel.Text = "OFF";
        textBottomLabel.FontAttributes = FontAttributes.Bold;
        textBottomLabel.FontSize = 30;
        textBottomLabel.HorizontalTextAlignment = TextAlignment.Center;
        textBottomLabel.VerticalOptions = LayoutOptions.Start;
        textBottomLabel.TextColor = Color.FromArgb("#d61717");
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

        StackLayout stackLayout = Content.FindByName<StackLayout>("Circular");
        if (stackLayout != null)
        {
            stackLayout.Children.Clear();
            stackLayout.Children.Add(circularProgressBar);
        }

        circularProgressBar.Progress = progress;

        if(progress == 100)
        {
            circularProgressBar.ProgressFill = new SolidColorBrush(Color.FromArgb("#45c53b"));//SOLID GREEN
        }
        else
        {
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#0dff00"), Value = 0 });//GREEN
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#0dff00"), Value = 75 });//GREEN
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#Fffa00"), Value = 85 });//YELLOW
            circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = Color.FromArgb("#Ff0000"), Value = 95 });//ORANGE
        }

        circularProgressBar.TrackFill = new SolidColorBrush(Color.FromArgb("#0dff00"));//GREEN Track
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

        Label textToplabel = new Label();
        textToplabel.Text = "POWER";
        textToplabel.HorizontalTextAlignment = TextAlignment.Center;
        textToplabel.VerticalOptions = LayoutOptions.End;
        textToplabel.TextColor = Color.FromArgb("#25be4a");
        Grid.SetRow(textToplabel, 0);
        grid.Children.Add(textToplabel);

        Label textBottomLabel = new Label();
        textBottomLabel.Text = "ON";
        textBottomLabel.FontAttributes = FontAttributes.Bold;
        textBottomLabel.FontSize = 30;
        textBottomLabel.HorizontalTextAlignment = TextAlignment.Center;
        textBottomLabel.VerticalOptions = LayoutOptions.Start;
        textBottomLabel.TextColor = Color.FromArgb("#25be4a");
        Grid.SetRow(textBottomLabel, 1);
        grid.Children.Add(textBottomLabel);
        circularProgressBar.Content = grid;
    }
}

