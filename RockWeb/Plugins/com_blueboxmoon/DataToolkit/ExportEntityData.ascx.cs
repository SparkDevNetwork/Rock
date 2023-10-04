using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.DataToolkit
{
    [DisplayName( "Export Entity Data" )]
    [Category( "Blue Box Moon > Data Toolkit" )]
    [Description( "Exports entity data to CSV or JSON format." )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this export block.", false, order: 0 )]
    public partial class ExportEntityData : RockBlock
    {
        #region Properties

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        /// <summary>
        /// The export column state information. Contains all the column
        /// descriptions of what is to be exported.
        /// </summary>
        protected List<ExportColumn> ExportColumnState { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Load the view state for the control.
        /// </summary>
        /// <param name="savedState">The previously saved view state.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ExportColumnState = ( List<ExportColumn> ) ViewState["ExportColumnState"];
        }

        /// <summary>
        /// Early initialization of the control.
        /// </summary>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );

            gColumns.Actions.ShowExcelExport = false;
            gColumns.Actions.ShowMergeTemplate = false;
            gColumns.Actions.ShowAdd = true;
            gColumns.Actions.AddClick += gColumns_AddClick;

            gPreview.Actions.ShowExcelExport = false;
            gPreview.Actions.ShowMergeTemplate = false;

            RegisterClientScript();
        }

        /// <summary>
        /// Late initialization of the control.
        /// </summary>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                etEntityType.EntityTypes = new EntityTypeService( new RockContext() ).GetEntities().ToList();
                BindPresetList();
            }
            else
            {
                UpdateExportColumnState();
                pnlPreview.Visible = false;
            }
        }

        /// <summary>
        /// Save the view state for the control.
        /// </summary>
        /// <returns>The view state object to be saved.</returns>
        protected override object SaveViewState()
        {
            ViewState["ExportColumnState"] = ExportColumnState;

            return base.SaveViewState();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Register client side scripts that are needed by this control.
        /// </summary>
        protected void RegisterClientScript()
        {
            string script = string.Format(
                @"
if ($('#{0}').text() != '') {{
    setTimeout(function () {{
        document.getElementById('{0}').scrollIntoView();
    }}, 1);
}}
",
                nbValidation.ClientID );

            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "ExportEntityData", script, true );
        }

        /// <summary>
        /// Bind the columns grid to the current export column state.
        /// </summary>
        protected void BindColumnsGrid()
        {
            gColumns.DataSource = ExportColumnState.OrderBy( c => c.Order );
            gColumns.DataBind();
        }

        /// <summary>
        /// Update the ExportColumnState with the current selections on the Grid.
        /// </summary>
        protected void UpdateExportColumnState()
        {
            var newState = new List<ExportColumn>();
            int i = 0;

            foreach ( GridViewRow row in gColumns.Rows.OfType<GridViewRow>() )
            {
                var column = new ExportColumn
                {
                    Order = i++
                };

                column.Name = ( ( TextBox ) row.FindControl( "tbColumnName" ) ).Text;
                column.LavaTemplate = ( ( CodeEditor ) row.FindControl( "ceLavaTemplate" ) ).Text;

                newState.Add( column );
            }

            ExportColumnState = newState;
        }

        /// <summary>
        /// Converts the string into one that can be used in a CSV field.
        /// </summary>
        /// <param name="value">The string value to be sanitized.</param>
        /// <returns>The sanitized string value.</returns>
        protected string ToCsvValue( string value )
        {
            if ( value.Contains( "," ) || value.Contains( "\"" ) || value.Contains( "\r" ) || value.Contains( "\n" ) )
            {
                return string.Format( "\"{0}\"", value.Replace( "\"", "\"\"" ) ).Left( 32000 );
            }
            else
            {
                return value.Left( 32000 );
            }
        }

        /// <summary>
        /// Get the data to be exported, optionally limited to a number of rows.
        /// </summary>
        /// <param name="progressCallback">Allows a callback method to be notified of the progress of the export.</param>
        /// <returns>A list of rows, each row consists of an array of objects.</returns>
        private List<object[]> GetExportData( IQueryable<IEntity> queryable, Action<int> progressCallback = null )
        {
            var entityData = new List<object[]>();
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage );
            var enabledLavaCommands = GetAttributeValue( "EnabledLavaCommands" );
            int rowCount = 0;

            foreach ( object row in queryable )
            {
                var data = new object[ExportColumnState.Count];

                mergeFields.AddOrReplace( "Row", row );

                for ( int i = 0; i < ExportColumnState.Count; i++ )
                {
                    var column = ExportColumnState[i];

                    if ( string.IsNullOrWhiteSpace( column.LavaTemplate ) )
                    {
                        data[i] = string.Empty;
                    }
                    else if ( LavaHelper.IsLavaTemplate( column.LavaTemplate ) )
                    {
                        var match = Regex.Match( column.LavaTemplate, @"{{\s*Row\.(\w+)\s*}}$" );
                        if ( match != null && match.Success )
                        {
                            data[i] = row.GetPropertyValue( match.Groups[1].Value );
                        }
                        else
                        {
                            data[i] = column.LavaTemplate.ResolveMergeFields( mergeFields, enabledLavaCommands ).Trim();
                        }
                    }
                    else
                    {
                        data[i] = column.LavaTemplate;
                    }
                }

                entityData.Add( data );
                rowCount++;

                if ( progressCallback != null )
                {
                    progressCallback( rowCount );
                }
            }

            return entityData;
        }

        /// <summary>
        /// Gets the queryable data for the user's selections.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A queryable object for database entities.</returns>
        private IQueryable<IEntity> GetQueryableData()
        {
            var rockContext = new RockContext();
            var entityType = Rock.Web.Cache.EntityTypeCache.Get( etEntityType.SelectedEntityTypeId.Value );
            var type = Type.GetType( entityType.AssemblyName );
            var service = Rock.Reflection.GetServiceForEntityType( type, rockContext );
            IQueryable<IEntity> query = null;

            if ( dvFilter.SelectedValueAsId().HasValue )
            {
                List<string> errorMessages;
                var dataView = new DataViewService( rockContext ).Get( dvFilter.SelectedValueAsId().Value );
                query = dataView.GetQuery( null, rockContext, null, out errorMessages );
            }
            else
            {
                var queryableMethod = service.GetType().GetMethod( "Queryable", new Type[] { } );
                if ( queryableMethod != null )
                {
                    query = ( IQueryable<IEntity> ) queryableMethod.Invoke( service, new object[] { } );
                }
            }

            return query;
        }

        /// <summary>
        /// Saves the exported data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>A BinaryFile that now contains the data.</returns>
        private BinaryFile SaveExportedData( List<object[]> data )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityType = Rock.Web.Cache.EntityTypeCache.Get( etEntityType.SelectedEntityTypeId.Value );
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFileTypeService = new BinaryFileTypeService( rockContext );
                var binaryFile = new BinaryFile
                {
                    BinaryFileTypeId = binaryFileTypeService.Get( Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid() ).Id,
                    IsTemporary = true
                };

                string content;

                if ( tglExportCsv.Checked )
                {
                    var sb = new StringBuilder();
                    var row = new string[ExportColumnState.Count];

                    //
                    // Build the CSV headers.
                    //
                    sb.Append( string.Join( ",", ExportColumnState.Select( c => ToCsvValue( c.Name ) ) ) + "\r\n" );

                    //
                    // Build all the rows.
                    //
                    foreach ( var r in data )
                    {
                        sb.Append( string.Join( ",", r.Select( f => ToCsvValue( f.ToStringSafe() ) ) ) + "\r\n" );
                    }

                    content = sb.ToString();
                    binaryFile.FileName = string.Format( "{0}Export.csv", entityType.FriendlyName.Replace( " ", "" ) );
                    binaryFile.MimeType = "text/csv";
                }
                else
                {
                    var dataList = new List<object>();
                    foreach ( var r in data )
                    {
                        var row = new Dictionary<string, object>();

                        for ( int i = 0; i < ExportColumnState.Count; i++ )
                        {
                            row.Add( ExportColumnState[i].Name, r[i] );
                        }

                        dataList.Add( row );
                    }

                    content = JsonConvert.SerializeObject( dataList, Formatting.Indented );
                    binaryFile.FileName = string.Format( "{0}Export.json", entityType.FriendlyName.Replace( " ", "" ) );
                    binaryFile.MimeType = "application/json";
                }

                //
                // Wrap the content in a MemoryStream.
                //
                var ms = new MemoryStream();
                var writer = new StreamWriter( ms );
                writer.Write( content );
                writer.Flush();
                ms.Position = 0;

                binaryFile.ContentStream = ms;
                binaryFileService.Add( binaryFile );
                rockContext.SaveChanges();

                return binaryFile;
            }
        }

        /// <summary>
        /// Binds the preset list.
        /// </summary>
        private void BindPresetList()
        {
            ddlPreset.Items.Clear();
            ddlPreset.Items.Add( "" );

            string presetJson = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.ExportEntityData.Presets" );
            try
            {
                var presets = JsonConvert.DeserializeObject<List<Preset>>( presetJson ) ?? new List<Preset>();
                foreach ( var preset in presets.OrderBy( p => p.Name ) )
                {
                    ddlPreset.Items.Add( new ListItem( preset.Name, preset.Id ) );
                }
            }
            catch
            {
                /* Intentionally left blank */
            }

            ddlPreset.SelectedValue = "";
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event for the control.
        /// Update all information for the user selecting a new Entity Type to export.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected void etEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? entityTypeId = etEntityType.SelectedEntityTypeId;
            pnlExportOptions.Visible = entityTypeId.HasValue;
            pnlActions.Visible = entityTypeId.HasValue;

            dvFilter.EntityTypeId = entityTypeId;

            if ( entityTypeId.HasValue )
            {
                var entityType = Rock.Web.Cache.EntityTypeCache.Get( entityTypeId.Value );
                if ( entityType != null )
                {
                    var type = Type.GetType( entityType.AssemblyName );

                    ExportColumnState = type.GetProperties()
                        .Where( p =>
                            !p.GetGetMethod().IsVirtual ||
                            p.GetCustomAttributes( typeof( IncludeForReportingAttribute ), true ).Any() ||
                            p.Name == "Order" || p.Name == "IsActive" ||
                            p.Name == "ForeignKey" || p.Name == "ForeignId" || p.Name == "ForeignGuid" ||
                            p.Name == "CreatedByPersonAliasId" || p.Name == "ModifiedByPersonAliasId" ||
                            p.Name == "CreatedDateTime" || p.Name == "ModifiedDateTime" )
                        .OrderBy( f => f.Name != "Id" )
                        .ThenBy( f => f.Name )
                        .Select( ( f, i ) => new ExportColumn
                        {
                            Order = i,
                            Name = f.Name,
                            LavaTemplate = string.Format( @"{{{{ Row.{0} }}}}", f.Name )
                        } )
                        .ToList();
                    var o = type.GetProperty( "Order" );

                    BindColumnsGrid();
                }
            }
            else
            {
                gColumns.Visible = false;
            }
        }

        /// <summary>
        /// Handles the AddClick event for the control.
        /// Add a new column that the user can specify the Name and Lava Template for.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The EventArgs that describe the event.</param>
        private void gColumns_AddClick( object sender, EventArgs e )
        {
            var column = new ExportColumn
            {
                Name = string.Empty,
                LavaTemplate = string.Empty,
                Order = ExportColumnState.Max( c => c.Order ) + 1
            };

            ExportColumnState.Add( column );

            BindColumnsGrid();
        }

        /// <summary>
        /// Handles the DeleteClick event for the control.
        /// Delete a row from the export column list.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The RowEventArgs that describe the event.</param>
        protected void gColumns_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            ExportColumnState.RemoveAt( e.RowIndex );

            BindColumnsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event for the control.
        /// Do custom binding of data to the UI.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The GridViewRowEventArgs that describe the event.</param>
        protected void gColumns_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var column = ( ExportColumn ) e.Row.DataItem;
                var tbColumnName = ( TextBox ) e.Row.FindControl( "tbColumnName" );
                var ceLavaTemplate = ( TextBox ) e.Row.FindControl( "ceLavaTemplate" );

                tbColumnName.Text = column.Name;
                ceLavaTemplate.Text = column.LavaTemplate;
            }
        }

        /// <summary>
        /// Handles the GridReorder event for the control.
        /// Do server-side reordering of the rows in the column export list.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The GridReorderEventArgs that describe the event.</param>
        protected void gColumns_GridReorder( object sender, Rock.Web.UI.Controls.GridReorderEventArgs e )
        {
            var movedColumn = ExportColumnState.Where( a => a.Order == e.OldIndex ).FirstOrDefault();
            if ( movedColumn != null )
            {
                if ( e.NewIndex < e.OldIndex )
                {
                    // Moved up
                    foreach ( var otherColumn in ExportColumnState.Where( a => a.Order < e.OldIndex && a.Order >= e.NewIndex ) )
                    {
                        otherColumn.Order = otherColumn.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherColumn in ExportColumnState.Where( a => a.Order > e.OldIndex && a.Order <= e.NewIndex ) )
                    {
                        otherColumn.Order = otherColumn.Order - 1;
                    }
                }

                movedColumn.Order = e.NewIndex;
            }

            BindColumnsGrid();
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// Export the matched entity objects to a CSV file and present the user with
        /// a link to download that file.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected void lbExport_Click( object sender, EventArgs e )
        {
            //
            // Make sure everything is valid.
            //
            nbValidation.Text = string.Empty;
            foreach ( var column in ExportColumnState )
            {
                if ( string.IsNullOrWhiteSpace( column.Name ) )
                {
                    nbValidation.Text = "All columns must have a non-blank name.";
                    return;
                }
            }

            //
            // Define the task that will run to process the data.
            //
            var exportTask = new Task( () =>
            {
                var queryable = GetQueryableData();
                int recordCount = queryable.Count();

                //
                // Wait for the browser to finish loading.
                //
                System.Threading.Thread.Sleep( 1000 );
                HubContext.Clients.Client( hfConnectionId.Value ).exportProgress( "0", string.Format( "{0:n0}", recordCount ) );
                DateTime lastNotified = DateTime.Now;

                //
                // Process the data.
                //
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var data = GetExportData( queryable, ( rowCount ) =>
                {
                    var timeDiff = DateTime.Now - lastNotified;
                    if ( timeDiff.TotalSeconds >= 2.5 )
                    {
                        HubContext.Clients.Client( hfConnectionId.Value ).exportProgress( string.Format( "{0:n0}", rowCount ), string.Format( "{0:n0}", recordCount ) );
                        lastNotified = DateTime.Now;
                    }
                } );

                //
                // Encode the data for export.
                //
                var binaryFile = SaveExportedData( data );
                stopwatch.Stop();

                //
                // Show the final status.
                //
                string status = string.Format( "Exported {0:n0} records in {1:n0} seconds.",
                    recordCount,
                    Math.Round( stopwatch.Elapsed.TotalSeconds ) );
                string url = string.Format( "/GetFile.ashx?Id={0}&attachment=True", binaryFile.Id );
                HubContext.Clients.Client( hfConnectionId.Value ).exportStatus( status, url );
            } );

            //
            // Define an error handler for the task.
            //
            exportTask.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    string status = "ERROR: " + t.Exception.InnerException.Message;
                    HubContext.Clients.Client( hfConnectionId.Value ).exportStatus( status, null );
                }
            } );

            exportTask.Start();
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// Preview the first 10 entity objects.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected void lbPreview_Click( object sender, EventArgs e )
        {
            nbValidation.Text = string.Empty;
            foreach ( var column in ExportColumnState )
            {
                if ( string.IsNullOrWhiteSpace( column.Name ) )
                {
                    nbValidation.Text = "All columns must have a non-blank name.";
                    return;
                }
            }

            var queryable = GetQueryableData();
            var data = GetExportData( queryable.Take( 10 ) );

            if ( tglExportCsv.Checked )
            {
                //
                // Setup all the columns for the exported data.
                //
                gPreview.Columns.Clear();
                foreach ( var column in ExportColumnState )
                {
                    var boundField = new Rock.Web.UI.Controls.RockBoundField
                    {
                        DataField = column.Name,
                        HeaderText = column.Name
                    };

                    gPreview.Columns.Add( boundField );
                }

                var dataList = new List<object>();
                foreach ( var r in data )
                {
                    var row = new Dictionary<string, object>();

                    for ( int i = 0; i < ExportColumnState.Count; i++ )
                    {
                        row.Add( ExportColumnState[i].Name, r[i] );
                    }

                    dataList.Add( JsonConvert.DeserializeObject( JsonConvert.SerializeObject( row ) ) );
                }

                gPreview.DataSource = dataList;
                gPreview.DataBind();
                gPreview.Visible = true;
                ltPreviewJson.Text = string.Empty;
            }
            else
            {
                var dataList = new List<object>();
                foreach ( var r in data )
                {
                    var row = new Dictionary<string, object>();

                    for ( int i = 0; i < ExportColumnState.Count; i++ )
                    {
                        row.Add( ExportColumnState[i].Name, r[i] );
                    }

                    dataList.Add( row );
                }

                ltPreviewJson.Text = JsonConvert.SerializeObject( dataList, Formatting.Indented ).EncodeHtml();
                gPreview.Visible = false;
            }

            pnlPreview.Visible = true;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the control.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected void tglExportCsv_CheckedChanged( object sender, EventArgs e )
        {
            /* Dummy function to clear any panels. */
        }

        /// <summary>
        /// Handles the Click event of the lbSaveAsPreset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void lbSaveAsPreset_Click( object sender, EventArgs e )
        {
            nbValidation.Text = string.Empty;
            foreach ( var column in ExportColumnState )
            {
                if ( string.IsNullOrWhiteSpace( column.Name ) )
                {
                    nbValidation.Text = "All columns must have a non-blank name.";
                    return;
                }
            }

            tbSaveAsPresetName.Text = string.Empty;
            mdSaveAsPreset.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSaveAsPreset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSaveAsPreset_SaveClick( object sender, EventArgs e )
        {
            var preset = new Preset
            {
                Id = Guid.NewGuid().ToString(),
                Name = tbSaveAsPresetName.Text,
                EntityTypeId = etEntityType.SelectedEntityTypeId.Value,
                DataViewFilterId = dvFilter.SelectedValueAsId(),
                ExportAsCsv = tglExportCsv.Checked,
                Columns = ExportColumnState
            };

            string presetJson = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.ExportEntityData.Presets" );
            List<Preset> presets;
            try
            {
                presets = JsonConvert.DeserializeObject<List<Preset>>( presetJson ) ?? new List<Preset>();
            }
            catch
            {
                presets = new List<Preset>();
            }

            presets.Add( preset );

            presetJson = JsonConvert.SerializeObject( presets );
            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.ExportEntityData.Presets", presetJson );

            BindPresetList();
            mdSaveAsPreset.Hide();
        }

        /// <summary>
        /// Handles the Click event of the lbLoadPreset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLoadPreset_Click( object sender, EventArgs e )
        {
            string presetJson = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.ExportEntityData.Presets" );
            List<Preset> presets;
            try
            {
                presets = JsonConvert.DeserializeObject<List<Preset>>( presetJson ) ?? new List<Preset>();
            }
            catch
            {
                presets = new List<Preset>();
            }

            var preset = presets.Where( p => p.Id == ddlPreset.SelectedValue ).FirstOrDefault();

            if ( preset != null )
            {
                etEntityType.SelectedEntityTypeId = preset.EntityTypeId;
                dvFilter.EntityTypeId = preset.EntityTypeId;
                dvFilter.SetValue( preset.DataViewFilterId );
                tglExportCsv.Checked = preset.ExportAsCsv;
                ExportColumnState = preset.Columns;

                pnlExportOptions.Visible = true;
                pnlActions.Visible = true;

                BindColumnsGrid();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDeletePreset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeletePreset_Click( object sender, EventArgs e )
        {
            string presetJson = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.ExportEntityData.Presets" );
            List<Preset> presets;
            try
            {
                presets = JsonConvert.DeserializeObject<List<Preset>>( presetJson ) ?? new List<Preset>();
            }
            catch
            {
                presets = new List<Preset>();
            }

            presets = presets.Where( p => p.Id != ddlPreset.SelectedValue ).ToList();

            presetJson = JsonConvert.SerializeObject( presets );
            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.ExportEntityData.Presets", presetJson );

            BindPresetList();
        }

        #endregion

        #region Support Classes

        [Serializable]
        public class ExportColumn
        {
            public string Name { get; set; }

            public string LavaTemplate { get; set; }

            public int Order { get; set; }
        }

        public class Preset
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public int EntityTypeId { get; set; }

            public int? DataViewFilterId { get; set; }

            public bool ExportAsCsv { get; set; }

            public List<ExportColumn> Columns { get; set; }
        }

        #endregion
    }
}
