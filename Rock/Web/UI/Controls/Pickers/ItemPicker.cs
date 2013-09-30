//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ItemPicker : CompositeControl, IRockControl
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
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private HiddenField _hfItemId;
        private HiddenField _hfInitialItemParentIds;
        private HiddenField _hfItemName;
        private HiddenField _hfItemRestUrlExtraParams;
        private LinkButton _btnSelect;
        private LinkButton _btnSelectNone;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public abstract string ItemRestUrl { get; }

        /// <summary>
        /// Gets or sets the item rest URL extra params.
        /// </summary>
        /// <value>
        /// The item rest URL extra params.
        /// </value>
        public string ItemRestUrlExtraParams
        {
            get
            {
                EnsureChildControls();
                return _hfItemRestUrlExtraParams.Value;
            }

            set
            {
                EnsureChildControls();
                _hfItemRestUrlExtraParams.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the item id.
        /// </summary>
        /// <value>
        /// The item id.
        /// </value>
        public string ItemId
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfItemId.Value ) )
                {
                    _hfItemId.Value = Constants.None.IdValue;
                }

                return _hfItemId.Value;
            }

            set
            {
                EnsureChildControls();
                _hfItemId.Value = value;
            }
        }

        /// <summary>
        /// Gets the item ids.
        /// </summary>
        /// <value>
        /// The item ids.
        /// </value>
        public IEnumerable<string> ItemIds
        {
            get
            {
                EnsureChildControls();
                var ids = new List<string>();

                if ( !string.IsNullOrWhiteSpace( _hfItemId.Value ) )
                {
                    ids.AddRange( _hfItemId.Value.Split( ',' ) );
                }

                return ids;
            }

            set
            {
                EnsureChildControls();
                _hfItemId.Value = string.Join( ",", value );
            }
        }

        /// <summary>
        /// Gets or sets the initial item parent ids.
        /// </summary>
        /// <value>
        /// The initial item parent ids.
        /// </value>
        public string InitialItemParentIds
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfInitialItemParentIds.Value ) )
                {
                    _hfInitialItemParentIds.Value = Constants.None.IdValue;
                }

                return _hfInitialItemParentIds.Value;
            }

            set
            {
                EnsureChildControls();
                _hfInitialItemParentIds.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.  NOTE: If nothing was previously set, it will return <see cref="Rock.Constants.None.IdValue"/>.
        /// </value>
        public string SelectedValue
        {
            get
            {
                return ItemId;
            }

            private set
            {
                ItemId = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected values.
        /// </summary>
        /// <value>
        /// The selected values.
        /// </value>
        public IEnumerable<string> SelectedValues
        {
            get { return ItemIds; }
            private set { ItemIds = value; }
        }

        /// <summary>
        /// Gets or sets the name of the item.
        /// </summary>
        /// <value>
        /// The name of the item.
        /// </value>
        public string ItemName
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfItemName.Value ) )
                {
                    _hfItemName.Value = !string.IsNullOrWhiteSpace( DefaultText ) ? DefaultText : Constants.None.TextHtml;
                }

                return _hfItemName.Value;
            }

            set
            {
                EnsureChildControls();
                _hfItemName.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the item names.
        /// </summary>
        /// <value>
        /// The item names.
        /// </value>
        public IEnumerable<string> ItemNames
        {
            get
            {
                EnsureChildControls();
                var names = new List<string>();

                if ( !string.IsNullOrWhiteSpace( _hfItemName.Value ) )
                {
                    names.AddRange( _hfItemName.Value.Split( ',' ) );
                }

                return names;
            }

            set
            {
                EnsureChildControls();
                _hfItemName.Value = string.Join( ",", value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multi select].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multi select]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiSelect { get; set; }

        /// <summary>
        /// Gets or sets the default text.
        /// </summary>
        /// <value>
        /// The default text.
        /// </value>
        public string DefaultText { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPicker" /> class.
        /// </summary>
        public ItemPicker()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
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
            RegisterJavaScript();
            var sm = ScriptManager.GetCurrent( this.Page );
            EnsureChildControls();

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnSelect );
                sm.RegisterAsyncPostBackControl( _btnSelectNone );
            }
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            const string treeViewScriptFormat = "Rock.controls.itemPicker.initialize({{ controlId: '{0}', restUrl: '{1}', allowMultiSelect: {2}, defaultText: '{3}' }});";
            string treeViewScript = string.Format( treeViewScriptFormat, this.ID, this.ResolveUrl( ItemRestUrl ), this.AllowMultiSelect.ToString().ToLower(), this.DefaultText );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "item_picker-treeviewscript_" + this.ID, treeViewScript, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _hfItemId = new HiddenField();
            _hfItemId.ClientIDMode = ClientIDMode.Static;
            _hfItemId.ID = string.Format( "hfItemId_{0}", this.ID );
            _hfInitialItemParentIds = new HiddenField();
            _hfInitialItemParentIds.ClientIDMode = ClientIDMode.Static;
            _hfInitialItemParentIds.ID = string.Format( "hfInitialItemParentIds_{0}", this.ID );
            _hfItemName = new HiddenField();
            _hfItemName.ClientIDMode = ClientIDMode.Static;
            _hfItemName.ID = string.Format( "hfItemName_{0}", this.ID );
            _hfItemRestUrlExtraParams = new HiddenField();
            _hfItemRestUrlExtraParams.ClientIDMode = ClientIDMode.Static;
            _hfItemRestUrlExtraParams.ID = string.Format( "hfItemRestUrlExtraParams_{0}", this.ID );

            _btnSelect = new LinkButton();
            _btnSelect.ClientIDMode = ClientIDMode.Static;
            _btnSelect.CssClass = "btn btn-xs btn-primary";
            _btnSelect.ID = string.Format( "btnSelect_{0}", this.ID );
            _btnSelect.Text = "Select";
            _btnSelect.CausesValidation = false;

            // we only need the postback on Select if SelectItem is assigned or if this is PagePicker
            if ( SelectItem != null || (this is PagePicker) )
            {
                _btnSelect.Click += btnSelect_Click;
            }

            _btnSelectNone = new LinkButton();
            _btnSelectNone.ClientIDMode = ClientIDMode.Static;
            _btnSelectNone.CssClass = "picker-select-none";
            _btnSelectNone.ID = string.Format( "btnSelectNone_{0}", this.ID );
            _btnSelectNone.Text = "<i class='icon-remove'></i>";
            _btnSelectNone.CausesValidation = false;
            _btnSelectNone.Style[HtmlTextWriterStyle.Display] = "none";

            // we only need the postback on SelectNone if SelectItem is assigned or if this is PagePicker
            if ( SelectItem != null || ( this is PagePicker ) )
            {
                _btnSelectNone.Click += btnSelect_Click;
            }

            Controls.Add( _hfItemId );
            Controls.Add( _hfInitialItemParentIds );
            Controls.Add( _hfItemName );
            Controls.Add( _hfItemRestUrlExtraParams );
            Controls.Add( _btnSelect );
            Controls.Add( _btnSelectNone );

            RequiredFieldValidator.InitialValue = "0";
            RockControlHelper.CreateChildControls( this, Controls );
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
        /// This is where you implment the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            _hfItemId.RenderControl( writer );
            _hfInitialItemParentIds.RenderControl( writer );
            _hfItemName.RenderControl( writer );
            _hfItemRestUrlExtraParams.RenderControl( writer );

            if ( this.Enabled )
            {
                writer.AddAttribute( "id", this.ID.ToString() );
                writer.AddAttribute( "class", "picker picker-select" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( @"
                    <a class='picker-label' href='#'>
                        <i class='{2}'></i>
                        <span id='selectedItemLabel_{0}'>{1}</span>
                        <b class='caret pull-right'></b>
                    </a>", this.ID, this.ItemName, this.IconCssClass );
                writer.WriteLine();

                _btnSelectNone.RenderControl( writer );

                // picker menu
                writer.AddAttribute( "class", "picker-menu dropdown-menu" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // treeview
                writer.Write( @"
                    <div id='treeview-scroll-container_{0}' class='scroll-container scroll-container-picker'>
                        <div class='scrollbar'>
                            <div class='track'>
                                <div class='thumb'>
                                    <div class='end'></div>
                                </div>
                            </div>
                        </div>
                        <div class='viewport'>
                            <div class='overview'>
                                <div id='treeviewItems_{0}' class='treeview treeview-items'></div>        
                            </div>
                        </div>
                    </div>", this.ID );

                // picker actions
                writer.AddAttribute( "class", "picker-actions" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _btnSelect.RenderControl( writer );
                writer.Write( "<a class='btn btn-xs' id='btnCancel_{0}'>Cancel</a>", this.ID );
                writer.WriteLine();
                writer.RenderEndTag();
                
                // closing div of picker-menu
                writer.RenderEndTag();

                // closing div of picker
                writer.RenderEndTag();
            }
            else
            {
                // this picker is not enabled (readonly), so just render a readonly version
                writer.Write( @"<i class='icon-file-alt'></i><span id='selectedItemLabel_{0}'>{1}</span>", this.ID, this.ItemName );
            }
        }
        
        /// <summary>
        /// Gets the selected value as int.
        /// </summary>
        /// <param name="noneAsNull">if set to <c>true</c> [none as null].</param>
        /// <returns></returns>
        /// <value>
        /// The selected value as int.
        ///   </value>
        public int? SelectedValueAsInt( bool noneAsNull = true )
        {
            if ( string.IsNullOrWhiteSpace( ItemId ) )
            {
                return null;
            }

            int result = int.Parse( ItemId );
            if ( noneAsNull )
            {
                if ( result == Constants.None.Id )
                {
                    return null;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the value of the currently selected item.
        /// It will return NULL if either <see cref="T:Rock.Constants.None"/> or <see cref="T:Rock.Constants.All"/> is selected. />
        /// </summary>
        /// <returns></returns>
        public int? SelectedValueAsId()
        {
            if ( string.IsNullOrWhiteSpace( ItemId ) )
            {
                return null;
            }

            int result = int.Parse( ItemId );

            if ( result == Constants.None.Id )
            {
                return null;
            }

            if ( result == Constants.All.Id )
            {
                return null;
            }

            return result;
        }

        /// <summary>
        /// Selecteds the values as int.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> SelectedValuesAsInt()
        {
            var ids = new List<int>();

            if ( ItemIds == null || !ItemIds.Any() )
            {
                return ids;
            }

            foreach ( string keyVal in ItemIds )
            {
                int id = int.MinValue;
                if ( int.TryParse( keyVal, out id ) )
                {
                    ids.Add( id );
                }
            }

            return ids;
        }

        /// <summary>
        /// Handles the Click event of the _btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            SetValueOnSelect();
            if ( SelectItem != null )
            {
                SelectItem( sender, e );
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="id">The id.</param>
        public void SetValue( int? id )
        {
            ItemId = id.HasValue ? id.Value.ToString() : Constants.None.IdValue;
            SetValueOnSelect();
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="ids">The ids.</param>
        public void SetValues( IEnumerable<int> ids )
        {
            ItemIds = ids.Select( i => i.ToString() );
            SetValuesOnSelect();
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected abstract void SetValueOnSelect();

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected abstract void SetValuesOnSelect();

        /// <summary>
        /// Occurs when [select item].
        /// </summary>
        public event EventHandler SelectItem;

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public void ShowErrorMessage( string errorMessage )
        {
            RequiredFieldValidator.ErrorMessage = errorMessage;
            RequiredFieldValidator.IsValid = false;
        }

        #endregion
    }
}