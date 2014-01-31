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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using Rock.Model;
using Rock.Workflow;

namespace Rock.Data
{
    /// <summary>
    /// Generic POCO service class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Service<T> : IService where T : Rock.Data.Entity<T>, new()
    {

        #region Fields

        private IRepository<T> _repository;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the save messages.
        /// </summary>
        /// <value>
        /// The save messages.
        /// </value>
        public virtual List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Gets the Repository.
        /// </summary>
        public IRepository<T> Repository
        {
            get { return _repository; }
        }

        /// <summary>
        /// Gets the rock context.
        /// </summary>
        /// <value>
        /// The rock context.
        /// </value>
        public RockContext RockContext
        {
            get
            {
                if (this.Repository is EFRepository<T>)
                {
                    return ( this.Repository as EFRepository<T> ).Context as RockContext;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a LINQ expression parameter.
        /// </summary>
        /// <value>
        /// The parameter expression.
        /// </value>
        public ParameterExpression ParameterExpression
        {
            get
            {
                return Expression.Parameter( typeof( T ), "p" );
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Service&lt;T&gt;"/> class.
        /// </summary>
        public Service()
        {
            var factory = new RepositoryFactory<T>();
            _repository = factory.FindRepository();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public Service( IRepository<T> repository )
        {
            _repository = repository;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service{T}"/> class.
        /// </summary>
        /// <param name="dbContext">The db context.</param>
        public Service( System.Data.Entity.DbContext dbContext )
            : this( new EFRepository<T>( dbContext ) )
        {
        }

        #endregion

        #region Methods

        #region Queryable

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> Queryable()
        {
            return _repository.AsQueryable();
        }

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> list of all models
        /// with eager loading of properties specified in includes
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> Queryable( string includes )
        {
            return _repository.AsQueryable( includes );
        }

        #endregion

        #region Get Methods

        /// <summary>
        /// Gets the model with the id value
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        public virtual T Get( int id )
        {
            return _repository.FirstOrDefault( t => t.Id == id );
        }

        /// <summary>
        /// Gets the model with the Guid value
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public virtual T Get( Guid guid )
        {
            return _repository.FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets a list of items that match the specified expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public IQueryable<T> Get( ParameterExpression parameterExpression, Expression whereExpression )
        {
            return Get( parameterExpression, whereExpression, null );
        }

        /// <summary>
        /// Gets a list of items that match the specified expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public IQueryable<T> Get( ParameterExpression parameterExpression, Expression whereExpression, Rock.Web.UI.Controls.SortProperty sortProperty )
        {
            if ( parameterExpression != null && whereExpression != null )
            {
                var lambda = Expression.Lambda<Func<T, bool>>( whereExpression, parameterExpression );
                var queryable = this.Queryable().Where( lambda );
                return sortProperty != null ? queryable.Sort( sortProperty ) : queryable;
            }

            return this.Queryable();
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public List<T> GetList( ParameterExpression parameterExpression, Expression whereExpression, Rock.Web.UI.Controls.SortProperty sortProperty )
        {
            return Get( parameterExpression, whereExpression, sortProperty ).ToList();
        }

        /// <summary>
        /// Gets the ids.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public IQueryable<int> GetIds( ParameterExpression parameterExpression, Expression whereExpression )
        {
            return Get( parameterExpression, whereExpression, null ).Select( t => t.Id );
        }

        /// <summary>
        /// Trys to get the model with the id value
        /// </summary>
        /// <returns></returns>
        public virtual bool TryGet( int id, out T item )
        {
            item = Get( id );
            if ( item == null )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets entities from a list of ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetByIds( List<int> ids )
        {
            return this.Queryable().Where( t => ids.Contains( t.Id ) ).ToList();
        }

        /// <summary>
        /// Gets entities from a list of guids
        /// </summary>
        /// <param name="guids">The guids.</param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetByGuids( List<Guid> guids )
        {
            return this.Queryable().Where( t => guids.Contains( t.Guid ) ).ToList();
        }

        /// <summary>
        /// Gets the model by the public encrypted key.
        /// </summary>
        /// <param name="encryptedKey">The encrypted key.</param>
        /// <returns></returns>
        public virtual T GetByEncryptedKey( string encryptedKey )
        {
            string publicKey = Rock.Security.Encryption.DecryptString( encryptedKey );
            return GetByPublicKey( publicKey );
        }

        /// <summary>
        /// Gets the model by URL encoded key.
        /// </summary>
        /// <param name="encodedKey">The encoded key.</param>
        /// <returns></returns>
        public virtual T GetByUrlEncodedKey( string encodedKey )
        {
            string key = encodedKey.Replace( '!', '%' );
            return GetByEncryptedKey( System.Web.HttpUtility.UrlDecode( key ) );
        }

        /// <summary>
        /// Gets the model by the public un-encrypted key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <returns></returns>
        public virtual T GetByPublicKey( string publicKey )
        {
            try
            {
                string[] idParts = publicKey.Split( '>' );
                if ( idParts.Length == 2 )
                {
                    int id = Int32.Parse( idParts[0] );
                    Guid guid = new Guid( idParts[1] );

                    T model = Get( id );

                    if ( model != null && model.Guid.CompareTo( guid ) == 0 )
                    {
                        return model;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Add

        /// <summary>
        /// Attaches the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public virtual void Attach( T item )
        {
            _repository.Attach( item );
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual bool Add( T item )
        {
            return Add( item, GetPersonAlias(null) );
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personAlias">The person alias. (CurrentPersonAlias if used in RockBlock)</param>
        /// <returns></returns>
        public virtual bool Add( T item, PersonAlias personAlias )
        {
            bool cancel = false;
            item.RaiseAddingEvent( out cancel, personAlias );
            if ( !cancel )
            {
                _repository.Add( item );
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual bool Save( T item )
        {
            return Save( item, GetPersonAlias( null ) );
        }

        /// <summary>
        /// Saves the specified itemsing alias.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personAlias">The person alias (if called from a RockBlock, use CurrentPersonAlias property).</param>
        /// <returns></returns>
        public virtual bool Save(T item, PersonAlias personAlias)
        {
            int? personAliasId = null;
            if ( personAlias != null )
            {
                personAliasId = personAlias.Id;
            } 

            ErrorMessages = new List<string>();

            if ( !TriggerWorkflows( item, WorkflowTriggerType.PreSave, personAlias ) )
            {
                return false;
            }

            if ( item != null && item.Guid == Guid.Empty )
                item.Guid = Guid.NewGuid();

            // Update the created by and modified by fields
            IModel model = item as IModel;
            if (model != null)
            {
                if ( model.Id <= 0 )
                {
                    if ( !model.CreatedDateTime.HasValue )
                    {
                        model.CreatedDateTime = RockDateTime.Now;
                    }
                    if ( !model.CreatedByPersonAliasId.HasValue )
                    {
                        model.CreatedByPersonAliasId = personAliasId;
                    }
                }

                model.ModifiedByPersonAliasId = personAliasId;
                model.ModifiedDateTime = RockDateTime.Now;
            }

            List<Audit> audits;
            List<string> errorMessages;

            if ( _repository.Save( personAlias, out audits, out errorMessages ) )
            {
                if ( audits != null && audits.Count > 0 )
                {
                    var transaction = new Rock.Transactions.AuditTransaction();
                    transaction.Audits = audits;
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                }

                TriggerWorkflows( item, WorkflowTriggerType.PostSave, personAlias );

                return true;
            }
            else
            {
                ErrorMessages = errorMessages;
                return false;
            }
        }

        #endregion

        #region Reorder

        /// <summary>
        /// Reorders the specified items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        /// <param name="personAlias">The person alias.</param>
        public virtual void Reorder( List<T> items, int oldIndex, int newIndex, PersonAlias personAlias )
        {
            T movedItem = items[oldIndex];
            items.RemoveAt( oldIndex );
            if ( newIndex >= items.Count )
                items.Add( movedItem );
            else
                items.Insert( newIndex, movedItem );

            int order = 0;
            foreach ( T item in items )
            {
                IOrdered orderedItem = item as IOrdered;
                if ( orderedItem != null )
                {
                    if ( orderedItem.Order != order )
                    {
                        orderedItem.Order = order;
                        Save( item, personAlias );
                    }
                }
                order++;
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the specified item.  Will try to determine current person 
        /// alias from HttpContext.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public virtual bool Delete (T item )
        {
            return Delete( item, GetPersonAlias( null ) );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        public virtual bool Delete(T item, PersonAlias personAlias)
        {
            ErrorMessages = new List<string>();

            if ( !TriggerWorkflows( item, WorkflowTriggerType.PreDelete, personAlias ) )
            {
                return false;
            }

            bool cancel = false;
            item.RaiseDeletingEvent( out cancel, personAlias );
            if ( !cancel )
            {
                _repository.Delete( item );

                TriggerWorkflows( item, WorkflowTriggerType.PostDelete, personAlias );

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Workflows

        /// <summary>
        /// Triggers the workflows.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        private bool TriggerWorkflows( IEntity entity, WorkflowTriggerType triggerType, PersonAlias personAlias )
        {
            Dictionary<string, PropertyInfo> properties = null;

            foreach ( var trigger in TriggerCache.Triggers( entity.TypeName, triggerType ).Where( t => t.IsActive == true ) )
            {
                bool match = true;

                if ( !string.IsNullOrWhiteSpace( trigger.EntityTypeQualifierColumn ) )
                {
                    if ( properties == null )
                    {
                        properties = new Dictionary<string,PropertyInfo>();
                        foreach ( PropertyInfo propertyInfo in entity.GetType().GetProperties() )
                        {
                            properties.Add( propertyInfo.Name.ToLower(), propertyInfo );
                        }
                    }

                    match = ( properties.ContainsKey( trigger.EntityTypeQualifierColumn.ToLower() ) &&
                        properties[trigger.EntityTypeQualifierColumn.ToLower()].GetValue( entity, null ).ToString()
                            == trigger.EntityTypeQualifierValue );
                }

                if ( match )
                {
                    if ( triggerType == WorkflowTriggerType.PreSave || triggerType == WorkflowTriggerType.PreDelete )
                    {
                        var workflowTypeService = new WorkflowTypeService();
                        var workflowType = workflowTypeService.Get( trigger.WorkflowTypeId );

                        if ( workflowType != null )
                        {
                            var workflow = Rock.Model.Workflow.Activate( workflowType, trigger.WorkflowName );

                            List<string> workflowErrors;
                            if ( !workflow.Process( entity, out workflowErrors ) )
                            {
                                ErrorMessages.AddRange( workflowErrors );
                                return false;
                            }
                            else
                            {
                                if ( workflowType.IsPersisted )
                                {
                                    var workflowService = new Rock.Model.WorkflowService();
                                    workflowService.Add( workflow, personAlias );
                                    workflowService.Save( workflow, personAlias );
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

        #endregion 

        #region Audits

        /// <summary>
        /// All the audits made to the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual IQueryable<Audit> Audits( T entity )
        {
            return _repository.Audits( entity );
        }

        /// <summary>
        /// All the audits made to the entity.
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual IQueryable<Audit> Audits( int entityTypeId, int entityId )
        {
            return _repository.Audits( entityTypeId, entityId );
        }

        #endregion

        #region Other

        /// <summary>
        /// Creates a raw sql query that will return entities
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<T> ExecuteQuery( string query, params object[] parameters )
        {
            return _repository.ExecuteQuery( query, parameters );
        }

        /// <summary>
        /// Anies the specified parameter expression.
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <returns></returns>
        public bool Any( ParameterExpression parameterExpression, Expression whereExpression )
        {
            var lambda = Expression.Lambda<Func<T, bool>>( whereExpression, parameterExpression );
            return this.Queryable().Any( lambda );
        }

        /// <summary>
        /// Transforms the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="transformation">The transformation.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <returns></returns>
        public IQueryable<T> Transform( IQueryable<T> source, Rock.Reporting.DataTransformComponent<T> transformation, Rock.Web.UI.Controls.SortProperty sortProperty = null )
        {
            var paramExpression = Expression.Parameter( source.ElementType, "p" );
            var whereExpression = transformation.GetExpression( this, source, paramExpression );
            return Get( paramExpression, whereExpression, sortProperty );
        }

        /// <summary>
        /// Copies the Values from a Source Entity into a Target Entity
        /// </summary>
        /// <param name="sourceItem">The source item.</param>
        /// <param name="targetItem">The target item.</param>
        public virtual void SetValues( T sourceItem, T targetItem )
        {
            _repository.SetValues( sourceItem, targetItem );
        }

        private PersonAlias GetPersonAlias( int? personId )
        {
            PersonAlias personAlias = null;
            var currentPerson = GetCurrentPerson();

            if ( currentPerson != null && (!personId.HasValue || currentPerson.Id == personId.Value))
            {
                personAlias = currentPerson.PrimaryAlias;
            }

            if (personAlias == null && personId.HasValue)
            {
                personAlias = new PersonAliasService().Queryable().FirstOrDefault( a => a.AliasPersonId == personId.Value );
            }

            return personAlias;
        }

        private Person GetCurrentPerson()
        {
            HttpContext context = HttpContext.Current;
            if ( context != null && context.Items.Contains( "CurrentPerson" ) )
            {
                var currentPerson = context.Items["CurrentPerson"] as Person;
                if ( currentPerson != null )
                {
                    return currentPerson;
                }
            }
            return null;
        }

        #endregion

        #endregion

    }

    /// <summary>
    /// Service class for non entity specific methods
    /// </summary>
    public class Service
    {

        #region Fields

        private EFRepository _repository;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the Repository.
        /// </summary>
        public EFRepository Repository
        {
            get { return _repository; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Service&lt;T&gt;"/> class.
        /// </summary>
        public Service()
        {
            _repository = new EFRepository();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public Service( EFRepository repository )
        {
            _repository = repository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a raw SQL query that will return elements of the given type.  The
        /// type can be any type that has properties that match the names of the columns
        /// returned from the query, or can be a simple primitive type. The type does
        /// not have to be an entity type. The results of this query are never tracked
        /// by the context even if the type of object returned is an entity type. Use
        /// the SqlQuery(System.String,System.Object[]) method
        /// to return entities that are tracked by the context.
        /// </summary>
        /// <param name="elementType">Type of the element.</param>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IEnumerable ExecuteQuery( Type elementType, string query, params object[] parameters )
        {
            return _repository.ExecuteQuery( elementType, query, parameters );
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public SqlCommand GetCommand(string query, CommandType commandType, Dictionary<string, object> parameters)
        {
            return _repository.GetCommand( query, commandType, parameters);
        }

        /// <summary>
        /// Gets a data set.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataSet GetDataSet( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            return _repository.GetDataSet( query, commandType, parameters );
        }

        /// <summary>
        /// Gets a data table.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataTable GetDataTable( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            return _repository.GetDataTable( query, commandType, parameters );
        }

        /// <summary>
        /// Gets a data reader.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IDataReader GetDataReader( string query, CommandType commandType, Dictionary<string, object> parameters )
        {
            return _repository.GetDataReader( query, commandType, parameters );
        }

        /// <summary>
        /// Executes the SQL command.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int ExecuteCommand( string query, params object[] parameters )
        {
            return _repository.ExecuteCommand( query, parameters );
        }

        #endregion

    }

}