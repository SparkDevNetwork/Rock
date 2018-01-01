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
using System.ComponentModel.Composition;

using Rock.Extension;
using Rock.Model;
using Rock.Security;

namespace Rock.Pbx
{
    /// <summary>
    /// MEF Container class for PBX Components
    /// </summary>
    public class PbxContainer : Container<PbxComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<PbxContainer> instance =
            new Lazy<PbxContainer>( () => new PbxContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static PbxContainer Instance
        {
            get {
                if ( instance != null )
                {
                    return instance.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        ///// <summary>
        ///// Gets a value indicating whether [indexing enabled].
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if [indexing enabled]; otherwise, <c>false</c>.
        ///// </value>
        //public static bool IndexingEnabled
        //{
        //    get
        //    {
        //        return GetActiveComponent() != null;
        //    }
        //}

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static PbxComponent GetComponent( string entityType )
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
        /// Gets the active component.
        /// </summary>
        /// <returns></returns>
        public static PbxComponent GetActiveComponent()
        {
            foreach ( var indexType in PbxContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive )
                {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the active component with origination support.
        /// </summary>
        /// <returns></returns>
        public static PbxComponent GetActiveComponentWithOriginationSupport()
        {
            foreach ( var indexType in PbxContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.SupportsOrigination )
                {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the allowed active component with origination support.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static PbxComponent GetAllowedActiveComponentWithOriginationSupport( Person person )
        {
            foreach ( var indexType in PbxContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive && component.SupportsOrigination )
                {
                    var isAuthorized = Rock.Security.Authorization.Authorized( component, Authorization.VIEW, person );

                    if ( isAuthorized )
                    {
                        return component;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( PbxComponent ) )]
        protected override IEnumerable<Lazy<PbxComponent, IComponentData>> MEFComponents { get; set; }
    }
}
