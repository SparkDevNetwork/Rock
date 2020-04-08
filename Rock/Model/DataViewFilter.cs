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
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a filter on a <see cref="Rock.Model.DataView"/> in Rock.
    /// </summary>
    [RockDomain( "Reporting" )]
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
        /// Gets or sets the value that the DataViewFilter is filtering by.
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
        [LavaInclude]
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
        /// Determines whether the specified action is authorized but instead of traversing child 
        /// filters (an expensive query), a list of all filters can be passed in and this will be 
        /// checked instead ( See DataViewPicker.LoadDropDownItems() for example of use ).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="allEntityFilters">All entity filters.</param>
        /// <returns></returns>
        public bool IsAuthorized( string action, Person person, List<DataViewFilter> allEntityFilters )
        {
            // First check if user is authorized for model
            bool authorized = base.IsAuthorized( action, person );

            // If viewing, make sure user is authorized to view the component that filter is using
            // and all the child models/components
            if ( authorized && string.Compare( action, Authorization.VIEW, true ) == 0 )
            {
                if ( EntityTypeId.HasValue )
                {
                    var filterComponent = Rock.Reporting.DataFilterContainer.GetComponent( EntityTypeCache.Get( this.EntityTypeId.Value )?.Name );
                    if ( filterComponent != null )
                    {
                        authorized = filterComponent.IsAuthorized( action, person );
                    }
                }

                if ( authorized )
                {
                    foreach ( var childFilter in allEntityFilters.Where( f => f.ParentId == Id ) )
                    {
                        if ( !childFilter.IsAuthorized( action, person, allEntityFilters ) )
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
            return GetExpression( filteredEntityType, serviceInstance, parameter, null, errorMessages );
        }

        /// <summary>
        /// Gets the Linq expression for the DataViewFilter.
        /// </summary>
        /// <param name="filteredEntityType">Type of the filtered entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public virtual Expression GetExpression( Type filteredEntityType, IService serviceInstance, ParameterExpression parameter, DataViewFilterOverrides dataViewFilterOverrides, List<string> errorMessages )
        {
            switch ( ExpressionType )
            {
                case FilterExpressionType.Filter:

                    if ( this.EntityTypeId.HasValue )
                    {
                        var entityType = EntityTypeCache.Get( this.EntityTypeId.Value );
                        if ( entityType != null )
                        {
                            var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                            if ( component != null )
                            {
                                try
                                {
                                    string selection; // A formatted string representing the filter settings: FieldName, <see cref="ComparisonType">Comparison Type</see>, (optional) Comparison Value(s)
                                    var dataViewFilterOverride = dataViewFilterOverrides?.GetOverride( this.Guid );
                                    if ( dataViewFilterOverride != null )
                                    {
                                        if ( dataViewFilterOverride.IncludeFilter == false )
                                        {
                                            return null;
                                        }
                                        else
                                        {
                                            selection = dataViewFilterOverride.Selection;
                                        }
                                    }
                                    else
                                    {
                                        selection = this.Selection;
                                    }

                                    if ( component is IDataFilterWithOverrides )
                                    {
                                        return ( component as IDataFilterWithOverrides ).GetExpressionWithOverrides( filteredEntityType, serviceInstance, parameter, dataViewFilterOverrides, selection );
                                    }
                                    else
                                    {
                                        return component.GetExpression( filteredEntityType, serviceInstance, parameter, selection );
                                    }
                                }
                                catch (SystemException ex)
                                {
                                    ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
                                    errorMessages.Add( string.Format( "{0}: {1}", component.FormatSelection( filteredEntityType, this.Selection ), ex.Message ) );
                                }
                            }
                        }
                    }
                    return null;

                case FilterExpressionType.GroupAll:
                case FilterExpressionType.GroupAnyFalse:

                    Expression andExp = null;
                    foreach ( var filter in this.ChildFilters )
                    {
                        Expression exp = filter.GetExpression( filteredEntityType, serviceInstance, parameter, dataViewFilterOverrides, errorMessages );
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

                    if ( ExpressionType == FilterExpressionType.GroupAnyFalse
                         && andExp != null )
                    {
                        // If only one of the conditions must be false, invert the expression so that it becomes the logical equivalent of "NOT ALL".
                        andExp = Expression.Not( andExp );
                    }

                    return andExp;

                case FilterExpressionType.GroupAny:
                case FilterExpressionType.GroupAllFalse:

                    Expression orExp = null;
                    foreach ( var filter in this.ChildFilters )
                    {
                        Expression exp = filter.GetExpression( filteredEntityType, serviceInstance, parameter, dataViewFilterOverrides, errorMessages );
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

                    if ( ExpressionType == FilterExpressionType.GroupAllFalse
                         && orExp != null )
                    {
                        // If all of the conditions must be false, invert the expression so that it becomes the logical equivalent of "NOT ANY".
                        orExp = Expression.Not( orExp );
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
                if ( EntityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( EntityTypeId.Value );
                    var component = Rock.Reporting.DataFilterContainer.GetComponent( entityType.Name );
                    if ( component != null )
                    {
                        return component.FormatSelection( filteredEntityType, this.Selection );
                    }
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                string conjunction;

                if (this.ExpressionType == FilterExpressionType.GroupAny
                    || this.ExpressionType == FilterExpressionType.GroupAllFalse)
                {
                    // If any of the conditions can be True or all of the conditions must be False, use a logical "OR" operation.
                    conjunction = " OR ";
                }
                else
                {
                    conjunction = " AND ";
                }

                var children = this.ChildFilters.OrderBy( f => f.ExpressionType).ToList();
                for(int i = 0; i < children.Count; i++)
                {
                    string childString = children[i].ToString( filteredEntityType );
                    if ( !string.IsNullOrWhiteSpace( childString ) )
                    {
                        sb.AppendFormat( "{0}{1}", i > 0 ? conjunction : string.Empty, childString );
                    }
                }

                if (children.Count > 1 && Parent != null)
                {
                    sb.Insert(0, "( ");
                    sb.Append(" )");
                }

                if (this.ExpressionType == FilterExpressionType.GroupAllFalse
                    || this.ExpressionType == FilterExpressionType.GroupAnyFalse)
                {
                    sb.Insert( 0, "NOT " );
                }

                return sb.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( this.ExpressionType == FilterExpressionType.Filter && this.EntityTypeId.HasValue )
            {
                return this.ToString( EntityTypeCache.Get( this.EntityTypeId.Value ).GetEntityType() );
            }
            else 
            {
                return this.ExpressionType.ConvertToString();
            }
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

    #region Classes

    /// <summary>
    /// DataViewFilterOverrides with a Dictionary of Filter Overrides where the Key is the DataViewFilter.Guid
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "{DebuggerDisplay}" )]
    public class DataViewFilterOverrides : Dictionary<Guid, DataViewFilterOverride>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataViewFilterOverrides"/> class.
        /// </summary>
        public DataViewFilterOverrides() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataViewFilterOverrides"/> class.
        /// </summary>
        /// <param name="list">The list.</param>
        public DataViewFilterOverrides( List<DataViewFilterOverride> list ) :
            base( list.ToDictionary( k => k.DataFilterGuid, v => v ) )
        { }

        /// <summary>
        /// List of DataViewIds that should not use Persisted Values
        /// </summary>
        /// <value>
        /// The ignore data view persisted values.
        /// </value>
        public HashSet<int> IgnoreDataViewPersistedValues { get; set; } = new HashSet<int>();

        /// <summary>
        /// Gets the override.
        /// </summary>
        /// <param name="dataViewFilterGuid">The data view filter unique identifier.</param>
        /// <returns></returns>
        public DataViewFilterOverride GetOverride( Guid dataViewFilterGuid )
        {
            if ( this.ContainsKey( dataViewFilterGuid ) )
            {
                return this[dataViewFilterGuid];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the debugger display.
        /// </summary>
        /// <value>
        /// The debugger display.
        /// </value>
        private string DebuggerDisplay
        {
            get
            {
                return $@"IgnoreDataViewPersistedValues for DataViewIds: {IgnoreDataViewPersistedValues.ToList().AsDelimited( "," )},DataViewFilterOverrides.Count:{this.Count}";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class DataViewFilterOverride
    {
        /// <summary>
        /// Gets or sets the data filter unique identifier.
        /// </summary>
        /// <value>
        /// The data filter unique identifier.
        /// </value>
        public Guid DataFilterGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include filter].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include filter]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeFilter { get; set; }

        /// <summary>
        /// Gets or sets the selection.
        /// </summary>
        /// <value>
        /// The selection.
        /// </value>
        public string Selection { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"[{DataFilterGuid}]  [{IncludeFilter}] Selection='{Selection}'";
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
        GroupAny = 2,

        /// <summary>
        /// A collection of expressions/conditions where all conditions must be false.  Expressions are combined using a logical OR and the group result must be FALSE.
        /// </summary>
        GroupAllFalse = 3,

        /// <summary>
        /// A collection of expressions/conditions where at least one condition must be false.  Expressions are combined using a logical AND and the group result must be FALSE.
        /// </summary>
        GroupAnyFalse = 4
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
        [EnumOrder( 1 )]
        EqualTo = 0x1,

        /// <summary>
        /// Not equal
        /// </summary>
        [EnumOrder( 2 )]
        NotEqualTo = 0x2,

        /// <summary>
        /// Starts with
        /// </summary>
        /// <remarks>
        /// The order for <see cref="StartsWith"/> is set so that it is displayed before <see cref="EndsWith"/>
        /// </remarks>
        [EnumOrder( 11 )]
        StartsWith = 0x4,

        /// <summary>
        /// Contains
        /// </summary>
        [EnumOrder( 3 )]
        Contains = 0x8,

        /// <summary>
        /// Does not contain
        /// </summary>
        [EnumOrder( 4 )]
        DoesNotContain = 0x10,

        /// <summary>
        /// Is blank
        /// </summary>
        [EnumOrder( 5 )]
        IsBlank = 0x20,

        /// <summary>
        /// Is not blank
        /// </summary>
        [EnumOrder( 6 )]
        IsNotBlank = 0x40,

        /// <summary>
        /// Greater than
        /// </summary>
        [EnumOrder( 7 )]
        GreaterThan = 0x80,

        /// <summary>
        /// Greater than or equal
        /// </summary>
        [EnumOrder( 8 )]
        GreaterThanOrEqualTo = 0x100,

        /// <summary>
        /// Less than
        /// </summary>
        [EnumOrder( 9 )]
        LessThan = 0x200,

        /// <summary>
        /// Less than or equal
        /// </summary>
        [EnumOrder( 10 )]
        LessThanOrEqualTo = 0x400,

        /// <summary>
        /// Ends with
        /// </summary>
        /// /// <remarks>
        /// The order for <see cref="StartsWith"/> is set so that it is displayed before <see cref="EndsWith"/>
        /// </remarks>
        [EnumOrder( 12 )]
        EndsWith = 0x800,

        /// <summary>
        /// Between
        /// </summary>
        [EnumOrder( 13 )]
        Between = 0x1000,

        /// <summary>
        /// Regular Expression
        /// </summary>
        [EnumOrder( 14 )]
        RegularExpression = 0x2000,
    }

    #endregion
}
