using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haven.Models;

namespace Haven.ViewModels
{
    class SettingsViewModel
    {
        public List<RouteItem> Options { get; } =
        [
            new("Family Members", "ManageFamilyPage")
        ];
    }
}
