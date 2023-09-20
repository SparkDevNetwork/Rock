using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.DataToolkit
{
    [DisplayName( "Import Entity Data" )]
    [Category( "Blue Box Moon > Data Toolkit" )]
    [Description( "Import entity data from CSV or JSON format." )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this import block.", false, order: 0 )]
    public partial class ImportEntityData : RockBlock
    {
        #region Properties

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        /// <summary>
        /// The import property state information. Contains all the property
        /// descriptions of what is to be imported.
        /// </summary>
        protected List<ImportProperty> ImportPropertyState { get; set; }

        /// <summary>
        /// Gets or sets the known fields from the uploaded data.
        /// </summary>
        protected List<string> KnownFields { get; set; }

        /// <summary>
        /// Gets or sets the sample data.
        /// </summary>
        protected List<Dictionary<string, string>> SampleData { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Load the view state for the control.
        /// </summary>
        /// <param name="savedState">The previously saved view state.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ImportPropertyState = ( List<ImportProperty> ) ViewState["ImportPropertyState"];
            KnownFields = ( List<string> ) ViewState["KnownFields"];
            SampleData = ( List<Dictionary<string, string>> ) ViewState["SampleData"];
        }

        /// <summary>
        /// Early initialization of the control.
        /// </summary>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );

            gProperties.Actions.ShowExcelExport = false;
            gProperties.Actions.ShowMergeTemplate = false;
            gProperties.Actions.ShowAdd = false;

            gSample.Actions.ShowExcelExport = false;
            gSample.Actions.ShowMergeTemplate = false;

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
                UpdateImportPropertyState();
                pnlPreview.Visible = false;
            }
        }

        /// <summary>
        /// Save the view state for the control.
        /// </summary>
        /// <returns>The view state object to be saved.</returns>
        protected override object SaveViewState()
        {
            ViewState["SampleData"] = SampleData;
            ViewState["KnownFields"] = KnownFields;
            ViewState["ImportPropertyState"] = ImportPropertyState;

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

            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "ImportEntityData", script, true );
        }

        /// <summary>
        /// Bind the properties grid to the current import property state.
        /// </summary>
        protected void BindPropertiesGrid()
        {
            gProperties.DataSource = ImportPropertyState;
            gProperties.DataBind();
        }

        /// <summary>
        /// Binds the sample data grid.
        /// </summary>
        protected void BindSampleGrid()
        {
            gSample.Columns.Clear();

            var dt = new DataTable();
            foreach ( var key in KnownFields )
            {
                var column = new Rock.Web.UI.Controls.RockBoundField
                {
                    HeaderText = key,
                    DataField = key
                };
                gSample.Columns.Add( column );

                dt.Columns.Add( key );
            }

            foreach ( var d in SampleData )
            {
                dt.Rows.Add( d.Values.ToArray() );
            }

            gSample.DataSource = dt;
            gSample.DataBind();
        }

        /// <summary>
        /// Update the ImportPropertyState with the current selections on the Grid.
        /// </summary>
        protected void UpdateImportPropertyState()
        {
            var newState = ImportPropertyState;
            int i = 0;

            foreach ( GridViewRow row in gProperties.Rows.OfType<GridViewRow>() )
            {
                var property = ImportPropertyState[i++];

                property.LavaTemplate = ( ( CodeEditor ) row.FindControl( "ceLavaTemplate" ) ).Text;
            }

            ImportPropertyState = newState;
        }

        /// <summary>
        /// Gets a queryable object for the data that has been uploaded.
        /// </summary>
        /// <returns>A queryable of dictionary objects.</returns>
        protected IQueryable<Dictionary<string, object>> GetQueryable()
        {
            var rockContext = new RockContext();
            var binaryFileService = new BinaryFileService( rockContext );
            var binaryFile = binaryFileService.Get( fupData.BinaryFileId.Value );

            try
            {
                //
                // First try to parse the data as a JSON import.
                //
                var content = binaryFile.ContentsToString();

                return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>( content ).AsQueryable();
            }
            catch
            {
                //
                // If that fails, try to parse as a CSV stream.
                //
                var parser = new CsvParser( binaryFile.ContentStream );
                var records = new List<Dictionary<string, object>>();

                var header = parser.ReadRecord();
                if ( header == null || header.Length == 0 )
                {
                    return null;
                }

                //
                // Generate a sample set of the first 10 rows.
                //
                while ( true )
                {
                    var row = parser.ReadRecord();
                    if ( row == null || row.Length != header.Length )
                    {
                        break;
                    }

                    var record = new Dictionary<string, object>();
                    for ( int f = 0; f < header.Length; f++ )
                    {
                        record.AddOrReplace( header[f], row[f].ToString() );
                    }

                    records.Add( record );
                }

                return records.AsQueryable();
            }
        }

        /// <summary>
        /// Imports the data in the queryable using the current configuration.
        /// </summary>
        /// <param name="progressCallback">Allows a callback method to be notified of the progress of the import.</param>
        private void ImportData( RockContext rockContext, IQueryable<Dictionary<string, object>> queryable, Action<int> progressCallback = null )
        {
            int rowCount = 0;
            var entityType = EntityTypeCache.Get( etEntityType.SelectedEntityTypeId.Value );
            var type = entityType.GetEntityType();
            IService service = Reflection.GetServiceForEntityType( entityType.GetEntityType(), rockContext );
            var addMethod = service.GetType().GetMethod( "Add", new Type[] { entityType.GetEntityType() } );
            IEntity entity = null;
            var enabledLavaCommands = GetAttributeValue( "EnabledLavaCommands" );

            if ( service == null )
            {
                throw new Exception( "Could not determine database service for entity." );
            }

            if ( addMethod == null )
            {
                throw new Exception( "Could not find Add method for entity." );
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage );

            foreach ( var row in queryable )
            {
                //
                // Create an empty IEntity object of the requested entity type.
                //
                entity = ( IEntity ) Activator.CreateInstance( type );
                addMethod.Invoke( service, new object[] { entity } );

                mergeFields.AddOrReplace( "Row", row );

                for ( int i = 0; i < ImportPropertyState.Count; i++ )
                {
                    var property = ImportPropertyState[i];
                    try
                    {
                        string value = GetPropertyValue( row, property, enabledLavaCommands, mergeFields );

                        SetProperty( entity, property.Name, value );
                    }
                    catch ( Exception ex )
                    {
                        var message = string.Format( "Error importing record #{0} property {1}\n{2}",
                            rowCount, property.Name, row.ToJson() );

                        throw new Exception( message, ex );
                    }
                }

                rowCount++;

                //
                // Verify that the entity is valid to be saved.
                //
                if ( !entity.IsValid )
                {
                    var errors = string.Join( "\n", entity.ValidationResults.Select( v => v.ErrorMessage ) );
                    var message = string.Format( "Error importing record #{0}\n{1}\n{2}",
                        rowCount, errors, row.ToJson() );

                    throw new Exception( message );
                }

                //
                // Save each entity one at a time so that we can provide useful
                // debugging information if an error occurs.
                //
                try
                {
                    rockContext.SaveChanges( true );
                }
                catch ( Exception e )
                {
                    throw new Exception( string.Format( "Error while importing record {0}:\n{1}", rowCount, row.ToJson() ), e );
                }

                //
                // Ensure we are no longer tracking the entity we just saved.
                //
                rockContext.ChangeTracker.Entries().ToList().ForEach( e => e.State = System.Data.Entity.EntityState.Detached );

                if ( progressCallback != null )
                {
                    progressCallback( rowCount );
                }
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the property value that will be used for import.
        /// </summary>
        /// <param name="row">The row data.</param>
        /// <param name="property">The property to be imported.</param>
        /// <param name="enabledLavaCommands">The enabled lava commands.</param>
        /// <param name="mergeFields">The merge fields made available during Lava merge.</param>
        /// <returns></returns>
        private static string GetPropertyValue( Dictionary<string, object> row, ImportProperty property, string enabledLavaCommands, Dictionary<string, object> mergeFields )
        {
            var value = property.LavaTemplate;

            if ( value.HasMergeFields() )
            {
                var match = Regex.Match( value, @"^{{\s*Row\.(\w+)\s*}}$" );

                if ( match != null && match.Success )
                {
                    value = row[match.Groups[1].Value].ToStringSafe();
                }
                else
                {
                    value = value.ResolveMergeFields( mergeFields, enabledLavaCommands ).Trim();
                }
            }

            return value;
        }

        /// <summary>
        /// Binds the preset list.
        /// </summary>
        private void BindPresetList()
        {
            ddlPreset.Items.Clear();
            ddlPreset.Items.Add( "" );

            string presetJson = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.ImportEntityData.Presets" );
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

        /// <summary>
        /// Shows the tab.
        /// </summary>
        /// <param name="tab">The tab.</param>
        private void ShowTab( string tab )
        {
            liSettings.RemoveCssClass( "active" );
            liSample.RemoveCssClass( "active" );

            pnlSettings.Visible = false;
            pnlSample.Visible = false;

            if ( tab == "Settings" )
            {
                liSettings.AddCssClass( "active" );
                pnlSettings.Visible = true;
            }
            else if ( tab == "Sample" )
            {
                liSample.AddCssClass( "active" );
                pnlSample.Visible = true;
            }
        }

        /// <summary>
        /// Set the specified property of an entity to a given value.
        /// </summary>
        /// <param name="entity">The entity whose property is to be set.</param>
        /// <param name="key">The name of the property to set.</param>
        /// <param name="value">The value to set the property to.</param>
        protected void SetProperty( IEntity entity, string key, string value )
        {
            var property = entity.GetType().GetProperty( key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );

            if ( property == null )
            {
                throw new Exception( string.Format( "Cannot find property '{0}'.", key ) );
            }

            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return;
            }

            if ( !property.CanWrite )
            {
                throw new Exception( string.Format( "Property '{0}' is read-only.", key ) );
            }

            var convertedValue = ConvertObject( value, property.PropertyType );

            if ( convertedValue != null )
            {
                property.SetValue( entity, convertedValue );
            }
        }

        /// <summary>
        /// Converts a string to the specified type of object.
        /// </summary>
        /// <param name="theObject">The string to convert.</param>
        /// <param name="objectType">The type of object desired.</param>
        /// <param name="tryToNull">If empty strings should return as null.</param>
        /// <returns></returns>
        protected object ConvertObject( string theObject, Type objectType, bool tryToNull = true )
        {
            if ( objectType.IsEnum )
            {
                return string.IsNullOrWhiteSpace( theObject ) ? null : Enum.Parse( objectType, theObject, true );
            }

            Type underType = Nullable.GetUnderlyingType( objectType );
            if ( underType == null ) // not nullable
            {
                if ( objectType == typeof( Guid ) )
                {
                    return theObject.AsGuidOrNull();
                }
                else if ( objectType == typeof( bool ) && theObject.AsIntegerOrNull() != null )
                {
                    return theObject.AsInteger() != 0;
                }
                else
                {
                    return Convert.ChangeType( theObject, objectType );
                }
            }

            if ( tryToNull && string.IsNullOrWhiteSpace( theObject ) )
            {
                return null;
            }

            return ConvertObject( theObject, underType, tryToNull );
        }

        /// <summary>
        /// Starts the import task.
        /// </summary>
        /// <param name="dryRun">if set to <c>true</c> [dry run].</param>
        protected void StartImportTask( bool dryRun )
        {
            //
            // Define the task that will run to process the data.
            //
            var importTask = new Task( () =>
            {
                var queryable = GetQueryable();
                int recordCount = queryable.Count();
                var rockContext = new RockContext();

                //
                // Wait for the browser to finish loading.
                //
                System.Threading.Thread.Sleep( 1000 );
                HubContext.Clients.Client( hfConnectionId.Value ).importProgress( "0", string.Format( "{0:n0}", recordCount ) );
                DateTime lastNotified = DateTime.Now;
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var transaction = rockContext.Database.BeginTransaction();
                try
                {
                    ImportData( rockContext, queryable, ( rowCount ) =>
                    {
                        var timeDiff = DateTime.Now - lastNotified;
                        if ( timeDiff.TotalSeconds >= 2.5 )
                        {
                            HubContext.Clients.Client( hfConnectionId.Value ).importProgress( string.Format( "{0:n0}", rowCount ), string.Format( "{0:n0}", recordCount ) );
                            lastNotified = DateTime.Now;
                        }
                    } );

                    if ( dryRun )
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                //
                // Show the final status.
                //
                stopwatch.Stop();
                string status = string.Format( "{0}Imported {1:n0} records in {2:n0} seconds.",
                    dryRun ? "Test " : string.Empty,
                    recordCount,
                    Math.Round( stopwatch.Elapsed.TotalSeconds ) );

                HubContext.Clients.Client( hfConnectionId.Value ).importStatus( status, true );
            } );

            //
            // Define an error handler for the task.
            //
            importTask.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    string status = string.Empty;
                    for ( Exception ex = t.Exception.InnerException; ex != null; ex = ex.InnerException )
                    {
                        status = status + ex.Message + "\n";
                    }

                    HubContext.Clients.Client( hfConnectionId.Value ).importStatus( status, false );

                    ExceptionLogService.LogException( t.Exception, null );
                }
            } );

            importTask.Start();
        }

        /// <summary>
        /// Gets known properties for a given type.
        /// </summary>
        /// <param name="type">The type.</param>
        protected List<ImportProperty> GetKnownPropertiesForType( Type type )
        {
            var state = type.GetProperties()
                .Where( p =>
                    !p.GetGetMethod().IsVirtual ||
                    p.GetCustomAttributes( typeof( IncludeForReportingAttribute ), true ).Any() ||
                    p.Name == "Order" || p.Name == "IsActive" ||
                    p.Name == "ForeignKey" || p.Name == "ForeignId" || p.Name == "ForeignGuid" ||
                    p.Name == "CreatedByPersonAliasId" || p.Name == "ModifiedByPersonAliasId" ||
                    p.Name == "CreatedDateTime" || p.Name == "ModifiedDateTime" )
                .Where( p => p.Name != "Id" )
                .Where( p => !p.GetCustomAttributes( typeof( DatabaseGeneratedAttribute ), true ).Any() )
                .OrderBy( p => p.Name )
                .Select( p => new ImportProperty
                {
                    Name = p.Name,
                    Required = p.GetCustomAttributes( typeof( RequiredAttribute ), true ).Any()
                } )
                .ToList();

            foreach ( var property in state )
            {
                if ( KnownFields.Contains( property.Name ) )
                {
                    property.LavaTemplate = string.Format( "{{{{ Row.{0} }}}}", property.Name );
                }
            }

            return state;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event for the control.
        /// Update all information for the user selecting a new Entity Type to import.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected void etEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? entityTypeId = etEntityType.SelectedEntityTypeId;
            gProperties.Visible = entityTypeId.HasValue;
            pnlActions.Visible = entityTypeId.HasValue;

            if ( entityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( entityTypeId.Value );
                if ( entityType != null )
                {
                    var type = Type.GetType( entityType.AssemblyName );

                    ImportPropertyState = GetKnownPropertiesForType( type );

                    BindPropertiesGrid();
                }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event for the control.
        /// Do custom binding of data to the UI.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The GridViewRowEventArgs that describe the event.</param>
        protected void gProperties_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var property = ( ImportProperty ) e.Row.DataItem;
                var ceLavaTemplate = ( CodeEditor ) e.Row.FindControl( "ceLavaTemplate" );

                ceLavaTemplate.Text = property.LavaTemplate;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gSample control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gSample_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindSampleGrid();
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected void lbImport_Click( object sender, EventArgs e )
        {
            StartImportTask( false );
        }

        /// <summary>
        /// Handles the Click event of the control.
        /// Tests an import by doing a full import without actually saving the changes.
        /// </summary>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="e">The EventArgs that describe the event.</param>
        protected void lbTestImport_Click( object sender, EventArgs e )
        {
            StartImportTask( true );
        }

        /// <summary>
        /// Handles the Click event of the lbPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void lbPreview_Click( object sender, EventArgs e )
        {
            var queryable = GetQueryable();
            var enabledLavaCommands = GetAttributeValue( "EnabledLavaCommands" );
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage );

            //
            // Setup all the columns for the exported data.
            //
            gPreview.Columns.Clear();
            foreach ( var property in ImportPropertyState )
            {
                var boundField = new RockBoundField
                {
                    DataField = property.Name,
                    HeaderText = property.Name
                };

                gPreview.Columns.Add( boundField );
            }

            var dataList = new List<object>();
            foreach ( var r in queryable.Take( 10 ) )
            {
                var row = new Dictionary<string, object>();

                mergeFields.AddOrReplace( "Row", r );

                for ( int i = 0; i < ImportPropertyState.Count; i++ )
                {
                    var property = ImportPropertyState[i];
                    string value = GetPropertyValue( r, property, enabledLavaCommands, mergeFields );

                    row.Add( property.Name, value );
                }

                dataList.Add( JsonConvert.DeserializeObject( JsonConvert.SerializeObject( row ) ) );
            }

            gPreview.DataSource = dataList;
            gPreview.DataBind();
            gPreview.Visible = true;

            pnlPreview.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbSaveAsPreset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void lbSaveAsPreset_Click( object sender, EventArgs e )
        {
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
                Columns = ImportPropertyState
            };

            string presetJson = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.ImportEntityData.Presets" );
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
            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.ImportEntityData.Presets", presetJson );

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
            string presetJson = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.ImportEntityData.Presets" );
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
                var entityType = EntityTypeCache.Get( preset.EntityTypeId );
                var type = Type.GetType( entityType.AssemblyName );

                var state = GetKnownPropertiesForType( type );
                foreach ( var c in preset.Columns )
                {
                    var property = state.Where( p => p.Name == c.Name ).FirstOrDefault();

                    if ( property != null )
                    {
                        property.LavaTemplate = c.LavaTemplate;
                    }
                }

                etEntityType.SelectedEntityTypeId = preset.EntityTypeId;
                ImportPropertyState = state;

                pnlActions.Visible = true;

                BindPropertiesGrid();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDeletePreset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeletePreset_Click( object sender, EventArgs e )
        {
            string presetJson = Rock.Web.SystemSettings.GetValue( "com.blueboxmoon.ImportEntityData.Presets" );
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
            Rock.Web.SystemSettings.SetValue( "com.blueboxmoon.ImportEntityData.Presets", presetJson );

            BindPresetList();
        }

        /// <summary>
        /// Handles the FileUploaded event of the fupData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fupData_FileUploaded( object sender, EventArgs e )
        {
            var queryable = GetQueryable();

            if ( queryable == null || queryable.Count() == 0 )
            {
                nbValidation.Text = "Could not detect format of the uploaded file.";
                fupData.BinaryFileId = null;
                return;
            }

            //
            // Because the data may come from JSON and each object may not contain
            // every key, sample the first 100 items to find all the keys.
            //
            var fields = new List<string>();
            foreach ( var row in queryable.Take( 100 ) )
            {
                foreach ( var f in row.Keys )
                {
                    if ( !fields.Contains( f ) )
                    {
                        fields.Add( f );
                    }
                }
            }
            KnownFields = fields;

            //
            // Generate a sample set of the first 10 rows.
            //
            SampleData = queryable.Take( 10 )
                .Select( d => d.ToDictionary( kvp => kvp.Key, kvp => kvp.Value.ToStringSafe() ) )
                .ToList();

            BindSampleGrid();

            pnlImportOptions.Visible = true;
            ShowTab( "Settings" );
        }

        /// <summary>
        /// Handles the FileRemoved event of the fupData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fupData_FileRemoved( object sender, Rock.Web.UI.Controls.FileUploaderEventArgs e )
        {
            pnlImportOptions.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTab_Click( object sender, EventArgs e )
        {
            ShowTab( ( ( LinkButton ) sender ).Text );
        }

        #endregion

        #region Support Classes

        [Serializable]
        public class ImportProperty
        {
            public string Name { get; set; }

            public string LavaTemplate { get; set; }

            public bool Required { get; set; }
        }

        public class Preset
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public int EntityTypeId { get; set; }

            public List<ImportProperty> Columns { get; set; }
        }

        public class CsvParser
        {
            Stream _stream;
            int _bufferPosition;
            byte[] _buffer;

            public CsvParser( Stream stream )
            {
                _stream = stream;
            }

            public string[] ReadRecord()
            {
                var record = new List<string>();
                bool lastField;

                string field = null;
                while ( ( field = ReadField( out lastField ) ) != null )
                {
                    record.Add( field );

                    if ( lastField )
                    {
                        break;
                    }
                }

                return record.Count > 0 ? record.ToArray() : null;
            }

            protected string ReadField( out bool lastField )
            {
                int c = ReadCharacter();

                //
                // Check if we are at EOF.
                //
                if ( c < 0 )
                {
                    lastField = true;
                    return null;
                }

                //
                // Check for a End of Line, which means this is the last field in this record.
                //
                if ( c == '\r' || c == '\n' )
                {
                    while ( true )
                    {
                        c = PeekCharacter();
                        if ( c != '\r' && c != '\n' )
                        {
                            lastField = true;
                            return string.Empty;
                        }

                        ReadCharacter();
                    };
                }

                //
                // Check for an empty field.
                //
                if ( c == ',' )
                {
                    lastField = false;
                    return string.Empty;
                }

                var sb = new StringBuilder();

                if ( c == '"' )
                {
                    //
                    // Process a quoted field value.
                    //
                    while ( c >= 0 )
                    {
                        c = ReadCharacter();
                        if ( c == '"' )
                        {
                            c = PeekCharacter();
                            if ( c != '"' )
                            {
                                break; /* End of quoted string. */
                            }

                            ReadCharacter(); /* Skip the second " */
                            sb.Append( '"' );
                        }
                        else
                        {
                            sb.Append( ( char ) c );
                        }
                    }

                    if ( c < 0 || c == '\r' || c == '\n' )
                    {
                        lastField = true;

                        while ( c == '\r' || c == '\n' )
                        {
                            ReadCharacter();
                            c = PeekCharacter();
                        }
                    }
                    else
                    {
                        ReadCharacter(); /* Consume the , */
                        lastField = false;
                    }

                    return sb.ToString();
                }
                else
                {
                    //
                    // Process an unquoted field value.
                    //
                    while ( c != ',' && c >= 0 && c != '\r' && c != '\n' )
                    {
                        sb.Append( ( char ) c );
                        c = ReadCharacter();
                    }

                    if ( c < 0 || c == '\r' || c == '\n' )
                    {
                        lastField = true;

                        c = PeekCharacter();
                        while ( c == '\r' || c == '\n' )
                        {
                            ReadCharacter();
                            c = PeekCharacter();
                        }
                    }
                    else
                    {
                        lastField = false;
                    }

                    return sb.ToString();
                }
            }

            protected int ReadCharacter()
            {
                if ( !EnsureBuffer() )
                {
                    return -1;
                }

                var c = _buffer[_bufferPosition++];

                return c;
            }

            protected int PeekCharacter()
            {
                if ( !EnsureBuffer() )
                {
                    return -1;
                }

                return _buffer[_bufferPosition];
            }

            protected bool EnsureBuffer()
            {
                if ( _buffer != null && _buffer.Length > _bufferPosition )
                {
                    return true;
                }

                byte[] data = new byte[1024];
                int len = _stream.Read( data, 0, 1024 );
                if ( len == 0 )
                {
                    return false;
                }

                _buffer = data.Take( len ).ToArray();
                _bufferPosition = 0;

                return true;
            }
        }

        #endregion
    }
}
