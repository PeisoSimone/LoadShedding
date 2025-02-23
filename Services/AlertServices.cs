﻿using loadshedding.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadshedding.Services
{
    public class AlertServices : IAlertServices
    {
        public async Task ShowAlert(string message)
          {
            Page currentPage = Application.Current.MainPage;
            await currentPage.DisplayAlert("Alert Message", message, "OK");
        }
    }
}
