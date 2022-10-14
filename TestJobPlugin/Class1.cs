using System.Diagnostics;

using Rock.Jobs;

namespace TestJobPlugin
{
    public class TestCompiledPluginJob : RockJob
    {
        public override void Execute()
        {
            Debug.WriteLine( "It worked" );
        }
    }
}
