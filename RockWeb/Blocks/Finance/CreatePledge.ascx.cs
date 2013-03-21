//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>
    [CustomDropdownListField( "Fund", "The fund that new pledges will be allocated toward.",
        listSource: "SELECT [Id] AS 'Value', [PublicName] AS 'Text' FROM [Fund] WHERE [IsPledgable] = 1 ORDER BY [SortOrder]", 
        key: "DefaultFund", required: true )]
    [TextField( "Legend Text", "Custom heading at the top of the form.", key: "LegendText", defaultValue: "Create a new pledge" )]
    public partial class CreatePledge : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                lLegendText.Text = GetAttributeValue( "LegendText" );
                BindFrequencyTypes();
            }
        }

        /// <summary>
        /// Binds the frequency types.
        /// </summary>
        private void BindFrequencyTypes()
        {
            var guid = new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY_TYPE );
            var definedType = new DefinedTypeService().Get( guid );
            ddlFrequencyType.DataTextField = "Description";
            ddlFrequencyType.DataValueField = "Id";
            ddlFrequencyType.DataSource = definedType.DefinedValues;
            ddlFrequencyType.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            throw new NotImplementedException();
        }
    }
}
