using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Rock.Extension;

namespace Rock.MergeTemplates
{
    public class MergeTemplateProviderContainer : Container<MergeTemplateProvider, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<MergeTemplateProviderContainer> instance =
            new Lazy<MergeTemplateProviderContainer>( () => new MergeTemplateProviderContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static MergeTemplateProviderContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static MergeTemplateProvider GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Gets or sets the MergeTemplateProvider MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( MergeTemplateProvider ) )]
        protected override IEnumerable<Lazy<MergeTemplateProvider, IComponentData>> MEFComponents { get; set; }
    }
}
