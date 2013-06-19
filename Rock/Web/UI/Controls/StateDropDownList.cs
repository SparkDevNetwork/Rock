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
        protected bool IsAbbreviated = false;

        /// <summary>
        /// Sets whether or not the State name is abbreviated. Default setting is false.
        /// </summary>
        /// <value>
        /// The boolean.
        /// </value>
        public bool UseAbbreviation
        {
            get
            {
                return IsAbbreviated;
            }
            set
            {
                IsAbbreviated = value;
            }
        }

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
            var states = definedType.DefinedValues.OrderBy( v => v.Order );
            if ( IsAbbreviated )
            {
                this.DataSource = states.Select( v => new { Id = v.Id, Value = v.Name } );
            }
            else 
            {
                this.DataSource = states.Select( v => new { Id = v.Id, Value = v.Description } );
            }                        
            this.DataTextField = "Value";
            this.DataValueField = "Id";
            this.DataBind();
        }
    }
}
