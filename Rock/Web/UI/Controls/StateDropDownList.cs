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
        public bool UseAbbreviation { get; set; }
        
        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            var definedType = DefinedTypeCache.Read( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );
            
            // Using custom solution instead of `BindToDefinedType` because we don't want the DefinedValue's `Id`
            // to be the value in the dropdown list.
            this.DataSource = definedType.DefinedValues.OrderBy( v => v.Order ).Select( v => new { Id = v.Name, Value = v.Description } );
            this.DataTextField = UseAbbreviation ? "Id" : "Value";
            this.DataValueField = "Id";
            this.DataBind();
        }
    }
}
