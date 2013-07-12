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
        private bool RebindRequired = false;

        /// <summary>
        /// Display an abbreviated state name
        /// </summary>
        public bool UseAbbreviation
        {
            get { return ViewState["UseAbbreviation"] as bool? ?? false; }
            set 
            {
                RebindRequired = (ViewState["UseAbbreviation"] as bool? ?? false) != value;
                ViewState["UseAbbreviation"] = value; 
            }
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.DataValueField = "Id";
            this.DataTextField = UseAbbreviation ? "Id" : "Value";

            RebindRequired = false;
            var definedType = DefinedTypeCache.Read( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );
            this.DataSource = definedType.DefinedValues.OrderBy( v => v.Order ).Select( v => new { Id = v.Name, Value = v.Description } );
            this.DataBind();
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( System.Web.UI.HtmlTextWriter writer )
        {
            if ( RebindRequired )
            {
                string value = this.SelectedValue;

                this.DataTextField = UseAbbreviation ? "Id" : "Value";
                this.DataBind();

                var li = this.Items.FindByValue( value );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }

            base.RenderControl( writer );
        }

    }
}
