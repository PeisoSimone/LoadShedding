using Syncfusion.Maui.ProgressBar;

namespace loadshedding.CustomControl;

public partial class CircularProgressBarControl : ContentView
{
    private SfCircularProgressBar circularProgressBar;
    private dynamic loadSheddingAreaResults;

    public CircularProgressBarControl()
    {
        InitializeComponent();

        circularProgressBar = new SfCircularProgressBar();

        this.Content = circularProgressBar;

        UpdateProgressBar(loadSheddingAreaResults);
    }


    public void UpdateProgressBar(dynamic loadSheddingAreaResults)
    {
        if (loadSheddingAreaResults?.events?.Count > 0)
        {
            var firstEvent = loadSheddingAreaResults.events[0];

            DateTime currentTime = DateTime.Now;
            DateTime eventStartTime = firstEvent.start;
            DateTime eventEndTime = firstEvent.end.AddMinutes(-30);
            

            //Therefore loadshedding is active
            if (IsTimeBetween(currentTime, eventStartTime, eventEndTime))
            {
                UpdateProgressBarUI(currentTime, eventStartTime, eventEndTime);
            }

            //Therefore loadshedding in not active
            if (!IsTimeBetween(currentTime, eventStartTime, eventEndTime))
            {
                eventStartTime = eventEndTime;
                eventEndTime = eventStartTime;

                UpdateProgressBarUI(currentTime, eventStartTime, eventEndTime);
            }
        }
        else
        {
            DateTime currentTime = DateTime.Now;
            DateTime eventStartTime = DateTime.Now.AddHours(-1);
            DateTime eventEndTime = eventStartTime.AddHours(2);

            UpdateProgressBarUI(currentTime, eventStartTime, eventEndTime);
        }
    }

    static bool IsTimeBetween(DateTime currentTime, DateTime eventStartTime, DateTime eventEndTime)
    {
        return currentTime >= eventStartTime && currentTime <= eventEndTime;
    }

    private void UpdateProgressBarUI(DateTime currentTime,DateTime eventStartTime, DateTime eventEndTime)
    {
        TimeSpan totalDuration = eventEndTime - eventStartTime;
        TimeSpan elapsedTime = currentTime - eventStartTime;

        double progress = (elapsedTime.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100;

        progress = Math.Max(0, Math.Min(100,progress));

        double remainingProgress = 100 - progress;

        if (circularProgressBar == null)
        {
            circularProgressBar = new SfCircularProgressBar();
            this.Content = circularProgressBar;

            StackLayout stackLayout = this.Content.FindByName<StackLayout>("Circular");
            if (stackLayout != null)
            {
                stackLayout.Children.Add(circularProgressBar);
            }
        }

        circularProgressBar.Progress = remainingProgress;
    }
}
