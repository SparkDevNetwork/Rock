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
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
            var definedType = DefinedTypeCache.Read( new Guid( SystemGuid.DefinedType.LOCATION_ADDRESS_STATE ) );

            // Using custom solution instead of `BindToDefinedType` because we don't want the DefinedValue's `Id`
            // to be the value in the dropdown list.
            var items = definedType.DefinedValues.OrderBy( v => v.Order ).Select( v => new { Id = v.Name, Value = v.Description } );
            this.DataSource = items;
            this.DataTextField = "Value";
            this.DataValueField = "Id";
            this.DataBind();
        }
    }
}
