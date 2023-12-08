using Syncfusion.Maui.ProgressBar;
using loadshedding.Services;
using System;
using System.Security.Cryptography.X509Certificates;

namespace loadshedding.CustomControl;

public partial class CircularProgressBarControl : ContentView
{
    private SfCircularProgressBar circularProgressBar;

    public CircularProgressBarControl()
    {
        InitializeComponent();

       // UpdateProgressBar(startdatetime, enddatetime);
    }

    public DateTime enddatetime { get; set; }
    public DateTime startdatetime { get; set; }

    public void UpdateProgressBar(DateTime startdatetime, DateTime enddatetime)
    {
        DateTime startTime = startdatetime;
        DateTime currentTime = DateTime.Now;
        DateTime endTime = enddatetime;
        DateTime enddTime = startTime.AddHours(2);

        TimeSpan totalDuration = enddTime - startTime;
        TimeSpan elapsedTime = currentTime - startTime;

        double progress = (elapsedTime.TotalMilliseconds / totalDuration.TotalMilliseconds) * 100;

        progress = Math.Max(0, Math.Min(100, progress));

        double remainingProgress = 100 - progress;

        circularProgressBar = new SfCircularProgressBar();

        circularProgressBar.Progress = 20;

        this.Content = circularProgressBar;
    }
}