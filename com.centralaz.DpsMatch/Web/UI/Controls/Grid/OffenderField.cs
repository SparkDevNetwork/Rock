using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Web.UI.Controls;


namespace com.centralaz.DpsMatch.Web.UI.Controls.Grid
{
    public class OffenderField : TemplateField, INotRowSelectedField
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
        /// Gets or sets the current index.
        /// </summary>
        /// <value>
        /// The current index.
        /// </value>
        public int CurrentIndex
        {
            get { return ViewState["CurrentIndex"] as int? ?? 0; }
            set { ViewState["CurrentIndex"] = value; }
        }

        /// <summary>
        /// Gets or sets the list size.
        /// </summary>
        /// <value>
        /// The list size.
        /// </value>
        public int ListCount
        {
            get { return ViewState["ListCount"] as int? ?? 0; }
            set { ViewState["ListCount"] = value; }
        }

        /// <summary>
        /// Gets or sets the data field.
        /// </summary>
        /// <value>
        /// The data field.
        /// </value>
        public string DataTextField
        {
            get
            {
                return ViewState["DataTextField"] as string;
            }

            set
            {
                ViewState["DataTextField"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the index of the column.
        /// </summary>
        /// <value>
        /// The index of the column.
        /// </value>
        public int ColumnIndex
        {
            get
            {
                return ViewState["ColumnIndex"] as int? ?? 0;
            }
            set
            {
                ViewState["ColumnIndex"] = value;
            }
        }

        /// <summary>
        /// Gets the parent grid.
        /// </summary>
        /// <value>
        /// The parent grid.
        /// </value>
        public Rock.Web.UI.Controls.Grid ParentGrid { get; internal set; }

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
            OffenderFieldHeaderTemplate headerTemplate = new OffenderFieldHeaderTemplate();
            this.HeaderTemplate = headerTemplate;
            this.ItemTemplate = new OffenderFieldTemplate();
            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
            this.ParentGrid = control as Rock.Web.UI.Controls.Grid;

            return false;
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class OffenderFieldHeaderTemplate : ITemplate
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
                var offenderField = cell.ContainingField as OffenderField;
                if ( offenderField != null )
                {
                    HtmlGenericContainer headerSummary = new HtmlGenericContainer( "div", "merge-header-summary" );
                    headerSummary.Attributes.Add( "data-col", offenderField.ColumnIndex.ToString() );
                    headerSummary.Controls.Add( new LiteralControl( String.Format("<div class='merge-heading-family'><h3>Offender</h3>({0} out of {1})</div>", offenderField.CurrentIndex, offenderField.ListCount) ) );

                    cell.Controls.Add( headerSummary );
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OffenderFieldTemplate : ITemplate
    {

        #region Properties

        /// <summary>
        /// Gets the data text field.
        /// </summary>
        /// <value>
        /// The data text field.
        /// </value>
        public string DataTextField { get; private set; }

        /// <summary>
        /// Gets the index of the column.
        /// </summary>
        /// <value>
        /// The index of the column.
        /// </value>
        public int ColumnIndex { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            var cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                var offenderField = cell.ContainingField as OffenderField;
                if ( offenderField != null )
                {
                    DataTextField = offenderField.DataTextField;
                    ColumnIndex = offenderField.ColumnIndex;

                    Literal lText = new Literal();
                    lText.ID = "lText_" + ColumnIndex.ToString();
                    lText.DataBinding += lText_DataBinding;
                    lText.Visible = true;
                    cell.Controls.Add( lText );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the DataBinding event of the cb control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lText_DataBinding( object sender, EventArgs e )
        {
            var lText = sender as Literal;
            if ( lText != null )
            {
                GridViewRow gridViewRow = lText.NamingContainer as GridViewRow;

                if ( gridViewRow.DataItem != null )
                {
                    if ( !string.IsNullOrWhiteSpace( DataTextField ) )
                    {
                        object dataValue = DataBinder.Eval( gridViewRow.DataItem, DataTextField );
                        lText.Text = dataValue.ToString();
                    }
                    else
                    {
                        lText.Text = String.Empty;
                    }
                    lText.Visible = true;
                }
            }
        }

        #endregion

    }

}
