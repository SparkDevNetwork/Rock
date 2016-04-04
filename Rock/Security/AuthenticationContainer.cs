﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace Rock.Security
{
    /// <summary>
    /// MEF Container class for Binary File Authentication Components
    /// </summary>
    public class AuthenticationContainer : Container<AuthenticationComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<AuthenticationContainer> instance =
            new Lazy<AuthenticationContainer>( () => new AuthenticationContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static AuthenticationContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static AuthenticationComponent GetComponent( string entityType )
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
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( AuthenticationComponent ) )]
        protected override IEnumerable<Lazy<AuthenticationComponent, IComponentData>> MEFComponents { get; set; }

    }
}
