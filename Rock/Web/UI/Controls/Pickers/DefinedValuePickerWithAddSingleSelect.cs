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
    /// A DefinedValuePicker control that allows a defined value to be added on the fly.
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.DefinedValuePickerWithAdd" />
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class DefinedValuePickerWithAddSingleSelect : DefinedValuePickerWithAdd
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
            writer.AddAttribute( "class", $"{this.ClientID}-js-defined-value-selector controls controls-row form-control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _ddlDefinedValues.RenderControl( writer );

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

        #region IDefinedValuePickerWtihAdd Implementation

        /// <summary>
        /// Gets the selected defined values identifier.
        /// The field type uses this value for GetEditValue(). This is so all the DefinedValue pickers can share a field type.
        /// This override will point to SelectedDefinedValueId.
        /// </summary>
        /// <value>
        /// Returns the SelectedDefinedValueId in an array.
        /// </value>
        public override int[] SelectedDefinedValuesId
        {
            get
            {
                return new int[] { SelectedDefinedValueId.Value };
            }

            set
            {
                SelectedDefinedValueId = value == null || value.Length == 0 ? 0 : value[0];
            }
        }

        /// <summary>
        /// Loads the defined values.
        /// </summary>
        public override void LoadDefinedValues()
        {
            _ddlDefinedValues.Items.Clear();

            if ( DefinedTypeId.HasValue )
            {
                if ( IncludeEmptyOption )
                {
                    // add Empty option first
                    _ddlDefinedValues.Items.Add( new ListItem() );
                }

                var definedTypeCache = DefinedTypeCache.Get( DefinedTypeId.Value );
                var definedValuesList = definedTypeCache?.DefinedValues
                    .Where( a => a.IsActive || IncludeInactive || a.Id == SelectedDefinedValueId )
                    .OrderBy( v => v.Order )
                    .ThenBy( v => v.Value )
                    .ToList();

                if ( definedValuesList != null && definedValuesList.Any() )
                {
                    foreach ( var definedValue in definedValuesList )
                    {
                        _ddlDefinedValues.Items.Add(
                            new ListItem
                            {
                                Text = DisplayDescriptions ? definedValue.Description : definedValue.Value,
                                Value = definedValue.Id.ToString(),
                                Selected = definedValue.Id == SelectedDefinedValueId
                            } );
                    }
                }
            }
        }

        #endregion IDefinedValuePickerWtihAdd Implementation

        private RockDropDownList _ddlDefinedValues;

        /// <summary>
        /// Gets or sets the selected defined value identifier.
        /// </summary>
        /// <value>
        /// The selected defined value identifier.
        /// </value>
        public int? SelectedDefinedValueId
        {
            get
            {
                int parsedInt = 0;
                int.TryParse( ViewState["SelectedDefinedValueId"].ToStringSafe(), out parsedInt );

                return parsedInt;
            }

            set
            {
                ViewState["SelectedDefinedValueId"] = value == null ? string.Empty : value.ToString();

                LoadDefinedValues();
            }
        }

        /// <summary>
        /// Gets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                return DefinedValueCache.GetValue( SelectedDefinedValueId );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // After adding a new value this will post back so we should re-load the defined value list so the new one is included.
            EnsureChildControls();
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _ddlDefinedValues = new RockDropDownList();
            _ddlDefinedValues.ID = this.ID + "_ddlDefinedValues";
            _ddlDefinedValues.EnhanceForLongLists = this.EnhanceForLongLists;
            _ddlDefinedValues.Style.Add( "width", "85%" );
            _ddlDefinedValues.SelectedIndexChanged += ddlDefinedValues_SelectedIndexChanged;
            _ddlDefinedValues.AutoPostBack = true;
            Controls.Add( _ddlDefinedValues );

            LinkButtonAddDefinedValue = new LinkButton();
            LinkButtonAddDefinedValue.ID = this.ID + "_lbAddDefinedValue";
            LinkButtonAddDefinedValue.CssClass = "btn btn-default btn-square js-button-add-defined-value";
            LinkButtonAddDefinedValue.OnClientClick = $"javascript:$('.{this.ClientID}-js-defined-value-selector').fadeToggle(400, 'swing', function() {{ $('#{DefinedValueEditorControl.ClientID}').fadeToggle(); }});  return false;";
            LinkButtonAddDefinedValue.Controls.Add( new HtmlGenericControl { InnerHtml = "<i class='fa fa-plus'></i>" } );
            Controls.Add( LinkButtonAddDefinedValue );

            LoadDefinedValues();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlDefinedValues control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDefinedValues_SelectedIndexChanged( object sender, EventArgs e )
        {
            SelectedDefinedValueId = ( ( RockDropDownList ) sender ).SelectedValue.AsIntegerOrNull();
        }
    }
}
