using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_shepherdchurch.Misc
{
    [DisplayName( "Share Workflow" )]
    [Category( "Shepherd Church > Misc" )]
    [Description( "Export and import workflows from Rock." )]

    public partial class ShareWorkflow : RockBlock
    {
        protected List<Export.EntityPreview> PreviewEntities
        {
            get
            {
                if ( _previewEntities == null && ViewState["PreviewEntities"] != null )
                {
                    _previewEntities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Export.EntityPreview>>( ViewState["PreviewEntities"].ToString() );
                }

                return _previewEntities;
            }
            set
            {
                ViewState["PreviewEntities"] = Newtonsoft.Json.JsonConvert.SerializeObject( value );
                _previewEntities = value;
            }
        }
        private List<Export.EntityPreview> _previewEntities;

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( btnExport );

            if ( !IsPostBack )
            {
            }
        }

        #endregion

        #region Core Methods


        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        #endregion

        protected void btnPreview_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( wtpExport.SelectedValueAsId().Value );
            var helper = new Export.Helper( new RockContext() );
            var exporter = new Export.WorkflowTypeExporter( helper );

            var entities = exporter.Preview2( workflowType );
            PreviewEntities = entities.
                Where( entity => !new string[] { "Rock.Model.Attribute", "Rock.Model.AttributeValue", "Rock.Model.AttributeQualifier", "Rock.Model.WorkflowActionFormAttribute" }.Contains( entity.Type ) )
                .ToList();
            BindPreviewGrid();
            //return;

            //var tree = exporter.Preview( workflowType );

            //Action<StringBuilder, Export.EntityReferenceTree, string> buildTree = null;
            //buildTree = ( sb, parent, indent ) => {
            //    if ( indent == string.Empty )
            //    {
            //        sb.AppendLine( string.Format( "<b>{0}</b> {1}{2}", parent.Name ?? parent.Guid.ToString(), parent.EntityType, parent.IsCritical ? " **" : "" ) );
            //    }

            //    if ( parent.Children.Any() )
            //    {
            //        var last = parent.Children.Last();
            //        foreach ( var c in parent.Children )
            //        {
            //            sb.Append( indent + ( c == last ? " \\- " : " |- " ) );
            //            sb.AppendLine( string.Format( "{0}: <b>{1}</b> {2}{3}", c.PropertyName, c.Name ?? c.Guid.ToString(), c.EntityType, c.IsCritical ? " **" : "" ) );

            //            buildTree( sb, c, indent + ( c == last ? "    " : " |  " ) );
            //        }
            //    }
            //};

            //StringBuilder content = new StringBuilder();
            //buildTree( content, tree, string.Empty );
            //ltDebug.Text = content.ToString();
        }

        protected void btnExport_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var workflowTypeService = new WorkflowTypeService( rockContext );
            var workflowType = workflowTypeService.Get( wtpExport.SelectedValueAsId().Value );
            var helper = new Export.Helper( new RockContext() );
            var exporter = new Export.WorkflowTypeExporter( helper );
            var container = exporter.Export( workflowType );

            Page.EnableViewState = false;
            Page.Response.Clear();
            Page.Response.ContentType = "application/json";
            Page.Response.AppendHeader( "Content-Disposition", string.Format( "attachment; filename=\"{0}_{1}.json\"" , workflowType.Name.MakeValidFileName(), RockDateTime.Now.ToString( "yyyyMMddHHmm" ) ) );
            Page.Response.Write( Newtonsoft.Json.JsonConvert.SerializeObject( container ) );
            Page.Response.Flush();
            Page.Response.End();
        }

        protected void fuImport_FileUploaded( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( fuImport.BinaryFileId ?? 0 );

                if ( binaryFile != null )
                {
                    try
                    {
                        var container = Newtonsoft.Json.JsonConvert.DeserializeObject<Export.DataContainer>( binaryFile.ContentsToString() );
                        List<string> messages;

                        Export.Helper.Import( container, true, new RockContext(), out messages );
                        ltDebug.Text = string.Empty;
                        foreach ( var msg in messages )
                        {
                            ltDebug.Text += string.Format( "{0}\n", msg );
                        }
                    }
                    finally
                    {
                        binaryFileService.Delete( binaryFile );
                        rockContext.SaveChanges();
                    }
                }

                fuImport.BinaryFileId = null;
            }
        }

        protected void gPreview_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var ddlAction = e.Row.FindControl( "ddlAction" ) as Rock.Web.UI.Controls.RockDropDownList;
            var item = ( Export.EntityPreview ) e.Row.DataItem;

            if ( ddlAction != null )
            {
                if ( item.Paths.Count == 1 && item.Paths.First().Paths.Count == 0 )
                {
                    ddlAction.Enabled = false;
                }
                ddlAction.SelectedValue = item.Action.ToString();
            }
        }

        protected void gPreview_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindPreviewGrid();
        }

        protected void BindPreviewGrid()
        {
            var query = PreviewEntities.AsQueryable();

            if ( gPreview.SortProperty != null )
            {
                query = query.Sort( gPreview.SortProperty );
            }
            gPreview.DataSource = query;
            gPreview.DataBind();
        }

        protected void btnStage2_Click( object sender, EventArgs e )
        {
            var previewEntities = PreviewEntities;

            for (int i = 0; i < gPreview.Rows.Count; i++ )
            {
                var ddlAction = ( Rock.Web.UI.Controls.RockDropDownList ) gPreview.Rows[i].FindControl( "ddlAction" );

                previewEntities[i].Action = ddlAction.SelectedValue.AsInteger();
            }
        }

        protected void lbTree_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var entity = PreviewEntities.Where( p => p.Guid == ( Guid ) e.RowKeyValue ).First();
            string ulli = "<ul class=\"fa-ul\"><li><i class=\"fa-li fa fa-angle-double-right\"></i>";

            StringBuilder content = new StringBuilder();
            foreach ( var reference in entity.Paths )
            {
                if ( reference.Paths.Count == 0 )
                {
                    content.Append( "This is a root object so there is no reference tree." );
                }
                else
                {
                    for ( int i = 0; i <= reference.Paths.Count; i++ )
                    {
                        if ( i == 0 )
                        {
                            content.AppendFormat( "{0}{1}", ulli, reference.Paths[i].Name );
                        }
                        else if ( i == reference.Paths.Count )
                        {
                            content.AppendFormat( "{0}{1}: <b>{{this object}}</b>", ulli, reference.Paths[i - 1].PropertyName );
                        }
                        else
                        {
                            content.AppendFormat( "{0}{1}: {2}", ulli, reference.Paths[i - 1].PropertyName, reference.Paths[i].Name );
                        }
                    }
                    for ( int i = 0; i <= reference.Paths.Count; i++ )
                    {
                        content.AppendFormat( "</li></ul>" );
                    }
                }
            }
            ltModalTree.Text = content.ToString();

            mdTree.Show();
        }
    }
}

namespace RockWeb.Plugins.com_shepherdchurch.Misc.Export
{
    public class WorkflowTypeExporter : IExporter, IExporter<WorkflowType>
    {
        Helper Helper { get; set; }

        public WorkflowTypeExporter( Helper helper )
        {
            Helper = helper;
        }

        public bool PathNeedsNewGuid( EntityPath path )
        {
            return ( path.Count == 0 ||
                path == "AttributeTypes" ||
                path == "AttributeTypes.AttributeQualifiers" ||
                path == "ActivityTypes" ||
                path == "ActivityTypes.AttributeTypes" ||
                path == "ActivityTypes.AttributeTypes.AttributeQualifiers" ||
                path == "ActivityTypes.ActionTypes" ||
                path == "ActivityTypes.ActionTypes.AttributeValues" ||
                path == "ActivityTypes.ActionTypes.WorkflowFormId" ||
                path == "ActivityTypes.ActionTypes.WorkflowFormId.FormAttributes" );
        }

        public bool IsPathCritical( EntityPath path )
        {
            return ( PathNeedsNewGuid( path ) );
        }

        /// <summary>
        /// Export a WorkflowType and all required information.
        /// </summary>
        /// <param name="entity">The WorkflowType to be exported.</param>
        /// <returns>A DataContainer that is ready to be encoded and saved.</returns>
        public DataContainer Export( WorkflowType entity )
        {
            Helper helper = new Helper( new RockContext() );

            helper.EnqueueEntity( entity, new EntityPath(), true, this );

            return helper.ProcessQueue();
        }

