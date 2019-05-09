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

using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication.SmsActions
{
    /// <summary>
    /// MEF Container class for Communication Sms Action Components
    /// </summary>
    public class SmsActionContainer : Container<SmsActionComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<SmsActionContainer> instance =
            new Lazy<SmsActionContainer>( () => new SmsActionContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static SmsActionContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Forces a reloading of all the components
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            // Create any attributes that need to be created
            int smsActionEntityTypeId = EntityTypeCache.Get( typeof( SmsAction ) ).Id;
            using ( var rockContext = new RockContext() )
            {
                foreach ( var action in this.Components )
                {
                    Type actionType = action.Value.Value.GetType();
                    int actionComponentEntityTypeId = EntityTypeCache.Get( actionType ).Id;
                    Rock.Attribute.Helper.UpdateAttributes( actionType, smsActionEntityTypeId, "SmsActionComponentEntityTypeId", actionComponentEntityTypeId.ToString(), rockContext );
                }
            }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static SmsActionComponent GetComponent( string entityType )
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
        [ImportMany( typeof( SmsActionComponent ) )]
        protected override IEnumerable<Lazy<SmsActionComponent, IComponentData>> MEFComponents { get; set; }
    }
}