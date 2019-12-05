using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for Related Entity
    /// </summary>
    public partial class RelatedEntityService
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="T:Rock.Data.Entity" /> target entities (related to the given source entity) for the given entity type and (optionally) also have a matching purpose key.
        /// </summary>
        /// <param name="sourceEntityId">A <see cref="System.Int32" /> representing the source entity identifier.</param>
        /// <param name="sourceEntityTypeId">A <see cref="System.Int32" /> representing the source entity type identifier.</param>
        /// <param name="relatedEntityTypeId">A <see cref="System.Int32" /> representing the related target entity type identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns>
        /// Returns a queryable collection of <see cref="T:Rock.Data.Entity" /> entities.
        /// </returns>
        public IQueryable<IEntity> GetRelatedToSource( int sourceEntityId, int sourceEntityTypeId, int relatedEntityTypeId, string purposeKey = "" )
        {
            EntityTypeCache relatedEntityTypeCache = EntityTypeCache.Get( relatedEntityTypeId );
            if ( relatedEntityTypeCache.AssemblyName != null )
            {
                var query = Queryable()
                        .Where( a => a.SourceEntityTypeId == sourceEntityTypeId
                            && a.SourceEntityId == sourceEntityId
                            && a.TargetEntityTypeId == relatedEntityTypeId );

                if ( purposeKey.IsNullOrWhiteSpace() )
                {
                    query = query.Where( a => string.IsNullOrEmpty( a.PurposeKey ) );
                }
                else
                {
                    query = query.Where( a => a.PurposeKey == purposeKey );
                }

                var rockContext = this.Context as RockContext;
                Type relatedEntityType = relatedEntityTypeCache.GetEntityType();
                if ( relatedEntityType != null )
                {
                    Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( relatedEntityType, rockContext );
                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                    entityQry = query.Join(
                        entityQry,
                        f => f.TargetEntityId,
                        e => e.Id,
                        ( f, e ) => e );

                    return entityQry;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="T:Rock.Data.Entity" /> source entities (related to the given target entity) for the given entity type and (optionally) also have a matching purpose key.
        /// </summary>
        /// <param name="targetEntityId">A <see cref="System.Int32" /> representing the target entity identifier.</param>
        /// <param name="targetEntityTypeId">A <see cref="System.Int32" /> representing the target entity type identifier.</param>
        /// <param name="relatedEntityTypeId">A <see cref="System.Int32" /> representing the related source entity type identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns>
        /// Returns a queryable collection of <see cref="T:Rock.Data.Entity" /> entities.
        /// </returns>
        public IQueryable<IEntity> GetRelatedToTarget( int targetEntityId, int targetEntityTypeId, int relatedEntityTypeId, string purposeKey = "" )
        {
            EntityTypeCache relatedEntityTypeCache = EntityTypeCache.Get( relatedEntityTypeId );
            if ( relatedEntityTypeCache.AssemblyName != null )
            {
                var query = Queryable()
                        .Where( a => a.TargetEntityTypeId == targetEntityTypeId
                            && a.TargetEntityId == targetEntityId
                            && a.SourceEntityTypeId == relatedEntityTypeId );

                if ( purposeKey.IsNullOrWhiteSpace() )
                {
                    query = query.Where( a => string.IsNullOrEmpty( a.PurposeKey ) );
                }
                else
                {
                    query = query.Where( a => a.PurposeKey == purposeKey );
                }

                var rockContext = this.Context as RockContext;
                Type relatedEntityType = relatedEntityTypeCache.GetEntityType();
                if ( relatedEntityType != null )
                {
                    Rock.Data.IService serviceInstance = Reflection.GetServiceForEntityType( relatedEntityType, rockContext );
                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                    entityQry = query.Join(
                        entityQry,
                        f => f.SourceEntityId,
                        e => e.Id,
                        ( f, e ) => e );

                    return entityQry;
                }
            }

            return null;
        }
    }
}
