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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to search and select a generic item.
    /// </summary>
    public class UniversalItemSearchPicker : CompositeControl, IRockControl, IRockChangeHandlerControl
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS Icon text.
        /// </summary>
        /// <value>
        /// The CSS icon class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string IconCssClass
        {
            get { return ViewState["IconCssClass"] as string ?? string.Empty; }
            set { ViewState["IconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
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
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
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
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
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
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return RequiredFieldValidator.ValidationGroup;
            }
            set
            {
                RequiredFieldValidator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private Panel _hiddenFieldsPanel;
        private HiddenFieldWithClass _hfItemValue;
        private HiddenFieldWithClass _hfItemName;

        private Panel _searchPanel;
        private RockTextBox _tbSearch;
        private RockCheckBox _cbIncludeInactive;

        private HtmlButton _btnSelect;
        private HtmlButton _btnSelectNone;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                EnsureChildControls();

                return _hfItemValue.Value;
            }

            set
            {
                EnsureChildControls();

                _hfItemValue.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        /// <value>
        /// The name of the item.
        /// </value>
        public string SelectedName
        {
            get
            {
                EnsureChildControls();

                return _hfItemName.Value;
            }

            set
            {
                EnsureChildControls();

                _hfItemName.Value = value;
            }
        }

        /// <summary>
        /// Gets ore sets a value that determines if the result details are
        /// always visible or only when clicked by the individual.
        /// </summary>
        public bool AreDetailsAlwaysVisible
        {
            get => ViewState[nameof( AreDetailsAlwaysVisible )] as bool? ?? false;
            set => ViewState[nameof( AreDetailsAlwaysVisible )] = value;
        }

        /// <summary>
        /// Gets ore sets a value that determines if the "include inactive"
        /// option should be visible in the search box.
        /// </summary>
        public bool IsIncludeInactiveVisible
        {
            get => ViewState[nameof( IsIncludeInactiveVisible )] as bool? ?? false;
            set => ViewState[nameof( IsIncludeInactiveVisible )] = value;
        }

        /// <summary>
        /// Gets or sets the URL to use when sending the POST request to
        /// search for results.
        /// </summary>
        public string SearchUrl
        {
            get => ViewState[nameof( SearchUrl )] as string ?? string.Empty;
            set => ViewState[nameof( SearchUrl )] = value;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalItemSearchPicker" /> class.
        /// </summary>
        public UniversalItemSearchPicker()
        {
            // TODO WHY?
            // note we are using HiddenFieldValidator instead of RequiredFieldValidator
            RequiredFieldValidator = new HiddenFieldValidator();

            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var sm = ScriptManager.GetCurrent( this.Page );
            EnsureChildControls();

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnSelect );
                sm.RegisterAsyncPostBackControl( _btnSelectNone );
            }
        }

        /// <summary>
        /// Registers the JavaScript.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            var restUrl = ResolveUrl( SearchUrl );
            var script = $@"Rock.controls.universalItemSearchPicker.initialize({{
    controlId: '{ClientID}',
    restUrl: '{restUrl}',
    areDetailsAlwaysVisible: {( AreDetailsAlwaysVisible ? "true" : "false" )}
}});";

            ScriptManager.RegisterStartupScript( this, GetType(), "universal_item_search-" + ClientID, script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _hiddenFieldsPanel = new Panel();
            _hiddenFieldsPanel.ID = "hiddenFieldsPanel";
            _hiddenFieldsPanel.Attributes.Add( "style", "display: none" );
            Controls.Add( _hiddenFieldsPanel );

            _hfItemValue = new HiddenFieldWithClass();
            _hfItemValue.CssClass = "js-item-value";
            _hiddenFieldsPanel.Controls.Add( _hfItemValue );
            _hfItemValue.ID = "hfItemId";
            _hfItemValue.Value = string.Empty;

            _hfItemName = new HiddenFieldWithClass();
            _hfItemName.CssClass = "js-item-name";
            _hiddenFieldsPanel.Controls.Add( _hfItemName );
            _hfItemName.ID = "hfItemName";

            _searchPanel = new Panel();
            _searchPanel.ID = "searchPanel";
            _searchPanel.CssClass = "js-universalitemsearchpicker-search-panel universalitemsearchpicker-search-panel";
            this.Controls.Add( _searchPanel );

            _tbSearch = new RockTextBox();
            _tbSearch.ID = "tbSearch";
            _tbSearch.CssClass = "input-group-sm js-universalitemsearchpicker-search-field js-universalitemsearchpicker-search-name universalitemsearchpicker-search-field";
            _tbSearch.Attributes["autocapitalize"] = "off";
            _tbSearch.Attributes["autocomplete"] = "off";
            _tbSearch.Attributes["autocorrect"] = "off";
            _tbSearch.Attributes["spellcheck"] = "false";
            _searchPanel.Controls.Add( _tbSearch );

            _cbIncludeInactive = new RockCheckBox();
            _cbIncludeInactive.ID = "cbIncludeInactive";
            _cbIncludeInactive.CssClass = "js-include-inactive";
            _cbIncludeInactive.ContainerCssClass = "mt-0 mb-0";
            _cbIncludeInactive.Text = "Include Inactive";
            Controls.Add( _cbIncludeInactive );

            _btnSelect = new HtmlButton();
            Controls.Add( _btnSelect );
            _btnSelect.Attributes["class"] = "btn btn-xs btn-primary js-universalitemsearchpicker-select";
            _btnSelect.ID = "btnSelect";
            _btnSelect.InnerText = "Select";
            _btnSelect.CausesValidation = false;
            _btnSelect.ServerClick += btnSelect_Click;

            _btnSelectNone = new HtmlButton();
            Controls.Add( _btnSelectNone );
            _btnSelectNone.Attributes["role"] = "button";
            _btnSelectNone.Attributes["type"] = "button";
            _btnSelectNone.Attributes["aria-label"] = "Clear selection";
            _btnSelectNone.Attributes["class"] = "btn picker-select-none js-picker-select-none";
            _btnSelectNone.ID = "btnSelectNone";
            _btnSelectNone.InnerHtml = "<i class='fa fa-times'></i>";
            _btnSelectNone.CausesValidation = false;
            _btnSelectNone.ServerClick += btnSelect_Click;

            RockControlHelper.CreateChildControls( this, Controls );

            // override a couple of property values on RequiredFieldValidator so that Validation works correctly
            RequiredFieldValidator.InitialValue = "";
            RequiredFieldValidator.ControlToValidate = _hfItemValue.ID;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            // Determine what text to display in the control. If there is no selection then show the placeholder.
            var selectedText = string.Empty;
            if ( SelectedName.IsNotNullOrWhiteSpace() )
            {
                selectedText = SelectedName;
            }

            if ( Enabled )
            {
                writer.AddAttribute( "id", ClientID );

                List<string> pickerClasses = new List<string>
                {
                    "picker",
                    "picker-fullwidth",
                    "picker-select picker-universalitemsearch " + CssClass
                };

                writer.AddAttribute( "class", pickerClasses.AsDelimited( " " ) );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _hiddenFieldsPanel.RenderControl( writer );

                // render picker-label
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "picker-label js-universalitemsearchpicker-toggle" );
                // href
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );

                writer.Write(
                    $@"
                <i class='{IconCssClass} fa-fw'></i>
                <span class='js-universalitemsearchpicker-selecteditem-label picker-selecteditem'>{selectedText}</span>
" );

                _btnSelectNone.RenderControl( writer );

                writer.Write( $@"<b class='fa fa-caret-down'></b>" );

                writer.RenderEndTag();

                // render picker-menu dropdown-menu
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "picker-menu dropdown-menu js-universalitemsearchpicker-menu" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "picker-search-header" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "<h4>Search</h4>" );

                // actions div
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "ml-auto" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( IsIncludeInactiveVisible )
                {
                    _cbIncludeInactive.RenderControl( writer );
                }

                // end actions div
                writer.RenderEndTag();

                // end row
                writer.RenderEndTag();

                _searchPanel.RenderControl( writer );

                writer.Write( @"
             <hr />
             <div class='js-universalitemsearchpicker-scroll-container scroll-container scroll-container-vertical scroll-container-picker'>
                <div class='scrollbar'>
                    <div class='track'>
                        <div class='thumb'>
                            <div class='end'></div>
                        </div>
                    </div>
                </div>
                <div class='viewport'>
                    <div class='overview'>
                        <ul class='picker-select-list js-universalitemsearchpicker-searchresults' style='padding: 0; list-style: none;'>
                        </ul>
                    </div>
                </div>
            </div>
             <div class='picker-actions'>
" );

                _btnSelect.RenderControl( writer );

                writer.Write( @"
            <button type='button' class='btn btn-link btn-xs js-universalitemsearchpicker-cancel'>Cancel</button>
            </div>
" );

                // picker-menu dropdown-menu
                writer.RenderEndTag();

                // picker picker-select picker-item
                writer.RenderEndTag();

                RegisterJavaScript();
            }
            else
            {
                // this picker is not enabled (readonly), so just render a readonly version
                List<string> pickerClasses = new List<string>
                {
                    "picker",
                    "picker-fullwidth",
                    "picker-select"
                };

                writer.AddAttribute( "class", pickerClasses.AsDelimited( " " ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                LinkButton linkButton = new LinkButton();
                linkButton.CssClass = "picker-label";
                linkButton.Text = $"<i class='fa fa-user'></i><span>{selectedText}</span>";
                linkButton.Enabled = false;
                linkButton.RenderControl( writer );

                writer.WriteLine();
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="item">The item to set.</param>
        public void SetValue( ListItemBag item )
        {
            if ( item != null )
            {
                SelectedValue = item.Value;
                SelectedName = item.Text;
            }
            else
            {
                SelectedValue = string.Empty;
                SelectedName = string.Empty;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            ValueChanged?.Invoke( sender, e );
        }

        #endregion

        #region IRockChangeHandlerControl

        /// <summary>
        /// Occurs when the selected value has changed
        /// </summary>
        public event EventHandler ValueChanged;

        #endregion IRockChangeHandlerControl
    }
}
