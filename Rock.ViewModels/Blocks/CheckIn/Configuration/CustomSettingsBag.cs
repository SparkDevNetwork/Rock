using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration
{
    public class CustomSettingsBag
    {
        public List<string> CheckInAreas { get; set; }
        public string CheckInConfiguration { get; set; }
        public string Kiosk { get; set; }
    }
}
