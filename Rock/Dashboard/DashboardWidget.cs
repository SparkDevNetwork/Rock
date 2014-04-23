using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Web.UI;

namespace Rock.Dashboard
{
    [TextField( "Title", "The title of the widget. Defaults to the title of the metric if a metric is specified.", false )]
    [TextField( "Subtitle", "The subtitle of the widget. Defaults to the subtitle of the metric if a metric is specified.", false )]
    [IntegerField( "Column Width", "The width of the widget.", true, 4 )]
    [ContextAware( typeof( Rock.Model.Campus ) )]
    public abstract class DashboardWidget : RockBlock
    {
        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( System.Web.UI.HtmlTextWriter writer )
        {
            // wrap DashboardWidget in a col-md-X div 
            string columnClass = string.Format("col-md-{0}", this.GetAttributeValue("ColumnWidth").AsInteger(false) ?? 4);
            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, columnClass );
            writer.AddAttribute( System.Web.UI.HtmlTextWriterAttribute.Class, "dashboard-widget" );
            writer.RenderBeginTag( System.Web.UI.HtmlTextWriterTag.Div );
            base.RenderControl( writer );
            writer.RenderEndTag();
        }
    }
}
