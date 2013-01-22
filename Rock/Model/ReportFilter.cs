using System;
//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Campus POCO Entity.
    /// </summary>
    [Table( "ReportFilter" )]
    [DataContract( IsReference = true )]
    public partial class ReportFilter : Model<ReportFilter>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the type of the filter.
        /// </summary>
        /// <value>
        /// The type of the filter.
        /// </value>
        [DataMember]
        public FilterType FilterType { get; set; }

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
        /// Gets or sets the type of the comparison.
        /// </summary>
        /// <value>
        /// The type of the comparison.
        /// </value>
        [DataMember]
        public FilterComparisonType ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
        public string Value { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        [DataMember]
        public virtual ReportFilter Parent { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the report filters.
        /// </summary>
        /// <value>
        /// The report filters.
        /// </value>
        [DataMember]
        public virtual ICollection<ReportFilter> ReportFilters
        {
            get { return _filters ?? ( _filters = new Collection<ReportFilter>() ); }
            set { _filters = value; }
        }
        private ICollection<ReportFilter> _filters;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public Expression GetExpression( ParameterExpression parameter )
        {
            switch ( FilterType )
            {
                case FilterType.Expression:

                    if ( this.EntityType != null )
                    {
                        foreach ( var serviceEntry in Rock.Reporting.FilterContainer.Instance.Components )
                        {
                            var component = serviceEntry.Value.Value;
                            string componentName = component.GetType().FullName;
                            if ( componentName == this.EntityType.Name )
                            {
                                return component.GetExpression( parameter, this.ComparisonType, this.Value );
                            }
                        }
                    }
                    return null;

                case FilterType.AndCollection:

                    Expression andExp = null;
                    foreach ( var filter in this.ReportFilters )
                    {
                        Expression exp = filter.GetExpression( parameter );
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

                case FilterType.OrCollection:

                    Expression orExp = null;
                    foreach ( var filter in this.ReportFilters )
                    {
                        Expression exp = filter.GetExpression( parameter );
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
            switch(this.FilterType)
            {
                case FilterType.AndCollection:
                    return "And";

                case FilterType.OrCollection:
                    return "Or";

                default:
                    StringBuilder sb = new StringBuilder();

                    foreach ( var serviceEntry in Rock.Reporting.FilterContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;
                        string componentName = component.GetType().FullName;
                        if ( componentName == this.EntityType.Name )
                        {
                            sb.AppendFormat("{0} ",component.Prompt);
                        }
                    }

                    if (ComparisonType != FilterComparisonType.None)
                    {
                        sb.AppendFormat("{0} ", ComparisonType.ConvertToString());
                    }

                    sb.Append(Value);

                    return sb.ToString();
            }
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class ReportFilterConfiguration : EntityTypeConfiguration<ReportFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFilterConfiguration"/> class.
        /// </summary>
        public ReportFilterConfiguration()
        {
            this.HasOptional( r => r.Parent ).WithMany( r => r.ReportFilters).HasForeignKey( r => r.ParentId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Type of Filter entry
    /// </summary>
    public enum FilterType
    {
        /// <summary>
        /// Expression filter
        /// </summary>
        Expression = 0,

        /// <summary>
        /// Collection of Expressions that should be and'd together
        /// </summary>
        AndCollection = 1,

        /// <summary>
        /// Collection of Expressions that should be or'd together
        /// </summary>
        OrCollection = 2
    }

    /// <summary>
    /// Reporting Field Comparison Types
    /// </summary>
    [Flags]
    public enum FilterComparisonType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Equal
        /// </summary>
        Equal = 0x1,

        /// <summary>
        /// Not equal
        /// </summary>
        NotEqual = 0x2,

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
        GreaterThanOrEqual = 0x100,

        /// <summary>
        /// Less than
        /// </summary>
        LessThan = 0x200,

        /// <summary>
        /// Less than or equal
        /// </summary>
        LessThanOrEqual = 0x400,

        /// <summary>
        /// Ends with
        /// </summary>
        EndsWith = 0x800

    }

    #endregion
}
