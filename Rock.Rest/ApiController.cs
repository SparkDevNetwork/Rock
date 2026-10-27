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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Tasks;
using Rock.Utility.Settings;
using Rock.Web.Cache;

/*
██   ██ ███████ ██    ██ ██ 
██   ██ ██       ██  ██  ██ 
███████ █████     ████   ██ 
██   ██ ██         ██       
██   ██ ███████    ██    ██ 
---------------------------------------
If you're here to add a new REST action to this file, you'll
also need to configure it in Rock.Rest/App_Start/WebApiConfig.cs.

You're welcome!
 */

namespace Rock.Rest
{
    /// <summary>
    /// ApiController for Rock REST Entity endpoints
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ApiController<T> : ApiControllerBase
        where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Gets or sets the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        protected Service<T> Service
        {
            get { return _service; }
            set { _service = value; }
        }

        private Service<T> _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiController{T}"/> class.
        /// </summary>
        /// <param name="service">The service.</param>
        public ApiController( Service<T> service )
        {
            Service = service;

            // Turn off proxy creation by default so that when querying objects through rest, EF does not automatically navigate all child properties for requested objects
            // When adding, updating, or deleting objects, the proxy should be enabled to properly track relationships that should or shouldn't be updated
            SetProxyCreation( false );
        }

