// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// User controls for managing defined values
    /// </summary>
    [DisplayName( "Defined Type Check List" )]
    [Category( "Utility" )]
    [Description( "Used for managing the values of a defined type as a checklist." )]
    [DefinedTypeField( "Defined Type", "The Defined Type to display values for." )]
    [TextField( "Attribute Key", "The attribute key on the Defined Type that is used to store whether item has been completed (should be a boolean field type)." )]
    [BooleanField( "Hide Checked Items", "Hide items that are already checked.", false )]
    [BooleanField( "Hide Block When Empty", "Hides entire block if no checklist items are available.", false )]
    [TextField( "Checklist Title", "Title for your checklist.", false, "", "Description", 1 )]
    [CodeEditorField( "Checklist Description", "Description for your checklist. Leave this blank and nothing will be displayed.",
        CodeEditorMode.Html, CodeEditorTheme.Rock, 100, false, "", "Description", 2 )]
    [Rock.SystemGuid.BlockTypeGuid( "15572974-DD86-43C8-BBBF-5181EE76E2C9" )]
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

            this.BlockUpdated += DefinedTypeCheckList_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upSettings );

            string script = @"
$('.checklist-item label strong, .checklist-desc-toggle').on('click', function (e) {
    e.stopImmediatePropagation();
    e.preventDefault();
    $(this).closest('.panel-widget').toggleClass('collapsed');
    var $header = $(this).closest('header');
    $header.siblings('.panel-body').slideToggle();
    $header.find('i').toggleClass('fa-chevron-up fa-chevron-down');
});
";
            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "DefinedValueChecklistScript", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( Page.IsPostBack )
            {
                using ( var rockContext = new RockContext() )
                {
                    var definedValueService = new DefinedValueService( rockContext );

                    foreach ( RepeaterItem item in rptrValues.Items )
                    {
                        var hfValue = item.FindControl( "hfValue" ) as HiddenField;
                        var cbValue = item.FindControl( "cbValue" ) as CheckBox;

                        if ( hfValue != null && cbValue != null )
                        {
                            var value = definedValueService.Get( hfValue.ValueAsInt() );
                            if ( value != null )
                            {
                                Helper.LoadAttributes( value );
                                if ( value.GetAttributeValue( attributeKey ) != cbValue.Checked.ToString() )
                                {
                                    value.SetAttributeValue( attributeKey, cbValue.Checked.ToString() );
                                    value.SaveAttributeValues( rockContext );
                                }
                            }
                        }
                    }
                }
            }

            bool wasVisible = this.Visible;

            ShowList();

            if ( Page.IsPostBack && wasVisible && this.Visible == false )
            {
                // If last item was just checked do a redirect back to the same page.
                // This is needed to hide the control since content is inside an update
                // panel
                Response.Redirect( CurrentPageReference.BuildUrl(), false );
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the DefinedTypeCheckList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void DefinedTypeCheckList_BlockUpdated( object sender, EventArgs e )
        {
            ShowList();
        }

        private void ShowList()
        {
            this.Visible = true;

            // Should selected items be displayed
            bool hideCheckedItems = GetAttributeValue( "HideCheckedItems" ).AsBoolean();

            // Should content be hidden when empty list
            bool hideBlockWhenEmpty = GetAttributeValue( "HideBlockWhenEmpty" ).AsBoolean();

            Guid guid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( "DefinedType" ), out guid ) )
            {
                var definedType = DefinedTypeCache.Get( guid );
                if ( definedType != null )
                {
                    // Get the values
                    var values = definedType.DefinedValues.OrderBy( v => v.Order ).ToList();

                    // Find all the unselected values
                    var selectedValues = new List<int>();
                    foreach ( var value in values )
                    {
                        bool selected = false;
                        if ( bool.TryParse( value.GetAttributeValue( attributeKey ), out selected ) && selected )
                        {
                            selectedValues.Add( value.Id );
                        }
                    }

                    var displayValues = hideCheckedItems ?
                        values.Where( v => !selectedValues.Contains( v.Id ) ) : values;

                    rptrValues.DataSource = displayValues
                        .Select( v => new
                        {
                            Id = v.Id,
                            Value = "<strong class='checklist-desc-toggle'>" + v.Value + "</strong>",
                            Description = v.Description,
                            Selected = selectedValues.Contains( v.Id )
                        } ).ToList();
                    rptrValues.DataBind();

                    if ( displayValues.Any() || !hideBlockWhenEmpty )
                    {
                        if ( !GetAttributeValue( "ChecklistTitle" ).IsNullOrWhiteSpace() ) {
                            lTitle.Text = "<h4>" + GetAttributeValue( "ChecklistTitle" ) + "</h4>";
                        }
                        lDescription.Text = GetAttributeValue( "ChecklistDescription" );
                    }
                    else
                    {
                        this.Visible = false;
                    }
                }
            }
        }
    }
}