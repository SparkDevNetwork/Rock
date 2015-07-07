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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Web;

using Rock.Model;
using Rock.Workflow;

namespace Rock.Data
{
    /// <summary>
    /// Entity Framework Context
    /// </summary>
    public abstract class DbContext : System.Data.Entity.DbContext
    {
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
        /// Wraps code in a BeginTransaction and CommitTransaction
        /// </summary>
        /// <param name="action">The action.</param>
        public void WrapTransaction( Action action )
        {
            if ( !_transactionInProgress )
            {
                _transactionInProgress = true;
                using ( var dbContextTransaction = this.Database.BeginTransaction() )
                {
                    try
                    {
                        action.Invoke();
                        dbContextTransaction.Commit();
                    }
                    catch ( Exception ex )
                    {
                        dbContextTransaction.Rollback();
                        throw ( ex );
                    }
                    finally
                    {
                        _transactionInProgress = false;
                    }
                }
            }
            else
            {
                action.Invoke();
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
            // Pre and Post processing has been disabled, just call the base
            // SaveChanges() method and return
            if ( disablePrePostProcessing )
            {
                return base.SaveChanges();
            }

            int result = 0;

            SaveErrorMessages = new List<string>();

            // Try to get the current person alias and id
            PersonAlias personAlias = null;
            if ( HttpContext.Current != null && HttpContext.Current.Items.Contains( "CurrentPerson" ) )
            {
                var currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
                if ( currentPerson != null && currentPerson.PrimaryAlias != null )
                {
                    personAlias = currentPerson.PrimaryAlias;
                }
            }

            bool enableAuditing = Rock.Web.Cache.GlobalAttributesCache.Value( "EnableAuditing" ).AsBoolean();

            // Evaluate the current context for items that have changes
            var updatedItems = RockPreSave( this, personAlias, enableAuditing );

            // If update was not cancelled by triggered workflow
            if ( updatedItems != null )
            {
                try
                {
                    // Save the context changes
                    result = base.SaveChanges();
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
                }
            }

            return result;
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
                    model.PreSaveChanges( this, entry );

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
                var contextItem = new ContextItem( entity, entry );

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
                            model.ModifiedByPersonAliasId = personAliasId;
                        }
                        else if ( entry.State == EntityState.Modified )
                        {
                            model.ModifiedDateTime = RockDateTime.Now;
                            model.ModifiedByPersonAliasId = personAliasId;
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
                    var audits = updatedItems.Select( i => i.Audit ).ToList();
                    if ( audits.Any( a => a.Details.Any() ) )
                    {
                        var transaction = new Rock.Transactions.AuditTransaction();
                        transaction.Audits = audits;
                        Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                    }
                }
                catch ( SystemException ex )
                {
                    ExceptionLogService.LogException( ex, null );
                }
            }

            foreach ( var item in updatedItems )
            {
                if ( item.State == EntityState.Detached || item.State == EntityState.Deleted )
                {
                    TriggerWorkflows( item, WorkflowTriggerType.PostDelete, personAlias );
                }
                else
                {
                    TriggerWorkflows( item, WorkflowTriggerType.ImmediatePostSave, personAlias );
                    TriggerWorkflows( item, WorkflowTriggerType.PostSave, personAlias );
                }
            }
        }

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

