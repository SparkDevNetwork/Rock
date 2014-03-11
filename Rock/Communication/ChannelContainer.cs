// <copyright>
// Copyright 2013 by the Spark Development Network
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

namespace Rock.Communication
{
    /// <summary>
    /// MEF Container class for Communication Channel Componenets
    /// </summary>
    public class ChannelContainer : Container<ChannelComponent, IComponentData>
    {
        private static ChannelContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static ChannelContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new ChannelContainer();
                return instance;
            }
        }

        private ChannelContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static ChannelComponent GetComponent( string entityTypeName )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( component.TypeName == entityTypeName )
                {
                    return component;
                }
            }

            return null;
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( ChannelComponent ) )]
        protected override IEnumerable<Lazy<ChannelComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}