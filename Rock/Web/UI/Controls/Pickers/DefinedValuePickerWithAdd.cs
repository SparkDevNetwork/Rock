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
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class DefinedValuePickerWithAdd : CompositeControl, IRockControl
    {
        #region IRockControl Implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The text for the label." )]
        public virtual string Label
        {
            get
            {
                return ViewState["Label"] as string ?? string.Empty;
            }

            set
            {
                ViewState["Label"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The help block." )]
        public virtual string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [Bindable( true )]
        [Category( "Appearance" )]
        [DefaultValue( "" )]
        [Description( "The warning block." )]
        public virtual string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [Bindable( true )]
        [Category( "Behavior" )]
        [DefaultValue( "false" )]
        [Description( "Is the value required?" )]
        public virtual bool Required
        {
            get
            {
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>The required error message.</value>
        public virtual string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        public virtual string FormGroupCssClass
        {
            get
            {
                return ViewState["FormGroupCssClass"] as string ?? string.Empty;
            }

            set
            {
                ViewState["FormGroupCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public virtual HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public virtual WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public virtual RequiredFieldValidator RequiredFieldValidator { get; set; }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public virtual string ValidationGroup
        {
            get
            {
                return ViewState["ValidationGroup"] as string;
            }

            set
            {
                ViewState["ValidationGroup"] = value;
            }
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            throw new NotImplementedException();
        }

        #endregion IRockControl Implementation

        /// <summary>
        /// Gets the selected defined values identifier.
        /// The field type uses this value for GetEditValue(). This is so all the DefinedValue pickers can share a field type.
        /// </summary>
        /// <value>
        /// Returns the SelectedDefinedValueId in an array.
        /// </value>
        public virtual int[] SelectedDefinedValuesId
        {
            get
            {
                var selectedDefinedValuesId = new List<int>();

                string selectedids = ViewState["SelectedDefinedValuesId"].ToStringSafe();
                if ( selectedids.IsNotNullOrWhiteSpace() )
                {
                    string[] ids = selectedids.Split( ',' );
                    foreach ( string id in ids )
                    {
                        int parsedint;
                        if ( int.TryParse( id, out parsedint ) )
                        {
                            selectedDefinedValuesId.Add( parsedint );
                        }
                    }
                }

                return selectedDefinedValuesId.ToArray();
            }

            set
            {
                if ( value == null )
                {
                    ViewState["SelectedDefinedValuesId"] = string.Empty;
                }
                else
                {
                    var selectedDefinedValuesId = new List<int>();

                    // check each value in the array to make sure it is valid and add it to the list
                    foreach ( int selectedId in value )
                    {
                        if ( DefinedValueCache.Get( selectedId ) != null )
                        {
                            selectedDefinedValuesId.Add( selectedId );
                        }
                    }

                    // join the list into a csv and save to ViewState["SelectedDefinedValueId"]
                    ViewState["SelectedDefinedValuesId"] = string.Join( ",", selectedDefinedValuesId );
                }

                LoadDefinedValues();
            }
        }

        /// <summary>
        /// Loads the defined values.
        /// </summary>
        /// <param name="selectedDefinedValueIds">The selected defined value ids. Only the first element in the array is used for this single select control.</param>
        public virtual void LoadDefinedValues( int[] selectedDefinedValueIds )
        {
            this.SelectedDefinedValuesId = selectedDefinedValueIds;
            LoadDefinedValues();
        }

        /// <summary>
        /// Loads the defined values.
        /// </summary>
        public virtual void LoadDefinedValues()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the defined value editor control.
        /// </summary>
        /// <value>
        /// The defined value editor control.
        /// </value>
        protected DefinedValueEditor DefinedValueEditorControl { get; private set; }

        /// <summary>
        /// Gets or sets the link button add defined value.
        /// </summary>
        /// <value>
        /// The link button add defined value.
        /// </value>
        protected LinkButton LinkButtonAddDefinedValue { get; set; }

        /// <summary>
        /// Gets or sets the defined type identifier.
        /// </summary>
        /// <value>
        /// The defined type identifier.
        /// </value>
        public int? DefinedTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [display descriptions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display descriptions]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayDescriptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include inactive].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow adding new values].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow adding new values]; otherwise, <c>false</c>.
        /// </value>
        public bool IsAllowAddDefinedValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include empty option].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include empty option]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeEmptyOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dropdownlist should allow a searc when used for single select
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enhance for long lists]; otherwise, <c>false</c>.
        /// </value>
        public bool EnhanceForLongLists { get; set; }

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
        /// This method will also create the DefinedValueEditor
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            DefinedValueEditorControl = new DefinedValueEditor();
            DefinedValueEditorControl.ID = this.ID + "_definedValueEditor";
            DefinedValueEditorControl.Hidden = true;
            DefinedValueEditorControl.DefinedTypeId = DefinedTypeId.Value;
            DefinedValueEditorControl.DefinedValueSelectorClientId = this.ClientID;
            Controls.Add( DefinedValueEditorControl );
        }
    }
}