            using ( var rockContext = new RockContext() )
            {
                var workflowTypeService = new WorkflowTypeService( rockContext );
                var workflowService = new WorkflowService( rockContext );

                // Look at each trigger for this entity and for the given trigger type
                // and see if it's a match.
                foreach ( var trigger in TriggerCache.Triggers( entity.TypeName, triggerType ).Where( t => t.IsActive == true ) )
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
                            var workflowType = workflowTypeService.Get( trigger.WorkflowTypeId );

                            if ( workflowType != null )
                            {
                                var workflow = Rock.Model.Workflow.Activate( workflowType, trigger.WorkflowName );

                                List<string> workflowErrors;
                                if ( !workflow.Process( rockContext, entity, out workflowErrors ) )
                                {
                                    SaveErrorMessages.AddRange( workflowErrors );
                                    return false;
                                }
                                else
                                {
                                    if ( workflow.IsPersisted || workflowType.IsPersisted )
                                    {
                                        workflowService.Add( workflow );
                                        rockContext.SaveChanges();
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
            var dbEntity = item.DbEntityEntry;

            // Now attempt to find a match taking into account the EntityTypeQualifierValue and/or EntityTypeQualifierValuePrevious
            if ( properties.ContainsKey( trigger.EntityTypeQualifierColumn.ToLower() ) )
            {
                var propertyInfo = properties[trigger.EntityTypeQualifierColumn.ToLower()];

                bool hasPrevious = !string.IsNullOrEmpty( trigger.EntityTypeQualifierValuePrevious );
                bool hasCurrent = !string.IsNullOrEmpty( trigger.EntityTypeQualifierValue );

                // it's apparently not possible to interrogate the dbEntity of virtual properties
                // so it's illegal if they somehow selected one of these; and it's not a match.
                if ( propertyInfo.GetGetMethod().IsVirtual )
                {
                    // TODO: log a silent exception?
                    return false;
                }

                var currentValue = propertyInfo.GetValue( item.Entity, null ).ToString();
                var previousValue = string.Empty;

                var dbPropertyEntry = dbEntity.Property( propertyInfo.Name );
                if ( dbPropertyEntry != null )
                {
                    previousValue = dbEntity.State == EntityState.Added ? string.Empty : dbPropertyEntry.OriginalValue.ToStringSafe();
                }

                if ( hasCurrent && !hasPrevious )
                {
                    // ...and previous cannot be the same as the current (must be a change)
                    match = ( currentValue == trigger.EntityTypeQualifierValue &&
                        currentValue != previousValue );
                }
                else if ( !hasCurrent && hasPrevious )
                {
                    // ...and previous cannot be the same as the current (must be a change)
                    match = ( previousValue == trigger.EntityTypeQualifierValuePrevious &&
                        previousValue != currentValue );
                }
                else if ( hasCurrent && hasPrevious )
                {
                    match = ( currentValue == trigger.EntityTypeQualifierValue &&
                        previousValue == trigger.EntityTypeQualifierValuePrevious );
                }
                else if ( !hasCurrent && !hasPrevious )
                {
                    // If they used an entity type qualifier column, at least one qualifier value is required.
                    // TODO: log as silent exception? 
                }
            }
            return match;
        }

        private static void GetAuditDetails( DbContext dbContext, ContextItem item, int? personAliasId )
        {
            // Get the base class (not the proxy class)
            Type rockEntityType = item.Entity.GetType();
            if ( rockEntityType.IsDynamicProxyType() )
            {
                rockEntityType = rockEntityType.BaseType;
            }

            // Check to make sure class does not have [NotAudited] attribute
            if ( AuditClass( rockEntityType ) )
            {
                var dbEntity = item.DbEntityEntry;
                var audit = item.Audit;

                PropertyInfo[] properties = rockEntityType.GetProperties();

                foreach ( PropertyInfo propInfo in properties )
                {
                    // Check to make sure property does not have the [NotAudited] attribute
                    if ( AuditProperty( propInfo ) )
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
                    var entityType = Rock.Web.Cache.EntityTypeCache.Read( rockEntityType );
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

        private static bool AuditClass( Type baseType )
        {
            var attribute = baseType.GetCustomAttribute( typeof( NotAuditedAttribute ) );
            return ( attribute == null );
        }

        private static bool AuditProperty( PropertyInfo propertyInfo )
        {
            if ( propertyInfo.GetCustomAttribute( typeof( NotAuditedAttribute ) ) == null &&
                ( !propertyInfo.GetGetMethod().IsVirtual || propertyInfo.Name == "Id" || propertyInfo.Name == "Guid" || propertyInfo.Name == "Order" ) )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// State of entity being changed during a context save
        /// </summary>
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
            /// Gets or sets the state.
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
            /// Initializes a new instance of the <see cref="ContextItem" /> class.
            /// </summary>
            /// <param name="entity">The entity.</param>
            /// <param name="dbEntityEntry">The database entity entry.</param>
            public ContextItem( IEntity entity, DbEntityEntry dbEntityEntry )
            {
                Entity = entity;
                DbEntityEntry = dbEntityEntry;
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
        }
    }
}
