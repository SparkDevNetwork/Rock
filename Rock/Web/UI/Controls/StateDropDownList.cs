//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class StateDropDownList : LabeledDropDownList
    {

        /// <summary>
        /// Display an abbreviated state name
        /// </summary>
        public bool UseAbbreviation
        {
            get { return ViewState["UseAbbreviation"] as bool? ?? false; }
            set { ViewState["UseAbbreviation"] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateDropDownList" /> class.
        /// </summary>
        public StateDropDownList()
            : base()
        {
            var definedType = DefinedTypeCache.Read( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );
            this.DataSource = definedType.DefinedValues.OrderBy( v => v.Order ).Select( v => new { Id = v.Name, Value = v.Description } );
            this.DataValueField = "Id";
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( System.Web.UI.HtmlTextWriter writer )
        {
            this.DataTextField = UseAbbreviation ? "Id" : "Value";
            this.DataBind();
            base.RenderControl( writer );
        }

    }
}
