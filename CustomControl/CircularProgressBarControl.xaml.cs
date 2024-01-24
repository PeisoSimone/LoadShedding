
using Microsoft.Maui.Dispatching;
using Syncfusion.Maui.ProgressBar;
using System;
using System.Runtime.Intrinsics.X86;


namespace loadshedding.CustomControl
{
    public static class ColorConstants
    {
        public static Color Red = Color.FromArgb("#c20c08");
        public static Color Yellow = Color.FromArgb("#Fffa00");
        public static Color Green = Color.FromArgb("#61c208");
    }

    public partial class CircularProgressBarControl : ContentView
    {
        private SfCircularProgressBar circularProgressBar;
        //private DispatcherTimer timer;

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
            DateTime defaultTime = DateTime.MinValue;
            //Active LoadShedding
            if (currentTime > EventStartTime && currentTime < EventEndTime)
            {
                ActiveLoadSheddingUI(currentTime, EventStartTime, EventEndTime);
            }
            //Not active LoadShedding
            else
            {
                DateTime futureEventStartTime = EventStartTime;
                DateTime futureEventBeforeStart = EventStartTime;

                if (defaultTime < currentTime && futureEventBeforeStart > currentTime)//default < current > start
                {
                    //futureEventBeforeStart count down 2 hours before load shedding starts
                    if (currentTime > futureEventStartTime.AddHours(-2))
                    {
                        futureEventBeforeStart = EventStartTime.AddHours(-2);
                    }
                    else if (currentTime < futureEventBeforeStart)
                    {
                        futureEventBeforeStart = EventStartTime;
                    }

                }
                else if (futureEventBeforeStart > defaultTime)
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

            double remainingProgress = CalculateProgress(totalDuration, elapsedTime);

            SetUpCircularBar();

            circularProgressBar.Progress = remainingProgress;

            if (remainingProgress < 100 && remainingProgress > 0)
            {
                //Implement count down

                circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = ColorConstants.Red, Value = 99.9 });//RED
                circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = ColorConstants.Red, Value = 25 });//RED
                circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = ColorConstants.Yellow, Value = 15 });//YELLOW
                circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = ColorConstants.Green, Value = 05 });//GREEN
            }
            else
            {
                circularProgressBar.ProgressFill = new SolidColorBrush(ColorConstants.Red);//SOLID RED
            }


            circularProgressBar.TrackFill = new SolidColorBrush(ColorConstants.Red);//RED Track

            CircularBarContent("POWER", ColorConstants.Red, "OFF", ColorConstants.Red);
        }

        private void NotActiveLoadSheddingUI(DateTime currentTime, DateTime newEventStartTime, DateTime newEventEndTime)
        {
            TimeSpan totalDuration = newEventEndTime - newEventStartTime;
            TimeSpan elapsedTime = currentTime - newEventStartTime;

            TimeSpan positiveElapsedTime = elapsedTime.Duration();

            double progress = (positiveElapsedTime.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100;

            progress = Math.Max(0, Math.Min(100, progress));

            SetUpCircularBar();

            double remainingProgress = 100 - progress;

            circularProgressBar.Progress = remainingProgress;

            if (remainingProgress < 100 && remainingProgress > 0)
            {
                circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = ColorConstants.Green, Value = 100 });//GREEN
                circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = ColorConstants.Green, Value = 25 });//GREEN
                circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = ColorConstants.Yellow, Value = 15 });//YELLOW
                circularProgressBar.GradientStops.Add(new ProgressGradientStop { Color = ColorConstants.Red, Value = 05 });//ORANGE
            }
            else
            {
                circularProgressBar.ProgressFill = new SolidColorBrush(ColorConstants.Green);//SOLID GREEN
            }
            
            circularProgressBar.TrackFill = new SolidColorBrush(ColorConstants.Green);//GREEN Track

            CircularBarContent("POWER", ColorConstants.Green, "ON", ColorConstants.Green);
        }

        private void CircularBarContent(string topLabelText, Color topLabelTextColor, string bottomLabelText, Color bottomLabelTextColor)
        {
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            Label textTopLabel = new Label();
            textTopLabel.FontFamily = "Nexa-Light";
            textTopLabel.HorizontalTextAlignment = TextAlignment.Center;
            textTopLabel.VerticalOptions = LayoutOptions.End;
            textTopLabel.Text = topLabelText;
            textTopLabel.TextColor = topLabelTextColor;
            Grid.SetRow(textTopLabel, 0);
            grid.Children.Add(textTopLabel);

            Label textBottomLabel = new Label();
            textBottomLabel.FontAttributes = FontAttributes.Bold;
            textBottomLabel.FontSize = 30;
            textBottomLabel.FontFamily = "Nexa-Heavy";
            textBottomLabel.HorizontalTextAlignment = TextAlignment.Center;
            textBottomLabel.VerticalOptions = LayoutOptions.Start;
            textBottomLabel.Text = bottomLabelText;
            textBottomLabel.TextColor = bottomLabelTextColor;
            Grid.SetRow(textBottomLabel, 1);
            grid.Children.Add(textBottomLabel);
            circularProgressBar.Content = grid;
        }

        private void SetUpCircularBar()
        {
            circularProgressBar = new SfCircularProgressBar();
            Content = circularProgressBar;

            circularProgressBar.HeightRequest = 300;
            circularProgressBar.TrackRadiusFactor = 0.8;
            circularProgressBar.ProgressRadiusFactor = 0.75;
            circularProgressBar.TrackThickness = 0.01;
            circularProgressBar.ProgressThickness = 0.075;
            circularProgressBar.ThicknessUnit = SizeUnit.Factor;
            circularProgressBar.ProgressCornerStyle = CornerStyle.EndCurve;
            circularProgressBar.FlowDirection = FlowDirection.LeftToRight;
        }

        private double CalculateProgress(TimeSpan totalDuration, TimeSpan elapsedTime)
        {
            TimeSpan positiveElapsedTime = elapsedTime.Duration();

            double progress = (positiveElapsedTime.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100;

            progress = Math.Max(0, Math.Min(100, progress));

            return 100 - progress;
        }
    }
}