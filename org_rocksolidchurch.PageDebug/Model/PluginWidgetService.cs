using Rock.Data;

namespace org_rocksolidchurch.PageDebug.Model
{
    public class PluginWidgetService : Service<PluginWidget>
    {
        public PluginWidgetService( RockContext rockContext ) : base( rockContext ) { }
    }
}
