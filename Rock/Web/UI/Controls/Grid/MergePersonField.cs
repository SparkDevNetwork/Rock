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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Special GridField to be used with the RockWeb.Blocks.Crm.PersonMerge block
    /// </summary>
    /// <seealso cref="Rock.Web.UI.Controls.RockTemplateField" />
    /// <seealso cref="Rock.Web.UI.Controls.INotRowSelectedField" />
    public class MergePersonField : RockTemplateField, INotRowSelectedField
    {
        #region Properties

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId
        {
            get { return ViewState["PersonId"] as int? ?? 0; }
            set { ViewState["PersonId"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName
        {
            get { return ViewState["PersonName"] as string; }
            set { ViewState["PersonName"] = value; }
        }

        /// <summary>
        /// Gets or sets the family names.
        /// </summary>
        /// <value>
        /// The family names.
        /// </value>
        public string HeaderContent
        {
            get
            {
                var headerContent = ViewState["HeaderContent"] as string;
                if ( headerContent == null )
                {
                    headerContent = string.Empty;
                    HeaderContent = headerContent;
                }

                return headerContent;
            }

            set
            {
                ViewState["HeaderContent"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        public DateTime? ModifiedDateTime
        {
            get { return ViewState["ModifiedDateTime"] as DateTime?; }
            set { ViewState["ModifiedDateTime"] = value; }
        }

        /// <summary>
        /// Gets or sets the modified by.
        /// </summary>
        /// <value>
        /// The modified by.
        /// </value>
        public string ModifiedBy
        {
            get { return ViewState["ModifiedBy"] as string; }
            set { ViewState["ModifiedBy"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is primary person].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is primary person]; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimaryPerson
        {
            get { return ViewState["IsPrimaryPerson"] as bool? ?? false; }
            set { ViewState["IsPrimaryPerson"] = value; }
        }

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public Grid ParentGrid { get; internal set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField" />.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            base.Initialize( sortingEnabled, control );

            MergePersonFieldHeaderTemplate headerTemplate = new MergePersonFieldHeaderTemplate();
            headerTemplate.LinkButtonClick += HeaderTemplate_LinkButtonClick;
            this.HeaderTemplate = headerTemplate;
            this.ItemTemplate = new MergePersonSelectFieldTemplate();
            this.ParentGrid = control as Grid;

            return false;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the LinkButtonClick event of the HeaderTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void HeaderTemplate_LinkButtonClick( object sender, EventArgs e )
        {
            Delete( e );
        }

        /// <summary>
        /// Raises the <see cref="E:Click"/> event.
        /// </summary>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        internal virtual void Delete( EventArgs e )
        {
            if ( OnDelete != null )
            {
                OnDelete( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete].
        /// </summary>
        public event EventHandler OnDelete;

        /// <summary>
        /// Occurs when [on data bound].
        /// </summary>
        public event EventHandler<MergePersonFieldRowEventArgs> DataBound;

        /// <summary>
        /// Handles the on data bound.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        internal void HandleOnDataBound( object sender, MergePersonFieldRowEventArgs e )
        {
            if ( this.DataBound != null )
            {
                this.DataBound( sender, e );
            }
        }

        #endregion

        #region MergeField Classes

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="Rock.Web.UI.Controls.RowEventArgs" />
        public class MergePersonFieldRowEventArgs : RowEventArgs
        {
            /// <summary>
            /// Gets the merge person field.
            /// </summary>
            /// <value>
            /// The merge person field.
            /// </value>
            public MergePersonField MergePersonField { get; private set; }

            /// <summary>
            /// Gets or sets the type of the selection control.
            /// </summary>
            /// <value>
            /// The type of the selection control.
            /// </value>
            public SelectionControlType SelectionControlType { set; get; }

            /// <summary>
            /// Gets or sets the display type of the content.
            /// </summary>
            /// <value>
            /// The display type of the content.
            /// </value>
            public ContentDisplayType ContentDisplayType { set; get; }

            /// <summary>
            /// Sets the content HTML.
            /// </summary>
            /// <value>
            /// The content HTML.
            /// </value>
            public string ContentHTML { set; internal get; }

            /// <summary>
            /// Sets a value indicating whether this <see cref="MergePersonFieldRowEventArgs"/> is selected.
            /// </summary>
            /// <value>
            ///   <c>true</c> if selected; otherwise, <c>false</c>.
            /// </value>
            public bool Selected { set; internal get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="MergePersonFieldRowEventArgs"/> class.
            /// </summary>
            /// <param name="row">The row.</param>
            /// <param name="mergePersonField">The merge person field.</param>
            public MergePersonFieldRowEventArgs( GridViewRow row, MergePersonField mergePersonField ) : base( row )
            {
                MergePersonField = mergePersonField;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public enum SelectionControlType
        {
            /// <summary>
            /// No selection control. For example, a section header
            /// </summary>
            None,

            /// <summary>
            /// Renders a check box
            /// </summary>
            Checkbox,

            /// <summary>
            /// Renders a radio button
            /// </summary>
            RadioButton,
        }

        /// <summary>
        ///
        /// </summary>
        public enum ContentDisplayType
        {
            /// <summary>
            /// Display the property/attribute value as the Checkbox/Radiobutton label
            /// </summary>
            SelectionLabel,

            /// <summary>
            /// Display the property/attribute value in a merge-property-value div
            /// </summary>
            ContentWrapper
        }

        /// <summary>
        ///
        /// </summary>
        /// <seealso cref="System.Web.UI.ITemplate" />
        public class MergePersonSelectFieldTemplate : ITemplate
        {
            /// <summary>
            /// Gets the merge person field.
            /// </summary>
            /// <value>
            /// The merge person field.
            /// </value>
            public MergePersonField MergePersonField { get; private set; }

            /// <summary>
            /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
            /// </summary>
            /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
            public void InstantiateIn( Control container )
            {
                DataControlFieldCell cell = container as DataControlFieldCell;
                this.MergePersonField = cell.ContainingField as MergePersonField;

                Panel selectionControlContainer = new Panel { ID = $"selectionControlContainer_{MergePersonField.PersonId}", CssClass = "js-selection-control-container selection-control-container", };
                cell.Controls.Add( selectionControlContainer );

                RockCheckBox selectionControlCheckbox = new RockCheckBox
                {
                    ID = $"selectionControlCheckbox_{MergePersonField.PersonId}",
                    CssClass = "js-selection-control selection-control"
                };

                selectionControlCheckbox.Attributes["data-person-id"] = MergePersonField.PersonId.ToString();
                selectionControlContainer.Controls.Add( selectionControlCheckbox );

                RockRadioButton selectionControlRadioButton = new RockRadioButton
                {
                    ID = $"selectionControlRadioButton_{MergePersonField.PersonId}",
                    CssClass = "js-selection-control selection-control"
                };

                selectionControlRadioButton.Attributes["data-person-id"] = MergePersonField.PersonId.ToString();
                selectionControlContainer.Controls.Add( selectionControlRadioButton );

                Literal contentHtmlLiteral = new Literal { ID = $"contentHtmlLiteral_{MergePersonField.PersonId}" };
                cell.Controls.Add( contentHtmlLiteral );

                cell.CssClass = "js-merge-field-cell merge-field-cell";

                cell.DataBinding += cell_DataBinding;
                cell.Load += cell_Load;
            }

            /// <summary>
            /// Handles the Load event of the cell control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
            private void cell_Load( object sender, EventArgs e )
            {
                DataControlFieldCell cell = sender as DataControlFieldCell;
                var row = cell.DataItemContainer as GridViewRow;

                if ( row.Page.IsPostBack )
                {
                    Panel selectionControlContainer = row.FindControl( $"selectionControlContainer_{MergePersonField.PersonId}" ) as Panel;
                    RockCheckBox selectionControlCheckbox = selectionControlContainer.FindControl( $"selectionControlCheckbox_{MergePersonField.PersonId}" ) as RockCheckBox;
                    RockRadioButton selectionControlRadioButton = selectionControlContainer.FindControl( $"selectionControlRadioButton_{MergePersonField.PersonId}" ) as RockRadioButton;
                }
            }

            /// <summary>
            /// Handles the DataBinding event of the cell control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
            private void cell_DataBinding( object sender, EventArgs e )
            {
                DataControlFieldCell cell = sender as DataControlFieldCell;
                var row = cell.DataItemContainer as GridViewRow;

                Literal contentHtmlLiteral = cell.FindControl( $"contentHtmlLiteral_{MergePersonField.PersonId}" ) as Literal;
                Panel selectionControlContainer = cell.FindControl( $"selectionControlContainer_{MergePersonField.PersonId}" ) as Panel;
                RockCheckBox selectionControlCheckbox = selectionControlContainer.FindControl( $"selectionControlCheckbox_{MergePersonField.PersonId}" ) as RockCheckBox;
                RockRadioButton selectionControlRadioButton = selectionControlContainer.FindControl( $"selectionControlRadioButton_{MergePersonField.PersonId}" ) as RockRadioButton;

                MergePersonFieldRowEventArgs mergePersonFieldRowEventArgs = new MergePersonFieldRowEventArgs( row, MergePersonField );
                MergePersonField.HandleOnDataBound( sender, mergePersonFieldRowEventArgs );

                if ( mergePersonFieldRowEventArgs.ContentDisplayType == ContentDisplayType.ContentWrapper )
                {
                    contentHtmlLiteral.Visible = true;
                    contentHtmlLiteral.Text = $"<div class='merge-property-value'>{mergePersonFieldRowEventArgs.ContentHTML}</div>";
                    selectionControlCheckbox.Text = null;
                    selectionControlRadioButton.Text = null;
                }
                else
                {
                    contentHtmlLiteral.Visible = false;
                    selectionControlCheckbox.Text = mergePersonFieldRowEventArgs.ContentHTML;
                    selectionControlRadioButton.Text = mergePersonFieldRowEventArgs.ContentHTML;
                }

                selectionControlCheckbox.Visible = mergePersonFieldRowEventArgs.SelectionControlType == SelectionControlType.Checkbox;
                selectionControlCheckbox.Checked = mergePersonFieldRowEventArgs.Selected;

                selectionControlRadioButton.Visible = mergePersonFieldRowEventArgs.SelectionControlType == SelectionControlType.RadioButton;
                selectionControlRadioButton.Checked = mergePersonFieldRowEventArgs.Selected;

                selectionControlRadioButton.GroupName = $"selectionControlRadioButtonGroup_{row.RowIndex}";
            }
        }

        /// <summary>
        ///
        /// </summary>
        public class MergePersonFieldHeaderTemplate : ITemplate
        {
            /// <summary>
            /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
            /// </summary>
            /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
            public void InstantiateIn( Control container )
            {
                var cell = container as DataControlFieldCell;
                if ( cell != null )
                {
                    var mergeField = cell.ContainingField as MergePersonField;
                    if ( mergeField != null )
                    {
                        var lbDelete = new LinkButton();
                        lbDelete.CausesValidation = false;
                        lbDelete.CssClass = "btn btn-danger btn-xs btn-square mr-2 pull-right";
                        lbDelete.ToolTip = "Remove Person";
                        cell.Controls.Add( lbDelete );

                        HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                        buttonIcon.Attributes.Add( "class", "fa fa-times" );
                        lbDelete.Controls.Add( buttonIcon );

                        lbDelete.Click += lbDelete_Click;

                        // make sure delete button is registered for async postback (needed just in case the grid was created at runtime)
                        var sm = ScriptManager.GetCurrent( mergeField.ParentGrid.Page );
                        sm.RegisterAsyncPostBackControl( lbDelete );

                        HtmlGenericContainer headerSummary = new HtmlGenericContainer( "div", "merge-header-summary cursor-pointer js-merge-header-summary" );
                        headerSummary.Attributes.Add( "data-person-id", mergeField.PersonId.ToString() );

                        var i = new HtmlGenericControl( "i" );
                        i.Attributes.Add( "class", "header-checkbox-icon js-header-checkbox-icon fa fa-2x " + ( mergeField.IsPrimaryPerson ? "fa-check-square-o" : "fa-square-o" ) );
                        headerSummary.Controls.Add( i );

                        headerSummary.Controls.Add( new LiteralControl( mergeField.HeaderContent ) );

                        string created = ( mergeField.ModifiedDateTime.HasValue ? mergeField.ModifiedDateTime.ToElapsedString() + " " : "" ) +
                            ( !string.IsNullOrWhiteSpace( mergeField.ModifiedBy ) ? "by " + mergeField.ModifiedBy : "" );
                        if ( created != string.Empty )
                        {
                            headerSummary.Controls.Add( new LiteralControl( string.Format( "<small>Last Modified {0}</small>", created ) ) );
                        }

                        cell.Controls.Add( headerSummary );
                    }
                }
            }

            /// <summary>
            /// Handles the Click event of the lbDelete control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            protected void lbDelete_Click( object sender, EventArgs e )
            {
                if ( LinkButtonClick != null )
                {
                    LinkButtonClick( sender, e );
                }
            }

            /// <summary>
            /// Occurs when [link button click].
            /// </summary>
            internal event EventHandler LinkButtonClick;
        }

        /// <summary>
        /// Gets the property selection.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public static PropertySelection GetPropertySelection( GridViewRow row, int personId )
        {
            Panel selectionControlContainer = row.FindControl( $"selectionControlContainer_{personId}" ) as Panel;

            RockCheckBox selectionControlCheckbox = selectionControlContainer.FindControl( $"selectionControlCheckbox_{personId}" ) as RockCheckBox;
            RockRadioButton selectionControlRadioButton = selectionControlContainer.FindControl( $"selectionControlRadioButton_{personId}" ) as RockRadioButton;

            var result = new PropertySelection
            {
                PropertyKey = ( row.NamingContainer as Grid ).DataKeys[row.RowIndex].Value as string
            };

            if ( selectionControlCheckbox.Visible )
            {
                result.Selected = selectionControlCheckbox.Checked;
            }
            else if ( selectionControlRadioButton.Visible )
            {
                result.Selected = selectionControlRadioButton.Checked;
            }
            else
            {
                result.IsSectionHeader = true;
                result.Selected = false;
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        public class PropertySelection
        {
            /// <summary>
            /// Gets or sets the property key.
            /// </summary>
            /// <value>
            /// The property key.
            /// </value>
            public string PropertyKey { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is section header.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is section header; otherwise, <c>false</c>.
            /// </value>
            public bool IsSectionHeader { get; set; } = false;

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="PropertySelection"/> is selected.
            /// </summary>
            /// <value>
            ///   <c>true</c> if selected; otherwise, <c>false</c>.
            /// </value>
            public bool Selected { get; set; }
        }

        #endregion MergeField Classes
    }
}
