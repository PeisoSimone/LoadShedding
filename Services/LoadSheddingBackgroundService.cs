using System;
using System.Timers;
using loadshedding.Interfaces;

namespace loadshedding.Services
{
    public class LoadSheddingBackgroundService
    {
        private readonly ILoadSheddingStatusServices _statusServices;
        private System.Timers.Timer _timer;

        public LoadSheddingBackgroundService(ILoadSheddingStatusServices statusServices)
        {
            _statusServices = statusServices;
        }

        public void Start()
        {
            _timer = new System.Timers.Timer(60000); // Check every second (1000ms)
            _timer.Elapsed += async (sender, e) => await CheckStatus();
            _timer.Start();
        }

        private async Task CheckStatus()
        {
            await _statusServices.GetNationalStatus();
        }
    }

}
