// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.
    /// </summary>
    [DisplayName( "Dynamic Data" )]
    [Category( "Reporting" )]
    [Description( "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure." )]

    [BooleanField( "Update Page", "If True, provides fields for updating the parent page's Name and Description", true, "", 0 )]
    [CodeEditorField( "Query", "The query to execute", CodeEditorMode.Sql, CodeEditorTheme.Rock, 400, false, "", "", 1 )]
    [TextField( "Query Params", "Parameters to pass to query", false, "", "", 2 )]
    [TextField( "Url Mask", "The Url to redirect to when a row is clicked", false, "", "", 3 )]
    [BooleanField( "Show Columns", "Should the 'Columns' specified below be the only ones shown (vs. the only ones hidden)", false, "", 4 )]
    [TextField( "Columns", "The columns to hide or show", false, "", "", 5 )]
    [CodeEditorField( "Formatted Output", "Optional formatting to apply to the returned results.  If left blank, a grid will be displayed. Example: {% for row in rows %} {{ row.FirstName }}<br/> {% endfor %}",
        CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, false, "", "", 7 )]
    [BooleanField( "Person Report", "Is this report a list of people.?", false, "", 8 )]
    [TextField( "Merge Fields", "Any fields to make available as merge fields for any new communications", false, "", "", 9 )]
    public partial class DynamicData : RockBlock
    {
        #region Fields

        bool updatePage = true;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += DynamicData_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gReport.GridRebind += gReport_GridRebind;
            gReport.RowSelected += gReport_RowSelected;

            updatePage = GetAttributeValue( "UpdatePage" ).AsBoolean( true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowView();
            }
        }

        public override List<Control> GetAdministrateControls( bool canConfig, bool canEdit )
        {
            List<Control> configControls = new List<Control>();

            if ( IsUserAuthorized( Authorization.EDIT ) )
            {
                LinkButton lbEdit = new LinkButton();
                lbEdit.CssClass = "edit";
                lbEdit.ToolTip = "Edit Criteria";
                lbEdit.Click += lbEdit_Click;
                configControls.Add( lbEdit );
                HtmlGenericControl iEdit = new HtmlGenericControl( "i" );
                lbEdit.Controls.Add( iEdit );
                lbEdit.CausesValidation = false;
                iEdit.Attributes.Add( "class", "fa fa-pencil-square-o" );

                // will toggle the block config so they are no longer showing
                lbEdit.Attributes["onclick"] = "Rock.admin.pageAdmin.showBlockConfig()";

                ScriptManager.GetCurrent( this.Page ).RegisterAsyncPostBackControl( lbEdit );
            }

            configControls.AddRange( base.GetAdministrateControls( canConfig, canEdit ) );

            return configControls;
        }

        #endregion

        #region Events

        protected void lbEdit_Click( object sender, EventArgs e )
        {
            pnlEditModel.Visible = true;
            upnlContent.Update();
            mdEdit.Show();

            if ( updatePage )
            {
                var pageCache = PageCache.Read( RockPage.PageId );
                tbName.Text = pageCache != null ? pageCache.PageTitle : string.Empty;
                tbDesc.Text = pageCache != null ? pageCache.Description : string.Empty;
            }

            tbName.Visible = updatePage;
            tbDesc.Visible = updatePage;

            ceQuery.Text = GetAttributeValue( "Query" );
            tbParams.Text = GetAttributeValue( "QueryParams" );
            tbUrlMask.Text = GetAttributeValue( "UrlMask" );
            ddlHideShow.SelectedValue = GetAttributeValue( "ShowColumns" );
            tbColumns.Text = GetAttributeValue( "Columns" );
            ceFormattedOutput.Text = GetAttributeValue( "FormattedOutput" );
            cbPersonReport.Checked = GetAttributeValue( "PersonReport" ).AsBoolean();
            tbMergeFields.Text = GetAttributeValue( "MergeFields" );
        }

        void DynamicData_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( updatePage )
            {
                var pageCache = PageCache.Read( RockPage.PageId );
                if ( pageCache != null &&
                    ( pageCache.PageTitle != tbName.Text || pageCache.Description != tbDesc.Text ) )
                {
                    var rockContext = new RockContext();
                    var service = new PageService( rockContext );
                    var page = service.Get( pageCache.Id );
                    page.InternalName = tbName.Text;
                    page.PageTitle = tbName.Text;
                    page.BrowserTitle = tbName.Text;
                    page.Description = tbDesc.Text;
                    rockContext.SaveChanges();

                    Rock.Web.Cache.PageCache.Flush( page.Id );
                    pageCache = PageCache.Read( RockPage.PageId );

                    var breadCrumb = RockPage.BreadCrumbs.Where( c => c.Url == RockPage.PageReference.BuildUrl() ).FirstOrDefault();
                    if ( breadCrumb != null )
                    {
                        breadCrumb.Name = pageCache.BreadCrumbText;
                    }
                }
            }

            SetAttributeValue( "Query", ceQuery.Text );
            SetAttributeValue( "QueryParams", tbParams.Text );
            SetAttributeValue( "UrlMask", tbUrlMask.Text );
            SetAttributeValue( "Columns", tbColumns.Text );
            SetAttributeValue( "ShowColumns", ddlHideShow.SelectedValue );
            SetAttributeValue( "FormattedOutput", ceFormattedOutput.Text );
            SetAttributeValue( "PersonReport", cbPersonReport.Checked.ToString() );
            SetAttributeValue( "MergeFields", tbMergeFields.Text );
            SaveAttributeValues();

            ShowView();
        }

        /// <summary>
        /// Handles the RowSelected event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gReport_RowSelected( object sender, RowEventArgs e )
        {
            string url = GetAttributeValue( "UrlMask" );
            if ( !string.IsNullOrWhiteSpace( url ) )
            {
                foreach ( string key in gReport.DataKeyNames )
                {
                    url = url.Replace( "{" + key + "}", gReport.DataKeys[e.RowIndex][key].ToString() );
                }

                Response.Redirect( url, false );
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gReport_GridRebind( object sender, EventArgs e )
        {
            ShowView();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void ShowView()
        {
            mdEdit.Hide();
            pnlEditModel.Visible = false;
            upnlContent.Update();

            bool personReport = GetAttributeValue( "PersonReport" ).AsBoolean();

            if ( personReport )
            {
                gReport.PersonIdField = "Id";
            }
            else
            {
                gReport.PersonIdField = null;
            }

            gReport.CommunicateMergeFields = GetAttributeValue( "MergeFields" ).SplitDelimitedValues().ToList<string>();
            gReport.Visible = true;
            nbError.Visible = false;

            string query = GetAttributeValue( "Query" );
            if ( !string.IsNullOrWhiteSpace( query ) )
            {
                try
                {
                    query = query.ResolveMergeFields( PageParameters() );

                    var parameters = GetParameters();
                    DataTable dataTable = DbService.GetDataTable( query, parameters != null ? CommandType.StoredProcedure : CommandType.Text, parameters );

                    string formattedOutput = GetAttributeValue( "FormattedOutput" );
                    if ( string.IsNullOrWhiteSpace( formattedOutput ) )
                    {
                        gReport.Visible = true;
                        phHtml.Visible = false;

                        AddGridColumns( dataTable );
                        SetDataKeyNames();
                        gReport.DataSource = GetSortedView( dataTable );
                        gReport.DataBind();
                    }
                    else
                    {
                        gReport.Visible = false;
                        phHtml.Visible = true;

                        var dropRows = new List<DataRowDrop>();
                        foreach ( DataRow row in dataTable.Rows )
                        {
                            dropRows.Add( new DataRowDrop( row ) );
                        }

                        var dropTable = new Dictionary<string, object>();
                        dropTable.Add( "rows", dropRows );

                        phHtml.Controls.Add( new LiteralControl( formattedOutput.ResolveMergeFields( dropTable ) ) );
                    }
                }
                catch ( System.Exception ex )
                {
                    gReport.Visible = false;
                    phHtml.Visible = false;

                    nbError.Text = ex.Message;
                    nbError.Visible = true;
                }
            }
        }

        /// <summary>
        /// Sets the data key names.
        /// </summary>
        private void SetDataKeyNames()
        {
            string urlMask = GetAttributeValue( "UrlMask" );
            if ( !string.IsNullOrWhiteSpace( urlMask ) )
            {
                Regex pattern = new Regex( @"\{.+\}" );
                var matches = pattern.Matches( urlMask );
                if ( matches.Count > 0 )
                {
                    string[] keyNames = new string[matches.Count];
                    for ( int i = 0; i < matches.Count; i++ )
                    {
                        keyNames[i] = matches[i].Value.TrimStart( '{' ).TrimEnd( '}' );
                    }

                    gReport.DataKeyNames = keyNames;
                }
            }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetParameters()
        {
            string[] queryParams = GetAttributeValue( "QueryParams" ).SplitDelimitedValues();
            if ( queryParams.Length > 0 )
            {
                var parameters = new Dictionary<string, object>();
                foreach ( string queryParam in queryParams )
                {
                    string[] paramParts = queryParam.Split( '=' );
                    if ( paramParts.Length == 2 )
                    {
                        string queryParamName = paramParts[0];
                        string queryParamValue = paramParts[1];

                        // Remove leading '@' character if was included
                        if ( queryParamName.StartsWith( "@" ) )
                        {
                            queryParamName = queryParamName.Substring( 1 );
                        }

                        // If a page parameter (query or form) value matches, use it's value instead
                        string pageValue = PageParameter( queryParamName );
                        if ( !string.IsNullOrWhiteSpace( pageValue ) )
                        {
                            queryParamValue = pageValue;
                        }
                        else if ( queryParamName.ToLower() == "currentpersonid" && CurrentPerson != null )
                        {
                            // If current person id, use the value of the current person id
                            queryParamValue = CurrentPerson.Id.ToString();
                        }

                        parameters.Add( queryParamName, queryParamValue );
                    }
                }

                return parameters;
            }

            return null;
        }

        /// <summary>
        /// Adds the grid columns.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        private void AddGridColumns( DataTable dataTable )
        {
            bool showColumns = bool.Parse( GetAttributeValue( "ShowColumns" ) );
            var columnList = GetAttributeValue( "Columns" ).SplitDelimitedValues().ToList();

            int rowsToEval = 10;
            if ( dataTable.Rows.Count < 10 )
            {
                rowsToEval = dataTable.Rows.Count;
            }

            gReport.Columns.Clear();

            if ( !string.IsNullOrWhiteSpace( gReport.PersonIdField ) )
            {
                gReport.Columns.Add( new SelectField() );
            }

            foreach ( DataColumn dataTableColumn in dataTable.Columns )
            {
                if ( columnList.Count > 0 &&
                    ( ( showColumns && !columnList.Contains( dataTableColumn.ColumnName, StringComparer.OrdinalIgnoreCase ) ) ||
                        ( !showColumns && columnList.Contains( dataTableColumn.ColumnName, StringComparer.OrdinalIgnoreCase ) ) ) )
                {
                    continue;
                }

                BoundField bf = new BoundField();

                if ( dataTableColumn.DataType == typeof( bool ) )
                {
                    bf = new BoolField();
                }

                else if ( dataTableColumn.DataType == typeof( DateTime ) )
                {
                    bf = new DateField();

                    for ( int i = 0; i < rowsToEval; i++ )
                    {
                        object dateObj = dataTable.Rows[i][dataTableColumn];
                        if ( dateObj is DateTime )
                        {
                            DateTime dateTime = (DateTime)dateObj;
                            if ( dateTime.TimeOfDay.Seconds != 0 )
                            {
                                bf = new DateTimeField();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    bf.HtmlEncode = false;
                }

                bf.DataField = dataTableColumn.ColumnName;
                bf.SortExpression = dataTableColumn.ColumnName;
                bf.HeaderText = dataTableColumn.ColumnName.SplitCase();
                gReport.Columns.Add( bf );
            }
        }

        /// <summary>
        /// Gets the sorted view.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns></returns>
        private System.Data.DataView GetSortedView( DataTable dataTable )
        {
            System.Data.DataView dataView = dataTable.DefaultView;

            SortProperty sortProperty = gReport.SortProperty;
            if ( sortProperty != null )
            {
                dataView.Sort = string.Format( "{0} {1}", sortProperty.Property, sortProperty.DirectionString );
            }

            return dataView;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        private class DataRowDrop : DotLiquid.Drop
        {
            private readonly DataRow _dataRow;

            public DataRowDrop( DataRow dataRow )
            {
                _dataRow = dataRow;
            }

            public override object BeforeMethod( string method )
            {
                if ( _dataRow.Table.Columns.Contains( method ) )
                {
                    return _dataRow[method];
                }

                return null;
            }
        }
    }
}