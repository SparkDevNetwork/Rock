using System.Diagnostics;

using Rock.Jobs;

namespace TestJobPlugin
{
    public class TestCompiledPluginJob : RockJob
    {
        public override void Execute()
        {
            Debug.WriteLine( "TestCompiledPluginJob It worked. It was compiled as a RockJob with old Quartz 2" );
        }
    }
}