        /// <summary>
        /// Generate a tree of all entities that can be exported from the parent entity.
        /// This can be used to present to the user and ask them if there are any entities
        /// they do not wish to export.
        /// </summary>
        /// <param name="entity">The root entity that is going to be exported.</param>
        /// <returns>An object that represents the entity tree.</returns>
        public EntityReferenceTree Preview( WorkflowType entity )
        {
            Helper helper = new Helper( new RockContext() );
            List<string> ignoredEntityTypes = new List<string>();

            helper.EnqueueEntity( entity, new EntityPath(), true, this );

            Dictionary<IEntity, EntityPath> entities = new Dictionary<IEntity, EntityPath>();
            helper.Entities.ForEach( e => entities.Add( e.Entity, e.ReferencePaths.OrderBy( p => p.Count ).FirstOrDefault() ) );
            EntityReferenceTree root = new EntityReferenceTree( entity, string.Empty, helper.GenerateTree( entity, entities ) );

            if ( entities.Count > 0 )
            {
                throw new Exception( "Found extra entities after building the tree. This shouldn't happen!" );
            }

            ignoredEntityTypes.Add( "Rock.Model.Attribute" );
            ignoredEntityTypes.Add( "Rock.Model.AttributeValue" );
            ignoredEntityTypes.Add( "Rock.Model.AttributeQualifier" );
            ignoredEntityTypes.Add( "Rock.Model.WorkflowActionFormAttribute" );
            root.CleanTree( ignoredEntityTypes );

            return root;
        }

        /// <summary>
        /// Generate a tree of all entities that can be exported from the parent entity.
        /// This can be used to present to the user and ask them if there are any entities
        /// they do not wish to export.
        /// </summary>
        /// <param name="entity">The root entity that is going to be exported.</param>
        /// <returns>An object that represents the entity tree.</returns>
        public List<EntityPreview> Preview2( WorkflowType entity )
        {
            Helper helper = new Helper( new RockContext() );
            List<EntityPreview> previewEntities = new List<EntityPreview>();

            helper.EnqueueEntity( entity, new EntityPath(), true, this );
            helper.Entities.ForEach( qe => previewEntities.Add( new EntityPreview( qe ) ) );

            return previewEntities;
        }
    }

    interface IExporter<T>
    {
        DataContainer Export( T entity );
        EntityReferenceTree Preview( T entity );
    }

    public interface IExporter
    {
        bool IsPathCritical( EntityPath queuedEntity );
        bool PathNeedsNewGuid( EntityPath queuedEntity );
    }

    /// <summary>
    /// A helper class for importing / exporting entities into and out of Rock.
    /// </summary>
    public class Helper
    {
        #region Properties

        /// <summary>
        /// The list of entities that are queued up to be encoded. This is
        /// an ordered list and the entities will be encoded/decoded in this
        /// order.
        /// </summary>
        public List<QueuedEntity> Entities { get; private set; }

        /// <summary>
        /// The database context to perform all our operations in.
        /// </summary>
        public RockContext RockContext { get; private set; }

        protected Func<QueuedEntity, bool> EntityNeedsGuid { get; set; }

        protected Func<IEntity, string, bool> ShouldDescend { get; set; }

        /// <summary>
        /// The map of original Guids to newly generated Guids.
        /// </summary>
        private Dictionary<Guid, Guid> GuidMap { get; set; }

        /// <summary>
        /// Internal list of cached IEntityProcessors that we have created for
        /// this session.
        /// </summary>
        private Dictionary<Type, List<IEntityProcessor>> CachedProcessors { get; set; }

        /// <summary>
        /// Internal list of cached entity types. This is a map of the full class
        /// name to the class Type object.
        /// </summary>
        private Dictionary<string, Type> CachedEntityTypes { get; set; }

