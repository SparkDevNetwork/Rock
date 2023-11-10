using System;
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class DataView
    {
        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns></returns>
        public IQueryable<IEntity> GetQuery()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="dataViewGetQueryArgs">The data view get query arguments.</param>
        /// <returns></returns>
        /// <exception cref="Rock.Reporting.RockReportingException">
        /// Unable to determine DbContext for {this}
        /// or
        /// Unable to determine ServiceInstance for {this}
        /// or
        /// Unable to determine IService.Get for {this}
        /// </exception>
        public IQueryable<IEntity> GetQuery( DataViewGetQueryArgs dataViewGetQueryArgs )
        {
            var dbContext = dataViewGetQueryArgs.DbContext;
            if ( dbContext == null )
            {
                dbContext = this.GetDbContext();
                if ( dbContext == null )
                {
                    // this could happen if the EntityTypeId id refers to an assembly/type that doesn't exist anymore
                    // we'll just default to new RockContext(), but it'll likely fail when we try to get a ServiceInstance below if the entityType doesn't exist in an assembly
                    dbContext = new RockContext();
                }
            }

            IService serviceInstance = this.GetServiceInstance( dbContext );
            if ( serviceInstance == null )
            {
                var entityTypeCache = EntityTypeCache.Get( this.EntityTypeId ?? 0 );
                throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"Unable to determine ServiceInstance from DataView EntityType {entityTypeCache} for {this}" );
            }

            var databaseTimeoutSeconds = dataViewGetQueryArgs.DatabaseTimeoutSeconds;
            if ( databaseTimeoutSeconds.HasValue )
            {
                dbContext.Database.SetCommandTimeout( databaseTimeoutSeconds.Value );
            }

            var dataViewFilterOverrides = dataViewGetQueryArgs.DataViewFilterOverrides;
            ParameterExpression paramExpression = serviceInstance.ParameterExpression;
            Expression whereExpression = GetExpression( serviceInstance, paramExpression, dataViewFilterOverrides );

            var getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( Rock.Web.UI.Controls.SortProperty ) } );
            if ( getMethod == null )
            {
                throw new RockDataViewFilterExpressionException( this.DataViewFilter, $"Unable to determine IService.Get for Report: {this}" );
            }

            var sortProperty = dataViewGetQueryArgs.SortProperty;

            if ( sortProperty == null )
            {
                // if no sorting is specified, just sort by Id
                sortProperty = new Rock.Web.UI.Controls.SortProperty { Direction = System.Web.UI.WebControls.SortDirection.Ascending, Property = "Id" };
            }

            var getResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression, sortProperty } );
            var qry = getResult as IQueryable<IEntity>;

            return qry;
        }
    }
}
