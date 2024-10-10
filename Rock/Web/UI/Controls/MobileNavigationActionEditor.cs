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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Enums.Mobile;
using Rock.Mobile;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// User control that allows for editing a <see cref="MobileNavigationAction"/>
    /// value.
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    internal class MobileNavigationActionEditor : CompositeControl, IRockControl
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
            get => ViewState["Label"] as string ?? string.Empty;
            set => ViewState["Label"] = value;
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
            get => ViewState["FormGroupCssClass"] as string ?? string.Empty;
            set => ViewState["FormGroupCssClass"] = value;
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
            get => HelpBlock?.Text ?? string.Empty;
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
            get => WarningBlock?.Text ?? string.Empty;
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
            get => ViewState["Required"] as bool? ?? false;
            set => ViewState["Required"] = value;
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get => RequiredFieldValidator?.ErrorMessage ?? string.Empty;
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
            get => ViewState["ValidationGroup"] as string;
            set
            {
                ViewState["ValidationGroup"] = value;

                EnsureChildControls();
                _rblAction.ValidationGroup = value;
                _nbPopCount.ValidationGroup = value;
                _ppPage.ValidationGroup = value;
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
            get => !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
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

        /// <summary>
        /// The <see cref="RockRadioButtonList"/> for selected the type of
        /// action to perform.
        /// </summary>
        private RockRadioButtonList _rblAction;

        /// <summary>
        /// The <see cref="NumberBox"/> for entering the number of pages to pop.
        /// </summary>
        private NumberBox _nbPopCount;

        /// <summary>
        /// The <see cref="PagePicker"/> for selecting the page to be navigated to.
        /// </summary>
        private PagePicker _ppPage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the navigation action.
        /// </summary>
        /// <value>
        /// The navigation action.
        /// </value>
        public MobileNavigationAction NavigationAction
        {
            get => GetNavigationAction();
            set => SetNavigationAction( value );
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Reset the child controls into a known state that is ready for
            // use as an IRockControl.
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            // Create a radio button list that contains all the action types
            // that can be performed.
            _rblAction = new RockRadioButtonList
            {
                ID = $"{ID}_rblAction",
                AutoPostBack = true,
                RepeatDirection = RepeatDirection.Horizontal
            };
            _rblAction.BindToEnum<MobileNavigationActionType>();
            _rblAction.SelectedIndexChanged += RblAction_SelectedIndexChanged;
            Controls.Add( _rblAction );

            // Create a number box that allows the individual to enter the
            // number of pages that will be popped off the stack.
            _nbPopCount = new NumberBox
            {
                ID = $"{ID}_nbPopCount",
                Label = "Page Count",
                Help = "The number of pages that will be removed from the navigation stack.",
                CssClass = "input-width-sm",
                MinimumValue = "1",
                MaximumValue = "100",
                NumberType = ValidationDataType.Integer,
            };
            Controls.Add( _nbPopCount );

            // Create a page picker that allows the individual to select which
            // page to navigate to for PopPage, ResetToPage, and PushPage actions.
            _ppPage = new PagePicker
            {
                ID = $"{ID}_ppPage",
                Label = "Page",
                Help = "The page to be navigated to when the action is executed.",
                PromptForPageRoute = false
            };
            Controls.Add( _ppPage );

            UpdateControlStates();
        }

        /// <inheritdoc/>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <inheritdoc/>
        void IRockControl.RenderBaseControl( HtmlTextWriter writer )
        {
            _rblAction.RenderControl( writer );

            if ( _nbPopCount.Visible || _ppPage.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "margin-t-sm margin-l-md" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                if ( _nbPopCount.Visible )
                {
                    _nbPopCount.RenderControl( writer );
                }

                if ( _ppPage.Visible )
                {
                    _ppPage.RenderControl( writer );
                }

                writer.RenderEndTag(); // class=margin-t-sm margin-l-md
            }
        }

        /// <summary>
        /// Updates the control states to match current UI selections.
        /// </summary>
        private void UpdateControlStates()
        {
            var actionType = _rblAction.SelectedValueAsEnum<MobileNavigationActionType>( MobileNavigationActionType.None );

            _nbPopCount.Visible = actionType == MobileNavigationActionType.PopPage;
            _nbPopCount.Required = _nbPopCount.Visible;

            _ppPage.Visible = actionType == MobileNavigationActionType.PushPage
                || actionType == MobileNavigationActionType.ReplacePage
                || actionType == MobileNavigationActionType.ResetToPage;
            _ppPage.Required = _ppPage.Visible;
            _ppPage.SiteType = Model.SiteType.Mobile;

            switch ( actionType )
            {
                case MobileNavigationActionType.PushPage:
                    _ppPage.Help = "The selected page will be pushed onto the navigation stack.";
                    break;

                case MobileNavigationActionType.ReplacePage:
                    _ppPage.Help = "The current page will be replaced with the selected page.";
                    break;

                case MobileNavigationActionType.ResetToPage:
                    _ppPage.Help = "The entire navigation stack will be cleared and the selected page will become the first page.";
                    break;
            }
        }

        /// <summary>
        /// Gets the navigation action as set in the UI.
        /// </summary>
        /// <returns>A new <see cref="MobileNavigationAction"/> that represents the UI selections.</returns>
        private MobileNavigationAction GetNavigationAction()
        {
            EnsureChildControls();

            var type = _rblAction.SelectedValueAsEnum<MobileNavigationActionType>( MobileNavigationActionType.None );
            int? count = null;
            Guid? pageGuid = null;

            // If action type is pop page then we can just take the value and
            // default to 1 if it isn't set as that is safe.
            if ( type == MobileNavigationActionType.PopPage )
            {
                count = _nbPopCount.IntegerValue ?? 1;
            }

            // If action type is for one of the navigate-to-page types then
            // we need to verify the user actually selected a page to navigate
            // to. Otherwise revert to "none".
            if ( type == MobileNavigationActionType.PushPage
                || type == MobileNavigationActionType.ReplacePage
                || type == MobileNavigationActionType.ResetToPage )
            {
                var pageId = _ppPage.PageId;

                if ( pageId.HasValue )
                {
                    pageGuid = PageCache.Get( pageId.Value )?.Guid;
                }

                if ( !pageGuid.HasValue )
                {
                    type = MobileNavigationActionType.None;
                }
            }

            return new MobileNavigationAction
            {
                Type = type,
                PopCount = count,
                PageGuid = pageGuid
            };
        }

        /// <summary>
        /// Sets the navigation action value provided by the caller.
        /// </summary>
        /// <param name="action">The action value to set.</param>
        private void SetNavigationAction( MobileNavigationAction action )
        {
            EnsureChildControls();

            if ( action == null )
            {
                _rblAction.SetValue( MobileNavigationActionType.None.ConvertToInt() );
                _nbPopCount.IntegerValue = 1;
                _ppPage.SetValue( ( int? ) null );

                return;
            }

            _rblAction.SetValue( action.Type.ConvertToInt() );
            _nbPopCount.IntegerValue = action.PopCount ?? 1;

            if ( action.PageGuid.HasValue )
            {
                var pageId = PageCache.Get( action.PageGuid.Value )?.Id;

                _ppPage.SetValue( pageId );
            }
            else
            {
                _ppPage.SetValue( ( int? ) null );
            }

            UpdateControlStates();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event of the RblAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RblAction_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateControlStates();
        }

        #endregion
    }
}
