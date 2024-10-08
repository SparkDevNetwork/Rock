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
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;

using Rock.Bus.Message;
using Rock.Model;
using Rock.Net;
using Rock.Tasks;
using Rock.Transactions;
using Rock.UniversalSearch;
using Rock.Web.Cache;

using Z.EntityFramework.Plus;

using Audit = Rock.Model.Audit;

namespace Rock.Data
{
    /// <summary>
    /// Entity Framework Context
    /// </summary>
    public abstract class DbContext : System.Data.Entity.DbContext
    {
        #region Properties

        /// <summary>
        /// Gets or sets the entity save hook provider.
        /// </summary>
        /// <value>
        /// The entity save hook provider.
        /// </value>
        internal Internal.EntitySaveHookProvider EntitySaveHookProvider { get; set; } = SharedSaveHookProvider;

        /// <summary>
        /// Gets any error messages that occurred during a SaveChanges
        /// </summary>
        /// <value>
        /// The save error messages.
        /// </value>
        public virtual List<string> SaveErrorMessages { get; private set; }

        /// <summary>
        /// Gets or sets the source of change. If the source of change is set then changes made to entities with this context will have History records marked with this Source of Change.
        /// </summary>
        /// <value>
        /// The source of change.
        /// </value>
        public string SourceOfChange { get; set; }

