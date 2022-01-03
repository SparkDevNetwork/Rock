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
using System.Linq.Expressions;
using System.Reflection;
using System.Web.UI.WebControls;

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
        /// Gets the queryable.
        /// </summary>
        /// <param name="reportGetQueryableArgs">The report get queryable arguments.</param>
        /// <returns></returns>
        /// <exception cref="Rock.Reporting.RockReportingException">
        /// At least one field must be defined for {this.Name}
        /// or
        /// Unable to get component for {reportField.Value.DataSelectComponentEntityType.Name}
        /// or
        /// Unable to get componentExpression for {reportField.Value.DataSelectComponentEntityType.Name}
        /// or
        /// Unable to get recipientPersonIdExpression for {reportField.Value.Selection}
        /// or
        /// Unable to determine IService.Get method for {serviceInstance}
        /// </exception>
        public IQueryable GetQueryable( ReportGetQueryableArgs reportGetQueryableArgs )
        {
            var reportEntityTypeCache = EntityTypeCache.Get( this.EntityTypeId.Value );

            if ( reportEntityTypeCache?.AssemblyName == null )
            {
                throw new RockReportException( this, $"Unable to determine Report EntityType/Assembly for EntityTypeId { EntityTypeId }" );
            }

            Type reportEntityTypeType = reportEntityTypeCache.GetEntityType();
            if ( reportEntityTypeType == null )
            {
                throw new RockReportException( this, $"Unable to determine Report EntityType for { reportEntityTypeType }." );
            }

            var reportDbContext = reportGetQueryableArgs.ReportDbContext;
            if ( reportDbContext == null )
            {
                reportDbContext = Reflection.GetDbContextForEntityType( reportEntityTypeType ) as Rock.Data.DbContext;
            }

            if (reportDbContext == null)
            {
                throw new RockReportException( this, $"Unable to determine ReportDbContext from Report EntityType {reportEntityTypeType}" );
            }

            IService serviceInstance = Reflection.GetServiceForEntityType( reportEntityTypeType, reportDbContext );
            if ( serviceInstance == null )
            {
                throw new RockReportException( this, $"Unable to determine ServiceInstance from Report EntityType {reportEntityTypeType}" );
            }

            var databaseTimeoutSeconds = reportGetQueryableArgs.DatabaseTimeoutSeconds;

            if ( databaseTimeoutSeconds.HasValue )
            {
                reportDbContext.Database.CommandTimeout = databaseTimeoutSeconds.Value;
            }

            var entityFields = reportGetQueryableArgs.EntityFields;
            var attributes = reportGetQueryableArgs.Attributes;
            var selectComponents = reportGetQueryableArgs.SelectComponents;
            var isCommunication = reportGetQueryableArgs.IsCommunication;

            ParameterExpression paramExpression = serviceInstance.ParameterExpression;
            MemberExpression idExpression = Expression.Property( paramExpression, "Id" );

            // Get AttributeValue queryable and parameter
            var attributeValues = reportDbContext.Set<AttributeValue>();
            ParameterExpression attributeValueParameter = Expression.Parameter( typeof( AttributeValue ), "v" );

            // Create the dynamic type
            var dynamicFields = new Dictionary<string, Type>();
            dynamicFields.Add( "Id", typeof( int ) );
            foreach ( var f in entityFields )
            {
                dynamicFields.Add( string.Format( "Entity_{0}_{1}", f.Value.Name, f.Key ), f.Value.PropertyType );
            }

            foreach ( var a in attributes )
            {
                dynamicFields.Add( string.Format( "Attribute_{0}_{1}", a.Value.Id, a.Key ), a.Value.FieldType.Field.AttributeValueFieldType );
            }

            foreach ( var reportField in selectComponents )
            {
                DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.Value.DataSelectComponentEntityType.Name );
                if ( selectComponent == null )
                {
                    throw new RockReportFieldExpressionException( reportField.Value, $"Unable to determine select component for {reportField.Value.DataSelectComponentEntityType.Name}" );
                }

                dynamicFields.Add( string.Format( "Data_{0}_{1}", selectComponent.ColumnPropertyName, reportField.Key ), selectComponent.ColumnFieldType );
                var customSortProperties = selectComponent.SortProperties( reportField.Value.Selection );
                if ( customSortProperties != null )
                {
                    foreach ( var customSortProperty in customSortProperties.Split( ',' ) )
                    {
                        if ( !string.IsNullOrWhiteSpace( customSortProperty ) )
                        {
                            var customSortPropertyType = reportEntityTypeType.GetPropertyType( customSortProperty );
                            dynamicFields.Add( string.Format( "Sort_{0}_{1}", customSortProperty, reportField.Key ), customSortPropertyType ?? typeof( string ) );
                        }
                    }
                }

                if ( isCommunication && selectComponent is IRecipientDataSelect )
                {
                    dynamicFields.Add( $"Recipient_{selectComponent.ColumnPropertyName}_{reportField.Key}", ( ( IRecipientDataSelect ) selectComponent ).RecipientColumnFieldType );
                }

            }

            if ( dynamicFields.Count == 0 )
            {
                throw new RockReportException( this, $"At least one field must be defined for {this.Name}" );
            }

            Type dynamicType = LinqRuntimeTypeBuilder.GetDynamicType( dynamicFields );
            ConstructorInfo methodFromHandle = dynamicType.GetConstructor( Type.EmptyTypes );

            // Bind the dynamic fields to their expressions
            var bindings = new List<MemberAssignment>();
            bindings.Add( Expression.Bind( dynamicType.GetField( "id" ), idExpression ) );

            foreach ( var f in entityFields )
            {
                bindings.Add( Expression.Bind( dynamicType.GetField( string.Format( "entity_{0}_{1}", f.Value.Name, f.Key ) ), Expression.Property( paramExpression, f.Value.Name ) ) );
            }

            foreach ( var a in attributes )
            {
                bindings.Add( Expression.Bind( dynamicType.GetField( string.Format( "attribute_{0}_{1}", a.Value.Id, a.Key ) ), GetAttributeValueExpression( attributeValues, attributeValueParameter, idExpression, a.Value.Id ) ) );
            }

            foreach ( var reportFieldKeyValue in selectComponents )
            {
                var reportField = reportFieldKeyValue.Value;
                DataSelectComponent selectComponent = DataSelectContainer.GetComponent( reportField.DataSelectComponentEntityType.Name );
                if ( selectComponent == null )
                {
                    throw new RockReportFieldExpressionException( reportField, $"Unable to get component for {reportField.DataSelectComponentEntityType.Name} " );
                }

                var componentExpression = selectComponent.GetExpression( reportDbContext, idExpression, reportField.Selection ?? string.Empty );
                if ( componentExpression == null )
                {
                    throw new RockReportFieldExpressionException( reportField, $"Unable to get componentExpression for {reportField.DataSelectComponentEntityType.Name} " );
                }

                var componentFieldInfo = dynamicType.GetField( $"data_{selectComponent.ColumnPropertyName}_{reportFieldKeyValue.Key}" );
                bindings.Add( Expression.Bind( componentFieldInfo, componentExpression ) );

                if ( isCommunication && selectComponent is IRecipientDataSelect )
                {
                    var recipientPersonIdExpression = ( ( IRecipientDataSelect ) selectComponent ).GetRecipientPersonIdExpression( reportDbContext, idExpression, reportField.Selection ?? string.Empty );

                    if ( recipientPersonIdExpression == null )
                    {
                        throw new RockReportFieldExpressionException( reportField, $"Unable to get recipientPersonIdExpression for {reportField.Selection} " );
                    }

                    var recipientFieldInfo = dynamicType.GetField( $"recipient_{selectComponent.ColumnPropertyName}_{reportFieldKeyValue.Key}" );
                    bindings.Add( Expression.Bind( recipientFieldInfo, recipientPersonIdExpression ) );
                }

                var customSortProperties = selectComponent.SortProperties( reportField.Selection );
                if ( !string.IsNullOrEmpty( customSortProperties ) )
                {
                    foreach ( var customSortProperty in customSortProperties.Split( ',' ) )
                    {
                        var customSortPropertyParts = customSortProperty.Split( '.' );
                        MemberInfo memberInfo = dynamicType.GetField( string.Format( "sort_{0}_{1}", customSortProperty, reportFieldKeyValue.Key ) );
                        Expression memberExpression = null;
                        foreach ( var customSortPropertyPart in customSortPropertyParts )
                        {
                            memberExpression = Expression.Property( memberExpression ?? paramExpression, customSortPropertyPart );
                        }

                        bindings.Add( Expression.Bind( memberInfo, memberExpression ) );
                    }
                }
            }

            ConstructorInfo constructorInfo = dynamicType.GetConstructor( Type.EmptyTypes );
            NewExpression newExpression = Expression.New( constructorInfo );
            MemberInitExpression memberInitExpression = Expression.MemberInit( newExpression, bindings );
            LambdaExpression selector = Expression.Lambda( memberInitExpression, paramExpression );

            // NOTE: having a NULL Dataview is OK, it just means to not filter the results
            Expression whereExpression = null;
            if ( this.DataView != null )
            {
                var dataViewFilterOverrides = reportGetQueryableArgs.DataViewFilterOverrides;
                whereExpression = this.DataView.GetExpression( serviceInstance, paramExpression, dataViewFilterOverrides );
            }

            MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( Rock.Web.UI.Controls.SortProperty ), typeof( int? ) } );
            if ( getMethod == null )
            {
                throw new RockReportException( this, $"Unable to determine IService.Get method for {serviceInstance}" );
            }

            var getResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression, null, null } );
            var qry = getResult as IQueryable<IEntity>;
            var qryExpression = qry.Expression;
            var sortProperty = reportGetQueryableArgs.SortProperty;

            // apply the OrderBy clauses to the Expression from whatever columns are specified in sortProperty.Property
            string orderByMethod = "OrderBy";
            if ( sortProperty == null )
            {
                // if no sorting was specified, sort by Id
                sortProperty = new Web.UI.Controls.SortProperty { Direction = SortDirection.Ascending, Property = "Id" };
            }

            /*
             NOTE:  The sort property sorting rules can be a little confusing. Here is how it works:
             * - SortProperty.Direction of Ascending means sort exactly as what the Columns specification says
             * - SortProperty.Direction of Descending means sort the _opposite_ of what the Columns specification says
             * Examples:
             *  1) SortProperty.Property "LastName desc, FirstName, BirthDate desc" and SortProperty.Direction = Ascending
             *     OrderBy should be: "order by LastName desc, FirstName, BirthDate desc"
             *  2) SortProperty.Property "LastName desc, FirstName, BirthDate desc" and SortProperty.Direction = Descending
             *     OrderBy should be: "order by LastName, FirstName desc, BirthDate"
             */

            foreach ( var column in sortProperty.Property.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                string propertyName;

                var direction = sortProperty.Direction;
                if ( column.EndsWith( " desc", StringComparison.OrdinalIgnoreCase ) )
                {
                    propertyName = column.Left( column.Length - 5 );

                    // if the column ends with " desc", toggle the direction if sortProperty is Descending
                    direction = sortProperty.Direction == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
                }
                else
                {
                    propertyName = column;
                }

                string methodName = direction == SortDirection.Descending ? orderByMethod + "Descending" : orderByMethod;

                // Call OrderBy on whatever the Expression is for that Column
                var sortMember = bindings.FirstOrDefault( a => a.Member.Name.Equals( propertyName, StringComparison.OrdinalIgnoreCase ) );
                LambdaExpression sortSelector = Expression.Lambda( sortMember.Expression, paramExpression );
                qryExpression = Expression.Call( typeof( Queryable ), methodName, new Type[] { qry.ElementType, sortSelector.ReturnType }, qryExpression, sortSelector );
                orderByMethod = "ThenBy";
            }

            var selectExpression = Expression.Call( typeof( Queryable ), "Select", new Type[] { qry.ElementType, dynamicType }, qryExpression, selector );

            var query = qry.Provider.CreateQuery( selectExpression ).AsNoTracking();

            // cast to a dynamic so that we can do a Queryable.Take (the compiler figures out the T in IQueryable at runtime)
            dynamic dquery = query;

            if ( FetchTop.HasValue )
            {
                dquery = Queryable.Take( dquery, FetchTop.Value );
            }

            return dquery as IQueryable;
        }
    }
}
