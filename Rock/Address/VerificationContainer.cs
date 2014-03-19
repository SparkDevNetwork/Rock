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
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

using Rock.Extension;

namespace Rock.Address
{
    /// <summary>
    /// Singleton class that uses MEF to load and cache all of the VerificationComponent classes
    /// </summary>
    public class VerificationContainer : Container<VerificationComponent, IComponentData>
    {
        #region Singleton Support

        private static VerificationContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static VerificationContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new VerificationContainer();
                return instance;
            }
        }

        private VerificationContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <param name="verificationComponentType">Type of the verification component.</param>
        /// <returns></returns>
        public static VerificationComponent GetComponent( Type verificationComponentType )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( component.TypeName == verificationComponentType.FullName )
                {
                    return component;
                }
            }

            return null;
        }

        #endregion

        #region MEF Support

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( VerificationComponent ) )]
        protected override IEnumerable<Lazy<VerificationComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore

        #endregion
    }
}