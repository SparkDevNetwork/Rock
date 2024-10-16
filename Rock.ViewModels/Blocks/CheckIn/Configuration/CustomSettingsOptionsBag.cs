using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.ViewModels.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration
{
    public class CustomSettingsOptionsBag
    {
        public List<ConfigurationTemplateBag> CheckInConfigurationOptions { get; set; }

        public List<ConfigurationAreaBag> CheckInAreas { get; set; }
        public List<CampusBag> CampusesAndKiosks { get; set; }
    }
}
