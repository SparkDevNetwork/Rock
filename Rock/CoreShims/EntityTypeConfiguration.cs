using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Rock.Data;

// EFTODO: This is such garbage. This should only be used during pre-migration.
namespace System.Data.Entity.ModelConfiguration
{
    public abstract class EntityTypeConfiguration<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets or sets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public EntityTypeBuilder<TEntity> Builder { get; set; }

        public EntityTypeConfiguration()
        {
            Builder = ContextHelper.ModelBuilder.Entity<TEntity>();
        }

        protected KeyBuilder HasKey( Expression<Func<TEntity, object>> keyExpression )
        {
            return Builder.HasKey( keyExpression );
        }

        protected void Ignore( Expression<Func<TEntity, object>> propertyExpression )
        {
            Builder.Ignore( propertyExpression );
        }

        protected RequiredReferenceNavigationBuilder<TEntity, TRelatedEntity> HasRequired<TRelatedEntity>( Expression<Func<TEntity, TRelatedEntity>> navigationExpression )
            where TRelatedEntity : class
        {
            return new RequiredReferenceNavigationBuilder<TEntity, TRelatedEntity>( Builder.HasOne( navigationExpression ), true );
        }

        protected RequiredReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOptional<TRelatedEntity>( Expression<Func<TEntity, TRelatedEntity>> navigationExpression )
            where TRelatedEntity : class
        {
            return new RequiredReferenceNavigationBuilder<TEntity, TRelatedEntity>( Builder.HasOne( navigationExpression ), false );
        }
    }

    public class RequiredReferenceNavigationBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        private ReferenceNavigationBuilder<TEntity, TRelatedEntity> _referenceNavigationBuilder;

        private bool _isRequired;

        public RequiredReferenceNavigationBuilder( ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder, bool isRequired )
        {
            _referenceNavigationBuilder = referenceNavigationBuilder;
            _isRequired = isRequired;
        }

        public ReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany()
        {
            return _referenceNavigationBuilder.WithMany().IsRequired( _isRequired );
        }

        public ReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany( Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression )
        {
            return _referenceNavigationBuilder.WithMany( navigationExpression ).IsRequired( _isRequired );
        }
    }

    public static class ReferenceCollectionBuilderExtensions
    {
        public static ReferenceCollectionBuilder<TRelatedEntity, TEntity> WillCascadeOnDelete<TRelatedEntity, TEntity>( this ReferenceCollectionBuilder<TRelatedEntity, TEntity> refBuilder, bool cascade )
            where TRelatedEntity : class
            where TEntity : class
        {
            if ( cascade )
            {
                return refBuilder.OnDelete( DeleteBehavior.Cascade );
            }
            else
            {
                return refBuilder.OnDelete( DeleteBehavior.Restrict );
            }
        }
    }
}