        /// <summary>
        /// Queryable GET endpoint
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [EnableQuery]
        public virtual IQueryable<T> Get()
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            var result = Service.Queryable().AsNoTracking();
            return result;
        }

        /// <summary>
        /// GET endpoint to get a single record
        /// </summary>
        /// <param name="id">The Id of the record</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public virtual T GetById( int id )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            T model;
            if ( !Service.TryGet( id, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return model;
        }

        /// <summary>
        /// GET endpoint to get a single record
        /// </summary>
        /// <param name="key">The Id of the record</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [EnableQuery]
        public virtual T Get( [FromODataUri] int key )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            T model;
            if ( !Service.TryGet( key, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return model;
        }

        /// <summary>
        /// Gets records that have a particular attribute value.
        /// Example: api/People/GetByAttributeValue?attributeKey=FirstVisit&amp;value=2012-12-15
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The value.</param>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [ActionName( "GetByAttributeValue" )]
        [EnableQuery]
        public virtual IQueryable<T> GetByAttributeValue( [FromUri] int? attributeId = null, [FromUri] string attributeKey = null, [FromUri] string value = null, [FromUri] bool caseSensitive = false )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            // Value is always required
            if ( value.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The value param is required" );
                throw new HttpResponseException( errorResponse );
            }

            // Either key or id is required, but not both
            var queryByKey = !attributeKey.IsNullOrWhiteSpace();
            var queryById = attributeId.HasValue;

            if ( queryByKey == queryById )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "Either attributeKey or attributeId must be specified, but not both" );
                throw new HttpResponseException( errorResponse );
            }

            // Query for the models that have the value for the attribute
            var rockContext = Service.Context as RockContext;
            var query = Service.Queryable().AsNoTracking();
            var valueComparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if ( queryById )
            {
                query = query.WhereAttributeValue( rockContext,
                    a => a.AttributeId == attributeId && a.Value.Equals( value, valueComparison ) );
            }
            else
            {
                query = query.WhereAttributeValue( rockContext,
                    a => a.Attribute.Key.Equals( attributeKey, StringComparison.OrdinalIgnoreCase ) && a.Value.Equals( value, valueComparison ) );
            }

            return query;
        }

        /// <summary>
        /// Gets items associated with a campus using the EntityCampusFilter model. The Entity must implement ICampusFilterable.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        [ActionName( "GetByCampus" )]
        [EnableQuery]
        [RockObsolete( "17.0" )]
        [Obsolete( "The EntityCampusFilter feature is no longer used and will be removed in the future." )]
        public virtual IQueryable<T> GetByCampus( [FromUri] int campusId )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            if ( !typeof( T ).GetInterfaces().Contains( typeof( ICampusFilterable ) ) )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The model does not support campus filtering." );
                throw new HttpResponseException( errorResponse );
            }

            var rockContext = Service.Context as RockContext;
            var result = Service
                .Queryable()
                .AsNoTracking()
                .WhereCampus( rockContext, campusId );

            return result;
        }

        /// <summary>
        /// POST endpoint. Use this to INSERT a new record
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        public virtual HttpResponseMessage Post( [FromBody] T value )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            if ( value == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            CheckNoUntrackedRelatedEntities( value );

            /*
                10/15/2026 - MSE

                POST always inserts a new record and EF ignores the posted key, so the Id
                in the body never names the record being saved. CheckCanEdit reloads the
                stored record by Id, so a non-zero Id would authorize that existing record
                in place of the posted values. Clearing the Id makes CheckCanEdit authorize
                the posted values as a new record. We reset it instead of returning 400 so
                clients that post a copy of an existing record keep working.

                Reason: Authorize the record that will be inserted, not an existing one.
            */
            value.Id = 0;

            SetProxyCreation( true );

            CheckCanEdit( value );

            Service.Add( value );

            /*
                10/19/2026 - MSE

                A new record never takes the storage columns (such as BinaryFile Path) from
                the client. The FileSystem provider trusts a stored Path that starts with ~,
                so a posted Path could point the record at any file under the web root.
                Clearing them lets Rock set them from the file type when the record is saved.
                Nothing may read ContentStream on the posted entity before this clear, because
                the cached stream would survive it and be written by the storage provider.

                Reason: Clients must not choose where a stored file lives.
            */
            NotClientWritableAttribute.ClearValues( Service.Context, value, typeof( T ) );

            if ( !value.IsValid )
            {
                return ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", value.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
            }

            System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

            Service.Context.SaveChanges();

            var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Created, value.Id );

            //// TODO set response.Headers.Location as per REST POST convention
            // response.Headers.Location = new Uri( Request.RequestUri, "/api/pages/" + page.Id.ToString() );
            return response;
        }

        /// <summary>
        /// PUT endpoint. Use this to UPDATE a record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        public virtual void Put( int id, [FromBody] T value )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            if ( value == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            SetProxyCreation( true );

            T targetModel;
            if ( !Service.TryGet( id, out targetModel ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( targetModel );

            Service.SetValues( value, targetModel );

            /*
                10/19/2026 - MSE

                PUT copies every mapped column, including the storage columns (such as
                BinaryFile Path) that only Rock may set. The FileSystem provider trusts a
                stored Path that starts with ~, so a posted Path could point the record at
                any file under the web root. We put the stored values back instead of
                rejecting the request so clients that echo the whole record keep working.

                Reason: Clients must not choose where a stored file lives.
            */
            NotClientWritableAttribute.RestoreOriginalValues( Service.Context, targetModel, typeof( T ) );

            // The check above authorized the record as it is stored. Authorize it again with the
            // posted values applied, since they may change who is allowed to edit it.
            CheckCanEditAfterUpdate( targetModel );

            if ( targetModel.IsValid )
            {
                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

                Service.Context.SaveChanges();
            }
            else
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", targetModel.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
                throw new HttpResponseException( response );
            }
        }

        /// <summary>
        /// PATCH endpoint. Use this to update a subset of the properties of the record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="values">The values.</param>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        public virtual void Patch( int id, [FromBody] Dictionary<string, object> values )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            // Check that something was sent in the body
            if ( values == null || !values.Keys.Any() )
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, "No values were sent in the body" );
                throw new HttpResponseException( response );
            }
            else if ( values.ContainsKey( "Id" ) )
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, "Cannot set Id" );
                throw new HttpResponseException( response );
            }

            SetProxyCreation( true );

            T targetModel;
            if ( !Service.TryGet( id, out targetModel ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( targetModel );
            var type = targetModel.GetType();
            var properties = type.GetProperties().ToList();

            // Same functionality as Service.SetValues but for a subset of properties
            foreach ( var key in values.Keys )
            {
                if ( properties.Any( p => p.Name.Equals( key ) ) )
                {
                    var property = type.GetProperty( key, BindingFlags.Public | BindingFlags.Instance );
                    var propertyType = Nullable.GetUnderlyingType( property.PropertyType ) ?? property.PropertyType;
                    var currentValue = values[key];

                    /*
                        10/15/2026 - MSE

                        Setting a navigation property changes the related record only when the
                        changes are saved, after the permission checks below have run on the
                        stored foreign key values. Related records are changed by their foreign
                        key properties instead.

                        Reason: Do not let PATCH change related records around the permission checks.
                    */
                    if ( IsNavigationProperty( property ) )
                    {
                        var response = ControllerContext.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            string.Format( "Cannot write {0}", key ) );
                        throw new HttpResponseException( response );
                    }

                    if ( property != null )
                    {
                        /*
                            10/19/2026 - MSE

                            The storage columns (such as BinaryFile Path) are set only by Rock. The
                            FileSystem provider trusts a stored Path that starts with ~, so a patched
                            Path could point the record at any file under the web root. This applies
                            to every caller, like the IsSystem rule.

                            Reason: Clients must not choose where a stored file lives.
                        */
                        if ( NotClientWritableAttribute.GetPropertyNames( typeof( T ) ).Contains( property.Name ) )
                        {
                            var response = ControllerContext.Request.CreateErrorResponse(
                                HttpStatusCode.BadRequest,
                                string.Format( "Modifying property '{0}' is not permitted.", key ) );
                            throw new HttpResponseException( response );
                        }

                        if ( property.GetValue( targetModel ) == currentValue )
                        {
                            continue;
                        }
                        else if ( property.CanWrite )
                        {
                            if ( currentValue == null )
                            {
                                // No need to parse anything
                                property.SetValue( targetModel, null );
                            }
                            else if ( propertyType == typeof( int ) || propertyType == typeof( int? ) || propertyType.IsEnum )
                            {
                                // By default, objects that hold integer values, hold int64, so coerce to int32
                                try
                                {
                                    var int32 = Convert.ToInt32( currentValue );

                                    if ( !propertyType.IsEnum )
                                    {
                                        property.SetValue( targetModel, int32 );
                                    }
                                    else
                                    {
                                        // Convert the int to the enum type per https://stackoverflow.com/a/54627581
                                        var convertedValue = Enum.ToObject( propertyType, int32 );
                                        property.SetValue( targetModel, convertedValue, null );
                                    }
                                }
                                catch ( OverflowException )
                                {
                                    var response = ControllerContext.Request.CreateErrorResponse(
                                        HttpStatusCode.BadRequest,
                                        string.Format( "Cannot cast {0} to int32", key ) );
                                    throw new HttpResponseException( response );
                                }
                            }
                            else
                            {
                                var castedValue = Convert.ChangeType( currentValue, propertyType );
                                property.SetValue( targetModel, castedValue );
                            }
                        }
                        else
                        {
                            var response = ControllerContext.Request.CreateErrorResponse(
                                HttpStatusCode.BadRequest,
                                string.Format( "Cannot write {0}", key ) );
                            throw new HttpResponseException( response );
                        }
                    }
                    else
                    {
                        // This shouldn't happen because we are checking that the property exists.
                        // Just to make sure reflection doesn't fail
                        var response = ControllerContext.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            string.Format( "Cannot find property {0}", key ) );
                        throw new HttpResponseException( response );
                    }
                }
                else
                {
                    var response = ControllerContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format( "{0} does not have attribute {1}", type.BaseType.Name, key ) );
                    throw new HttpResponseException( response );
                }
            }

            // The check above authorized the record as it is stored. Authorize it again with the
            // patched values applied, since they may change who is allowed to edit it.
            CheckCanEditAfterUpdate( targetModel );

            // Verify model is valid before saving
            if ( targetModel.IsValid )
            {
                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );

                Service.Context.SaveChanges();
            }
            else
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Join( ",", targetModel.ValidationResults.Select( r => r.ErrorMessage ).ToArray() ) );
                throw new HttpResponseException( response );
            }
        }

        /// <summary>
        /// DELETE endpoint. To delete the record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        public virtual void Delete( int id )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            SetProxyCreation( true );

            T model;
            if ( !Service.TryGet( id, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( model );

            Service.Delete( model );
            Service.Context.SaveChanges();
        }

        /// <summary>
        /// Gets a list of objects represented by the selected data view
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [ActionName( "DataView" )]
        [EnableQuery]
        public virtual IQueryable<T> GetDataView( int id )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            var dataView = DataViewCache.Get( id );

            ValidateDataView( dataView );

            var paramExpression = Service.ParameterExpression;
            var whereExpression = dataView.GetExpression( Service, paramExpression );
            return Service.GetNoTracking( paramExpression, whereExpression );
        }

        /// <summary>
        /// Determines if the entity id is in the data view
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [ActionName( "InDataView" )]
        [EnableQuery]
        [HttpGet]
        public virtual bool InDataView( int dataViewId, int entityId )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */
            var dataView = DataViewCache.Get( dataViewId );

            ValidateDataView( dataView );

            var qryGroupsInDataView = dataView.GetQuery() as IQueryable<T>;
            qryGroupsInDataView = qryGroupsInDataView.Where( d => d.Id == entityId );

            return qryGroupsInDataView.Any();
        }

        /// <summary>
        /// Checks to makes sure DataView exists, has the correct entityType, and person has View rights, etc
        /// </summary>
        /// <param name="dataView">The data view.</param>
        /// <exception cref="HttpResponseException">
        /// </exception>
        private void ValidateDataView( DataViewCache dataView )
        {
            if ( dataView == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            var controllerEntityType = EntityTypeCache.Get<T>();
            if ( dataView.EntityTypeId != controllerEntityType?.Id )
            {
                HttpResponseMessage errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, $"{dataView.Name} is not a {controllerEntityType?.Name} dataview" );
                throw new HttpResponseException( errorResponse );
            }

            // since DataViews can be secured at the DataView or Category level, specifically check for CanView
            // Note: We can't use CheckCanView because that will end up loading
            // an instance of T with the Id of the DataView. Meaning, it might load
            // a Group with the same Id number and then check security on that.
            if ( !dataView.IsAuthorized( Rock.Security.Authorization.VIEW, GetPerson() ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }

        /// <summary>
        /// Launches a workflow. And optionally passes the entity with selected id as the entity for the workflow
        /// </summary>
        /// <param name="id">The Id of the entity to pass to workflow, if entity cannot be loaded workflow will still be launched but without passing an entity</param>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        /// <param name="workflowName">The Name of the workflow.</param>
        /// <param name="workflowAttributeValues">Optional list of workflow values to set.</param>
        [Authenticate, Secured]
        [ActionName( "LaunchWorkflow" )]
        [HttpPost]
        public void LaunchWorkflow( int id, Guid workflowTypeGuid, string workflowName, [FromBody] Dictionary<string, string> workflowAttributeValues )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            /*
                10/16/2026 - MSE

                Launching a workflow needs VIEW on its workflow type, the same rule as the
                Workflow Launch and Workflow Entry blocks and the v2 launch endpoint. EDIT on
                the controller only says the caller may use this endpoint. The check runs
                before the entity is loaded so an unauthorized call does no work. The caller
                is recorded as the initiator like every other launch path.

                Reason: Staff could launch any workflow type with chosen values through any v1 controller.
            */
            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );
            if ( workflowType == null )
            {
                // An unknown workflow type launches nothing, the same as before.
                return;
            }

            var person = GetPerson();
            if ( !CanLaunchWorkflowType( workflowType, person ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            var initiatorPersonAliasId = person?.PrimaryAliasId;

            T entity = null;
            if ( id > 0 )
            {
                entity = Get( id );
            }

            if ( entity != null )
            {
                entity.LaunchWorkflow( workflowTypeGuid, workflowName, workflowAttributeValues, initiatorPersonAliasId );
            }
            else
            {
                var transaction = new Rock.Transactions.LaunchWorkflowTransaction( workflowTypeGuid, workflowName );
                if ( workflowAttributeValues != null )
                {
                    transaction.WorkflowAttributeValues = workflowAttributeValues;
                }

                transaction.InitiatorPersonAliasId = initiatorPersonAliasId;
                transaction.Enqueue();
            }
        }

        /// <summary>
        /// Launches a workflow. And optionally passes the entity with selected id as the entity for the workflow
        /// </summary>
        /// <param name="id">The Id of the entity to pass to workflow, if entity cannot be loaded workflow will still be launched but without passing an entity</param>
        /// <param name="workflowTypeId">The workflow type identifier.</param>
        /// <param name="workflowName">Name of the workflow.</param>
        /// <param name="workflowAttributeValues">Optional list of workflow values to set.</param>
        [Authenticate, Secured]
        [ActionName( "LaunchWorkflow" )]
        [HttpPost]
        public void LaunchWorkflow( int id, int workflowTypeId, string workflowName, [FromBody] Dictionary<string, string> workflowAttributeValues )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            /*
                10/16/2026 - MSE

                Launching a workflow needs VIEW on its workflow type, the same rule as the
                Workflow Launch and Workflow Entry blocks and the v2 launch endpoint. EDIT on
                the controller only says the caller may use this endpoint. The check runs
                before the entity is loaded so an unauthorized call does no work. The caller
                is recorded as the initiator like every other launch path.

                Reason: Staff could launch any workflow type with chosen values through any v1 controller.
            */
            var workflowType = WorkflowTypeCache.Get( workflowTypeId );
            if ( workflowType == null )
            {
                // An unknown workflow type launches nothing, the same as before.
                return;
            }

            var person = GetPerson();
            if ( !CanLaunchWorkflowType( workflowType, person ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            var initiatorPersonAliasId = person?.PrimaryAliasId;

            T entity = null;
            if ( id > 0 )
            {
                entity = Get( id );
            }

            if ( entity != null )
            {
                entity.LaunchWorkflow( workflowTypeId, workflowName, workflowAttributeValues, initiatorPersonAliasId );
            }
            else
            {
                var transaction = new Rock.Transactions.LaunchWorkflowTransaction( workflowTypeId, workflowName );
                if ( workflowAttributeValues != null )
                {
                    transaction.WorkflowAttributeValues = workflowAttributeValues;
                }

                transaction.InitiatorPersonAliasId = initiatorPersonAliasId;
                transaction.Enqueue();
            }
        }

        /// <summary>
        /// Gets a query of the items that are followed by a specific person. For example, ~/api/Groups/FollowedItems
        /// would return a list of groups that the person is following. Either ?personId= or ?personAliasId= can be
        /// specified to indicate what person you want to see the followed items for.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [ActionName( "FollowedItems" )]
        [EnableQuery]
        public virtual IQueryable<T> GetFollowedItems( int? personId = null, int? personAliasId = null )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            if ( !personId.HasValue )
            {
                if ( personAliasId.HasValue )
                {
                    personId = new PersonAliasService( this.Service.Context as RockContext ).GetPersonId( personAliasId.Value );
                }
            }

            if ( personId.HasValue )
            {
                return Service.GetFollowed( personId.Value );
            }

            throw new HttpResponseException( new HttpResponseMessage( HttpStatusCode.BadRequest ) { ReasonPhrase = "either personId or personAliasId must be specified" } );
        }

        /// <summary>
        /// DELETE to delete the specified attribute value for the record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpDelete]
        public virtual HttpResponseMessage DeleteAttributeValue( int id, string attributeKey )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            return SetAttributeValue( id, attributeKey, string.Empty );
        }

        /// <summary>
        /// POST an attribute value. Use this to set an attribute value for the record
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        /// <exception cref="HttpResponseMessage">
        /// </exception>
        [Authenticate, Secured]
        [HttpPost]
        public virtual HttpResponseMessage SetAttributeValue( int id, string attributeKey, string attributeValue )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            T model;
            if ( !Service.TryGet( id, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( model );

            IHasAttributes modelWithAttributes = model as IHasAttributes;
            if ( modelWithAttributes != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    modelWithAttributes.LoadAttributes( rockContext );
                    Rock.Web.Cache.AttributeCache attributeCache = modelWithAttributes.Attributes.ContainsKey( attributeKey ) ? modelWithAttributes.Attributes[attributeKey] : null;

                    if ( attributeCache != null )
                    {
                        if ( !attributeCache.IsAuthorized( Rock.Security.Authorization.EDIT, this.GetPerson() ) )
                        {
                            throw new HttpResponseException( new HttpResponseMessage( HttpStatusCode.Forbidden ) { ReasonPhrase = string.Format( "Not authorized to edit {0} on {1}", modelWithAttributes.GetType().GetFriendlyTypeName(), attributeKey ) } );
                        }

                        Rock.Attribute.Helper.SaveAttributeValue( modelWithAttributes, attributeCache, attributeValue, rockContext );
                        var response = ControllerContext.Request.CreateResponse( HttpStatusCode.Accepted, modelWithAttributes.Id );
                        return response;
                    }
                    else
                    {
                        throw new HttpResponseException( new HttpResponseMessage( HttpStatusCode.BadRequest ) { ReasonPhrase = string.Format( "{0} does not have a {1} attribute", modelWithAttributes.GetType().GetFriendlyTypeName(), attributeKey ) } );
                    }
                }
            }
            else
            {
                throw new HttpResponseException( new HttpResponseMessage( HttpStatusCode.BadRequest ) { ReasonPhrase = "specified item does not have attributes" } );
            }
        }

        /// <summary>
        /// Sets the Context Cookie to the specified record. Use this to set the Campus Context, Group Context, etc
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [HttpPut, HttpOptions]
        [ActionName( "SetContext" )]
        public virtual HttpResponseMessage SetContext( int id )
        {
            /*
             * 4-JAN-2022 DMV
             *
             * *** WARNING ***
             * Changing this method signature will cause Rock
             * implementations to lose custom security settings.
             * See: https://app.asana.com/0/495431846745457/1201138340787294/f
             *
             */

            var guid = Service.GetGuid( id );
            if ( !guid.HasValue )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            // Set a sitewide context cookie.
            var cookieName = RockRequestContext.GetContextCookieName( null );
            var typeName = typeof( T ).FullName;

            var identifier =
                typeName + "|" +
                id.ToString() + ">" +
                guid.ToString();
            var contextValue = System.Web.HttpUtility.UrlEncode( Rock.Security.Encryption.EncryptString( identifier ) );

            var httpContext = System.Web.HttpContext.Current;
            if ( httpContext == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            try
            {
                var contextCookie = httpContext.Request.Cookies[cookieName] ?? new System.Web.HttpCookie( cookieName );
                var contextItems = contextCookie.Value.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();

                contextItems.AddOrReplace( typeName, contextValue );

                contextCookie.Value = contextItems.ToJson();
                contextCookie.Expires = RockDateTime.Now.AddYears( 1 );

                Rock.Web.UI.RockPage.AddOrUpdateCookie( contextCookie );
            }
            catch
            {
                // Intentionally ignore exception in case JSON [de]serialization fails.
                var errorResponse = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    $"Unable to set context for ID {id}." );
                throw new HttpResponseException( errorResponse );
            }

            return ControllerContext.Request.CreateResponse( HttpStatusCode.OK );
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="entity">The entity.</param>
        protected virtual void CheckCanEdit( T entity )
        {
            if ( entity is ISecured )
            {
                CheckCanEdit( ( ISecured ) entity );
            }
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        protected virtual void CheckCanEdit( ISecured securedModel )
        {
            CheckCanEdit( securedModel, GetPerson() );
        }

        /// <summary>
        /// Checks the can edit.
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        /// <param name="person">The person.</param>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        protected virtual void CheckCanEdit( ISecured securedModel, Person person )
        {
            if ( securedModel != null )
            {
                if ( IsProxy( securedModel ) )
                {
                    if ( !securedModel.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                    {
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
                else
                {
                    // Need to reload using service with a proxy enabled so that if model has custom
                    // parent authorities, those properties can be lazy-loaded and checked for authorization
                    SetProxyCreation( true );
                    ISecured reloadedModel = ( ISecured ) Service.Get( securedModel.Id );
                    if ( reloadedModel != null )
                    {
                        if ( !reloadedModel.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                        {
                            throw new HttpResponseException( HttpStatusCode.Unauthorized );
                        }
                    }
                    else if ( !IsAuthorizedForNewModel( securedModel, person ) )
                    {
                        // The model has no matching record in the database (e.g. an insert where the
                        // Id is 0), so the reload above returns null. Authorize the posted values so
                        // that inserts are held to the same EDIT standard as updates.
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the person is authorized to EDIT a model that does not yet exist in
        /// the database (such as an insert via POST). The posted model is not a dynamic proxy, so its
        /// parent-authority navigation properties cannot be lazy-loaded and evaluated directly. A
        /// proxy copy is created in a separate context and populated from the posted values so that
        /// any parent authorities resolve from the posted foreign keys and the authorization check
        /// evaluates correctly. That context is never saved, so the model being inserted by the
        /// caller is left completely unaffected.
        /// </summary>
        /// <param name="securedModel">The posted model to authorize.</param>
        /// <param name="person">The person to check authorization for.</param>
        /// <returns><c>true</c> if the person is authorized to EDIT the model; otherwise <c>false</c>.</returns>
        private bool IsAuthorizedForNewModel( ISecured securedModel, Person person )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = ( Service<T> ) Activator.CreateInstance( Service.GetType(), rockContext );
                var proxyModel = rockContext.Set<T>().Create();

                /*
                    10/27/2026 - MSE

                    Attach the copy with the DbSet instead of service.Add. SetValues needs the
                    copy to be tracked, but some services run their own rules in Add (for example
                    StepService checks the StepTypeId), and the copy is still empty at this point,
                    so those rules fail for valid values. On POST the real insert still goes
                    through service.Add, so those rules still run on the record that is saved.

                    Reason: Creating a Step through the REST API failed for a valid step type.
                */
                rockContext.Set<T>().Add( proxyModel );
                service.SetValues( ( T ) securedModel, proxyModel );

                /*
                    10/15/2026 - MSE

                    Some IsAuthorized overrides grant EDIT to the record's author (for example
                    Note) or to the record itself (Person matches on Guid), and SetValues copied
                    the posted CreatedByPersonAliasId and Guid onto this copy. A caller could name
                    themselves as author, or post their own Guid, to skip the check. Clear the
                    author and Id and give the copy a new Guid so it is checked as a brand new
                    record. This copy is never saved, so the record being inserted is not affected.

                    Reason: Do not let posted audit values or Guid grant EDIT on a new record.
                */
                proxyModel.Id = 0;
                proxyModel.Guid = Guid.NewGuid();
                if ( proxyModel is IModel proxyAuditModel )
                {
                    proxyAuditModel.CreatedByPersonAliasId = null;
                }

                if ( proxyModel is ISecured securedProxy )
                {
                    return securedProxy.IsAuthorized( Rock.Security.Authorization.EDIT, person );
                }
            }

            // Not an ISecured model, so there is nothing to authorize.
            return true;
        }

        /// <summary>
        /// Checks that the current person can still EDIT the entity after the caller's values have been applied.
        /// </summary>
        /// <param name="entity">The tracked entity with the caller's values applied.</param>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        protected virtual void CheckCanEditAfterUpdate( T entity )
        {
            if ( !( entity is ISecured ) )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                rockContext.Configuration.ProxyCreationEnabled = true;

                // Use a fresh copy so the parent authority and any related records load from the new
                // foreign keys, not the ones already loaded on the tracked model. This context is never saved.
                var service = ( Service<T> ) Activator.CreateInstance( Service.GetType(), rockContext );
                var updatedModel = service.Get( entity.Id );
                if ( updatedModel == null )
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }

                service.SetValues( entity, updatedModel );

                // Must run while this context is open so related records can lazy-load.
                CheckCanEdit( ( ISecured ) updatedModel );

                /*
                    10/15/2026 - MSE

                    The copy above keeps the stored Id and author, so IsAuthorized overrides that
                    grant EDIT to the author (such as Note) still pass after the record is moved
                    under a parent the person could never add to. A record moved under a different
                    parent must meet the same bar as creating it there.

                    Reason: Authorize a parent change like a create under the new parent.
                */
                if ( GetParentAuthorityKey( updatedModel as ISecured ) != GetStoredParentAuthorityKey( entity.Id )
                    && !IsAuthorizedForNewModel( ( ISecured ) entity, GetPerson() ) )
                {
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );
                }
            }
        }

        /// <summary>
        /// Checks that the current person can administrate the REST controller that
        /// serves this request. Use it for records that are not secured themselves but
        /// hold administrator settings.
        /// </summary>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        protected void CheckCanAdministrateController()
        {
            /*
                10/16/2026 - MSE

                Records that are not ISecured have no record security, so the only gate on a
                v1 write is the REST controller rule, and staff hold EDIT on every controller
                through the seeded entity type rule. Controllers for administrator settings
                call this to require ADMINISTRATE on the controller, which by default only
                Rock Administration holds. An unknown controller falls back to a new
                RestController the same way SecuredAttribute does.

                This must stay protected. Web API treats public methods as actions, so a
                public method would add an endpoint and new RestAction rows.

                Reason: Staff must not change administrator settings through v1 REST.
            */
            ISecured controller = RestControllerCache.Get( GetType().FullName );
            if ( controller == null )
            {
                controller = new RestController();
            }

            if ( !controller.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, GetPerson() ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }

        /// <summary>
        /// Gets a key that identifies the parent authorities of the secured item.
        /// </summary>
        /// <param name="secured">The secured item.</param>
        /// <returns>A string made from the type and identifier of each parent authority.</returns>
        private static string GetParentAuthorityKey( ISecured secured )
        {
            if ( secured == null )
            {
                return string.Empty;
            }

            return $"{secured.ParentAuthority?.TypeId}:{secured.ParentAuthority?.Id}|{secured.ParentAuthorityPre?.TypeId}:{secured.ParentAuthorityPre?.Id}";
        }

        /// <summary>
        /// Gets the parent authority key of the model as it is stored in the database.
        /// The caller's tracked model is not saved yet, so this reads the stored parent
        /// without touching the tracked model's navigation properties.
        /// </summary>
        /// <param name="id">The identifier of the model.</param>
        /// <returns>The parent authority key of the stored model.</returns>
        private string GetStoredParentAuthorityKey( int id )
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Configuration.ProxyCreationEnabled = true;

                var service = ( Service<T> ) Activator.CreateInstance( Service.GetType(), rockContext );

                return GetParentAuthorityKey( service.Get( id ) as ISecured );
            }
        }

        /// <summary>
        /// Checks that the posted model does not carry related records that would be
        /// inserted along with it.
        /// </summary>
        /// <param name="value">The posted model that is about to be added.</param>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        protected void CheckNoUntrackedRelatedEntities( T value )
        {
            /*
                10/15/2026 - MSE

                Adding the posted model also inserts every related record it carries (such as
                a note's ChildNotes or a group member's Person), and only the posted model itself
                is authorized. A caller could nest a record they are not allowed to add (such as
                a note in a restricted note type) inside one they are. Related records must be
                posted on their own so they get their own permission check. We return 400
                instead of dropping them so a client never loses data without knowing it.
                Records already loaded by this request (such as the categories the Attributes
                controller loads) are not inserted, so they are allowed.

                Reason: Authorize every record a POST inserts.
            */
            var propertyName = GetUntrackedRelatedEntityPropertyName( value, Service.Context, IsRelatedEntityCollectionAllowed );

            if ( propertyName != null )
            {
                var response = ControllerContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    string.Format( "Related records in {0} cannot be saved with this record. Save them separately.", propertyName ) );
                throw new HttpResponseException( response );
            }
        }

        /// <summary>
        /// Determines whether the related records in the named collection property may be
        /// inserted along with a posted model. Only allow a collection whose records take
        /// their permissions from the posted model, since they are not checked on their own.
        /// </summary>
        /// <param name="propertyName">The name of the collection property.</param>
        /// <returns><c>true</c> if the related records may be inserted with the posted model; otherwise <c>false</c>.</returns>
        protected virtual bool IsRelatedEntityCollectionAllowed( string propertyName )
        {
            return false;
        }

        /// <summary>
        /// Checks to see if the person is authorized to VIEW
        /// </summary>
        /// <param name="securedModel">The secured model.</param>
        /// <param name="person">The person.</param>
        /// <exception cref="System.Web.Http.HttpResponseException">
        /// </exception>
        protected virtual void CheckCanView( ISecured securedModel, Person person )
        {
            if ( securedModel != null )
            {
                if ( IsProxy( securedModel ) )
                {
                    if ( !securedModel.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
                else
                {
                    // Need to reload using service with a proxy enabled so that if model has custom
                    // parent authorities, those properties can be lazy-loaded and checked for authorization
                    SetProxyCreation( true );
                    ISecured reloadedModel = ( ISecured ) Service.Get( securedModel.Id );
                    if ( reloadedModel != null && !reloadedModel.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        throw new HttpResponseException( HttpStatusCode.Unauthorized );
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable proxy creation]. This is needed if lazy loading is needed or Editing/Deleting an entity, etc
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable proxy creation]; otherwise, <c>false</c>.
        /// </value>
        protected void SetProxyCreation( bool enabled )
        {
            Service.Context.Configuration.ProxyCreationEnabled = enabled;
        }

        /// <summary>
        /// Determines whether the specified type is proxy.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected bool IsProxy( object type )
        {
            return type != null && System.Data.Entity.Core.Objects.ObjectContext.GetObjectType( type.GetType() ) != type.GetType();
        }
    }
}
