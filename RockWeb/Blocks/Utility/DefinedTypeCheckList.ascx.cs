//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// User controls for managing defined values
    /// </summary>    
    [DefinedTypeField("Defined Type", "The Defined Type to display values for.")]
    [TextField( "Attribute Key", "The attribute key on the Defined Type that is used to store whether item has been completed (should be a boolean field type)." )]
    public partial class DefinedTypeCheckList : RockBlock
    {        
        private string attributeKey = string.Empty;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            attributeKey = GetAttributeValue( "AttributeKey" );

            BindList( !Page.IsPostBack );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblList_SelectedIndexChanged( object sender, EventArgs e )
        {
            var definedValueService = new DefinedValueService();
            foreach(ListItem item in cblList.Items)
            {
                int id = int.MinValue;
                if (int.TryParse(item.Value, out id))
                {
                    var value = definedValueService.Get( id );
                    if (value != null)
                    {
                        Helper.LoadAttributes( value );
                        value.SetAttributeValue( attributeKey, item.Selected.ToString() );
                        Helper.SaveAttributeValues( value, CurrentPersonId );
                    }
                }
            }

            BindList( true );
        }

        private void BindList( bool setValues )
        {
            Guid guid = Guid.Empty;
            if (Guid.TryParse(GetAttributeValue("DefinedType"), out guid))
            {
                var definedType = new DefinedTypeService().Get( guid );
                cblList.DataSource = definedType.DefinedValues;
                cblList.DataBind();

                if (setValues)
                {
                    foreach(var value in definedType.DefinedValues)
                    {
                        ListItem li = cblList.Items.FindByValue( value.Id.ToString() );
                        if (li != null)
                        {
                            Helper.LoadAttributes( value );
                            bool selected = false;
                            if (!bool.TryParse(value.GetAttributeValue(attributeKey), out selected))
                            {
                                selected = false;
                            }

                            li.Selected = selected;
                        }
                    }
                }
            }
        }
    }
}