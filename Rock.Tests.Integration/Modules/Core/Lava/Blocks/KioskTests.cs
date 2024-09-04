using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.CheckIn;
using Rock.Lava;
using Rock.Tests.Shared.Lava;

namespace Rock.Tests.Integration.Modules.Core.Lava.Blocks
{
    [TestClass]
    public class KioskTests : LavaIntegrationTestBase
    {
        [TestMethod]
        public void KioskDevice_Returns_CampusId()
        {
            const int checkInLabelPrinterDeviceId = 1;
            var configuraedGroupTypes = new List<int>();
            var kioskDevice = KioskDevice.Get( checkInLabelPrinterDeviceId, configuraedGroupTypes );

            var values = new LavaDataDictionary();
            values.AddOrReplace( "Kiosk", kioskDevice );
            var options = new LavaTestRenderOptions { MergeFields = values };

            const string template = "Kiosk CampusId = {{ Kiosk.CampusId }}";
            string outputExpected = $"Kiosk CampusId = {kioskDevice.CampusId}";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }

        [TestMethod]
        public void KioskDevice_Returns_Device()
        {
            const int checkInLabelPrinterDeviceId = 1;
            var configuraedGroupTypes = new List<int>();
            var kioskDevice = KioskDevice.Get( checkInLabelPrinterDeviceId, configuraedGroupTypes );

            var values = new LavaDataDictionary();
            values.AddOrReplace( "Kiosk", kioskDevice );
            var options = new LavaTestRenderOptions { MergeFields = values };

            const string template = "Kiosk Device Name = {{ Kiosk.Device.Name }}";
            string outputExpected = $"Kiosk Device Name = {kioskDevice.Device.Name}";

            TestHelper.AssertTemplateOutput( outputExpected,
                template,
                options );
        }
    }
}
