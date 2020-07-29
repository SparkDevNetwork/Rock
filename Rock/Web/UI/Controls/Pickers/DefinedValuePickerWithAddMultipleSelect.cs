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
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A multiple DefinedValuePicker control that allows a defined value to be added on the fly.
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.DefinedValuePickerWithAdd" />
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class DefinedValuePickerWithAddMultipleSelect : DefinedValuePickerWithAdd
    {
        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( "id", this.ClientID.ToString() );
            writer.AddAttribute( "class", this.CssClass );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Defined Value selector with Add button
            string additionalControlCSS = !EnhanceForLongLists ? "checkboxlist-group" : string.Empty;

            writer.AddAttribute( "class", $"{this.ClientID}-js-defined-value-selector controls controls-row form-control-group {additionalControlCSS}" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( EnhanceForLongLists )
            {
                _lboxDefinedValues.RenderControl( writer );
            }
            else
            {
                _cblDefinedValues.RenderControl( writer );
            }

            // Only render the Add button if the user is authorized to edit the defined type
            var definedType = DefinedTypeCache.Get( DefinedTypeId.Value );
            if ( definedType.IsAuthorized( Authorization.EDIT, ( ( RockPage ) Page ).CurrentPerson ) && IsAllowAddDefinedValue )
            {
                LinkButtonAddDefinedValue.RenderControl( writer );
            }

            writer.RenderEndTag();

            // Defined Value Editor
            DefinedValueEditorControl.RenderControl( writer );

            // picker div end tag
            writer.RenderEndTag();
        }

        /// <summary>
        /// Loads the defined values.
        /// </summary>
        public override void LoadDefinedValues()
        {
            if ( DefinedTypeId.HasValue )
            {
                var definedTypeCache = DefinedTypeCache.Get( DefinedTypeId.Value );
                var definedValuesList = definedTypeCache?.DefinedValues
                    .Where( a => a.IsActive || IncludeInactive || SelectedDefinedValuesId.Contains( a.Id ) )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .ToList();

                if ( definedValuesList != null && definedValuesList.Any() )
                {
                    if ( EnhanceForLongLists )
                    {
                        LoadListBox( definedValuesList );
                    }
                    else
                    {
                        LoadCheckBoxList( definedValuesList );
                    }
                }
            }
        }

        private void LoadCheckBoxList( List<DefinedValueCache> definedValuesList )
        {
            _cblDefinedValues.Items.Clear();

            foreach ( var definedValue in definedValuesList )
            {
                _cblDefinedValues.Items.Add(
                    new ListItem
                    {
                        Text = DisplayDescriptions ? definedValue.Description : definedValue.Value,
                        Value = definedValue.Id.ToString(),
                        Selected = SelectedDefinedValuesId.Contains( definedValue.Id )
                    } );
            }
        }

        private void LoadListBox( List<DefinedValueCache> definedValuesList )
        {
            _lboxDefinedValues.Items.Clear();

            if ( IncludeEmptyOption )
            {
                _lboxDefinedValues.Items.Add( new ListItem() );
            }

            foreach ( var definedValue in definedValuesList )
            {
                _lboxDefinedValues.Items.Add(
                    new ListItem
                    {
                        Text = DisplayDescriptions ? definedValue.Description : definedValue.Value,
                        Value = definedValue.Id.ToString(),
                        Selected = SelectedDefinedValuesId.Contains( definedValue.Id )
                    } );
            }
        }

        private RockCheckBoxList _cblDefinedValues;
        private RockListBox _lboxDefinedValues;

        /// <summary>
        /// Gets or sets the repeat direction.
        /// </summary>
        /// <value>
        /// The repeat direction.
        /// </value>
        public RepeatDirection RepeatDirection { get; set; } = System.Web.UI.WebControls.RepeatDirection.Horizontal;

        /// <summary>
        /// Gets or sets the number of columns the checkbox will use.
        /// </summary>
        /// <value>
        /// The repeat columns.
        /// </value>
        public int RepeatColumns { get; set; } = 4;

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if ( EnhanceForLongLists )
            {
                _lboxDefinedValues = new RockListBox();
                _lboxDefinedValues.ID = this.ID + "_lboxDefinedValues";
                _lboxDefinedValues.Style.Add( "width", "85%" );
                _lboxDefinedValues.AutoPostBack = true;
                _lboxDefinedValues.SelectedIndexChanged += lboxDefinedValues_SelectedIndexChanged;
                Controls.Add( _lboxDefinedValues );
            }
            else
            {
                _cblDefinedValues = new RockCheckBoxList();
                _cblDefinedValues.ID = this.ID + "_cblDefinedValues";
                _cblDefinedValues.Style.Add( "width", "85%" );
                _cblDefinedValues.RepeatColumns = this.RepeatColumns;
                _cblDefinedValues.RepeatDirection = this.RepeatDirection;
                _cblDefinedValues.AutoPostBack = true;
                _cblDefinedValues.SelectedIndexChanged += cblDefinedValues_SelectedIndexChanged;
                Controls.Add( _cblDefinedValues );
            }

            LinkButtonAddDefinedValue = new LinkButton();
            LinkButtonAddDefinedValue.ID = this.ID + "_lbAddDefinedValue";
            LinkButtonAddDefinedValue.Text = "Add Item";
            LinkButtonAddDefinedValue.CssClass = "btn btn-default btn-link js-button-add-defined-value";
            LinkButtonAddDefinedValue.OnClientClick = $"javascript:$('.{this.ClientID}-js-defined-value-selector').fadeToggle(400, 'swing', function() {{ $('#{DefinedValueEditorControl.ClientID}').fadeToggle(); }});  return false;";
            Controls.Add( LinkButtonAddDefinedValue );

            DefinedValueEditorControl.IsMultiSelection = true;

            LoadDefinedValues();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void cblDefinedValues_SelectedIndexChanged( object sender, EventArgs e )
        {
            SelectedDefinedValuesId = ( ( RockCheckBoxList ) sender )
                .Items.OfType<ListItem>()
                .Where( a => a.Selected )
                .Select( a => a.Value )
                .AsIntegerList()
                .ToArray();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the lboxDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lboxDefinedValues_SelectedIndexChanged( object sender, EventArgs e )
        {
            SelectedDefinedValuesId = ( ( RockListBox ) sender )
                .Items.OfType<ListItem>()
                .Where( a => a.Selected )
                .Select( a => a.Value )
                .AsIntegerList()
                .ToArray();
        }
    }
}
