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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

using Rock.Data;
using Rock.Reporting;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents a filter on a <see cref="Rock.Model.DataView"/> in Rock.
    /// </summary>
    [NotAudited]
    [Table( "DataViewFilter" )]
    [DataContract]
    public partial class DataViewFilter : Model<DataViewFilter>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the expression type of this DataViewFilter.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.FilterExpressionType" /> that represents the expression type for the filter.  When <c>FilterExpressionType.Filter</c> it represents a filter expression, when <c>FilterExpressionType.GroupAll</c> it means that 
        /// all conditions found in child expressions must be met, when <c>FilterExpressionType.GroupOr</c> it means that at least one condition found in the child filter expressions must be met.
        /// </value>
        [DataMember]
        public FilterExpressionType ExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the DataViewFilterId of the parent DataViewFilter.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DataViewFilterId of the parent DataViewFilter. If this DataViewFilter does not have a parent, this value will be null.
        /// </value>
        [DataMember]
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that either is being filtered by or contains the property that the DataView is being filtered by.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is being used in the filter.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the value that the DataViewFitler is filtering by.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the value to be used as a filter.
        /// </value>
        [DataMember]
        public string Selection { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets sets the parent DataViewFilter.
        /// </summary>
        /// <value>
        /// The parent DataViewFilter.
        /// </value>
        public virtual DataViewFilter Parent { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> that the DataView is being filtered by or that contains the property/properties that the DataView is being filtered by.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that the DataView is being filtered by.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DataViewFilter" /> 
        /// is currently expanded.  This property is only used by the DataView ui to 
        /// track which filters are currently expanded
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual bool Expanded { get; set; }

        /// <summary>
        /// Gets or sets the child DataViewFilters.
        /// </summary>
        /// <value>
        /// The child DataViewFilters.
        /// </value>
        [DataMember]
        public virtual ICollection<DataViewFilter> ChildFilters
        {
            get { return _filters ?? ( _filters = new Collection<DataViewFilter>() ); }
            set { _filters = value; }
        }
        private ICollection<DataViewFilter> _filters;

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">A <see cref="System.String" /> containing the action that is being performed.</param>
        /// <param name="person">the <see cref="Rock.Model.Person" /> who is trying to perform the action.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // First check if user is authorized for model
            bool authorized = base.IsAuthorized( action, person );

            // If viewing, make sure user is authorized to view the component that filter is using
            // and all the child models/components
            if ( authorized && string.Compare( action, Authorization.VIEW, true ) == 0 )
            {
                if ( EntityType != null )
                {
                    var filterComponent = Rock.Reporting.DataFilterContainer.GetComponent( EntityType.Name );
                    if ( filterComponent != null )
                    {
                        authorized = filterComponent.IsAuthorized( action, person );
                    }
                }

                if ( authorized )
                {
                    foreach ( var childFilter in ChildFilters )
                    {
                        if ( !childFilter.IsAuthorized( action, person ) )
                        {
                            return false;
                        }
                    }
                }
            }

            return authorized;
        }
        
        /// <summary>
        /// Gets the Linq expression for the DataViewFilter.
        /// </summary>
        /// <param name="filteredEntityType">The object type of the filtered entity.</param>
        /// <param name="serviceInstance">A <see cref="System.Object"/> that contains the service reference.</param>
        /// <param name="parameter">A <see cref="System.Linq.Expressions.ParameterExpression"/> containing the parameter for the expression.</param>
        /// <param name="errorMessages">A <see cref="System.Collections.Generic.List{String}"/> that contains any error/exception messages that are returned.</param>
        /// <returns></returns>
        public virtual Expression GetExpression( Type filteredEntityType, IService serviceInstance, ParameterExpression parameter, List<string> errorMessages )
        {
            switch ( ExpressionType )
            {
                case FilterExpressionType.Filter:

                    if ( this.EntityTypeId.HasValue )
                    {
                        var entityType = Rock.Web.Cache.EntityTypeCache.Read( this.EntityTypeId.Value );
                        if ( entityType != null )
                        {
                            var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                            if ( component != null )
                            {
                                try
                                {
                                    return component.GetExpression( filteredEntityType, serviceInstance, parameter, this.Selection );
                                }
                                catch (SystemException ex)
                                {
                                    errorMessages.Add( string.Format( "{0}: {1}", component.FormatSelection( filteredEntityType, this.Selection ), ex.Message ) );
                                }
                            }
                        }
                    }
                    return null;

                case FilterExpressionType.GroupAll:

                    Expression andExp = null;
                    foreach ( var filter in this.ChildFilters )
                    {
                        Expression exp = filter.GetExpression( filteredEntityType, serviceInstance, parameter, errorMessages );
                        if ( exp != null )
                        {
                            if ( andExp == null )
                            {
                                andExp = exp;
                            }
                            else
                            {

                                andExp = Expression.AndAlso( andExp, exp );
                            }
                        }
                    }

                    return andExp;

                case FilterExpressionType.GroupAny:

                    Expression orExp = null;
                    foreach ( var filter in this.ChildFilters )
                    {
                        Expression exp = filter.GetExpression( filteredEntityType, serviceInstance, parameter, errorMessages );
                        if ( exp != null )
                        {
                            if ( orExp == null )
                            {
                                orExp = exp;
                            }
                            else
                            {
                                orExp = Expression.OrElse( orExp, exp );
                            }
                        }
                    }

                    return orExp;
            }

            return null;

        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(Type filteredEntityType)
        {
            if ( this.ExpressionType == FilterExpressionType.Filter )
            {
                if ( EntityType != null )
                {
                    var component = Rock.Reporting.DataFilterContainer.GetComponent( EntityType.Name );
                    if ( component != null )
                    {
                        return component.FormatSelection( filteredEntityType, this.Selection );
                    }
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                string conjuction = this.ExpressionType == FilterExpressionType.GroupAll ? " AND " : " OR ";

                var children = this.ChildFilters.OrderBy( f => f.ExpressionType).ToList();
                for(int i = 0; i < children.Count; i++)
                {
                    string childString = children[i].ToString( filteredEntityType );
                    if ( !string.IsNullOrWhiteSpace( childString ) )
                    {
                        sb.AppendFormat( "{0}{1}", i > 0 ? conjuction : string.Empty, childString );
                    }
                }

                if (children.Count > 1 && Parent != null)
                {
                    sb.Insert(0, "( ");
                    sb.Append(" )");
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class DataViewFilterConfiguration : EntityTypeConfiguration<DataViewFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataViewFilterConfiguration"/> class.
        /// </summary>
        public DataViewFilterConfiguration()
        {
            this.HasOptional( r => r.Parent ).WithMany( r => r.ChildFilters).HasForeignKey( r => r.ParentId ).WillCascadeOnDelete( false );
            this.HasOptional( e => e.EntityType ).WithMany().HasForeignKey( e => e.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Type of Filter entry
    /// </summary>
    public enum FilterExpressionType
    {
        /// <summary>
        /// Expression filter
        /// </summary>
        Filter = 0,

        /// <summary>
        /// A collection of expressions/conditions that must match and should be "and'd" together.
        /// </summary>
        GroupAll = 1,

        /// <summary>
        /// A collection of expressions/conditions where at least one condition/expression must match.  Expressions are "or'd" together.
        /// </summary>
        GroupAny = 2
    }

    /// <summary>
    /// Reporting Field Comparison Types
    /// </summary>
    [Flags]
    public enum ComparisonType
    {
        /// <summary>
        /// Equal
        /// </summary>
        EqualTo = 0x1,

        /// <summary>
        /// Not equal
        /// </summary>
        NotEqualTo = 0x2,

        /// <summary>
        /// Starts with
        /// </summary>
        StartsWith = 0x4,

        /// <summary>
        /// Contains
        /// </summary>
        Contains = 0x8,

        /// <summary>
        /// Does not contain
        /// </summary>
        DoesNotContain = 0x10,

        /// <summary>
        /// Is blank
        /// </summary>
        IsBlank = 0x20,

        /// <summary>
        /// Is not blank
        /// </summary>
        IsNotBlank = 0x40,

        /// <summary>
        /// Greater than
        /// </summary>
        GreaterThan = 0x80,

        /// <summary>
        /// Greater than or equal
        /// </summary>
        GreaterThanOrEqualTo = 0x100,

        /// <summary>
        /// Less than
        /// </summary>
        LessThan = 0x200,

        /// <summary>
        /// Less than or equal
        /// </summary>
        LessThanOrEqualTo = 0x400,

        /// <summary>
        /// Ends with
        /// </summary>
        EndsWith = 0x800,

        /// <summary>
        /// Between
        /// </summary>
        Between = 0x1000,

    }

    #endregion

}
