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
using System.Web.UI;

using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.PersonProfile
{
    /// <summary>
    /// Base class for person profile badges
    /// </summary>
    public abstract class BadgeComponent : Component
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "Active", "True" );
                defaults.Add( "Order", "0" );
                return defaults;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public override bool IsActive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return 0;
            }
        }


        /// <summary>
        /// Gets or sets the parent person block.
        /// </summary>
        /// <value>
        /// The parent person block.
        /// </value>
        public PersonBlock ParentPersonBlock
        {
            get { return _parentPersonBlock; }
            set { _parentPersonBlock = value; }
        }
        private PersonBlock _parentPersonBlock;

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public virtual Person Person
        {
            get { return _person; }
            set { _person = value; }
        }
        private Person _person;

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeComponent" /> class.
        /// </summary>
        public BadgeComponent()
        {
            // Override default constructor of Component that loads attributes (needs to be done by each instance)
        }

        /// <summary>
        /// Loads the attributes for the badge.  The attributes are loaded by the framework prior to executing the badge, 
        /// so typically Person Badges do not need to load the attributes
        /// </summary>
        /// <param name="badge">The badge.</param>
        public void LoadAttributes( PersonBadge badge )
        {
            badge.LoadAttributes();
        }

        /// <summary>
        /// Use GetAttributeValue( PersonBadge badge, string key) instead.  Person Badge attribute values are 
        /// specific to the badge instance (rather than global).  This method will throw an exception
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Person Badge attributes are saved specific to the current badge, which requires that the current badge is included in order to load or retrieve values.  Use the GetAttributeValue( PersonBadge badge, string key ) method instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Person Badge attributes are saved specific to the current badge, which requires that the current badge is included in order to load or retrieve values.  Use the GetAttributeValue( PersonBadge badge, string key ) method instead." );
        }

        /// <summary>
        /// Gets the attribute value for the badge
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetAttributeValue( PersonBadgeCache badge, string key )
        {
            return badge.GetAttributeValue( key );
        }
        
        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        public abstract void Render( PersonBadgeCache badge, HtmlTextWriter writer );

    }
}
