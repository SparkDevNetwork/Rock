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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Model
{
    /// <summary>
    /// Represents a filterable dataview in Rock.
    /// </summary>
    [Table( "DataView" )]
    [DataContract]
    public partial class DataView : Model<DataView>, ICategorized
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this DataView is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if it is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name of the DataView.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the DataView.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the DataView
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the DataView.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that this DataView belongs to. If there is no Category, this value will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that this DataView belongs to. If it is not part of a Category this value will be null.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the DataViewFilterId of the root/base <see cref="Rock.Model.DataViewFilter"/> that is used to generate this DataView. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the DataViewFilterId of the root/base <see cref="Rock.Model.DataViewFilter"/> that is used to generate this DataView. If there is 
        /// not a filter on this DataView, this value will be null.
        /// </value>
        [DataMember]
        public int? DataViewFilterId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is used for an optional transformation on this DataView.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is used for an optional transformation on this DataView. If there
        /// is not a transformation on this DataView, this value will be null.
        /// </value>
        [DataMember]
        public int? TransformEntityTypeId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this DataView belongs to
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Category"/> that this DataView belongs to.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the base <see cref="Rock.Model.DataViewFilter"/> that is used to generate this DataView.
        /// </summary>
        /// <value>
        /// The base <see cref="Rock.Model.DataViewFilter"/>.
        /// </value>
        [DataMember]
        public virtual DataViewFilter DataViewFilter { get; set; }


        /// <summary>
        /// Gets or sets the type of the entity used for an optional transformation
        /// </summary>
        /// <value>
        /// The transformation type of entity.
        /// </value>
        [DataMember]
        public virtual EntityType TransformEntityType { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// Gets the parent security authority for the DataView which is its Category
        /// </summary>
        /// <value>
        /// The parent authority of the DataView.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.Category != null )
                {
                    return this.Category;
                }

                return base.ParentAuthority;
            }
        }

        /// <summary>
        /// Determines whether [is authorized for all data view components] [the specified data view].
        /// </summary>
        /// <param name="dataViewAction">The data view action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="authorizationMessage">The authorization message.</param>
        /// <returns></returns>
        public bool IsAuthorizedForAllDataViewComponents( string dataViewAction, Person person, RockContext rockContext, out string authorizationMessage )
        {
            bool isAuthorized = true;
            authorizationMessage = string.Empty;

            // can't edit an existing dataview if not authorized for that dataview
            if ( this.Id != 0 && !this.IsAuthorized( dataViewAction, person ) )
            {
                isAuthorized = false;
                authorizationMessage = Rock.Constants.EditModeMessage.ReadOnlyEditActionNotAllowed( DataView.FriendlyTypeName );
            }

            if ( this.EntityType != null && !this.EntityType.IsAuthorized( Authorization.VIEW, person ) )
            {
                isAuthorized = false;
                authorizationMessage = "INFO: Data view uses an entity type that you do not have access to view.";
            }

            if ( this.DataViewFilter != null && !this.DataViewFilter.IsAuthorized( Authorization.VIEW, person ) )
            {
                isAuthorized = false;
                authorizationMessage = "INFO: Data view contains a filter that you do not have access to view.";
            }

            if ( this.TransformEntityTypeId != null )
            {
                string dataTransformationComponentTypeName = EntityTypeCache.Read( this.TransformEntityTypeId ?? 0 ).GetEntityType().FullName;
                var dataTransformationComponent = Rock.Reporting.DataTransformContainer.GetComponent( dataTransformationComponentTypeName );
                if ( dataTransformationComponent != null )
                {
                    if ( !dataTransformationComponent.IsAuthorized( Authorization.VIEW, person ) )
                    {
                        isAuthorized = false;
                        authorizationMessage = "INFO: Data view contains a data transformation that you do not have access to view.";
                    }
                }
            }

            return isAuthorized;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public IQueryable<IEntity> GetQuery( SortProperty sortProperty, int? databaseTimeoutSeconds,  out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            if ( EntityTypeId.HasValue )
            {
                var cachedEntityType = EntityTypeCache.Read( EntityTypeId.Value );
                if ( cachedEntityType != null && cachedEntityType.AssemblyName != null )
                {
                    Type entityType = cachedEntityType.GetEntityType();

                    if ( entityType != null )
                    {
                        System.Data.Entity.DbContext reportDbContext = Reflection.GetDbContextForEntityType( entityType );
                        if ( databaseTimeoutSeconds.HasValue )
                        {
                            reportDbContext.Database.CommandTimeout = databaseTimeoutSeconds.Value;
                        }

                        IService serviceInstance = Reflection.GetServiceForEntityType( entityType, reportDbContext );

                        if ( serviceInstance != null )
                        {
                            ParameterExpression paramExpression = serviceInstance.ParameterExpression;
                            Expression whereExpression = GetExpression( serviceInstance, paramExpression, out errorMessages );

                            MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( SortProperty ) } );
                            if ( getMethod != null )
                            {
                                if (sortProperty == null)
                                {
                                    // if no sorting is specified, just sort by Id
                                    sortProperty = new SortProperty { Direction = SortDirection.Ascending, Property = "Id" };
                                }

                                var getResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression, sortProperty } );
                                var qry = getResult as IQueryable<IEntity>;

                                return qry;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="paramExpression">The param expression.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public Expression GetExpression( IService serviceInstance, ParameterExpression paramExpression, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var cachedEntityType = EntityTypeCache.Read( EntityTypeId.Value );
            if ( cachedEntityType != null && cachedEntityType.AssemblyName != null )
            {
                Type filteredEntityType = cachedEntityType.GetEntityType();

                if ( filteredEntityType != null )
                {
                    Expression filterExpression = DataViewFilter != null ? DataViewFilter.GetExpression( filteredEntityType, serviceInstance, paramExpression, errorMessages ) : null;

                    Expression transformedExpression = GetTransformExpression( serviceInstance, paramExpression, filterExpression, errorMessages );
                    if ( transformedExpression != null )
                    {
                        return transformedExpression;
                    }

                    return filterExpression;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the transform expression.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="whereExpression">The where expression.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        private Expression GetTransformExpression( IService service, ParameterExpression parameterExpression, Expression whereExpression, List<string> errorMessages )
        {
            if ( this.TransformEntityTypeId.HasValue )
            {
                var entityType = Rock.Web.Cache.EntityTypeCache.Read( this.TransformEntityTypeId.Value );
                if ( entityType != null )
                {
                    var component = Rock.Reporting.DataTransformContainer.GetComponent( entityType.Name );
                    if ( component != null )
                    {
                        try
                        {
                            return component.GetExpression( service, parameterExpression, whereExpression );
                        }
                        catch ( SystemException ex )
                        {
                            errorMessages.Add( string.Format( "{0}: {1}", component.Title, ex.Message ) );
                        }
                    }
                }
            }

            return null;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class DataViewConfiguration : EntityTypeConfiguration<DataView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        public DataViewConfiguration()
        {
            this.HasOptional( v => v.Category ).WithMany().HasForeignKey( v => v.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( v => v.DataViewFilter ).WithMany().HasForeignKey( v => v.DataViewFilterId ).WillCascadeOnDelete( true );
            this.HasRequired( v => v.EntityType ).WithMany().HasForeignKey( v => v.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( e => e.TransformEntityType ).WithMany().HasForeignKey( e => e.TransformEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
