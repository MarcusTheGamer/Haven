using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Models = Haven.Models;

namespace Haven.ViewModels
{
    internal class FindDevicesViewModel : ObservableObject
    {
        public ObservableCollection<Models.FoundDevice> FoundDevices { get; } = new()
        {
            new()
            {
                ModelName = "Thermostat",
            },
            new()
            {
                ModelName = "Light",
            },
            new()
            {
                ModelName = "Curtain",
            }
        };
    }
}
