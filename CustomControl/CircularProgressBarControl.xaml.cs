

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
            DateTime activeEventStartTime = EventStartTime;
            DateTime activeEventEndTime = EventEndTime;
            DateTime futureEventStartTime = secEventStartTime;

            DateTime newEventStartTime = activeEventEndTime;
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
            stackLayout.Children.Add(circularProgressBar);
        }

        circularProgressBar.Progress = progress;

        if (progress >= 75)
        {
            circularProgressBar.ProgressFill = new SolidColorBrush(Color.FromArgb("#b96516"));
            circularProgressBar.TrackFill = new SolidColorBrush(Color.FromArgb("#f5e2d1"));
        }
        else if (progress < 75)
        {
            circularProgressBar.ProgressFill = new SolidColorBrush(Color.FromArgb("#fcd1d1"));
            circularProgressBar.TrackFill = new SolidColorBrush(Color.FromArgb("#d61717"));
        }

        circularProgressBar.HeightRequest = 280;
        circularProgressBar.TrackThickness = 10;
        circularProgressBar.ProgressThickness = 10;

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

        double progress = (elapsedTime.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100;

        progress = Math.Max(0, Math.Min(100, progress));

        circularProgressBar = new SfCircularProgressBar();
        Content = circularProgressBar;

        StackLayout stackLayout = Content.FindByName<StackLayout>("Circular");
        if (stackLayout != null)
        {
            stackLayout.Children.Add(circularProgressBar);
        }

        circularProgressBar.Progress = progress;

        if (progress >= 75)
        {
            circularProgressBar.ProgressFill = new SolidColorBrush(Color.FromArgb("#b96516"));
            circularProgressBar.TrackFill = new SolidColorBrush(Color.FromArgb("#f5e2d1"));
        }
        else if (progress < 75)
        {
            circularProgressBar.ProgressFill = new SolidColorBrush(Color.FromArgb("#beecc9"));

            circularProgressBar.TrackFill = new SolidColorBrush(Color.FromArgb("#25be4a"));
        }

        circularProgressBar.HeightRequest = 280;
        circularProgressBar.TrackThickness = 10;
        circularProgressBar.ProgressThickness = 10;

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
        textBottomLabel.FontSize = 25;
        textBottomLabel.HorizontalTextAlignment = TextAlignment.Center;
        textBottomLabel.VerticalOptions = LayoutOptions.Start;
        textBottomLabel.TextColor = Color.FromArgb("#25be4a");
        Grid.SetRow(textBottomLabel, 1);
        grid.Children.Add(textBottomLabel);
        circularProgressBar.Content = grid;

    }
}

