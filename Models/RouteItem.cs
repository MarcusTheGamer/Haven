using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haven.Models
{
    public class RouteItem
    {
        public string Name { get; set; }
        public string Route { get; set; }

        public RouteItem(string name, string route)
        {
            Name = name;
            Route = route;
        }
    }
}
