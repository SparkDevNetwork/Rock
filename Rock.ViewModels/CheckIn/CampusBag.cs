using System.Collections.Generic;

using Rock.ViewModels.Blocks.CheckIn.CheckInKiosk;

namespace Rock.ViewModels.CheckIn
{
    public class CampusBag : CheckInItemBag
    {
        public List<WebKioskBag> Kiosks { get; set; }
    }
}