        /// <summary>
        /// Internal list of cached PropertyInfo definitions for the given Type.
        /// </summary>
        private Dictionary<Type, List<PropertyInfo>> CachedEntityProperties { get; set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Attempt to import the container of entities into the Rock database. Creates
        /// a transaction inside the RockContext to perform all the entity creation so
        /// if an error occurs everything will be left in a clean state.
        /// </summary>
        /// <param name="container">The container of all the encoded entities.</param>
        /// <param name="newGuids">Wether or not to generate new Guids during import of entities that have requested new Guids.</param>
        /// <param name="rockContext">The database context to operate in when creating and loading entities.</param>
        /// <param name="messages">Any messages, errors or otherwise, that should be displayed to the user.</param>
        /// <returns>true if the import succeeded, false if it did not.</returns>
        static public bool Import( DataContainer container, bool newGuids, RockContext rockContext, out List<string> messages )
        {
            messages = new List<string>();
            var helper = new Helper( rockContext );

            //
            // Ensure we know about all referenced entity types.
            //
            var missingTypes = container.MissingEntityTypes();
            if ( missingTypes.Any() )
            {
                messages.Add( string.Format( "The following EntityTypes are unknown and indicate you may be missing a plug-in: <ul><li>{0}</li></ul>", string.Join( "</li><li>", missingTypes ) ) );
                return false;
            }

            using ( var transaction = rockContext.Database.BeginTransaction() )
            {
                try
                {
                    //
                    // Generate a new Guid if we were asked to.
                    //
                    if ( newGuids )
                    {
                        foreach ( var encodedEntity in container.Entities )
                        {
                            if ( encodedEntity.NewGuid )
                            {
                                helper.MapNewGuid( encodedEntity.Guid );
                            }
                        }
                    }

                    //
                    // Walk each encoded entity and either verify an existing entity or
                    // create a new entity.
                    //
                    foreach ( var encodedEntity in container.Entities )
                    {
                        Type entityType = helper.FindEntityType( encodedEntity.EntityType );
                        Guid entityGuid = helper.MapGuid( encodedEntity.Guid );
                        var entity = helper.GetExistingEntity( encodedEntity.EntityType, entityGuid );

                        if ( entity == null )
                        {
                            try
                            {
                                entity = helper.CreateNewEntity( encodedEntity );
                            }
                            catch ( Exception e )
                            {
                                throw new Exception( String.Format( "Error importing encoded entity: {0}", encodedEntity.ToJson() ), e );
                            }

                            messages.Add( string.Format( "Created: {0}, {1}", encodedEntity.EntityType, entityGuid ) );
                        }
                    }

                    transaction.Commit();

                    return true;
                }
                catch ( Exception e )
                {
                    transaction.Rollback();

                    for ( Exception ex = e; ex != null; ex = ex.InnerException )
                    {
                        messages.Add( e.Message );
                    }

                    return false;
                }
            }
        }

        /// <summary>
        /// Determines the real Entity type of the given IEntity object. Because
        /// many IEntity objects are dynamic proxies created by Entity Framework
        /// we need to get the actual underlying type.
        /// </summary>
        /// <param name="entity">The entity whose type we want to obtain.</param>
        /// <returns>The true IEntity type (such as Rock.Model.Person).</returns>
        static public Type GetEntityType( IEntity entity )
        {
            Type type = entity.GetType();

            return type.IsDynamicProxyType() ? type.BaseType : type;
        }

        /// <summary>
        /// Convert the given object value to the target type. This extends
        /// the IConvertable.ChangeType support since some things don't
        /// implement IConvertable, like Guid and Nullable.
        /// </summary>
        /// <param name="t">The target data type to convert to.</param>
        /// <param name="obj">The object value to be converted.</param>
        /// <returns>The value converted to the target type.</returns>
        static public object ChangeType( Type t, object obj )
        {
            Type u = Nullable.GetUnderlyingType( t );

            if ( u != null )
            {
                return ( obj == null ) ? null : ChangeType( u, obj );
            }
            else
            {
                if ( t.IsEnum )
                {
                    return Enum.Parse( t, obj.ToString() );
                }
                else if ( t == typeof( Guid ) && obj is string )
                {
                    return new Guid( ( string ) obj );
                }
                else if ( t == typeof( string ) && obj is Guid )
                {
                    return obj.ToString();
                }
                else
                {
                    return Convert.ChangeType( obj, t );
                }
            }
        }

        /// <summary>
        /// Get a friendly name for the entity, optionally including the short name for the
        /// entity type. This attempts a ToString() on the entity and if that returns what
        /// appears to be a valid name (no &lt; character and less than 40 characters) then
        /// it us used as the name. Otherwise the Guid is used for the name.
        /// </summary>
        /// <param name="entity">The entity whose name we wish to retrieve.</param>
        /// <param name="includeType">If true include the short type name.</param>
        /// <returns>A string that can be displayed to the user to identify this entity.</returns>
        static public string EntityFriendlyName( IEntity entity, bool includeType )
        {
            string name;

            name = entity.ToString();
            if ( name.Length > 40 || name.Contains( "<" ) )
            {
                name = entity.Guid.ToString();
            }

            if ( includeType )
            {
                return string.Format( "{0} ({1})", name, GetEntityType( entity ).Name );
            }
            else
            {
                return name;
            }
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Initialize a new Helper object for facilitating the export/import of entities.
        /// </summary>
        /// <param name="rockContext">The RockContext to work in when exporting or importing.</param>
        public Helper( RockContext rockContext )
        {
            Entities = new List<QueuedEntity>();
            GuidMap = new Dictionary<Guid, Guid>();
            CachedEntityTypes = new Dictionary<string, Type>();
            CachedProcessors = new Dictionary<Type, List<IEntityProcessor>>();
            CachedEntityProperties = new Dictionary<Type, List<PropertyInfo>>();
            RockContext = rockContext;
        }

        /// <summary>
        /// Process the queued list of entities that are waiting to be encoded. This
        /// encodes all entities, generates new Guids for any entities that need them,
        /// and then maps all references to the new Guids.
        /// </summary>
        /// <param name="guidEvaluation">A function that is called for each entity to determine if it needs a new Guid or not.</param>
        /// <returns>A DataContainer that is ready for JSON export.</returns>
        public DataContainer ProcessQueue()
        {
            DataContainer container = new DataContainer();

            //
            // Find out if we need to give new Guid values to any entities.
            //
            foreach ( var queuedEntity in Entities )
            {
                queuedEntity.EncodedEntity = Export( queuedEntity.Entity );

                if ( queuedEntity.ReferencePaths[0].Count == 0 || queuedEntity.Flags.HasFlag( QueuedEntityFlags.NewGuid ) )
                {
                    queuedEntity.EncodedEntity.NewGuid = true;
                }
            }

            //
            // Convert to a data container.
            //
            foreach ( var queuedEntity in Entities )
            {
                container.Entities.Add( queuedEntity.EncodedEntity );

                if ( queuedEntity.ReferencePaths.Count == 1 && queuedEntity.ReferencePaths[0].Count == 0 )
                {
                    container.RootEntities.Add( queuedEntity.EncodedEntity.Guid );
                }
            }

            return container;
        }

        /// <summary>
        /// Adds an entity to the queue list. This provides circular reference checking as
        /// well as ensuring that proper order is maintained for all entities.
        /// </summary>
        /// <param name="entity">The entity that is to be included in the export.</param>
        /// <param name="path">The entity path that lead to this entity being encoded.</param>
        /// <param name="entityIsCritical">True if the entity is critical, that is referenced directly.</param>
        public void EnqueueEntity( IEntity entity, EntityPath path, bool entityIsCritical, IExporter exporter )
        {
            List<KeyValuePair<string, IEntity>> entities;

            //
            // These are system generated rows, we should never try to backup or restore them.
            //
            if ( entity.TypeName == "Rock.Model.EntityType" || entity.TypeName == "Rock.Model.FieldType" )
            {
                return;
            }

            //
            // If the entity is already in our path that means we are beginning a circular
            // reference so we can just ignore this one.
            //
            if ( path.Where( e => e.Entity.Guid == entity.Guid ).Any() )
            {
                return;
            }

            //
            // Find the entities that this entity references, in other words entities that must
            // exist before this one can be created.
            //
            entities = FindReferencedEntities( entity );
            entities.ForEach( e => EnqueueEntity( e.Value, path.PathByAddingComponent( new EntityPathComponent( entity, e.Key ) ), true, exporter ) );

            //
            // If we already know about the entity, add a reference to it and return.
            //
            var queuedEntity = Entities.Where( e => e.Entity.Guid == entity.Guid ).FirstOrDefault();
            if ( queuedEntity == null )
            {
                queuedEntity = new QueuedEntity( entity, path.Clone() );
                Entities.Add( queuedEntity );
            }
            else
            {
                //
                // We have already visited this entity from the same parent. Not sure why we are here.
                //
                if ( path.Any() && queuedEntity.ReferencePaths.Where( r => r.Any() && r.Last().Entity.Guid == path.Last().Entity.Guid ).Any() )
                {
                    return;
                }

                queuedEntity.AddReferencePath( path.Clone() );
            }

            if ( !queuedEntity.Flags.HasFlag( QueuedEntityFlags.Critical ) && ( path.Count == 0 || entityIsCritical || exporter.IsPathCritical( path ) ) )
            {
                queuedEntity.Flags |= QueuedEntityFlags.Critical;
            }
            if ( !queuedEntity.Flags.HasFlag( QueuedEntityFlags.NewGuid ) && exporter.PathNeedsNewGuid( path ) )
            {
                queuedEntity.Flags |= QueuedEntityFlags.NewGuid;
            }

            //
            // Find the entities that this entity has as children. This is usually the many side
            // of a one-to-many reference (such as a Workflow has many WorkflowActions, this would
            // get a list of the WorkflowActions).
            //
            entities = FindChildEntities( entity );
            entities.ForEach( e => EnqueueEntity( e.Value, path.PathByAddingComponent( new EntityPathComponent( entity, e.Key ) ), false, exporter ) );
        }

        /// <summary>
        /// Find entities that this object references directly. These are entities that must be
        /// created before this entity can be re-created.
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, IEntity>> FindReferencedEntities( IEntity parentEntity )
        {
            List<KeyValuePair<string, IEntity>> children = new List<KeyValuePair<string, IEntity>>();

            var properties = GetEntityProperties( parentEntity );

            //
            // Take a stab at any properties that end in "Id" and likely reference another
            // entity, such as a property called "WorkflowId" probably references the Workflow
            // entity and should be linked by Guid.
            //
            foreach ( var property in properties )
            {
                if ( property.Name.EndsWith( "Id" ) && ( property.PropertyType == typeof( int ) || property.PropertyType == typeof( Nullable<int> ) ) )
                {
                    var entityProperty = parentEntity.GetType().GetProperty( property.Name.Substring( 0, property.Name.Length - 2 ) );

                    if ( entityProperty != null )
                    {
                        IEntity childEntity = entityProperty.GetValue( parentEntity ) as IEntity;

                        if ( childEntity != null )
                        {
                            children.Add( new KeyValuePair<string, IEntity>( property.Name, childEntity ) );
                        }
                    }
                }
            }

            //
            // Allow for processors to adjust the list of children.
            //
            foreach ( var processor in FindEntityProcessors( GetEntityType( parentEntity ) ) )
            {
                processor.EvaluateReferencedEntities( parentEntity, children, this );
            }

            return children;
        }

        /// <summary>
        /// Generate the list of entities that reference this parent entity. These are entities that
        /// must be created after this entity has been created.
        /// </summary>
        /// <param name="parentEntity">The parent entity to find reverse-references to.</param>
        /// <returns></returns>
        public List<KeyValuePair<string, IEntity>> FindChildEntities( IEntity parentEntity )
        {
            List<KeyValuePair<string, IEntity>> children = new List<KeyValuePair<string, IEntity>>();

            var properties = GetEntityProperties( parentEntity );

            //
            // Take a stab at any properties that are an ICollection<IEntity> and treat those
            // as child entities.
            //
            foreach ( var property in properties )
            {
                if ( property.PropertyType.GetInterface( "IEnumerable" ) != null && property.PropertyType.GetGenericArguments().Length == 1 )
                {
                    if ( typeof( IEntity ).IsAssignableFrom( property.PropertyType.GetGenericArguments()[0] ) )
                    {
                        IEnumerable childEntities = ( IEnumerable ) property.GetValue( parentEntity );

                        foreach ( IEntity childEntity in childEntities )
                        {
                            children.Add( new KeyValuePair<string, IEntity>( property.Name, childEntity ) );
                        }
                    }
                }
            }

            //
            // We also need to pull in any attribute values. We have to pull attributes as well
            // since we might not have an actual value for that attribute yet and would need
            // it to pull the default value and definition.
            //
            var attributedEntity = parentEntity as IHasAttributes;
            if ( attributedEntity != null )
            {
                if ( attributedEntity.Attributes == null )
                {
                    attributedEntity.LoadAttributes( RockContext );
                }

                foreach ( var item in attributedEntity.Attributes )
                {
                    var attrib = new AttributeService( RockContext ).Get( item.Value.Guid );

                    children.Add( new KeyValuePair<string, IEntity>( "Attributes", attrib ) );

                    var value = new AttributeValueService( RockContext ).Queryable()
                        .Where( v => v.AttributeId == attrib.Id && v.EntityId == attributedEntity.Id )
                        .FirstOrDefault();
                    if ( value != null )
                    {
                        children.Add( new KeyValuePair<string, IEntity>( "AttributeValues", value ) );
                    }
                }
            }

            //
            // Allow for processors to adjust the list of children.
            //
            foreach ( var processor in FindEntityProcessors( GetEntityType( parentEntity ) ) )
            {
                processor.EvaluateChildEntities( parentEntity, children, this );
            }

            return children;
        }

        /// <summary>
        /// Get the list of properties from the entity that should be stored or re-created.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<PropertyInfo> GetEntityProperties( IEntity entity )
        {
            Type entityType = GetEntityType( entity );

            if ( !CachedEntityProperties.ContainsKey( entityType ) )
            {
                //
                // Get all data member mapped properties and filter out any "local only"
                // properties that should not be exported.
                //
                CachedEntityProperties.Add( entityType, entityType.GetProperties()
                    .Where( p => System.Attribute.IsDefined( p, typeof( DataMemberAttribute ) ) )
                    .Where( p => !System.Attribute.IsDefined( p, typeof( NotMappedAttribute ) ) )
                    .Where( p => !System.Attribute.IsDefined( p, typeof( DatabaseGeneratedAttribute ) ) )
                    .Where( p => p.Name != "Id" && p.Name != "Guid" )
                    .Where( p => p.Name != "ForeignId" && p.Name != "ForeignGuid" && p.Name != "ForeignKey" )
                    .Where( p => p.Name != "CreatedByPersonAliasId" && p.Name != "ModifiedByPersonAliasId" )
                    .ToList() );
            }

            return CachedEntityProperties[entityType];
        }

        /// <summary>
        /// Creates a new map entry for the oldGuid. This generates a new Guid and
        /// stores a reference between the two.
        /// </summary>
        /// <param name="oldGuid">The original Guid value to be mapped from.</param>
        /// <returns>A new Guid value that should be used in place of oldGuid.</returns>
        public Guid MapNewGuid( Guid oldGuid )
        {
            GuidMap.Add( oldGuid, Guid.NewGuid() );

            return GuidMap[oldGuid];
        }

        /// <summary>
        /// Finds and returns a Guid from the mapping dictionary. If no mapping
        /// exists then the original Guid is returned.
        /// </summary>
        /// <param name="oldGuid">The original Guid value to map from.</param>
        /// <returns>The Guid value that should be used, may be the same as oldGuid.</returns>
        public Guid MapGuid( Guid oldGuid )
        {
            return GuidMap.ContainsKey( oldGuid ) ? GuidMap[oldGuid] : oldGuid;
        }

        /// <summary>
        /// Export the given entity into an EncodedEntity object. This can be used later to
        /// reconstruct the entity.
        /// </summary>
        /// <param name="entity">The entity to be exported.</param>
        /// <returns>The exported data that can be imported.</returns>
        protected EncodedEntity Export( IEntity entity )
        {
            EncodedEntity encodedEntity = new EncodedEntity();
            Type entityType = GetEntityType( entity );

            encodedEntity.Guid = entity.Guid;
            encodedEntity.EntityType = entityType.FullName;

            var attributeEntity = entity as Rock.Model.Attribute;
            if ( attributeEntity != null )
            {
                if ( attributeEntity.EntityTypeQualifierColumn == "WorkflowTypeId" )
                {
                }
            }

            //
            // Generate the standard properties and references.
            //
            foreach ( var property in GetEntityProperties( entity ) )
            {
                //
                // Don't encode IEntity properties, we should have the Id encoded instead.
                //
                if ( typeof( IEntity ).IsAssignableFrom( property.PropertyType ) )
                {
                    continue;
                }

                //
                // Don't encode IEnumerable properties. Those should be included as
                // their own entities to be encoded later.
                //
                if ( property.PropertyType.GetInterface( "IEnumerable" ) != null &&
                    property.PropertyType.GetGenericArguments().Length == 1 &&
                    typeof( IEntity ).IsAssignableFrom( property.PropertyType.GetGenericArguments()[0] ) )
                {
                    continue;
                }

                encodedEntity.Properties.Add( property.Name, property.GetValue( entity ) );
            }

            //
            // Run any post-process transforms.
            //
            foreach ( var processor in FindEntityProcessors( entityType ) )
            {
                var data = processor.PostProcessExportedEntity( entity, encodedEntity, this );

                if ( data != null )
                {
                    encodedEntity.AddTransform( processor.Identifier.ToString(), data );
                }
            }

            //
            // Generate the references to other entities.
            //
            foreach ( var x in FindReferencedEntities( entity ) )
            {
                encodedEntity.MakeIntoReference( x.Key, x.Value );
            }

            return encodedEntity;
        }

        /// <summary>
        /// Attempt to load an entity from the database based on it's Guid and entity type.
        /// </summary>
        /// <param name="entityType">The type of entity to load.</param>
        /// <param name="guid">The unique identifier of the entity.</param>
        /// <returns>The loaded entity or null if not found.</returns>
        public IEntity GetExistingEntity( string entityType, Guid guid )
        {
            var service = Reflection.GetServiceForEntityType( FindEntityType( entityType ), RockContext );

            if ( service != null )
            {
                var getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( Guid ) } );

                if ( getMethod != null )
                {
                    return ( IEntity ) getMethod.Invoke( service, new object[] { guid } );
                }
            }

            return null;
        }

