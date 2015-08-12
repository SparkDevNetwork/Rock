using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Web.UI.Controls;
using com.centralaz.DpsMatch.Data;
using com.centralaz.DpsMatch.Model;


namespace com.centralaz.DpsMatch.Web.UI.Controls.Grid
{
    public class MatchField : TemplateField, INotRowSelectedField
    {

        #region Properties

        /// <summary>
        /// Gets or sets the match identifier.
        /// </summary>
        /// <value>
        /// The match identifier.
        /// </value>
        public int MatchId
        {
            get { return ViewState["MatchId"] as int? ?? 0; }
            set { ViewState["MatchId"] = value; }
        }

        /// <summary>
        /// Gets or sets the match likelyhood percentage
        /// </summary>
        public int? MatchPercentage
        {
            get
            {
                var matchPercentage = ViewState["MatchPercentage"] as int?;
                if ( matchPercentage == null )
                {
                    matchPercentage = null;
                    MatchPercentage = matchPercentage;
                }
                return matchPercentage;
            }

            set
            {
                ViewState["MatchPercentage"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the match positive identifier
        /// </summary>
        public bool? MatchIsMatch
        {
            get
            {
                var matchIsMatch = ViewState["MatchIsMatch"] as bool?;
                if ( matchIsMatch == null )
                {
                    matchIsMatch = null;
                    MatchIsMatch = matchIsMatch;
                }
                return matchIsMatch;
            }

            set
            {
                ViewState["MatchIsMatch"] = value;
            }
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
            this.ItemTemplate = new MatchFieldTemplate();
            MatchFieldHeaderTemplate headerTemplate = new MatchFieldHeaderTemplate();
            this.HeaderTemplate = headerTemplate;
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
    public class MatchFieldHeaderTemplate : ITemplate
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
                var matchField = cell.ContainingField as MatchField;
                if ( matchField != null )
                {
                    HtmlGenericContainer headerSummary = new HtmlGenericContainer( "div", "merge-header-summary" );
                    headerSummary.Attributes.Add( "data-col", matchField.ColumnIndex.ToString() );
                    double percentage = matchField.MatchPercentage.Value / 100.0;
                    headerSummary.Controls.Add( new LiteralControl( String.Format( "<div class='col-md-6'><div class='merge-heading-family'><h3>{0:0%}</h3></div></div><div class='col-md-6'>", percentage ) ) );

                    var rbList = new RadioButtonList();
                    rbList.Items.Add( new ListItem( "Is Match" ) );
                    rbList.Items.Add( new ListItem( "Is Not Match" ) );
                    rbList.Items.Add( new ListItem( "Unknown" ) );

                    if ( matchField.MatchIsMatch == true )
                    {
                        rbList.SelectedIndex = 0;
                    }
                    if ( matchField.MatchIsMatch == false )
                    {
                        rbList.SelectedIndex = 1;
                    }
                    if ( matchField.MatchIsMatch == null )
                    {
                        rbList.SelectedIndex = 2;
                    }
                    rbList.CausesValidation = false;
                    rbList.AutoPostBack = true;
                    headerSummary.Controls.Add( rbList );
                    headerSummary.Controls.Add( new LiteralControl( "</div>" ) );

                    cell.Controls.Add( headerSummary );

                    rbList.SelectedIndexChanged += rbList_SelectedIndexChanged;

                    cell.Controls.Add( headerSummary );
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rbList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void rbList_SelectedIndexChanged( object sender, EventArgs e )
        {
            RadioButtonList rbList = sender as RadioButtonList;
            var cell = rbList.Parent.Parent as DataControlFieldCell;
            var matchField = cell.ContainingField as MatchField;
            DpsMatchContext dpsMatchContext = new DpsMatchContext();
            Match match = new MatchService( dpsMatchContext ).Queryable().Where( m => m.Id == matchField.MatchId ).FirstOrDefault();
            if ( rbList.SelectedIndex == 0 )
            {
                match.IsMatch=true;
            }
            if ( rbList.SelectedIndex == 1 )
            {
                match.IsMatch = false;
            }
            if ( rbList.SelectedIndex == 2 )
            {
                match.IsMatch = null;
            }
            dpsMatchContext.SaveChanges();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MatchFieldTemplate : ITemplate
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
                var matchField = cell.ContainingField as MatchField;
                if ( matchField != null )
                {
                    DataTextField = matchField.DataTextField;
                    ColumnIndex = matchField.ColumnIndex;

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
