//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
using Rock.DataFilters;

namespace Rock.Model
{
    /// <summary>
    /// DataViewFilter POCO Entity.
    /// </summary>
    [NotAudited]
    [Table( "DataViewFilter" )]
    [DataContract]
    public partial class DataViewFilter : Model<DataViewFilter>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the type of the filter.
        /// </summary>
        /// <value>
        /// The type of the filter.
        /// </value>
        [DataMember]
        public FilterExpressionType ExpressionType { get; set; }

        /// <summary>
        /// Gets or sets the parent id.
        /// </summary>
        /// <value>
        /// The parent id.
        /// </value>
        [DataMember]
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
        public string Selection { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public virtual DataViewFilter Parent { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
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
        /// Gets or sets the child filters.
        /// </summary>
        /// <value>
        /// The child filters.
        /// </value>
        [DataMember]
        public virtual ICollection<DataViewFilter> ChildFilters
        {
            get { return _filters ?? ( _filters = new Collection<DataViewFilter>() ); }
            set { _filters = value; }
        }
        private ICollection<DataViewFilter> _filters;

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // First check if user is authorized for model
            bool authorized = base.IsAuthorized( action, person );

            // If viewing, make sure user is authorized to view the component that filter is using
            // and all the child models/components
            if ( authorized && string.Compare( action, "View", true ) == 0 )
            {
                if ( EntityType != null )
                {
                    var filterComponent = Rock.DataFilters.DataFilterContainer.GetComponent( EntityType.Name );
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

        #endregion

        #region Methods

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public virtual Expression GetExpression( object serviceInstance, ParameterExpression parameter )
        {
            switch ( ExpressionType )
            {
                case FilterExpressionType.Filter:

                    if ( this.EntityTypeId.HasValue )
                    {
                        var entityType = Rock.Web.Cache.EntityTypeCache.Read( this.EntityTypeId.Value );
                        if ( entityType != null )
                        {
                            var component = Rock.DataFilters.DataFilterContainer.GetComponent( entityType.Name );
                            if ( component != null )
                            {
                                return component.GetExpression( serviceInstance, parameter, this.Selection );
                            }
                        }
                    }
                    return null;

                case FilterExpressionType.GroupAll:

                    Expression andExp = null;
                    foreach ( var filter in this.ChildFilters )
                    {
                        Expression exp = filter.GetExpression( serviceInstance, parameter );
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
                        Expression exp = filter.GetExpression( serviceInstance, parameter );
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
        public override string ToString()
        {
            if ( this.ExpressionType == FilterExpressionType.Filter )
            {
                var component = Rock.DataFilters.DataFilterContainer.GetComponent( EntityType.Name );
                if ( component != null )
                {
                    return component.FormatSelection( this.Selection );
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                string conjuction = this.ExpressionType == FilterExpressionType.GroupAll ? " AND " : " OR ";

                var children = this.ChildFilters.OrderBy( f => f.ExpressionType).ToList();
                for(int i = 0; i < children.Count; i++)
                {
                    string childString = children[i].ToString();
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
        /// Collection of Expressions that should be and'd together
        /// </summary>
        GroupAll = 1,

        /// <summary>
        /// Collection of Expressions that should be or'd together
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
        EndsWith = 0x800

    }

    #endregion

}
