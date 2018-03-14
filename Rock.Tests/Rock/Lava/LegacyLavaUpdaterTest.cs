using System;

using Xunit;

using Rock.Lava;
using System.Collections.Generic;

namespace Rock.Tests.Rock.Lava
{
    public class LegacyLavaUpdaterTest
    {
        [Fact]
        public void UpdateLegacyLava()
        {
            LegacyLavaUpdater legacyLavaUpdater = new LegacyLavaUpdater();
            legacyLavaUpdater.FindLegacyLava();

            Assert.NotEmpty( legacyLavaUpdater.SQLUpdateScripts );

        }
    }
}