        /// <summary>
        /// Attempt to load an entity from the database based on it's Guid and entity type.
        /// </summary>
        /// <param name="entityType">The type of entity to load.</param>
        /// <param name="guid">The unique identifier of the entity.</param>
        /// <returns>The loaded entity or null if not found.</returns>
        public IEntity GetExistingEntity( string entityType, int id )
        {
            var service = Reflection.GetServiceForEntityType( FindEntityType( entityType ), RockContext );

            if ( service != null )
            {
                var getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );

                if ( getMethod != null )
                {
                    return ( IEntity ) getMethod.Invoke( service, new object[] { id } );
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new entity in the database from the encoded information. The entity
        /// is saved before being returned.
        /// </summary>
        /// <param name="encodedEntity">The encoded entity information to create the new entity from.</param>
        /// <returns>A reference to the new entity.</returns>
        protected IEntity CreateNewEntity( EncodedEntity encodedEntity )
        {
            Type entityType = Reflection.FindType( typeof( IEntity ), encodedEntity.EntityType );
            var service = Reflection.GetServiceForEntityType( entityType, RockContext );

            if ( service != null )
            {
                var addMethod = service.GetType().GetMethod( "Add", new Type[] { entityType } );

                if ( addMethod != null )
                {
                    IEntity entity = ( IEntity ) Activator.CreateInstance( entityType );

                    RestoreEntityProperties( entity, encodedEntity );
                    entity.Guid = MapGuid( encodedEntity.Guid );

                    //
                    // Do custom pre-save processing.
                    //
                    foreach ( var processor in FindEntityProcessors( entityType ) )
                    {
                        processor.PreProcessImportedEntity( entity, encodedEntity, encodedEntity.GetTransform( processor.Identifier.ToString() ), this );
                    }

                    //
                    // Special handling of AttributeQualifier because Guids may not be the same
                    // across installations and the AttributeId+Key columns make up a unique key.
                    //
                    if ( encodedEntity.EntityType == "Rock.Model.AttributeQualifier" )
                    {
                        var reference = encodedEntity.References.Where( r => r.Property == "AttributeId" ).First();
                        var attribute = GetExistingEntity( "Rock.Model.Attribute", MapGuid( new Guid( ( string ) reference.Data ) ) );
                        string key = ( string ) encodedEntity.Properties["Key"];

                        var existingEntity = new AttributeQualifierService( RockContext )
                            .GetByAttributeId( attribute.Id )
                            .Where( a => a.Key == key )
                            .FirstOrDefault();

                        if ( existingEntity != null )
                        {
                            if ( entity.Guid != encodedEntity.Guid )
                            {
                                throw new Exception( "AttributeQualifier marked for new Guid but conflicting value already exists." );
                            }

                            GuidMap.AddOrReplace( encodedEntity.Guid, existingEntity.Guid );

                            return existingEntity;
                        }
                    }

                    //
                    // Special handling of Attribute's. The guid's might be different but if the entity type,
                    // entity qualifiers and key are the same, assume it's the same.
                    //
                    else if ( encodedEntity.EntityType == "Rock.Model.Attribute" )
                    {
                        var attribute = ( Rock.Model.Attribute ) entity;
                        var existingEntity = new AttributeService( RockContext )
                            .GetByEntityTypeId( attribute.EntityTypeId )
                            .Where( a => a.EntityTypeQualifierColumn == attribute.EntityTypeQualifierColumn && a.EntityTypeQualifierValue == attribute.EntityTypeQualifierValue && a.Key == attribute.Key )
                            .FirstOrDefault();

                        if ( existingEntity != null )
                        {
                            if ( entity.Guid != encodedEntity.Guid )
                            {
                                throw new Exception( "Attribute marked for new Guid but conflicting value already exists." );
                            }

                            GuidMap.AddOrReplace( encodedEntity.Guid, existingEntity.Guid );

                            return existingEntity;
                        }
                    }

                    //
                    // Special handling of AttributeValue's. The guid's might be different but if the attribute Id
                    // and entity Id are the same, assume it's the same.
                    //
                    else if ( encodedEntity.EntityType == "Rock.Model.AttributeValue" )
                    {
                        var attributeReference = encodedEntity.References.Where( r => r.Property == "AttributeId" ).First();
                        var attribute = GetExistingEntity( "Rock.Model.Attribute", MapGuid( new Guid( ( string ) attributeReference.Data ) ) );
                        var entityReference = encodedEntity.References.Where( r => r.Property == "EntityId" ).First();
                        var entityRef = GetExistingEntity( entityReference.EntityType, MapGuid( new Guid( ( string ) entityReference.Data ) ) );

                        var existingEntity = new AttributeValueService( RockContext )
                            .Queryable().Where( a => a.AttributeId == attribute.Id && a.EntityId == entityRef.Id )
                            .FirstOrDefault();

                        if ( existingEntity != null )
                        {
                            if ( entity.Guid != encodedEntity.Guid )
                            {
                                throw new Exception( "AttributeValue marked for new Guid but conflicting value already exists." );
                            }

                            GuidMap.AddOrReplace( encodedEntity.Guid, existingEntity.Guid );

                            return existingEntity;
                        }
                    }

                    addMethod.Invoke( service, new object[] { entity } );
                    RockContext.SaveChanges( true );

                    return entity;
                }
            }

            throw new Exception( string.Format( "Failed to create new database entity for {0}_{1}", encodedEntity.EntityType, encodedEntity.Guid ) );
        }

        /// <summary>
        /// Restore the property information from encodedEntity into the newly created entity.
        /// </summary>
        /// <param name="entity">The blank entity to be populated.</param>
        /// <param name="encodedEntity">The encoded entity data.</param>
        protected void RestoreEntityProperties( IEntity entity, EncodedEntity encodedEntity )
        {
            foreach ( var property in GetEntityProperties( entity ) )
            {
                //
                // If this is a plain property, just set the value.
                //
                if ( encodedEntity.Properties.ContainsKey( property.Name ) )
                {
                    var value = encodedEntity.Properties[property.Name];

                    //
                    // If this is a Guid, see if we need to remap it.
                    //
                    Guid? guidValue = null;
                    if ( value is Guid )
                    {
                        guidValue = ( Guid ) value;
                        value = MapGuid( guidValue.Value );
                    }
                    else if ( value is string )
                    {
                        guidValue = ( ( string ) value ).AsGuidOrNull();
                        if ( guidValue.HasValue && guidValue.Value != MapGuid( guidValue.Value ) )
                        {
                            value = MapGuid( guidValue.Value ).ToString();
                        }
                    }

                    property.SetValue( entity, ChangeType( property.PropertyType, value ) );
                }
            }

            //
            // Restore all references.
            //
            foreach ( var reference in encodedEntity.References )
            {
                reference.Restore( entity, this );
            }
        }

        /// <summary>
        /// Find the given IEntity Type from the full class name. Uses a cache to
        /// save processing time.
        /// </summary>
        /// <param name="entityName">The full class name of the IEntity type.</param>
        /// <returns>The Type object for the class name or null if not found.</returns>
        public Type FindEntityType( string entityName )
        {
            if ( CachedEntityTypes.ContainsKey( entityName ) )
            {
                return CachedEntityTypes[entityName];
            }

            Type type = Reflection.FindType( typeof( IEntity ), entityName );

            if ( type != null )
            {
                CachedEntityTypes.Add( entityName, type );
            }

            return type;
        }

        /// <summary>
        /// Retrieve an enumerable list of processor objects for the given
        /// IEntity type.
        /// </summary>
        /// <param name="entityType">The Type object of the IEntity to get processors for.</param>
        /// <returns>Enumerable of IEntityProcessor objects that will pre- and post-process this entity.</returns>
        public IEnumerable<IEntityProcessor> FindEntityProcessors( Type entityType )
        {
            if ( CachedProcessors.ContainsKey( entityType ) )
            {
                return CachedProcessors[entityType];
            }

            Type processorBaseType = typeof( EntityProcessor<> ).MakeGenericType( entityType );
            List<IEntityProcessor> processors = new List<IEntityProcessor>();
            foreach ( var processorType in Reflection.FindTypes( processorBaseType ) )
            {
                processors.Add( ( IEntityProcessor ) Activator.CreateInstance( processorType.Value ) );
            }

            CachedProcessors.Add( entityType, processors );

            return processors;
        }

        /// <summary>
        /// Recursively determine the tree structure based on the shortest entity path for each entity.
        /// </summary>
        /// <param name="parent">The parent entity that we are currently considering.</param>
        /// <param name="entities">The full list of entities still available to pick from.</param>
        /// <returns>The tree from this parent entity on down.</returns>
        public List<EntityReferenceTree> GenerateTree( IEntity parent, Dictionary<IEntity, EntityPath> entities )
        {
            List<EntityReferenceTree> leafs = new List<EntityReferenceTree>();

            entities.Remove( parent );

            foreach ( var e in entities.Where( x => x.Value.Count > 0 && x.Value.Last().Entity.Guid == parent.Guid ).ToList() )
            {
                EntityReferenceTree leaf = new EntityReferenceTree( e.Key, e.Value.Last().PropertyName, GenerateTree( e.Key, entities ) );
                var queuedEntity = Entities.Where( et => et.Entity == e.Key ).First();

                leaf.IsCritical = queuedEntity.Flags.HasFlag( QueuedEntityFlags.Critical );
                leafs.Add( leaf );
            }

            return leafs.OrderBy( l => l.PropertyName ).ToList();
        }

        #endregion
    }

    public class EntityPathPreview
    {
        public class Component
        {
            public Guid Guid { get; set; }
            public string Name { get; set; }
            public string PropertyName { get; set; }
        }

        public List<Component> Paths { get; set; }

        public EntityPathPreview()
        {
            Paths = new List<Component>();
        }

        public EntityPathPreview( EntityPath path )
        {
            Paths = new List<Component>();

            foreach ( var c in path )
            {
                Paths.Add( new Component { Guid = c.Entity.Guid, Name = Helper.EntityFriendlyName( c.Entity, true ), PropertyName = c.PropertyName } );
            }
        }
    }

    public class EntityPreview
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string ShortType { get; set; }
        public List<EntityPathPreview> Paths { get; set; }
        public bool IsCritical { get; set; }
        public bool IsNewGuid { get; set; }
        public int Action { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public List<KeyValuePair<string, string>> Parents
        {
            get
            {
                return Paths.Where( r => r.Paths.Any() )
                    .Select( r => r.Paths.Last() )
                    .Select( p => new KeyValuePair<string, string>( p.Name, p.PropertyName ) )
                    .ToList();
            }
        }

        public EntityPreview()
        {
            Paths = new List<EntityPathPreview>();
        }

        public EntityPreview(QueuedEntity queuedEntity)
        {
            Guid = queuedEntity.Entity.Guid;
            Name = Helper.EntityFriendlyName( queuedEntity.Entity, false );
            Type = Helper.GetEntityType( queuedEntity.Entity ).FullName;
            ShortType = Helper.GetEntityType( queuedEntity.Entity ).Name;
            IsCritical = queuedEntity.Flags.HasFlag( QueuedEntityFlags.Critical );
            IsNewGuid = queuedEntity.Flags.HasFlag( QueuedEntityFlags.NewGuid );
            Action = IsCritical ? 1 : 2;

            Paths = new List<EntityPathPreview>();
            foreach ( var path in queuedEntity.ReferencePaths )
            {
                Paths.Add( new EntityPathPreview( path ) );
            }
        }
    }


    /// <summary>
    /// Describes a single element of an entity path.
    /// </summary>
    public class EntityPathComponent
    {
        /// <summary>
        /// The entity at this specific location in the path.
        /// </summary>
        public IEntity Entity { get; private set; }

        /// <summary>
        /// The name of the property used to reach the next location in the path.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Create a new entity path component.
        /// </summary>
        /// <param name="entity">The entity at this specific location in the path.</param>
        /// <param name="propertyName">The name of the property used to reach the next location in the path.</param>
        public EntityPathComponent( IEntity entity, string propertyName )
        {
            Entity = entity;
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// Describes the entity and property path used to reach this point in
    /// the entity tree.
    /// </summary>
    public class EntityPath : List<EntityPathComponent>
    {
        #region Instance Methods

        /// <summary>
        /// Create a duplicate copy of this entity path and return it.
        /// </summary>
        /// <returns>A duplicate of this entity path.</returns>
        public EntityPath Clone()
        {
            EntityPath path = new EntityPath();

            path.AddRange( this );

            return path;
        }

        /// <summary>
        /// Create a new EntityPath by appending the path component. The original
        /// path is not modified.
        /// </summary>
        /// <param name="component">The new path component to append to this path.</param>
        /// <returns></returns>
        public EntityPath PathByAddingComponent( EntityPathComponent component )
        {
            EntityPath path = this.Clone();

            path.Add( component );

            return path;
        }

        public static bool operator ==( EntityPath a, string b )
        {
            var properties = b.Split( '.' );

            if ( a.Count == properties.Length )
            {
                int i = 0;
                for ( i = 0; i < a.Count; i++ )
                {
                    if ( a[i].PropertyName != properties[i] )
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public static bool operator !=( EntityPath a, string b )
        {
            return !( a == b );
        }

        #endregion
    }

    [Flags]
    public enum QueuedEntityFlags
    {
        NewGuid = 0x01,
        Critical = 0x02
    }

    /// <summary>
    /// Tracks entities and related information of entities that are queued up to be encoded.
    /// </summary>
    public class QueuedEntity
    {
        #region Properties

        /// <summary>
        /// The entity that is queued up for processing.
        /// </summary>
        public IEntity Entity { get; private set; }

        /// <summary>
        /// A list of all paths that we took to reach this entity.
        /// </summary>
        public List<EntityPath> ReferencePaths { get; private set; }

        public QueuedEntityFlags Flags { get; set; }

        /// <summary>
        /// During the encode process this will be filled in with the encoded
        /// entity data so that we can keep a link between the IEntity and the
        /// encoded data until we are done.
        /// </summary>
        public EncodedEntity EncodedEntity { get; set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Initialize a new queued entity for the given access path.
        /// </summary>
        /// <param name="entity">The entity that is going to be placed in the queue.</param>
        /// <param name="path">The initial path used to reach this entity.</param>
        public QueuedEntity( IEntity entity, EntityPath path )
        {
            Entity = entity;
            ReferencePaths = new List<EntityPath>();
            ReferencePaths.Add( path );
        }

        /// <summary>
        /// Add a new entity path reference to this existing entity.
        /// </summary>
        /// <param name="path">The path that can be used to reach this entity.</param>
        public void AddReferencePath( EntityPath path )
        {
            ReferencePaths.Add( path );
        }

        /// <summary>
        /// This is used for debug output to display the entity information and the path(s)
        /// that we took to find it.
        /// </summary>
        /// <returns>A string that describes this queued entity.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            EntityPath primaryPath = ReferencePaths[0];

            sb.AppendFormat( "{0} {1}", Entity.TypeName, Entity.Guid );
            foreach ( var p in ReferencePaths )
            {
                sb.AppendFormat( "\n\tPath" );
                foreach ( var e in p )
                {
                    sb.AppendFormat( "\n\t\t{0} {2} {1}", e.Entity.TypeName, e.PropertyName, e.Entity.Guid );
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks if this entity can be reached by the property path. For example if exporting a
        /// WorkflowType and you want to check if this entity is a WorkflowActionType for this
        /// workflow then you would use the property path "ActivityTypes.ActionTypes".
        /// </summary>
        /// <param name="propertyPath">The period delimited list of properties to reach this entity.</param>
        /// <returns>true if this entity can be reached by the property path, false if not.</returns>
        public bool ContainsPropertyPath( string propertyPath )
        {
            var properties = propertyPath.Split( '.' );

            foreach ( var path in ReferencePaths )
            {
                if ( path.Count == properties.Length )
                {
                    int i = 0;
                    for ( i = 0; i < path.Count; i++ )
                    {
                        if ( path[i].PropertyName != properties[i] )
                        {
                            break;
                        }
                    }

                    if ( i == path.Count )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }

    public enum ReferenceType
    {
        Null = 0,
        Guid = 1,
        EntityType = 2,
        FieldType = 3
    }

    /// <summary>
    /// Most entities in Rock reference other entities by Id number, i.e. CategoryId.
    /// This is not useful when exporting/importing entities between systems. So we
    /// embed a Reference object that contains the Property name that originally
    /// contained the Id number. During an import operation that Property is filled in
    /// with the Id number of the object identified by the EntityType and the Guid.
    /// </summary>
    public class Reference
    {
        #region Properties

        /// <summary>
        /// The name of the property to be filled in with the Id number of the
        /// referenced entity.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// The entity type name that will be loaded by it's Guid.
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// The type of reference this is.
        /// </summary>
        public ReferenceType Type { get; set; }

        /// <summary>
        /// The data used to re-create the reference, depends on Type.
        /// </summary>
        public object Data { get; set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Create a new empty reference. This should only be used by Newtonsoft when deserializing.
        /// </summary>
        public Reference()
        {
        }

        /// <summary>
        /// Creates a new entity reference object that is used to reconstruct the
        /// link between two entities in the database.
        /// </summary>
        /// <param name="entity">The entity we are creating a reference to.</param>
        /// <param name="propertyName">The name of the property in the containing entity.</param>
        public Reference( IEntity entity, string propertyName )
        {
            Type entityType = Helper.GetEntityType( entity );

            EntityType = entityType.FullName;
            Property = propertyName;

            if ( entity is EntityType )
            {
                Type = ReferenceType.EntityType;
                Data = ( ( EntityType ) entity ).Name;
            }
            else if ( entity is FieldType )
            {
                Type = ReferenceType.FieldType;
                Data = ( ( FieldType ) entity ).Class;
            }
            else
            {
                Type = ReferenceType.Guid;
                Data = entity.Guid;
            }
        }

        /// <summary>
        /// Restore this reference into the entity object.
        /// </summary>
        /// <param name="entity">The entity to restore the reference into.</param>
        /// <param name="helper">The helper that provides us data access.</param>
        public void Restore( IEntity entity, Helper helper )
        {
            PropertyInfo property = entity.GetType().GetProperty( Property );
            object otherEntity = null;

            if ( property == null || Type == ReferenceType.Null )
            {
                return;
            }

            //
            // Find the referenced entity based on the reference type. This whole section should
            // probably be moved into a method of Reference.
            //
            if ( Type == ReferenceType.Guid )
            {
                otherEntity = helper.GetExistingEntity( EntityType, helper.MapGuid( new Guid( ( string ) Data ) ) );
            }
            else if ( Type == ReferenceType.EntityType )
            {
                otherEntity = new EntityTypeService( helper.RockContext ).Queryable().Where( e => e.Name == ( string ) Data ).FirstOrDefault();
            }
            else if ( Type == ReferenceType.FieldType )
            {
                otherEntity = new FieldTypeService( helper.RockContext ).Queryable().Where( f => f.Class == ( string ) Data ).FirstOrDefault();
            }
            else
            {
                throw new Exception( string.Format( "Don't know how to handle reference type {0}.", Type ) );
            }

            //
            // If we found an entity then get its Id number and store that.
            //
            if ( otherEntity != null )
            {
                var idProperty = otherEntity.GetType().GetProperty( "Id" );

                if ( idProperty != null )
                {
                    property.SetValue( entity, Helper.ChangeType( property.PropertyType, idProperty.GetValue( otherEntity ) ) );
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// TODO: This needs to be updated to be JSON encodable. So it should probably have
    /// a Guid, friendly name, property name and children. The friendly name could be
    /// based on "Name" or "Value" property if one is available, otherwise "EntityType Guid"
    /// format.
    /// </summary>
    public class EntityReferenceTree
    {
        public string EntityType { get; private set; }
        public Guid Guid { get; private set; }
        public string Name { get; private set; }
        public string PropertyName { get; private set; }
        public List<EntityReferenceTree> Children { get; private set; }
        public bool IsCritical { get; set; }

        public EntityReferenceTree( IEntity entity, string propertyName, List<EntityReferenceTree> children )
        {
            EntityType = Helper.GetEntityType( entity ).FullName;
            Guid = entity.Guid;
            PropertyName = propertyName;
            Children = children;

            Name = entity.GetPropertyValue( "Name" ) as string;
            if ( Name == null )
            {
                Name = entity.GetPropertyValue( "Value" ) as string;
            }
        }

        /// <summary>
        /// Cleans the entire reference tree. Any leaf that is an ignored entity type and
        /// whose entire sub-tree is also of the ignored entity types is removed from the
        /// tree.
        /// </summary>
        /// <param name="ignoredEntityTypes">The list of entity types to ignore from the list.</param>
        public void CleanTree( List<string> ignoredEntityTypes )
        {
            foreach ( var c in Children.ToList() )
            {
                if ( c.TreeContainsOnlyTypes( ignoredEntityTypes ) )
                {
                    Children.Remove( c );
                }
                else
                {
                    c.CleanTree( ignoredEntityTypes );
                }
            }
        }

        /// <summary>
        /// Determines if this tree only contains entities from the list of types.
        /// </summary>
        /// <param name="types">The list of entity types that should be considered.</param>
        /// <returns>true if the tree, including this entity, contains only entities from the given list. Otherwise false.</returns>
        protected bool TreeContainsOnlyTypes( List<string> types )
        {
            if ( !types.Contains( EntityType ) )
            {
                return false;
            }

            foreach ( var c in Children )
            {
                if ( !c.TreeContainsOnlyTypes( types ) )
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// General container for exported entity data. This object should be encoded
    /// and decoded as JSON data.
    /// </summary>
    public class DataContainer
    {
        #region Properties

        /// <summary>
        /// The encoded entities that will be used to identify all the database
        /// entities that are to be recreated.
        /// </summary>
        public List<EncodedEntity> Entities { get; private set; }

        /// <summary>
        /// The Guid values of the root entities that were used when exporting.
        /// </summary>
        public List<Guid> RootEntities { get; private set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Create a new, empty, instance of the data container.
        /// </summary>
        public DataContainer()
        {
            Entities = new List<EncodedEntity>();
            RootEntities = new List<Guid>();
        }

        /// <summary>
        /// Check for any missing entity types that would be encountered during an import
        /// operation.
        /// </summary>
        /// <returns>A list of strings that identify the missing entity type class names.</returns>
        public List<string> MissingEntityTypes()
        {
            //
            // Ensure we know about all referenced entity types.
            //
            var missingTypes = new List<string>();

            //
            // Get all the explicit EntityTypes for entities that are to be imported.
            //
            var entityTypeStrings = Entities.Select( e => e.EntityType ).ToList();

            //
            // Check for GUID and EntityType references.
            //
            var references = Entities.SelectMany( e => e.References );

            entityTypeStrings.AddRange( references
                .Where( r => r.Type == ReferenceType.Guid )
                .Select( r => r.EntityType ) );

            entityTypeStrings.AddRange( references
                .Where( r => r.Type == ReferenceType.EntityType )
                .Select( r => ( string ) r.Data ) );

            //
            // Just check the unique ones.
            //
            entityTypeStrings = entityTypeStrings.Distinct().ToList();

            foreach ( var entityType in entityTypeStrings )
            {
                if ( EntityTypeCache.Get( entityType, false, null ) == null )
                {
                    missingTypes.Add( entityType );
                }
            }

            return missingTypes;
        }

        #endregion
    }

    /// <summary>
    /// Describes an Entity object in a portable manner that can be used
    /// to re-create the entity on another Rock installation.
    /// </summary>
    public class EncodedEntity
    {
        #region Properties

        /// <summary>
        /// The entity class name that we are describing.
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// The guid to use to check if this entity already exists.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Specifies if this entity should be allowed to get a new Guid during import.
        /// </summary>
        public bool NewGuid { get; set; }

        /// <summary>
        /// The values that describe the entities properties.
        /// </summary>
        public Dictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// Any processor transform data that is needed to re-create the entity.
        /// </summary>
        public Dictionary<string, object> Transforms { get; private set; }

        /// <summary>
        /// List of references that will be used to re-create inter-entity references.
        /// </summary>
        public List<Reference> References { get; private set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Create a new instance of an encoded entity.
        /// </summary>
        public EncodedEntity()
        {
            Properties = new Dictionary<string, object>();
            Transforms = new Dictionary<string, object>();
            References = new List<Reference>();
        }

        /// <summary>
        /// Replace a "by id" property that references another entity with a reference
        /// object that contains the information we will need to re-create that property
        /// at import time.
        /// </summary>
        /// <param name="originalProperty">The original property name that we are replacing.</param>
        /// <param name="entity">The entity that is being referenced.</param>
        public void MakeIntoReference( string originalProperty, IEntity entity )
        {
            Reference reference = new Reference( entity, originalProperty );

            References.Add( reference );
            Properties.Remove( originalProperty );
        }

        /// <summary>
        /// Get the transform value for the given processor.
        /// </summary>
        /// <param name="name">The full class name of the processor.</param>
        /// <returns>An object containing the data for the processor, or null if none was found.</returns>
        public object GetTransform( string name )
        {
            if ( Transforms.ContainsKey( name ) )
            {
                return Transforms[name];
            }

            return null;
        }

        /// <summary>
        /// Add a new transform object to this encoded entity. These are used by EntityProcessor
        /// implementations to facilitate in exporting and imported complex entities that need
        /// a little extra customization done to them.
        /// </summary>
        /// <param name="name">The name of the transform, this is the full class name of the processor.</param>
        /// <param name="value">The black box value for the transform.</param>
        public void AddTransform( string name, object value )
        {
            Transforms.Add( name, value );
        }

        #endregion
    }

    /// <summary>
    /// Interface for indicating that the inheriting class is a processor for
    /// export and import of entities from and to Rock.
    /// </summary>
    public interface IEntityProcessor
    {
        /// <summary>
        /// The unique identifier for this entity processor. This is used to identify the correct
        /// processor to use when importing so we match the one used during export.
        /// </summary>
        Guid Identifier { get; }

        /// <summary>
        /// Evaluate the list of referenced entities. This is a list of key value pairs that identify
        /// the property that the reference came from as well as the referenced entity itself. Implementations
        /// of this method may add or remove from this list. For example, an AttributeValue has
        /// the entity it is referencing in a EntityId column, but there is no general use information for
        /// what kind of entity it is. The processor can provide that information.
        /// </summary>
        /// <param name="entity">The parent entity of the references.</param>
        /// <param name="children">The referenced entities and what properties of the parent they came from.</param>
        /// <param name="helper">The helper class for this export.</param>
        void EvaluateReferencedEntities( IEntity entity, List<KeyValuePair<string, IEntity>> children, Helper helper );

        /// <summary>
        /// Evaluate the list of child entities. This is a list of key value pairs that identify
        /// the property that the child came from as well as the child entity itself. Implementations
        /// of this method may add or remove from this list. For example, a WorkflowActionForm has
        /// it's actions encoded in a single string. This should must processed to include any other
        /// objects that should exist (such as a DefinedValue for the button type).
        /// </summary>
        /// <param name="entity">The parent entity of the children.</param>
        /// <param name="children">The child entities and what properties of the parent they came from.</param>
        /// <param name="helper">The helper class for this export.</param>
        void EvaluateChildEntities( IEntity entity, List<KeyValuePair<string, IEntity>> children, Helper helper );

        /// <summary>
        /// An entity has been exported and can now have any post-processing done to it
        /// that is needed. For example a processor might remove some properties that shouldn't
        /// actually be exported.
        /// </summary>
        /// <param name="entity">The source entity that was exported.</param>
        /// <param name="encodedEntity">The exported data from the entity.</param>
        /// <param name="helper">The helper that is doing the exporting.</param>
        /// <returns>An object that will be encoded with the entity and passed to the ProcessImportEntity method later, or null.</returns>
        object PostProcessExportedEntity( IEntity entity, EncodedEntity encodedEntity, Helper helper );

        /// <summary>
        /// This method is called before the entity is saved and allows any final changes to the
        /// entity before it is stored in the database. Any Guid references that are not standard
        /// properties must also be updated, such as the Actions string of a WorkflowActionForm.
        /// </summary>
        /// <param name="entity">The in-memory entity that is about to be saved.</param>
        /// <param name="encodedEntity">The encoded information that was used to reconstruct the entity.</param>
        /// <param name="data">Custom data that was previously returned by ProcessExportedEntity.</param>
        /// <param name="helper">The helper in charge of the import process.</param>
        void PreProcessImportedEntity( IEntity entity, EncodedEntity encodedEntity, object data, Helper helper );

        /// <summary>
        /// This method is called after all entities have been imported. If there are any last
        /// minute changes to the entity that are needed to be made after other objects have been
        /// created then they may be made here. If any changes are made to the object then true
        /// must be returned to indicate a need to re-save the entity, otherwise return false.
        /// </summary>
        /// <param name="entity">The entity that now exists in the database.</param>
        /// <param name="encodedEntity">The encoded information that was used to reconstruct the entity.</param>
        /// <param name="data">Custom data that was previously returned by ProcessExportedEntity.</param>
        /// <param name="helper">The helper in charge of the import process.</param>
        /// <returns>true if the entity needs to be saved again, otherwise false.</returns>
        bool PostProcessImportedEntity( IEntity entity, EncodedEntity encodedEntity, object data, Helper helper );
    }

    /// <summary>
    /// Entity processors must inherit from this class to be able to provide
    /// custom processing capabilities.
    /// </summary>
    /// <typeparam name="T">The IEntity class type that this processor is for.</typeparam>
    public abstract class EntityProcessor<T> : IEntityProcessor where T : IEntity
    {
        abstract public Guid Identifier { get; }

        public void EvaluateReferencedEntities( IEntity entity, List<KeyValuePair<string, IEntity>> references, Helper helper )
        {
            EvaluateReferencedEntities( ( T ) entity, references, helper );
        }

        protected virtual void EvaluateReferencedEntities( T entity, List<KeyValuePair<string, IEntity>> references, Helper helper )
        {
        }


        public void EvaluateChildEntities( IEntity entity, List<KeyValuePair<string, IEntity>> children, Helper helper )
        {
            EvaluateChildEntities( ( T ) entity, children, helper );
        }

        protected virtual void EvaluateChildEntities( T entity, List<KeyValuePair<string, IEntity>> children, Helper helper )
        {
        }


        public object PostProcessExportedEntity( IEntity entity, EncodedEntity encodedEntity, Helper helper )
        {
            return PostProcessExportedEntity( ( T ) entity, encodedEntity, helper );
        }

        protected virtual object PostProcessExportedEntity( T entity, EncodedEntity encodedEntity, Helper helper )
        {
            return null;
        }


        public void PreProcessImportedEntity( IEntity entity, EncodedEntity encodedEntity, object data, Helper helper )
        {
            PreProcessImportedEntity( ( T ) entity, encodedEntity, data, helper );
        }

        public virtual void PreProcessImportedEntity( T entity, EncodedEntity encodedEntity, object data, Helper helper )
        {
        }


        public bool PostProcessImportedEntity( IEntity entity, EncodedEntity encodedEntity, object data, Helper helper )
        {
            return PostProcessImportedEntity( ( T ) entity, encodedEntity, data, helper );
        }

        public virtual bool PostProcessImportedEntity( T entity, EncodedEntity encodedEntity, object data, Helper helper )
        {
            return false;
        }
    }
}

namespace RockWeb.Plugins.com_shepherdchurch.Misc.Export.Processors
{
    class WorkflowTypeProcessor : EntityProcessor<WorkflowType>
    {
        public override Guid Identifier { get { return new Guid( "d924193f-bd22-4dd7-b203-4399673dcd32" ); } }

        protected override void EvaluateChildEntities( WorkflowType entity, List<KeyValuePair<string, IEntity>> children, Helper helper )
        {
            var attributeService = new AttributeService( helper.RockContext );

            var items = attributeService
                .GetByEntityTypeId( new Workflow().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( entity.Id.ToString() ) )
                .ToList();

            //
            // We have to special process the attributes since we modify them.
            //
            foreach ( var item in items )
            {
                children.Add( new KeyValuePair<string, IEntity>( "AttributeTypes", item ) );
            }
        }
    }

    class WorkflowActivityTypeProcessor : EntityProcessor<WorkflowActivityType>
    {
        public override Guid Identifier { get { return new Guid( "e67efa08-fd93-4278-83db-55397f8dc9f0" ); } }

        protected override void EvaluateChildEntities( WorkflowActivityType entity, List<KeyValuePair<string, IEntity>> children, Helper helper )
        {
            var attributeService = new AttributeService( helper.RockContext );

            var items = attributeService
                .GetByEntityTypeId( new WorkflowActivity().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ActivityTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( entity.Id.ToString() ) )
                .ToList();

            //
            // We have to special process the attributes since we modify them.
            //
            foreach ( var item in items )
            {
                children.Add( new KeyValuePair<string, IEntity>( "AttributeTypes", item ) );
            }
        }
    }

    class AttributeProcessor : EntityProcessor<Rock.Model.Attribute>
    {
        public override Guid Identifier { get { return new Guid( "67c72560-e047-495a-ad75-09ed920dae8a" ); } }

        protected override void EvaluateReferencedEntities( Rock.Model.Attribute entity, List<KeyValuePair<string, IEntity>> references, Helper helper )
        {
            int entityId;

            if ( entity.EntityTypeQualifierColumn != null && entity.EntityTypeQualifierColumn.EndsWith( "Id" ) && int.TryParse( entity.EntityTypeQualifierValue, out entityId ) )
            {
                var entityType = helper.FindEntityType( entity.EntityType.Name );

                if ( entityType != null )
                {
                    var property = entityType.GetProperty( entity.EntityTypeQualifierColumn.ReplaceLastOccurrence( "Id", string.Empty ) );

                    if ( property != null )
                    {
                        if ( typeof( IEntity ).IsAssignableFrom( property.PropertyType ) )
                        {
                            var target = helper.GetExistingEntity( property.PropertyType.FullName, entityId );
                            if ( target != null )
                            {
                                references.Add( new KeyValuePair<string, IEntity>( "EntityTypeQualifierValue", target ) );
                            }
                            else
                            {
                                throw new Exception( string.Format( "Could not find referenced qualifier of Attribute {0}", entity.Guid ) );
                            }
                        }
                    }
                }
            }
        }
    }

    class AttributeValueProcessor : EntityProcessor<AttributeValue>
    {
        public override Guid Identifier { get { return new Guid( "0c733598-46b1-4f59-854d-c5477dcfea17" ); } }

        protected override void EvaluateReferencedEntities( AttributeValue entity, List<KeyValuePair<string, IEntity>> references, Helper helper )
        {
            if ( entity.EntityId.HasValue && entity.Attribute != null )
            {
                var target = helper.GetExistingEntity( entity.Attribute.EntityType.Name, entity.EntityId.Value );
                if ( target != null )
                {
                    references.Add( new KeyValuePair<string, IEntity>( "EntityId", target ) );
                }
                else
                {
                    throw new Exception( string.Format( "Cannot export AttributeValue {0} because we cannot determine what entity it references.", entity.Guid ) );
                }
            }
        }
    }

    class WorkflowActionFormProcessor : EntityProcessor<WorkflowActionForm>
    {
        public override Guid Identifier { get { return new Guid( "5df81bd2-6d4e-438e-98a8-6e756b738264" ); } }

        protected override void EvaluateReferencedEntities( WorkflowActionForm entity, List<KeyValuePair<string, IEntity>> references, Helper helper )
        {
            List<string> actions = entity.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            for ( int i = 0; i < actions.Count; i++ )
            {
                var details = actions[i].Split( new char[] { '^' } );
                if ( details.Length > 2 )
                {
                    Guid definedValueGuid = details[1].AsGuid();
                    IEntity definedValue = helper.GetExistingEntity( "Rock.Model.DefinedValue", definedValueGuid );

                    if ( definedValue != null )
                    {
                        references.Add( new KeyValuePair<string, IEntity>( "Actions", definedValue ) );
                    }
                }
            }
        }

        protected override object PostProcessExportedEntity( WorkflowActionForm entity, EncodedEntity encodedEntity, Helper helper )
        {
            return entity.Actions;
        }

        public override void PreProcessImportedEntity( WorkflowActionForm entity, EncodedEntity encodedEntity, object data, Helper helper )
        {
            if ( data != null && data is string )
            {
                //
                // Update the Guids in all the action buttons.
                //
                List<string> actions = ( ( string ) data ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                for ( int i = 0; i < actions.Count; i++ )
                {
                    var details = actions[i].Split( new char[] { '^' } );
                    if ( details.Length > 2 )
                    {
                        Guid definedValueGuid = details[1].AsGuid();
                        Guid? activityTypeGuid = details[2].AsGuidOrNull();

                        details[1] = helper.MapGuid( definedValueGuid ).ToString();
                        if ( activityTypeGuid.HasValue )
                        {
                            details[2] = helper.MapGuid( activityTypeGuid.Value ).ToString();
                        }

                        actions[i] = string.Join( "^", details );
                    }
                }

                entity.Actions = string.Join( "|", actions );
            }
        }
    }
}