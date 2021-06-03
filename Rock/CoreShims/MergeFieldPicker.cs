using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    public static class MergeFieldPicker
    {
        /// <summary>
        /// Gets the entity type information from the merge field identifier.
        /// </summary>
        /// <param name="mergeFieldId">The merge field identifier.</param>
        /// <returns></returns>
        public static EntityTypeInfo GetEntityTypeInfoFromMergeFieldId( string mergeFieldId )
        {
            var entityTypeInfo = new EntityTypeInfo();
            var entityTypeParts = mergeFieldId.Split( new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries );
            var entityTypeName = entityTypeParts[0];
            var entityType = EntityTypeCache.Get( entityTypeName, false );
            if ( entityType?.IsEntity == true )
            {
                entityTypeInfo.EntityType = entityType;
            }
            else
            {
                return null;
            }

            if ( entityTypeParts.Length > 1 )
            {
                var entityTypeQualifiersParts = entityTypeParts.Skip( 1 ).ToArray();
                var qualifiers = new List<EntityTypeInfo.EntityTypeQualifier>();

                foreach ( var entityTypeQualifiersPart in entityTypeQualifiersParts )
                {
                    var qualifierParts = entityTypeQualifiersPart.Split( new char[] { '+', ' ' } ).ToArray();

                    if ( qualifierParts.Length == 2 )
                    {
                        qualifiers.Add( new EntityTypeInfo.EntityTypeQualifier( qualifierParts[0], qualifierParts[1] ) );
                    }
                }

                entityTypeInfo.EntityTypeQualifiers = qualifiers.ToArray();
            }

            return entityTypeInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        public class EntityTypeInfo
        {
            /// <summary>
            /// Gets or sets the type of the entity.
            /// </summary>
            /// <value>
            /// The type of the entity.
            /// </value>
            public EntityTypeCache EntityType { get; set; }

            /// <summary>
            /// Gets or sets the entity type qualifiers.
            /// </summary>
            /// <value>
            /// The entity type qualifiers.
            /// </value>
            public EntityTypeQualifier[] EntityTypeQualifiers { get; set; }

            /// <summary>
            /// Gets the merge field identifier which includes the information to add an entity as a merge field. For example "GroupMember, groupMember"
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="entityTypeQualifiers">The entity type qualifiers.</param>
            /// <returns></returns>
            public static string GetMergeFieldId<T>( EntityTypeQualifier[] entityTypeQualifiers )
            {
                StringBuilder entityTypeMergeFieldIdBuilder = new StringBuilder( $"{EntityTypeCache.Get<T>().Name}" );
                if ( entityTypeQualifiers?.Any() == true )
                {
                    foreach ( var entityTypeQualifier in entityTypeQualifiers )
                    {
                        entityTypeMergeFieldIdBuilder.Append( $"~{entityTypeQualifier.Column}+{entityTypeQualifier.Value}" );
                    }
                }

                return entityTypeMergeFieldIdBuilder.ToString();
            }

            /// <summary>
            /// 
            /// </summary>
            public class EntityTypeQualifier
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="EntityTypeQualifier"/> class.
                /// </summary>
                /// <param name="column">The column.</param>
                /// <param name="value">The value.</param>
                public EntityTypeQualifier( string column, string value )
                {
                    this.Column = column;
                    this.Value = value;
                }

                /// <summary>
                /// Gets or sets the column.
                /// </summary>
                /// <value>
                /// The column.
                /// </value>
                public string Column { get; set; }

                /// <summary>
                /// Gets or sets the value.
                /// </summary>
                /// <value>
                /// The value.
                /// </value>
                public string Value { get; set; }
            }
        }
    }
}
