using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Rock.Extension;

namespace Rock.MergeTemplates
{
    /// <summary>
    /// 
    /// </summary>
    public class MergeTemplateTypeContainer : Container<MergeTemplateType, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<MergeTemplateTypeContainer> instance =
            new Lazy<MergeTemplateTypeContainer>( () => new MergeTemplateTypeContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static MergeTemplateTypeContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static MergeTemplateType GetComponent( string entityType )
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
        /// Gets or sets the MergeTemplateType MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( MergeTemplateType ) )]
        protected override IEnumerable<Lazy<MergeTemplateType, IComponentData>> MEFComponents { get; set; }
    }
}
