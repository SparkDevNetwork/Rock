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
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Rock.Bus.Message;
using Rock.Model;
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
        /// <summary>
        /// Is there a transaction in progress?
        /// </summary>
        private bool _transactionInProgress = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        public DbContext() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">Either the database name or a connection string.</param>
        public DbContext( string nameOrConnectionString ) : base( nameOrConnectionString ) { }

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
                using ( var dbContextTransaction = this.Database.BeginTransaction() )
                {
                    try
                    {
                        if ( action.Invoke() )
                        {
                            dbContextTransaction.Commit();
                        }
                        else
                        {
                            dbContextTransaction.Rollback();
                            return false;
                        }
                    }
                    catch
                    {
                        dbContextTransaction.Rollback();
                        throw;
                    }
                    finally
                    {
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
        public int SaveChanges( bool disablePrePostProcessing )
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
        public SaveChangesResult SaveChanges( SaveChangesArgs args )
        {
            var saveChangesResult = new SaveChangesResult();

            // Pre and Post processing has been disabled, just call the base
            // SaveChanges() method and return
            if ( args.DisablePrePostProcessing )
            {
                saveChangesResult.RecordsUpdated = base.SaveChanges();
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
                    saveChangesResult.RecordsUpdated = base.SaveChanges();
                }
                catch ( System.Data.Entity.Validation.DbEntityValidationException ex )
                {
                    var validationErrors = new List<string>();
                    foreach ( var error in ex.EntityValidationErrors )
                    {
                        foreach ( var prop in error.ValidationErrors )
                        {
                            validationErrors.Add( string.Format( "{0} ({1}): {2}", error.Entry.Entity.GetType().Name, prop.PropertyName, prop.ErrorMessage ) );
                        }
                    }

                    throw new SystemException( "Entity Validation Error: " + validationErrors.AsDelimited( ";" ), ex );
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
            }

            return saveChangesResult;
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

            var preSavedEntities = new HashSet<Guid>();

            // First loop through all models calling the PreSaveChanges
            foreach ( var entry in dbContext.ChangeTracker.Entries()
                .Where( c =>
                    c.Entity is IEntity &&
                    ( c.State == EntityState.Added || c.State == EntityState.Modified || c.State == EntityState.Deleted ) ) )
            {
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
            var updatedItems = new List<ContextItem>();
            foreach ( var entry in dbContext.ChangeTracker.Entries()
                .Where( c =>
                    c.Entity is IEntity &&
                    ( c.State == EntityState.Added || c.State == EntityState.Modified || c.State == EntityState.Deleted ) ) )
            {
                // Cast entry as IEntity
                var entity = entry.Entity as IEntity;

                // Get the context item to track audits
                var contextItem = new ContextItem( entity, entry, enableAuditing );

                // If entity was added or modified, update the Created/Modified fields
                if ( entry.State == EntityState.Added || entry.State == EntityState.Modified )
                {
                    // instead of passing "true" the trigger model and UI would support a
                    // on-value-changed checkbox (or perhaps it should be the default/only behavior)
                    // and its value would be passed in to the onValueChange
                    if ( !TriggerWorkflows( contextItem, WorkflowTriggerType.PreSave, personAlias ) )
                    {
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
                        return null;
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

                updatedItems.Add( contextItem );
            }

            return updatedItems;
        }

        /// <summary>
        /// Creates audit logs and/or triggers workflows for items that were changed
        /// </summary>
        /// <param name="updatedItems">The updated items.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="enableAuditing">if set to <c>true</c> [enable auditing].</param>
        protected virtual void RockPostSave( List<ContextItem> updatedItems, PersonAlias personAlias, bool enableAuditing = false )
        {
            if ( enableAuditing )
            {
                try
                {
                    var audits = updatedItems.Where( a => a.Audit != null ).Select( i => i.Audit ).ToList();
                    if ( audits.Any( a => a.Details.Any() ) )
                    {
                        var transaction = new Rock.Transactions.AuditTransaction();
                        transaction.Audits = audits.Where( a => a.Details.Any() == true ).ToList();
                        Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                    }
                }
                catch ( SystemException ex )
                {
                    ExceptionLogService.LogException( ex, null );
                }
            }

            List<ITransaction> indexTransactions = new List<ITransaction>();
            foreach ( var item in updatedItems )
            {
                // Publish on the message bus if the entity type is configured
                EntityWasUpdatedMessage.PublishIfShould( item.Entity, item.PreSaveState );

                if ( item.State == EntityState.Detached || item.State == EntityState.Deleted )
                {
                    TriggerWorkflows( item, WorkflowTriggerType.PostDelete, personAlias );
                }
                else
                {
                    if ( item.PreSaveState == EntityState.Added )
                    {
                        TriggerWorkflows( item, WorkflowTriggerType.PostAdd, personAlias );
                    }

                    TriggerWorkflows( item, WorkflowTriggerType.ImmediatePostSave, personAlias );
                    TriggerWorkflows( item, WorkflowTriggerType.PostSave, personAlias );
                }

                if ( item.Entity is IModel )
                {
                    var model = item.Entity as IModel;
                    model.PostSaveChanges( this );
                }

                // check if this entity should be passed on for indexing
                if ( item.Entity is IRockIndexable )
                {
                    if ( item.State == EntityState.Detached || item.State == EntityState.Deleted )
                    {
                        DeleteIndexEntityTransaction transaction = new DeleteIndexEntityTransaction();
                        transaction.EntityTypeId = item.Entity.TypeId;
                        transaction.EntityId = item.Entity.Id;

                        indexTransactions.Add( transaction );
                    }
                    else
                    {
                        IndexEntityTransaction transaction = new IndexEntityTransaction();
                        transaction.EntityTypeId = item.Entity.TypeId;
                        transaction.EntityId = item.Entity.Id;

                        indexTransactions.Add( transaction );
                    }
                }

                if ( item.Entity is ICacheable cacheable )
                {
                    cacheable.UpdateCache( item.PreSaveState, this );
                }
            }

            // check if Indexing is enabled in another thread to avoid deadlock when Snapshot Isolation is turned off when the Index components upload/load attributes
            if ( indexTransactions.Any() )
            {
                System.Threading.Tasks.Task.Run( () =>
                {
                    var indexingEnabled = IndexContainer.GetActiveComponent() == null ? false : true;
                    if ( indexingEnabled )
                    {
                        indexTransactions.ForEach( t => RockQueue.TransactionQueue.Enqueue( t ) );
                    }
                } );
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

            if ( updatedItem.State == EntityState.Detached || updatedItem.State == EntityState.Deleted || updatedItem.Entity == null )
            {
                return new List<AchievementAttempt>();
            }

            return AchievementTypeCache.ProcessAchievements( updatedItem.Entity );
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
            var isAchievementsEnabled = canUseCache && EntityTypeCache.Get<T>()?.IsAchievementsEnabled == true;

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
            return queryable.Delete( d =>
            {
                d.BatchSize = batchSize ?? 4000;
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
            foreach ( var trigger in WorkflowTriggersCache.Triggers( entity.TypeName, triggerType ).Where( t => t.IsActive == true ) )
            {
                bool match = true;

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

                    match = IsQualifierMatch( item, properties, trigger );
                }

                // If we found a matching trigger, then fire it; otherwise do nothing.
                if ( match )
                {
                    // If it's one of the pre or immediate triggers, fire it immediately; otherwise queue it.
                    if ( triggerType == WorkflowTriggerType.PreSave || triggerType == WorkflowTriggerType.PreDelete || triggerType == WorkflowTriggerType.ImmediatePostSave )
                    {
                        var workflowType = WorkflowTypeCache.Get( trigger.WorkflowTypeId );
                        if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                        {
                            var workflow = Rock.Model.Workflow.Activate( workflowType, trigger.WorkflowName );

                            List<string> workflowErrors;

                            using ( var rockContext = new RockContext() )
                            {
                                var workflowService = new WorkflowService( rockContext );
                                if ( !workflowService.Process( workflow, entity, out workflowErrors ) )
                                {
                                    SaveErrorMessages.AddRange( workflowErrors );
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        var transaction = new Rock.Transactions.WorkflowTriggerTransaction();
                        transaction.Trigger = trigger;
                        transaction.Entity = entity.Clone();
                        transaction.PersonAlias = personAlias;
                        Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                    }
                }
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
                        if ( dbPropertyEntry != null && item.PreSaveState != EntityState.Added )
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

        /// <summary>
        /// State of entity being changed during a context save
        /// </summary>
        [System.Diagnostics.DebuggerDisplay( "{Entity.GetType()}:{Entity}, State:{State}" )]
        protected class ContextItem
        {
            /// <summary>
            /// Gets or sets the entity.
            /// </summary>
            /// <value>
            /// The entity.
            /// </value>
            public IEntity Entity { get; set; }

            /// <summary>
            /// Gets or sets the current state of the item in the ChangeTracker. Note: Use PreSaveState to see the state of the item before SaveChanges was called.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public EntityState State
            {
                get
                {
                    return this.DbEntityEntry.State;
                }
            }

            /// <summary>
            /// Gets the EntityState of the item (before it was saved the to database)
            /// </summary>
            /// <value>
            /// The state of the pre save.
            /// </value>
            public EntityState PreSaveState { get; private set; }

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
            /// only valid when the entity-state is Modified.
            /// </summary>
            /// <value>
            /// The original entity values.
            /// </value>
            public Dictionary<string, object> OriginalValues { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ContextItem" /> class.
            /// </summary>
            /// <param name="entity">The entity.</param>
            /// <param name="dbEntityEntry">The database entity entry.</param>
            /// <param name="enableAuditing">if set to <c>true</c> [enable auditing].</param>
            public ContextItem( IEntity entity, DbEntityEntry dbEntityEntry, bool enableAuditing )
            {
                Entity = entity;
                DbEntityEntry = dbEntityEntry;
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

                PreSaveState = dbEntityEntry.State;

                if ( dbEntityEntry.State == EntityState.Modified )

                {
                    var triggers = WorkflowTriggersCache.Triggers( entity.TypeName )
                        .Where( t => t.WorkflowTriggerType == WorkflowTriggerType.ImmediatePostSave || t.WorkflowTriggerType == WorkflowTriggerType.PostSave );

                    if ( triggers.Any() )
                    {
                        OriginalValues = new Dictionary<string, object>();
                        foreach ( var p in DbEntityEntry.OriginalValues.PropertyNames )
                        {
                            OriginalValues.Add( p, DbEntityEntry.OriginalValues[p] );
                        }
                    }
                }

            }
        }
    }
}
