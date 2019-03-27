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
    /// The ASP:CheckBoxField doesn't work very well for retrieving changed values, especially when the value is changed from True to False (weird)
    /// This CheckBoxEditableField works like the ASP:CheckBoxField except it gives the CheckBox's IDs so their changed values will consistently persist on postbacks
    /// </summary>
    public class PersonMergeField : SelectField, INotRowSelectedField
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
                if (headerContent == null)
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

            PersonMergeFieldHeaderTemplate headerTemplate = new PersonMergeFieldHeaderTemplate();
            headerTemplate.LinkButtonClick += HeaderTemplate_LinkButtonClick;
            this.HeaderTemplate = headerTemplate;
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
        void HeaderTemplate_LinkButtonClick( object sender, EventArgs e )
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

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class PersonMergeFieldHeaderTemplate : ITemplate
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
                var mergeField = cell.ContainingField as PersonMergeField;
                if ( mergeField != null )
                {
                    var lbDelete = new LinkButton();
                    lbDelete.CausesValidation = false;
                    lbDelete.CssClass = "btn btn-danger btn-xs pull-right";
                    lbDelete.ToolTip = "Remove Person";
                    cell.Controls.Add( lbDelete );

                    HtmlGenericControl buttonIcon = new HtmlGenericControl( "i" );
                    buttonIcon.Attributes.Add( "class", "fa fa-times" );
                    lbDelete.Controls.Add( buttonIcon );

                    lbDelete.Click += lbDelete_Click;

                    // make sure delete button is registered for async postback (needed just in case the grid was created at runtime)
                    var sm = ScriptManager.GetCurrent( mergeField.ParentGrid.Page );
                    sm.RegisterAsyncPostBackControl( lbDelete );

                    HtmlGenericContainer headerSummary = new HtmlGenericContainer( "div", "merge-header-summary" );
                    headerSummary.Attributes.Add( "data-col", mergeField.ColumnIndex.ToString() );

                    var i = new HtmlGenericControl( "i" );
                    i.Attributes.Add( "class", "fa fa-2x " + ( mergeField.IsPrimaryPerson ? "fa-check-square-o" : "fa-square-o" ) );
                    headerSummary.Controls.Add( i );
                   
                    headerSummary.Controls.Add(new LiteralControl(mergeField.HeaderContent));

                    string created = (mergeField.ModifiedDateTime.HasValue ? mergeField.ModifiedDateTime.ToElapsedString() + " " : "") +
                        (!string.IsNullOrWhiteSpace(mergeField.ModifiedBy) ? "by " + mergeField.ModifiedBy : "");
                    if ( created != string.Empty )
                    {
                        headerSummary.Controls.Add( new LiteralControl( string.Format( "<small>Last Modified {0}</small>", created ) ) );
                    }

                    cell.Controls.Add(headerSummary);
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
}
