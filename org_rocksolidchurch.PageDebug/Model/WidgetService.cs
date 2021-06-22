using Rock.Data;

namespace com_rocksolidchurchdemo.PageDebug.Model
{
    public class WidgetService : Service<Widget>
    {
        public WidgetService( RockContext rockContext ) : base( rockContext ) { }
    }
}
