using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDay.iCal;
using Mono.CSharp;
using Rock.Address;
using Rock.Extension;
using Rock.UniversalSearch;

namespace Rock.AI.Provider
{
    
    public class AIProviderContainer : Container<AIProviderComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<AIProviderContainer> instance =
            new Lazy<AIProviderContainer>( () => new AIProviderContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static AIProviderContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static AIProviderComponent GetComponent( string entityType )
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
        /// Returns the active component
        /// </summary>
        /// <returns></returns>
        public static AIProviderComponent GetActiveComponent()
        {
            return Instance.Components.Select( c => c.Value.Value ).Where( c => c.IsActive ).FirstOrDefault();
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( AIProviderComponent ) )]
        protected override IEnumerable<Lazy<AIProviderComponent, IComponentData>> MEFComponents { get; set; }
    }
}
