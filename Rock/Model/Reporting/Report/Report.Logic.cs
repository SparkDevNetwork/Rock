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
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Report (based off of a <see cref="Rock.Model.DataView"/> in Rock).
    /// </summary>
    public partial class Report : Model<Report>, ICategorized
    {
        /// <summary>
        /// Gets the parent security authority for the Report which is its Category
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
        /// Gets the data source.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="selectComponents">The select components.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetQueryable( reportGetQueryableArgs ) instead" )]
        public List<object> GetDataSource( Type entityType, Dictionary<int, EntityField> entityFields, Dictionary<int, AttributeCache> attributes, Dictionary<int, ReportField> selectComponents, Rock.Web.UI.Controls.SortProperty sortProperty, int? databaseTimeoutSeconds, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            return GetDataSource( entityType, entityFields, attributes, selectComponents, sortProperty, databaseTimeoutSeconds );
        }

        /// <summary>
        /// Gets the data source.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="selectComponents">The select components.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetQueryable( reportGetQueryableArgs ) instead" )]
        public List<object> GetDataSource( Type entityType, Dictionary<int, EntityField> entityFields, Dictionary<int, AttributeCache> attributes, Dictionary<int, ReportField> selectComponents, Rock.Web.UI.Controls.SortProperty sortProperty, int? databaseTimeoutSeconds )
        {
            var reportDbContext = Reflection.GetDbContextForEntityType( entityType );

            ReportGetQueryableArgs reportGetQueryableArgs = new ReportGetQueryableArgs
            {
                ReportDbContext = reportDbContext as Rock.Data.DbContext,
                EntityFields = entityFields,
                Attributes = attributes,
                SelectComponents = selectComponents,
                SortProperty = sortProperty,
                DatabaseTimeoutSeconds = databaseTimeoutSeconds,
            };

            var qry = GetQueryable( reportGetQueryableArgs );

            // enumerate thru the query results and put into a list
            var reportResult = new List<object>();
            var enumerator = ( qry as System.Collections.IEnumerable ).GetEnumerator();
            while ( enumerator.MoveNext() )
            {
                reportResult.Add( enumerator.Current );
            }

            return reportResult;
        }

        /// <summary>
        /// Returns a IQueryable of the report
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="selectComponents">The select components.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetQueryable( ReportGetQueryableArgs reportGetQueryableArgs ) instead" )]
        public IQueryable GetQueryable( Type entityType, Dictionary<int, EntityField> entityFields, Dictionary<int, AttributeCache> attributes, Dictionary<int, ReportField> selectComponents, Rock.Web.UI.Controls.SortProperty sortProperty, int? databaseTimeoutSeconds, out List<string> errorMessages )
        {
            System.Data.Entity.DbContext reportDbContext;
            return GetQueryable( entityType, entityFields, attributes, selectComponents, sortProperty, null, databaseTimeoutSeconds, false, out errorMessages, out reportDbContext );
        }

        /// <summary>
        /// Gets the queryable.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="selectComponents">The select components.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <param name="reportDbContext">The report database context.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetQueryable( ReportGetQueryableArgs reportGetQueryableArgs ) instead" )]
        public IQueryable GetQueryable( Type entityType, Dictionary<int, EntityField> entityFields, Dictionary<int, AttributeCache> attributes, Dictionary<int, ReportField> selectComponents, Rock.Web.UI.Controls.SortProperty sortProperty, int? databaseTimeoutSeconds, out List<string> errorMessages, out System.Data.Entity.DbContext reportDbContext )
        {
            return GetQueryable( entityType, entityFields, attributes, selectComponents, sortProperty, null, databaseTimeoutSeconds, false, out errorMessages, out reportDbContext );
        }

        /// <summary>
        /// Returns a IQueryable of the report
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="selectComponents">The select components.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="isCommunication">if set to <c>true</c> [is communication].</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <param name="reportDbContext">The report database context that was used.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="System.Exception"></exception>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetQueryable( ReportGetQueryableArgs reportGetQueryableArgs ) instead" )]
        public IQueryable GetQueryable( Type entityType, Dictionary<int, EntityField> entityFields, Dictionary<int, AttributeCache> attributes, Dictionary<int, ReportField> selectComponents, Rock.Web.UI.Controls.SortProperty sortProperty, int? databaseTimeoutSeconds, bool isCommunication, out List<string> errorMessages, out System.Data.Entity.DbContext reportDbContext )
        {
            return GetQueryable( entityType, entityFields, attributes, selectComponents, sortProperty, null, databaseTimeoutSeconds, isCommunication, out errorMessages, out reportDbContext );
        }

        /// <summary>
        /// Gets the queryable.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityFields">The entity fields.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="selectComponents">The select components.</param>
        /// <param name="sortProperty">The sort property.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <param name="databaseTimeoutSeconds">The database timeout seconds.</param>
        /// <param name="isCommunication">if set to <c>true</c> [is communication].</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <param name="reportDbContext">The report database context.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetQueryable( ReportGetQueryableArgs reportGetQueryableArgs ) instead" )]
        public IQueryable GetQueryable( Type entityType, Dictionary<int, EntityField> entityFields, Dictionary<int, AttributeCache> attributes, Dictionary<int, ReportField> selectComponents, Rock.Web.UI.Controls.SortProperty sortProperty,
            DataViewFilterOverrides dataViewFilterOverrides, int? databaseTimeoutSeconds, bool isCommunication, out List<string> errorMessages, out System.Data.Entity.DbContext reportDbContext )
        {
            reportDbContext = Reflection.GetDbContextForEntityType( entityType );

            ReportGetQueryableArgs reportGetQueryableArgs = new ReportGetQueryableArgs
            {
                ReportDbContext = reportDbContext as Rock.Data.DbContext,
                EntityFields = entityFields,
                Attributes = attributes,
                SelectComponents = selectComponents,
                SortProperty = sortProperty,
                DataViewFilterOverrides = dataViewFilterOverrides,
                DatabaseTimeoutSeconds = databaseTimeoutSeconds,
                IsCommunication = isCommunication
            };

            errorMessages = null;


            return GetQueryable( reportGetQueryableArgs );
        }

        /// <summary>
        /// Gets the attribute value expression.
        /// </summary>
        /// <param name="attributeValues">The attribute values.</param>
        /// <param name="attributeValueParameter">The attribute value parameter.</param>
        /// <param name="parentIdProperty">The parent identifier property.</param>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <returns></returns>
        private Expression GetAttributeValueExpression( IQueryable<AttributeValue> attributeValues, ParameterExpression attributeValueParameter, Expression parentIdProperty, int attributeId )
        {
            MemberExpression attributeIdProperty = Expression.Property( attributeValueParameter, "AttributeId" );
            MemberExpression entityIdProperty = Expression.Property( attributeValueParameter, "EntityId" );
            Expression attributeIdConstant = Expression.Constant( attributeId );

            Expression attributeIdCompare = Expression.Equal( attributeIdProperty, attributeIdConstant );
            Expression entityIdCompre = Expression.Equal( entityIdProperty, Expression.Convert( parentIdProperty, typeof( int? ) ) );
            Expression andExpression = Expression.And( attributeIdCompare, entityIdCompre );

            var match = new Expression[] {
                Expression.Constant(attributeValues),
                Expression.Lambda<Func<AttributeValue, bool>>( andExpression, new ParameterExpression[] { attributeValueParameter })
            };

            Expression whereExpression = Expression.Call( typeof( Queryable ), "Where", new Type[] { typeof( AttributeValue ) }, match );

            var attributeCache = AttributeCache.Get( attributeId );
            var attributeValueFieldName = "Value";
            Type attributeValueFieldType = typeof( string );
            if ( attributeCache != null )
            {
                attributeValueFieldName = attributeCache.FieldType.Field.AttributeValueFieldName;
                attributeValueFieldType = attributeCache.FieldType.Field.AttributeValueFieldType;
            }

            MemberExpression valueProperty = Expression.Property( attributeValueParameter, attributeValueFieldName );

            Expression valueLambda = Expression.Lambda( valueProperty, new ParameterExpression[] { attributeValueParameter } );

            Expression selectValue = Expression.Call( typeof( Queryable ), "Select", new Type[] { typeof( AttributeValue ), attributeValueFieldType }, whereExpression, valueLambda );

            Expression firstOrDefault = Expression.Call( typeof( Queryable ), "FirstOrDefault", new Type[] { attributeValueFieldType }, selectValue );

            return firstOrDefault;
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
    }
}