        /// <summary>
        /// If <see cref="WrapTransaction(Action)"/> is in progress, this will return a task that will return completed
        /// after the transaction is committed. Otherwise, it will return a completed task immediately.
        /// </summary>
        /// <value>
        /// The wrapped transaction completed.
        /// </value>
        public Task<bool> WrappedTransactionCompletedTask
        {
            get
            {
                if ( _transactionInProgress )
                {
                    return _wrappedTransactionCompleted?.Task ?? Task.FromResult( true );
                }
                else
                {
                    return Task.FromResult( true );
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Rock RealTime messages
        /// should be sent in response to calls to one of the SaveChanges
        /// methods.
        /// </summary>
        /// <remarks>
        /// This <em>only</em> affects real-time messages. Other forms of
        /// notifications will still take place.
        /// </remarks>
        /// <value><c>true</c> if RealTime messages should be sent by this context; otherwise, <c>false</c>.</value>
        public bool IsRealTimeEnabled { get; set; } = true;

        #endregion

        #region Fields

        /// <summary>
        /// The shared save hook provider that is used by default by all
        /// instances of DbContext.
        /// </summary>
        internal static readonly Internal.EntitySaveHookProvider SharedSaveHookProvider = new Internal.EntitySaveHookProvider();

        /// <summary>
        /// Is there a transaction in progress?
        /// </summary>
        private bool _transactionInProgress = false;
        private TaskCompletionSource<bool> _wrappedTransactionCompleted = null;

        /// <summary>
        /// A list of action delegates to execute once the data has been committed
        /// to the database.
        /// </summary>
        private List<Action> _commitedActions = new List<Action>();

        /// <summary>
        /// The options currently tracked by this context instance. Options
        /// can contain anything and are made unique by their Type. Meaning,
        /// only one instance of a given Type can exist in the options.
        /// </summary>
        private readonly Dictionary<Type, object> _options = new Dictionary<Type, object>();

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        public DbContext() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        public DbContext( string nameOrConnectionString ) : base( nameOrConnectionString ) { }

        /// <inheritdoc />
        internal protected DbContext( ObjectContext objectContext, bool dbContextOwnsObjectContext ) :
            base( objectContext, dbContextOwnsObjectContext )
        {
        }


        /// <summary>
        /// Wraps the action in a BeginTransaction and CommitTransaction.
        /// Note that this will *always* commit the transaction (unless an exception occurs).
        /// If need to rollback the transaction within your action (for example, to show a validation warning),
        /// use <see cref="WrapTransactionIf(Func{bool})" /> instead.
        /// </summary>
        /// <param name="action">The action.</param>
        public void WrapTransaction( Action action )
        {
            WrapTransactionIf( () =>
            {
                action.Invoke();
                return true;
            } );
        }

        /// <summary>
        /// Wraps code in a BeginTransaction and CommitTransaction.
        /// If the action returns false, the transaction will be rolled back.
        /// </summary>
        /// <param name="action">The action.</param>
        public bool WrapTransactionIf( Func<bool> action )
        {
            if ( !_transactionInProgress )
            {
                _transactionInProgress = true;
                _wrappedTransactionCompleted = new TaskCompletionSource<bool>();

                using ( var dbContextTransaction = this.Database.BeginTransaction() )
                {
                    try
                    {
                        if ( action.Invoke() )
                        {
                            dbContextTransaction.Commit();
                            _wrappedTransactionCompleted.SetResult( true );

                            ExecuteAfterCommitActions();
                        }
                        else
                        {
                            dbContextTransaction.Rollback();
                            _wrappedTransactionCompleted.SetResult( false );
                            return false;
                        }
                    }
                    catch
                    {
                        dbContextTransaction.Rollback();
                        _wrappedTransactionCompleted.SetResult( false );
                        throw;
                    }
                    finally
                    {
                        _wrappedTransactionCompleted = null;
                        _transactionInProgress = false;
                    }
                }

                return true;
            }
            else
            {
                return action.Invoke();
            }
        }

        /// <summary>
        /// Executes the action delegate method after the changes have been
        /// commited to the database. If there is no transaction this is just
        /// before SaveChanges() returns. If there is a transaction then it
        /// is just before WrapTransaction returns.
        /// </summary>
        /// <remarks>
        /// Because this action will be called after the commit has completed
        /// this <see cref="RockContext"/> should be considered invalid by
        /// the action delegate method. Meaning, create your own context.
        /// </remarks>
        /// <param name="action">The action delegate to execute after the changes have been committed.</param>
        internal void ExecuteAfterCommit( Action action )
        {
            _commitedActions.Add( action );
        }

        /// <summary>
        /// Executes all the "after-commit" action delegate methods that have
        /// been registered.
        /// </summary>
        private void ExecuteAfterCommitActions()
        {
            // An executed action might not know that it is already in the after
            // commit state and try to enqueue another ExecuteAfterCommit call.
            // This will allow us to catch those and execute them.
            while ( _commitedActions.Any() )
            {
                // Create a new array for committed actions. This is so that if
                // some action registers yet another action then it will go into
                // the new queue rather than cause an enumeration error.
                var actions = _commitedActions;
                _commitedActions = new List<Action>();

                foreach ( var action in actions )
                {
                    try
                    {
                        action();
                    }
                    catch ( Exception ex )
                    {
                        // Log but do not throw, this ensures all commit
                        // actions get executed.
                        ExceptionLogService.LogException( ex );
                    }
                }
            }
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        public override int SaveChanges()
        {
            return SaveChanges( false );
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.  The
        /// default pre and post processing can also optionally be disabled.  This
        /// would disable audit records being created, workflows being triggered, and
        /// any PreSaveChanges() methods being called for changed entities.
        /// </summary>
        /// <param name="disablePrePostProcessing">if set to <c>true</c> disables
        /// the Pre and Post processing from being run. This should only be disabled
        /// when updating a large number of records at a time (e.g. importing records).</param>
        /// <returns></returns>
        public virtual int SaveChanges( bool disablePrePostProcessing )
        {
            var result = SaveChanges( new SaveChangesArgs
            {
                DisablePrePostProcessing = disablePrePostProcessing
            } );

            return result.RecordsUpdated;
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.  The
        /// default pre and post processing can also optionally be disabled.  This
        /// would disable audit records being created, workflows being triggered, and
        /// any PreSaveChanges() methods being called for changed entities.
        /// </summary>
        /// <param name="args">Arguments determining behavior of the save.</param>
        /// <returns></returns>
        public virtual SaveChangesResult SaveChanges( SaveChangesArgs args )
        {
            var saveChangesResult = new SaveChangesResult();

            // Pre and Post processing has been disabled, just call the base
            // SaveChanges() method and return
            if ( args.DisablePrePostProcessing )
            {
                saveChangesResult.RecordsUpdated = SaveChangesInternal();
                return saveChangesResult;
            }

            SaveErrorMessages = new List<string>();

            // Try to get the current person alias and id
            PersonAlias personAlias = GetCurrentPersonAlias();

            bool enableAuditing = GlobalAttributesCache.Value( "EnableAuditing" ).AsBoolean();

            // Evaluate the current context for items that have changes
            var updatedItems = RockPreSave( this, personAlias, enableAuditing );

            // If update was not cancelled by triggered workflow
            if ( updatedItems != null )
            {
                try
                {
                    // Save the context changes
                    saveChangesResult.RecordsUpdated = SaveChangesInternal();
                }
                catch
                {
                    // Let all the hooks that were called know that the save
                    // was aborted.
                    CallSaveFailedHooks( updatedItems );

                    throw;
                }

                // If any items changed process audit and triggers
                if ( updatedItems.Any() )
                {
                    RockPostSave( updatedItems, personAlias, enableAuditing );

                    if ( args.IsAchievementsEnabled )
                    {
                        var attempts = ProcessAchievements( updatedItems );
                        saveChangesResult.AchievementAttempts = attempts;
                    }
                }

                if ( !_transactionInProgress )
                {
                    ExecuteAfterCommitActions();
                }
            }

            return saveChangesResult;
        }

        /// <summary>
        /// Save changes to the context, and capture additional details for any Entity Framework validation errors.
        /// </summary>
        /// <returns></returns>
        private int SaveChangesInternal()
        {
            try
            {
                // Save the context changes
                return base.SaveChanges();
            }
            catch ( System.Data.Entity.Validation.DbEntityValidationException ex )
            {
                // This exception stores specific validation messages in a custom property.
                // These messages are often useful for debugging purposes, so we will repackage the exception
                // to include the additional information in the standard error message.
                var validationErrors = new List<string>();
                foreach ( var error in ex.EntityValidationErrors )
                {
                    var entry = error.Entry;
                    var entityType = entry.Entity.GetType();
                    if ( entityType.IsDynamicProxyType() )
                    {
                        entityType = entityType.BaseType;
                    }

                    var entityDescription = $"{entityType.Name}/{entry.State}";

                    if ( error.Entry.Entity is IEntity entity )
                    {
                        entityDescription += $"/Id={entity.Id}";
                    }

                    foreach ( var prop in error.ValidationErrors )
                    {
                        validationErrors.Add( $"[{entityDescription}/Property={prop.PropertyName}] {prop.ErrorMessage}" );
                    }
                }

                throw new SystemException( $"Entity Validation Error: {validationErrors.AsDelimited( "; " )}" );
            }
        }

        /// <summary>
        /// Gets the current person alias.
        /// </summary>
        /// <returns></returns>
        internal PersonAlias GetCurrentPersonAlias()
        {
            if ( HttpContext.Current != null && HttpContext.Current.Items.Contains( "CurrentPerson" ) )
            {
                var currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
                if ( currentPerson != null && currentPerson.PrimaryAlias != null )
                {
                    return currentPerson.PrimaryAlias;
                }
            }

            return null;
        }

        /// <summary>
        /// Updates the Created/Modified data for any model being created or modified
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="enableAuditing">if set to <c>true</c> [enable auditing].</param>
        /// <returns></returns>
        protected virtual List<ContextItem> RockPreSave( DbContext dbContext, PersonAlias personAlias, bool enableAuditing = false )
        {
            int? personAliasId = null;
            if ( personAlias != null )
            {
                personAliasId = personAlias.Id;
            }

            // This triggers the change detection, so it must be called before
            // we check for the implied relationship changes.
            var entries = dbContext.ChangeTracker.Entries().ToList();

            // Check for any many-to-many relationships that were added or
            // deleted and flag the related entities as modified.
            DetectImpliedRelationshipChanges( dbContext );

            // First loop through all models calling the PreSaveChanges
            var preSavedEntities = new HashSet<Guid>();
            var updatedItems = new Dictionary<IEntity, ContextItem>();

            try
            {
                foreach ( var entry in entries
                    .Where( c =>
                        c.Entity is IEntity &&
                        ( c.State == EntityState.Added || c.State == EntityState.Modified || c.State == EntityState.Deleted ) ) )
                {
                    var entity = entry.Entity as IEntity;

                    // Get the context item to track audits
                    var contextItem = new ContextItem( entity, this, entry, enableAuditing );
                    updatedItems.Add( entity, contextItem );

                    contextItem.InProcessSaveHooks = new List<IEntitySaveHook>();

                    var saveHooks = EntitySaveHookProvider.GetHooksForEntityType( entity.GetType() ).ToList();
                    foreach ( var hook in saveHooks )
                    {
                        hook.PreSave( contextItem );
                        contextItem.InProcessSaveHooks.Add( hook );
                    }

                    if ( entry.Entity is IModel )
                    {
                        var model = entry.Entity as IModel;
                        model.PreSaveChanges( this, entry, entry.State );

                        if ( !preSavedEntities.Contains( model.Guid ) )
                        {
                            preSavedEntities.Add( model.Guid );
                        }
                    }
                }

                // Then loop again, as new models may have been added by PreSaveChanges events
                foreach ( var entry in dbContext.ChangeTracker.Entries()
                    .Where( c =>
                        c.Entity is IEntity &&
                        ( c.State == EntityState.Added || c.State == EntityState.Modified || c.State == EntityState.Deleted ) ) )
                {
                    // Cast entry as IEntity
                    var entity = entry.Entity as IEntity;

                    if ( !updatedItems.TryGetValue( entity, out var contextItem ) )
                    {
                        // Entity was added by a previous save hook.
                        contextItem = new ContextItem( entity, this, entry, enableAuditing );
                        updatedItems.Add( entity, contextItem );

                        contextItem.InProcessSaveHooks = new List<IEntitySaveHook>();

                        var saveHooks = EntitySaveHookProvider.GetHooksForEntityType( entity.GetType() ).ToList();
                        foreach ( var hook in saveHooks )
                        {
                            hook.PreSave( contextItem );
                            contextItem.InProcessSaveHooks.Add( hook );
                        }
                    }

                    // If entity was added or modified, update the Created/Modified fields
                    if ( entry.State == EntityState.Added || entry.State == EntityState.Modified )
                    {
                        // instead of passing "true" the trigger model and UI would support a
                        // on-value-changed checkbox (or perhaps it should be the default/only behavior)
                        // and its value would be passed in to the onValueChange
                        if ( !TriggerWorkflows( contextItem, WorkflowTriggerType.PreSave, personAlias ) )
                        {
                            // If any workflow has aborted the save operation
                            // then let all the save hooks know the save was
                            // aborted.
                            CallSaveFailedHooks( updatedItems.Values );

                            return null;
                        }

                        if ( entry.Entity is IModel )
                        {
                            var model = entry.Entity as IModel;

                            if ( !preSavedEntities.Contains( model.Guid ) )
                            {
                                model.PreSaveChanges( this, entry );
                            }

                            // Update Guid/Created/Modified person and times
                            if ( entry.State == EntityState.Added )
                            {
                                if ( !model.CreatedDateTime.HasValue )
                                {
                                    model.CreatedDateTime = RockDateTime.Now;
                                }
                                if ( !model.CreatedByPersonAliasId.HasValue )
                                {
                                    model.CreatedByPersonAliasId = personAliasId;
                                }

                                if ( model.Guid == Guid.Empty )
                                {
                                    model.Guid = Guid.NewGuid();
                                }

                                model.ModifiedDateTime = RockDateTime.Now;

                                if ( !model.ModifiedAuditValuesAlreadyUpdated || model.ModifiedByPersonAliasId == null )
                                {
                                    model.ModifiedByPersonAliasId = personAliasId;
                                }
                            }
                            else if ( entry.State == EntityState.Modified )
                            {
                                model.ModifiedDateTime = RockDateTime.Now;

                                if ( !model.ModifiedAuditValuesAlreadyUpdated || model.ModifiedByPersonAliasId == null )
                                {
                                    model.ModifiedByPersonAliasId = personAliasId;
                                }
                            }
                        }
                    }
                    else if ( entry.State == EntityState.Deleted )
                    {
                        if ( !TriggerWorkflows( contextItem, WorkflowTriggerType.PreDelete, personAlias ) )
                        {
                            // Let all the hooks that were called know that
                            // the save was aborted.
                            CallSaveFailedHooks( updatedItems.Values );
                            return null;
                        }

                        /*
                             11/18/2021 - SK
    
                             Reason: It may look irrelevant to update the ModifiedByPersonAliasId and ModifiedDateTime here but
                             this play vital role in displaying the Who column in history summary.
                        */
                        if ( entry.Entity is IModel )
                        {
                            var model = entry.Entity as IModel;
                            model.ModifiedDateTime = RockDateTime.Now;
                            if ( !model.ModifiedAuditValuesAlreadyUpdated || model.ModifiedByPersonAliasId == null )
                            {
                                model.ModifiedByPersonAliasId = personAliasId;
                            }
                        }
                    }

                    if ( enableAuditing )
                    {
                        try
                        {
                            GetAuditDetails( dbContext, contextItem, personAliasId );
                        }
                        catch ( SystemException ex )
                        {
                            contextItem.Audit = null;
                            System.Diagnostics.Debug.WriteLine( $"Exception when getting Audit details for {contextItem?.GetType().Name} - {ex}" );
                            ExceptionLogService.LogException( ex, null );
                        }
                    }
                }
            }
            catch
            {
                // If any errors happened let all the hooks that were called
                // know that the save was aborted.
                CallSaveFailedHooks( updatedItems.Values );

                throw;
            }

            return updatedItems.Values.ToList();
        }

        /// <summary>
        /// Creates audit logs and/or triggers workflows for items that were changed
        /// </summary>
        /// <param name="updatedItems">The updated items.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="enableAuditing">if set to <c>true</c> [enable auditing].</param>
        protected virtual void RockPostSave( List<ContextItem> updatedItems, PersonAlias personAlias, bool enableAuditing = false )
        {
            // Triggers when the post-save actions have completed.
            var tcsPostSave = new TaskCompletionSource<bool>();

            if ( enableAuditing )
            {
                var audits = updatedItems
                    .Where( a => a.Audit?.Details?.Any() == true )
                    .Select( i => i.Audit )
                    .ToList();

                if ( audits.Any() )
                {
                    Task.Run( async () =>
                    {
                        // Wait for all post-save tasks to complete.
                        await tcsPostSave.Task;

                        try
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                var auditService = new AuditService( rockContext );
                                auditService.AddRange( audits );
                                rockContext.SaveChanges( true );
                            }
                        }
                        catch ( SystemException ex )
                        {
                            ExceptionLogService.LogException( ex, null );
                        }
                    } );
                }
            }

            try
            {
                foreach ( var item in updatedItems )
                {
                    // Publish on the message bus if the entity type is configured
                    EntityWasUpdatedMessage.PublishIfShould( item.Entity, item.PreSaveStateLegacy );

                    if ( item.State == EntityContextState.Detached || item.State == EntityContextState.Deleted )
                    {
                        TriggerWorkflows( item, WorkflowTriggerType.PostDelete, personAlias );
                    }
                    else
                    {
                        if ( item.PreSaveState == EntityContextState.Added )
                        {
                            TriggerWorkflows( item, WorkflowTriggerType.PostAdd, personAlias );
                        }

                        TriggerWorkflows( item, WorkflowTriggerType.ImmediatePostSave, personAlias );
                        TriggerWorkflows( item, WorkflowTriggerType.PostSave, personAlias );
                    }

                    if ( item.Entity is IEntity entity )
                    {
                        // If this is an entity and it was modified, check if any
                        // attributes (and values) should now be considered dirty.
                        if ( item.PreSaveState == EntityContextState.Modified )
                        {
                            var dependantAttributeIds = AttributeCache.GetDirtyAttributeIdsForPropertyChange( entity.TypeId, () => item.ModifiedProperties );

                            if ( dependantAttributeIds.Any() )
                            {
                                ExecuteAfterCommit( () =>
                                {
                                    using ( var rockContext = new RockContext() )
                                    {
                                        Rock.Attribute.Helper.UpdateDependantAttributesAndValues( dependantAttributeIds, entity.TypeId, entity.Id, rockContext );
                                    }
                                } );
                            }
                        }

                        // If this is a model and it was deleted, check if any
                        // sttributes (and values) should now be considered dirty.
                        if ( item.PreSaveState == EntityContextState.Deleted )
                        {
                            // No need to check modified properties, assume any attribute
                            // value that references this entity needs to be updated.
                            ExecuteAfterCommit( () =>
                            {
                                using ( var rockContext = new RockContext() )
                                {
                                    Rock.Attribute.Helper.UpdateDependantAttributesAndValues( null, entity.TypeId, entity.Id, rockContext );
                                }
                            } );
                        }
                    }

                    if ( item.Entity is IModel model )
                    {
                        ExecuteAfterCommit( () =>
                        {
                            model.PostSaveChanges( this );
                        } );
                    }
                }
            }
            finally
            {
                // At this point, even if a workflow trigger fails or a legacy
                // PostSaveChanges() call fails, the save still worked so call
                // all post save hooks with success state.
                ExecuteAfterCommit( () =>
                {
                    CallPostSaveHooks( updatedItems );
                    tcsPostSave.SetResult( true );
                } );
            }

            List<ITransaction> indexTransactions = new List<ITransaction>();
            var deleteContentCollectionIndexingMsgs = new List<BusStartedTaskMessage>();
            var addInteractionEntityTransactions = new List<AddInteractionEntityTransaction>();
            var interactionGuid = RockRequestContextAccessor.Current?.RelatedInteractionGuid;

            foreach ( var item in updatedItems )
            {
                // check if this entity should be passed on for indexing
                if ( item.Entity is IRockIndexable )
                {
                    if ( item.State == EntityContextState.Detached || item.State == EntityContextState.Deleted )
                    {
                        DeleteIndexEntityTransaction deleteIndexEntityTransaction = new DeleteIndexEntityTransaction
                        {
                            EntityTypeId = item.Entity.TypeId,
                            EntityId = item.Entity.Id
                        };

                        indexTransactions.Add( deleteIndexEntityTransaction );
                    }
                    else
                    {
                        var indexEntityTransaction = new IndexEntityTransaction(
                            new EntityIndexInfo
                            {
                                EntityTypeId = item.Entity.TypeId,
                                EntityId = item.Entity.Id
                            } );

                        indexTransactions.Add( indexEntityTransaction );
                    }
                }

                var itemEntityTypeCache = EntityTypeCache.Get( item.Entity.TypeId );

                // Check if this item should be processed by the content collection.
                if ( itemEntityTypeCache != null && itemEntityTypeCache.IsContentCollectionIndexingEnabled )
                {
                    // We only handle deleted states here. The detail blocks where
                    // an entity is edited should send the bus message to update
                    // the index when an item is saved. The job will catch anything
                    // else that is missed.
                    if ( item.State == EntityContextState.Detached || item.State == EntityContextState.Deleted )
                    {
                        var msg = new DeleteContentCollectionDocument.Message
                        {
                            EntityTypeId = item.Entity.TypeId,
                            EntityId = item.Entity.Id
                        };

                        deleteContentCollectionIndexingMsgs.Add( msg );
                    }
                }

                if ( item.Entity is ICacheable cacheable )
                {
                    /* 04/14/2022 MDP

                     If we are in WrapTransaction, some other thread could update the cached item from the
                     database before we have committed the transaction. That could cause the cache to have the
                     previous value instead of the new value. To prevent that from happening,
                     we'll use the ContinueWith on WrappedTransactionCompletedTask take care of flushing the
                     cache after the data is committed to the database.

                     Using the TaskContinuationOptions.ExecuteSynchronously option so that it runs in the same thread
                     as WrapTransaction.

                    */

                    WrappedTransactionCompletedTask.ContinueWith( ( task ) =>
                    {
                        var commitedSuccessfully = task.Result;
                        if ( commitedSuccessfully )
                        {
                            using ( var rockContextUpdateCache = new RockContext() )
                            {
                                cacheable.UpdateCache( item.PreSaveStateLegacy, rockContextUpdateCache );
                            }
                        };
                    }, TaskContinuationOptions.ExecuteSynchronously );
                }

                // If we are supposed to track this entity and the Interaction
                // that it's creation is related to then prepare the transaction
                // that will be queued up later.
                if ( interactionGuid.HasValue && itemEntityTypeCache?.IsRelatedToInteractionTrackedOnCreate == true && item.PreSaveState == EntityContextState.Added )
                {
                    var transaction = new AddInteractionEntityTransaction( itemEntityTypeCache.Id, item.Entity.Id, interactionGuid.Value );

                    addInteractionEntityTransactions.Add( transaction );
                }
            }

            // check if Indexing is enabled in another thread to avoid deadlock when Snapshot Isolation is turned off when the Index components upload/load attributes
            if ( indexTransactions.Any() || deleteContentCollectionIndexingMsgs.Any() )
            {
                System.Threading.Tasks.Task.Run( () =>
                {
                    var indexingEnabled = IndexContainer.GetActiveComponent() != null;
                    if ( indexingEnabled )
                    {
                        indexTransactions.ForEach( t => t.Enqueue() );
                        deleteContentCollectionIndexingMsgs.ForEach( t => t.SendWhen( WrappedTransactionCompletedTask ) );
                    }
                } );
            }

            // If we had any InteractionEntity transactions to process then
            // queue them up now.
            if ( addInteractionEntityTransactions.Any() )
            {
                ExecuteAfterCommit( () =>
                {
                    addInteractionEntityTransactions.ForEach( t => t.Enqueue() );
                } );
            }
        }

        /// <summary>
        /// Calls <see cref="IEntitySaveHook.PostSave(IEntitySaveEntry)"/> on
        /// all hooks that had their <see cref="IEntitySaveHook.PreSave(IEntitySaveEntry)"/>
        /// method called successfully already.
        /// </summary>
        /// <param name="contextItems">The context items.</param>
        private void CallPostSaveHooks( IEnumerable<ContextItem> contextItems )
        {
            foreach ( var contextItem in contextItems )
            {
                foreach ( var hook in contextItem.InProcessSaveHooks )
                {
                    try
                    {
                        hook.PostSave( contextItem );
                    }
                    catch ( Exception ex )
                    {
                        // Intentionally ignored, this is cleanup so if one
                        // thing fails to clean up don't let that affect the
                        // rest of the cleanup.
                        ExceptionLogService.LogException( ex, null );
                    }
                }
            }
        }

        /// <summary>
        /// Calls <see cref="IEntitySaveHook.SaveFailed(IEntitySaveEntry)"/> on
        /// all hooks that had their <see cref="IEntitySaveHook.PreSave(IEntitySaveEntry)"/>
        /// method called successfully already.
        /// </summary>
        /// <param name="contextItems">The context items.</param>
        private void CallSaveFailedHooks( IEnumerable<ContextItem> contextItems )
        {
            foreach ( var contextItem in contextItems )
            {
                foreach ( var hook in contextItem.InProcessSaveHooks )
                {
                    try
                    {
                        hook.SaveFailed( contextItem );
                    }
                    catch
                    {
                        // Intentionally ignored, this is cleanup so if one
                        // thing fails to clean up don't let that affect the
                        // rest of the cleanup.
                    }
                }
            }
        }

        /// <summary>
        /// Processes achievements, checking if any of the recently updated items (this is post save) caused any <see cref="AchievementType"/> progress.
        /// </summary>
        /// <param name="updatedItems">The updated items.</param>
        private List<AchievementAttempt> ProcessAchievements( List<ContextItem> updatedItems )
        {
            var updatedAttempts = new Dictionary<int, AchievementAttempt>();

            foreach ( var item in updatedItems )
            {
                var loopUpdatedAttempts = ProcessAchievements( item );

                foreach ( var attempt in loopUpdatedAttempts )
                {
                    updatedAttempts[attempt.Id] = attempt;
                }
            }

            return updatedAttempts.Values.ToList();
        }

        /// <summary>
        /// Processes achievements, checking if any of the recently updated items (this is post save) caused any <see cref="AchievementType"/> progress.
        /// </summary>
        /// <param name="updatedItem">The updated item.</param>
        private List<AchievementAttempt> ProcessAchievements( ContextItem updatedItem )
        {
            if ( updatedItem == null )
            {
                return new List<AchievementAttempt>();
            }

            if ( updatedItem.State == EntityContextState.Detached || updatedItem.State == EntityContextState.Deleted || updatedItem.Entity == null )
            {
                return new List<AchievementAttempt>();
            }

            return AchievementTypeCache.ProcessAchievements( updatedItem.Entity );
        }

        /// <summary>
        /// Gets the options object for the specified type. If it is not found
        /// in this context then <c>null</c> is returned.
        /// </summary>
        /// <typeparam name="T">The type of the options object.</typeparam>
        /// <returns>An instance of <typeparamref name="T"/> if it was found on the context, otherwise <c>null</c>.</returns>
        public T GetOptions<T>()
            where T : class
        {
            if ( _options.TryGetValue( typeof( T ), out var options ) )
            {
                return ( T ) options;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the options object for the specified type. If it is not found
        /// in this context a new instance of it will be created and added to
        /// the context.
        /// </summary>
        /// <typeparam name="T">The type of the options object.</typeparam>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        public T GetOrCreateOptions<T>()
            where T : class, new()
        {
            var options = GetOptions<T>();

            if ( options != null )
            {
                return options;
            }

            options = new T();

            AddOrReplaceOptions( options );

            return options;
        }

        /// <summary>
        /// Adds or replaces an existing options object of the given type. If
        /// the context already has an options object a type of
        /// <typeparamref name="T"/> then it will be replaced.
        /// </summary>
        /// <typeparam name="T">The type of the options object.</typeparam>
        /// <param name="options">The options to add to the context.</param>
        public void AddOrReplaceOptions<T>( T options )
            where T : class
        {
            _options.AddOrReplace( typeof( T ), options );
        }

        #region Bulk Operations

        /// <summary>
        /// Use SqlBulkInsert to quickly insert a large number records.
        /// NOTE: This bypasses the Rock and a bunch of the EF Framework and automatically commits the changes to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records">The records.</param>
        /// <param name="canUseCache">if set to <c>true</c> [can use cache]. Set false for migrations.</param>
        public virtual void BulkInsertWithConditionalCacheUse<T>( IEnumerable<T> records, bool canUseCache ) where T : class
        {
            // This logic is normally handled in the SaveChanges method, but since the BulkInsert bypasses those
            // model hooks, achievements need to be updated here. Also, it is not necessary for this logic to complete before this
            // transaction can continue processing and exit.
            var entitiesForAchievements = new List<IEntity>();

            bool isAchievementsEnabled = false;

            if ( canUseCache )
            {
                var entityType = EntityTypeCache.Get<T>();
                if ( entityType != null )
                {
                    isAchievementsEnabled = entityType.IsAchievementsEnabled == true
                        && AchievementTypeCache.HasActiveAchievementTypesForEntityTypeId( entityType.Id );
                }
            }

            // ensure CreatedDateTime and ModifiedDateTime is set
            var currentDateTime = RockDateTime.Now;
            var currentPersonAliasId = this.GetCurrentPersonAlias()?.Id;

            foreach ( var record in records )
            {
                var model = record as IModel;
                if ( model != null )
                {
                    model.CreatedDateTime = model.CreatedDateTime ?? currentDateTime;
                    model.ModifiedDateTime = model.ModifiedDateTime ?? currentDateTime;

                    if ( currentPersonAliasId.HasValue )
                    {
                        model.CreatedByPersonAliasId = model.CreatedByPersonAliasId ?? currentPersonAliasId;
                        model.ModifiedByPersonAliasId = model.ModifiedByPersonAliasId ?? currentPersonAliasId;
                    }

                    if ( isAchievementsEnabled )
                    {
                        entitiesForAchievements.Add( model );
                    }
                }
            }

            // if the CommandTimeout is less than 5 minutes (or null with a default of 30 seconds), set timeout to 5 minutes
            int minTimeout = 300;
            if ( this.Database.CommandTimeout.HasValue && this.Database.CommandTimeout.Value > minTimeout )
            {
                EntityFramework.Utilities.Configuration.BulkCopyTimeout = this.Database.CommandTimeout.Value;
            }
            else
            {
                EntityFramework.Utilities.Configuration.BulkCopyTimeout = minTimeout;
            }

            EntityFramework.Utilities.Configuration.SqlBulkCopyOptions = System.Data.SqlClient.SqlBulkCopyOptions.CheckConstraints;
            EntityFramework.Utilities.EFBatchOperation.For( this, this.Set<T>() ).InsertAll( records );

            // Send the achievements messages
            foreach ( var entityForAchievement in entitiesForAchievements )
            {
                new ProcessAchievements.Message
                {
                    EntityGuid = entityForAchievement.Guid,
                    EntityTypeName = entityForAchievement.TypeName
                }.Send();
            }
        }

        /// <summary>
        /// Use SqlBulkInsert to quickly insert a large number records.
        /// NOTE: This bypasses the Rock and a bunch of the EF Framework and automatically commits the changes to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records">The records.</param>
        public virtual void BulkInsert<T>( IEnumerable<T> records ) where T : class
        {
            BulkInsertWithConditionalCacheUse( records, true );
        }

        /// <summary>
        /// Does a direct bulk UPDATE.
        /// Example: rockContext.BulkUpdate( personQuery, p => new Person { LastName = "Decker" } );
        /// NOTE: This bypasses the Rock and a bunch of the EF Framework and automatically commits the changes to the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">The queryable for the records to update</param>
        /// <param name="updateFactory">Linq expression to specify the updated property values</param>
        /// <returns>the number of records updated</returns>
        public virtual int BulkUpdate<T>( IQueryable<T> queryable, Expression<Func<T, T>> updateFactory ) where T : class
        {
            var currentDateTime = RockDateTime.Now;
            PersonAlias currentPersonAlias = this.GetCurrentPersonAlias();
            var rockExpressionVisitor = new RockBulkUpdateExpressionVisitor( currentDateTime, currentPersonAlias );
            var updatedExpression = rockExpressionVisitor.Visit( updateFactory ) as Expression<Func<T, T>> ?? updateFactory;
            int recordsUpdated = queryable.Update( updatedExpression, batchUpdateBuilder =>
            {
                batchUpdateBuilder.Executing = ( e ) => { e.CommandTimeout = this.Database.CommandTimeout ?? 30; };
            } );
            return recordsUpdated;
        }

        /// <summary>
        /// Does a direct bulk DELETE.
        /// Example: rockContext.BulkDelete( groupMembersToDeleteQuery );
        /// NOTES:
        /// - This bypasses the Rock and a bunch of the EF Framework and automatically commits the changes to the database.
        /// - This will use the Database.CommandTimeout value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable">The queryable for the records to delete</param>
        /// <param name="batchSize">The BatchSize property sets the amount of rows to delete in a single batch (Default 4000)</param>
        /// <returns></returns>
        public virtual int BulkDelete<T>( IQueryable<T> queryable, int? batchSize = null ) where T : class
        {
            /*
                 5/25/2023 - NA

                 We're changing the default batch size for deletes from 4000 to 1500 because
                 once SQL Server has more 5000 row locks, it will escalate to a table lock.
                 https://nathancooper.dev/articles/2020-04/lock-escalation

                 Reason: To avoid table lock escalation.
            */
            return queryable.Delete( d =>
            {
                d.BatchSize = batchSize ?? 1500;
                d.Executing = ( e ) => { e.CommandTimeout = this.Database.CommandTimeout ?? 30; };
            } );
        }

        #endregion Bulk Operations

        /// <summary>
        /// Triggers all the workflows of the given triggerType for the given entity item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        private bool TriggerWorkflows( ContextItem item, WorkflowTriggerType triggerType, PersonAlias personAlias )
        {
            IEntity entity = item.Entity;
            Dictionary<string, PropertyInfo> properties = null;

            // Look at each trigger for this entity and for the given trigger type
            // and see if it's a match.
            var triggers = WorkflowTriggersCache.Triggers( entity.TypeName, triggerType )
                .Where( t => t.IsActive == true )
                .ToList();

            if ( !triggers.Any() )
            {
                return true;
            }

            // Determine if this trigger type is one that happens immediately
            // or after the save is completed in the background.
            var isImmediate = triggerType == WorkflowTriggerType.PreSave
                || triggerType == WorkflowTriggerType.PreDelete
                || triggerType == WorkflowTriggerType.ImmediatePostSave;

            var matchingTriggers = triggers
                .Where( trigger =>
                {
                    // If a qualifier column was given, then we need to check the previous or current qualifier value
                    // otherwise it's just an automatic match.
                    if ( !string.IsNullOrWhiteSpace( trigger.EntityTypeQualifierColumn ) )
                    {
                        // Get and cache the properties https://lotsacode.wordpress.com/2010/04/13/reflection-type-getproperties-and-performance/
                        // (Note: its possible that none of the triggers need them, so future TODO could be to
                        // bypass all this in that case.
                        if ( properties == null )
                        {
                            properties = new Dictionary<string, PropertyInfo>();
                            foreach ( PropertyInfo propertyInfo in entity.GetType().GetProperties() )
                            {
                                properties.Add( propertyInfo.Name.ToLower(), propertyInfo );
                            }
                        }

                        return IsQualifierMatch( item, properties, trigger );
                    }

                    return true;
                } );

            // If we are doing immediate triggers then process now; otherwise queue them.
            if ( isImmediate )
            {
                var entityTypeCache = EntityTypeCache.Get( entity.TypeId );

                using ( var activity = Observability.ObservabilityHelper.StartActivity( $"WT: {entityTypeCache?.FriendlyName}" ) )
                {
                    activity?.SetTag( "rock.trigger.entity_type_id", entity.TypeId );
                    activity?.SetTag( "rock.trigger.entity_type", entity.TypeName );
                    activity?.SetTag( "rock.trigger.entity_id", entity.Id );

                    foreach ( var trigger in matchingTriggers )
                    {
                        var workflowType = WorkflowTypeCache.Get( trigger.WorkflowTypeId );
                        if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                        {
                            var workflow = Rock.Model.Workflow.Activate( workflowType, trigger.WorkflowName );

                            using ( var rockContext = new RockContext() )
                            {
                                var workflowService = new WorkflowService( rockContext );
                                if ( !workflowService.Process( workflow, entity, out var workflowErrors ) )
                                {
                                    SaveErrorMessages.AddRange( workflowErrors );
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var processWorkflowTriggerMsg = new ProcessWorkflowTrigger.Message
                {
                    WorkflowTriggerGuids = matchingTriggers.Select( t => t.Guid ).ToList(),
                    EntityId = entity.Id,
                    EntityTypeId = entity.TypeId
                };

                processWorkflowTriggerMsg.SendWhen( this.WrappedTransactionCompletedTask );
            }

            return true;
        }

        /// <summary>
        /// Determines whether the entity matches the current and/or previous qualifier values.
        /// If
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="trigger">The trigger.</param>
        /// <returns>true if matches; false otherwise</returns>
        private static bool IsQualifierMatch( ContextItem item, Dictionary<string, PropertyInfo> properties, WorkflowTrigger trigger )
        {
            bool match = false;

            try
            {
                var dbEntity = item.DbEntityEntry;

                // Now attempt to find a match taking into account the EntityTypeQualifierValue and/or EntityTypeQualifierValuePrevious
                if ( properties.ContainsKey( trigger.EntityTypeQualifierColumn.ToLower() ) )
                {
                    var propertyInfo = properties[trigger.EntityTypeQualifierColumn.ToLower()];

                    bool hasPrevious = !string.IsNullOrEmpty( trigger.EntityTypeQualifierValuePrevious );
                    bool hasCurrent = !string.IsNullOrEmpty( trigger.EntityTypeQualifierValue );

                    var currentProperty = propertyInfo.GetValue( item.Entity, null );
                    var currentValue = currentProperty != null ? currentProperty.ToString() : string.Empty;
                    var alternateCurrentValue = currentValue; // used for comparisons of EntityTypeQualifierValue in case the user enters the numeric value of an enum.
                    if ( propertyInfo.PropertyType.IsEnum )
                    {
                        alternateCurrentValue = Convert.ChangeType( currentProperty, Enum.GetUnderlyingType( currentProperty.GetType() ) ).ToString();
                    }

                    var previousValue = string.Empty;
                    var alternatePreviousValue = previousValue; // used for comparisons of EntityTypeQualifierValue in case the user enters the numeric value of an enum.

                    if ( item.OriginalValues != null && item.OriginalValues.ContainsKey( propertyInfo.Name ) )
                    {
                        var previousProperty = item.OriginalValues[propertyInfo.Name];
                        previousValue = previousProperty.ToStringSafe();
                        if ( propertyInfo.PropertyType.IsEnum )
                        {
                            alternatePreviousValue = Convert.ChangeType( previousProperty, Enum.GetUnderlyingType( previousProperty.GetType() ) ).ToString();
                        }
                    }
                    else
                    {
                        var dbPropertyEntry = dbEntity.Property( propertyInfo.Name );
                        if ( dbPropertyEntry != null && item.PreSaveState != EntityContextState.Added )
                        {
                            previousValue = dbPropertyEntry.OriginalValue.ToStringSafe();
                            if ( propertyInfo.PropertyType.IsEnum )
                            {
                                alternatePreviousValue = Convert.ChangeType( dbPropertyEntry.OriginalValue, Enum.GetUnderlyingType( dbPropertyEntry.OriginalValue.GetType() ) ).ToString();
                            }
                        }
                    }

                    if ( trigger.WorkflowTriggerType == WorkflowTriggerType.PreDelete ||
                        trigger.WorkflowTriggerType == WorkflowTriggerType.PostDelete )
                    {
                        match = ( previousValue == trigger.EntityTypeQualifierValue || alternatePreviousValue == trigger.EntityTypeQualifierValue );
                    }

                    if ( trigger.WorkflowTriggerType == WorkflowTriggerType.PostAdd )
                    {
                        match = ( currentValue == trigger.EntityTypeQualifierValue || alternateCurrentValue == trigger.EntityTypeQualifierValue );
                    }

                    if ( trigger.WorkflowTriggerType == WorkflowTriggerType.ImmediatePostSave ||
                        trigger.WorkflowTriggerType == WorkflowTriggerType.PostSave ||
                        trigger.WorkflowTriggerType == WorkflowTriggerType.PreSave )
                    {
                        if ( trigger.WorkflowTriggerValueChangeType == WorkflowTriggerValueChangeType.ValueEqual )
                        {
                            match = ( trigger.EntityTypeQualifierValue == currentValue || trigger.EntityTypeQualifierValue == alternateCurrentValue );
                        }
                        else
                        {
                            if ( hasCurrent && !hasPrevious )
                            {
                                // ...and previous cannot be the same as the current (must be a change)
                                match = ( currentValue != previousValue &&
                                    ( currentValue == trigger.EntityTypeQualifierValue ||
                                    alternateCurrentValue == trigger.EntityTypeQualifierValue ) );
                            }
                            else if ( !hasCurrent && hasPrevious )
                            {
                                // ...and previous cannot be the same as the current (must be a change)
                                match = ( previousValue != currentValue &&
                                    ( previousValue == trigger.EntityTypeQualifierValuePrevious ||
                                    alternatePreviousValue == trigger.EntityTypeQualifierValuePrevious ) );
                            }
                            else if ( hasCurrent && hasPrevious )
                            {
                                match = ( ( currentValue == trigger.EntityTypeQualifierValue ||
                                    alternateCurrentValue == trigger.EntityTypeQualifierValue ) &&
                                    ( previousValue == trigger.EntityTypeQualifierValuePrevious ||
                                    alternatePreviousValue == trigger.EntityTypeQualifierValuePrevious ) );
                            }
                            else if ( !hasCurrent && !hasPrevious )
                            {
                                match = previousValue != currentValue;
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
            }

            return match;
        }

        /// <summary>
        /// Gets the audit details.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="item">The item.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        private static void GetAuditDetails( DbContext dbContext, ContextItem item, int? personAliasId )
        {
            // Get the base class (not the proxy class)
            Type rockEntityType = item.Entity.GetType();
            if ( rockEntityType.IsDynamicProxyType() )
            {
                rockEntityType = rockEntityType.BaseType;
            }

            if ( IsAuditableClass( rockEntityType ) )
            {
                var dbEntity = item.DbEntityEntry;
                var audit = item.Audit;

                PropertyInfo[] properties = rockEntityType.GetProperties();

                foreach ( PropertyInfo propInfo in properties )
                {
                    if ( IsAuditableProperty( propInfo ) )
                    {
                        // If entire entity was added or deleted or this property was modified
                        var dbPropertyEntry = dbEntity.Property( propInfo.Name );
                        if ( dbPropertyEntry != null && (
                            dbEntity.State == EntityState.Added ||
                            dbEntity.State == EntityState.Deleted ||
                            dbPropertyEntry.IsModified ) )
                        {
                            var currentValue = dbEntity.State == EntityState.Deleted ? string.Empty : dbPropertyEntry.CurrentValue;
                            var originalValue = dbEntity.State == EntityState.Added ? string.Empty : dbPropertyEntry.OriginalValue;

                            var detail = new AuditDetail();
                            detail.Property = propInfo.Name;
                            detail.CurrentValue = currentValue != null ? currentValue.ToString() : string.Empty;
                            detail.OriginalValue = originalValue != null ? originalValue.ToString() : string.Empty;
                            if ( detail.CurrentValue != detail.OriginalValue )
                            {
                                audit.Details.Add( detail );
                            }
                        }
                    }
                }

                if ( audit.Details.Any() )
                {
                    var entityType = EntityTypeCache.Get( rockEntityType );
                    if ( entityType != null )
                    {
                        string title;
                        try
                        {
                            title = item.Entity.ToString();
                        }
                        catch
                        {
                            // ignore exception (Entity often overrides ToString() and we don't want that prevent the audit if it fails)
                            title = null;
                        }

                        if ( string.IsNullOrWhiteSpace( title ) )
                        {
                            title = entityType.FriendlyName ?? string.Empty;
                        }
                        audit.DateTime = RockDateTime.Now;
                        audit.PersonAliasId = personAliasId;
                        audit.EntityTypeId = entityType.Id;
                        audit.EntityId = item.Entity.Id;
                        audit.Title = title.Truncate( 195 );
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether [is auditable class] [the specified base type].
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <returns>
        ///   <c>true</c> if [is auditable class] [the specified base type]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsAuditableClass( Type baseType )
        {
            var attribute = baseType.GetCustomAttribute( typeof( NotAuditedAttribute ) );
            return ( attribute == null );
        }

        /// <summary>
        /// Determines whether [is auditable property] [the specified property information].
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <returns>
        ///   <c>true</c> if [is auditable property] [the specified property information]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsAuditableProperty( PropertyInfo propertyInfo )
        {
            // if it is specifically marked as NotAudited, don't include it
            if ( propertyInfo.GetCustomAttribute<NotAuditedAttribute>() != null )
            {
                return false;
            }

            // Otherwise, make sure it is a real database field
            return Reflection.IsMappedDatabaseProperty( propertyInfo );
        }

        #region WebForms

#if WEBFORMS

        /// <summary>
        /// The reflected property that gives access to DbContext.InternalContext.
        /// </summary>
        private static PropertyInfo _dbContextInternalContextProperty;

        /// <summary>
        /// The reflected property that gives access to LazyInternalContext.ObjectContext.
        /// </summary>
        private static PropertyInfo _internalContextObjectContextProperty;

        /// <summary>
        /// <para>
        /// Detects the implied relationship changes in the context. A many-to-many
        /// relationship without an associated model is implied. Because there is
        /// no model, Entity Framework does not report it in the list of changed
        /// models.
        /// </para>
        /// <para>
        /// This allows us to detect when one of these many-to-many relationships
        /// is added or removed. When detected it updates the related entity
        /// entries to mark them as modified. This allows our save hooks and
        /// related code to properly run when the entity is indirectly modified.
        /// </para>
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        private static void DetectImpliedRelationshipChanges( DbContext dbContext )
        {
            /*
                 1/30/2024 - DSH

                 This bit of code replaces the following line:

                 var objectContext = ( ( IObjectContextAdapter ) this ).ObjectContext;

                 The above line also eager initializes all the DbSet properties,
                 which we don't need because we aren't going to access them.
                 That eager initialization takes 80-100ms. If we later remove
                 all, or most, of the DbSet properties from RockContext then
                 we can switch away from reflection and use the standard call.

                 Performance is very good. Assuming the ObjectContext property has
                 already been accessed once to get the initialization out of the
                 way:
                 Direct property access: 0.1212ms
                 Reflection access: 0.136ms

                 Reason: To avoid 100ms penalty when detecting many-to-many
                 relationship changes.
            */
            if ( _dbContextInternalContextProperty == null )
            {
                _dbContextInternalContextProperty = typeof( DbContext ).GetProperty( "InternalContext", BindingFlags.NonPublic | BindingFlags.Instance );
                _internalContextObjectContextProperty = _dbContextInternalContextProperty.PropertyType.GetProperty( "ObjectContext" );
            }
            var internalContext = _dbContextInternalContextProperty.GetValue( dbContext, BindingFlags.Default, null, null, null );
            var objectContext = ( ObjectContext ) _internalContextObjectContextProperty.GetValue( internalContext, BindingFlags.Default, null, null, null );

            var addedItems = objectContext.ObjectStateManager.GetObjectStateEntries( EntityState.Added )
                .Where( e => e.IsRelationship );

            foreach ( var addedItem in addedItems )
            {
                MarkEntryAsModifiedByKey( dbContext, objectContext, addedItem.CurrentValues[0] );
                MarkEntryAsModifiedByKey( dbContext, objectContext, addedItem.CurrentValues[1] );
            }

            var removedItems = objectContext.ObjectStateManager.GetObjectStateEntries( EntityState.Deleted )
                .Where( e => e.IsRelationship );

            foreach ( var removedItem in removedItems )
            {
                MarkEntryAsModifiedByKey( dbContext, objectContext, removedItem.OriginalValues[0] );
                MarkEntryAsModifiedByKey( dbContext, objectContext, removedItem.OriginalValues[1] );
            }
        }

        /// <summary>
        /// Marks the entry as modified by its DbContext key. This is only meant to be used
        /// by the <see cref="DetectImpliedRelationshipChanges(DbContext)"/> method.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="objectContext">The object context.</param>
        /// <param name="key">The key.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static void MarkEntryAsModifiedByKey( DbContext dbContext, ObjectContext objectContext, object key )
        {
            if ( key is EntityKey entityKey && objectContext.TryGetObjectByKey( entityKey, out var removedEntity ) )
            {
                var entry = dbContext.Entry( removedEntity );

                if ( entry.State == EntityState.Unchanged )
                {
                    entry.State = EntityState.Modified;
                }
            }
        }

#endif

        #endregion

        /// <summary>
        /// State of entity being changed during a context save
        /// </summary>
        [System.Diagnostics.DebuggerDisplay( "{Entity.GetType()}:{Entity}, State:{State}" )]
        protected class ContextItem : IEntitySaveEntry
        {
            #region Fields

            /// <summary>
            /// The lazy wrapper around <see cref="ModifiedProperties"/>.
            /// </summary>
            private readonly Lazy<IReadOnlyList<string>> _lazyModifiedProperties;

            #endregion

            /// <summary>
            /// Gets or sets the entity.
            /// </summary>
            /// <value>
            /// The entity.
            /// </value>
            public IEntity Entity { get; set; }

            /// <inheritdoc/>
            object IEntitySaveEntry.Entity => Entity;

            /// <summary>
            /// Gets or sets the current state of the item in the ChangeTracker. Note: Use PreSaveState to see the state of the item before SaveChanges was called.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public EntityContextState State { get; }

            /// <inheritdoc/>
            public EntityContextState PreSaveState { get; private set; }

            /// <summary>
            /// Gets or sets the save hooks that have had their
            /// <see cref="IEntitySaveHook.PreSave(IEntitySaveEntry)"/> method
            /// called and return successfully. In other words, these are save
            /// hooks that will need to have either <see cref="IEntitySaveHook.SaveFailed(IEntitySaveEntry)"/>
            /// or <see cref="IEntitySaveHook.PostSave(IEntitySaveEntry)"/> called.
            /// </summary>
            /// <value>
            /// The save hooks that are pending completion.
            /// </value>
            internal List<IEntitySaveHook> InProcessSaveHooks { get; set; }

            /// <summary>
            /// Gets the pre-save state legacy value.
            /// </summary>
            /// <value>
            /// The pre-save state legacy value.
            /// </value>
            /// <remarks>
            /// This can be removed once the Model{T}.PostSaveChanges() methods are removed.
            /// </remarks>
            internal EntityState PreSaveStateLegacy { get; }

            /// <inheritdoc/>
            public object DataContext { get; private set; }

            /// <summary>
            /// Gets or sets the database entity entry.
            /// </summary>
            /// <value>
            /// The database entity entry.
            /// </value>
            public DbEntityEntry DbEntityEntry { get; set; }

            /// <summary>
            /// Gets or sets the audit.
            /// </summary>
            /// <value>
            /// The audit.
            /// </value>
            public Audit Audit { get; set; }

            /// <summary>
            /// Gets or sets the collection of original entity values before the save occurs,
            /// only valid when the entity-state is <seealso cref="EntityState.Modified"/>
            /// or <seealso cref="EntityState.Deleted"/>.
            /// </summary>
            /// <value>
            /// The original entity values.
            /// </value>
            public IReadOnlyDictionary<string, object> OriginalValues { get; set; }

            /// <summary>
            /// Gets the collection of property names that have been modified. This
            /// will include any additional changes made during the PreSave event.
            /// </summary>
            /// <remarks>
            /// This is a relatively expensive operation of up to 1.5ms so this
            /// propery should not be accessed unless you really need to.
            /// </remarks>
            /// <value>A collection of modified property names.</value>
            public IReadOnlyList<string> ModifiedProperties => _lazyModifiedProperties.Value;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContextItem" /> class.
            /// </summary>
            /// <param name="entity">The entity.</param>
            /// <param name="dbContext">The database context that is tracking this entity.</param>
            /// <param name="dbEntityEntry">The database entity entry.</param>
            /// <param name="enableAuditing">if set to <c>true</c> then auditing is enabled for this entity item.</param>
            public ContextItem( IEntity entity, DbContext dbContext, DbEntityEntry dbEntityEntry, bool enableAuditing )
            {
                Entity = entity;
                DbEntityEntry = dbEntityEntry;
                State = DbEntityEntry.State.ToEntityContextState();
                DataContext = dbContext;

                if ( enableAuditing )
                {
                    Audit = new Audit();

                    switch ( dbEntityEntry.State )
                    {
                        case EntityState.Added:
                            {
                                Audit.AuditType = AuditType.Add;
                                break;
                            }
                        case EntityState.Deleted:
                            {
                                Audit.AuditType = AuditType.Delete;
                                break;
                            }
                        case EntityState.Modified:
                            {
                                Audit.AuditType = AuditType.Modify;
                                break;
                            }
                    }
                }

                PreSaveState = dbEntityEntry.State.ToEntityContextState();
                PreSaveStateLegacy = dbEntityEntry.State;

                if ( dbEntityEntry.State == EntityState.Modified || dbEntityEntry.State == EntityState.Deleted )
                {
                    var originalValues = new Dictionary<string, object>();

                    foreach ( var p in DbEntityEntry.OriginalValues.PropertyNames )
                    {
                        originalValues.Add( p, DbEntityEntry.OriginalValues[p] );
                    }

                    OriginalValues = originalValues;


                    // Construct this lazily because not all save hooks will
                    // even use this. It takes about 0.6ms to run.
                    _lazyModifiedProperties = new Lazy<IReadOnlyList<string>>( () =>
                    {
                        if ( PreSaveState != EntityContextState.Modified )
                        {
                            return null;
                        }

                        var modifiedProperties = new List<string>();

                        foreach ( var p in OriginalValues.Keys )
                        {
                            var originalValue = OriginalValues[p];
                            var currentValue = DbEntityEntry.CurrentValues[p];

                            // Both are null, no change.
                            if ( originalValue == null && currentValue == null )
                            {
                                continue;
                            }

                            // One is null and the other is not, changed value.
                            if ( ( originalValue == null && currentValue != null ) || ( originalValue != null && currentValue == null ) )
                            {
                                modifiedProperties.Add( p );
                            }

                            // At this point, both are not null. Do an Equals check.
                            else if ( !originalValue.Equals( currentValue ) )
                            {
                                modifiedProperties.Add( p );
                            }
                        }

                        return modifiedProperties;
                    } );
                }
            }
        }
    }
}
