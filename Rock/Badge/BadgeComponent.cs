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
using System.IO;
using System.Web;
using System.Web.UI;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Badge
{
    /// <summary>
    /// Base class for person profile badges
    /// </summary>
    public abstract class BadgeComponent : Rock.Extension.Component
    {
        /// <summary>
        /// Determines of this badge component applies to the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public virtual bool DoesApplyToEntityType( string type )
        {
            return true;
        }

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

        #region Obsolete Properties

        /*
         * Daniel Hazelbaker - 5/20/2022
         * 
         * Many of these properties are not async safe. Meaning if two Tasks
         * try to render the same badge for two different entities at the same
         * time then they will collide with each other.
         * 
         * Additionally, HtmlTextWriter is a System.Web specific. It provides
         * helper methods to write entire HTML elements rather than needing
         * to properly write them by hand. However, all but one of the core
         * badges didn't even use those methods and just wrote the raw HTML
         * themselves.
         * 
         * Therefore, it was decided to mark these properties (and the related
         * methods below) as obsolete and create new methods that each take
         * in the entity to be processed as well as use a standard TextWriter
         * that will work better in .NET core in the future.
         */

        /// <summary>
        /// Gets or sets the parent context block.
        /// </summary>
        [RockObsolete( "1.14" )]
        [Obsolete( "ParentContextEntityBlock is deprecated and will be removed in a future version." )]
        public ContextEntityBlock ParentContextEntityBlock
        {
            get
            {
                if ( HttpContext.Current != null )
                {
                    return HttpContext.Current.Items[$"{this.GetType().FullName}:ParentContextEntityBlock"] as ContextEntityBlock;
                }

                return _nonHttpContextParentContextEntityBlock;
            }

            set
            {
                if ( HttpContext.Current != null )
                {
                    HttpContext.Current.Items[$"{this.GetType().FullName}:ParentContextEntityBlock"] = value;
                }
                else
                {
                    _nonHttpContextParentContextEntityBlock = value;
                }
            }
        }

        /// <summary>
        /// The parent context entity block when HttpContext.Current is null
        /// NOTE: ThreadStatic is per thread, but ASP.NET threads are ThreadPool threads, so they will be used again.
        /// see https://www.hanselman.com/blog/ATaleOfTwoTechniquesTheThreadStaticAttributeAndSystemWebHttpContextCurrentItems.aspx
        /// So be careful and only use the [ThreadStatic] trick if absolutely necessary
        /// </summary>
        [RockObsolete( "1.14" )]
        [Obsolete]
        [ThreadStatic]
        private static ContextEntityBlock _nonHttpContextParentContextEntityBlock;

        /// <summary>
        /// Gets or sets the parent person block.
        /// </summary>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use the ParentContextEntityBlock instead.", true )]
        public PersonBlock ParentPersonBlock
        {
            get => ParentContextEntityBlock as PersonBlock;
        }

        /// <summary>
        /// Optional: The Entity that should be used when determining which PropertyFields and Attributes to show (instead of just basing it off of EntityType)
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [RockObsolete( "1.14" )]
        [Obsolete( "Entity is deprecated and will be removed in a future version." )]
        public IEntity Entity
        {
            get
            {
                if ( HttpContext.Current != null )
                {
                    return HttpContext.Current.Items[$"{this.GetType().FullName}:Entity"] as IEntity;
                }

                return _nonHttpContextEntity;
            }

            set
            {
                if ( HttpContext.Current != null )
                {
                    HttpContext.Current.Items[$"{this.GetType().FullName}:Entity"] = value;
                }
                else
                {
                    _nonHttpContextEntity = value;
                }
            }
        }

        /// <summary>
        /// Thread safe storage of Entity property when HttpContext.Current is null
        /// NOTE: ThreadStatic is per thread, but ASP.NET threads are ThreadPool threads, so they will be used again.
        /// see https://www.hanselman.com/blog/ATaleOfTwoTechniquesTheThreadStaticAttributeAndSystemWebHttpContextCurrentItems.aspx
        /// So be careful and only use the [ThreadStatic] trick if absolutely necessary, and only if it can't be stored in HttpContext.Current
        /// </summary>
        [ThreadStatic]
        [RockObsolete( "1.14" )]
        [Obsolete]
        private static IEntity _nonHttpContextEntity;

        /// <summary>
        /// Gets or sets the entity as a person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [RockObsolete( "1.14" )]
        [Obsolete( "Person is no longer used and will be removed in a future version." )]
        public Person Person
        {
            get => Entity as Person;
            set => Entity = value;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeComponent" /> class.
        /// </summary>
        public BadgeComponent() : base( false )
        {
            // Override default constructor of Component that loads attributes (needs to be done by each instance)
        }

        /// <summary>
        /// Loads the attributes for the badge.  The attributes are loaded by the framework prior to executing the badge, 
        /// so typically Person Badges do not need to load the attributes
        /// </summary>
        /// <param name="badge">The badge.</param>
        public void LoadAttributes( Model.Badge badge )
        {
            badge.LoadAttributes();
        }

        /// <summary>
        /// Use GetAttributeValue( BadgeCache badge, string key) instead.  Person Badge attribute values are 
        /// specific to the badge instance (rather than global).  This method will throw an exception
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Person Badge attributes are saved specific to the current badge, which requires that the current badge is included in order to load or retrieve values.  Use the GetAttributeValue( PersonBadge badge, string key ) method instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Badge attributes are saved specific to the current badge, which requires that the current badge is included in order to load or retrieve values.  Use the GetAttributeValue( BadgeCache badge, string key ) method instead." );
        }

        /// <summary>
        /// Gets the attribute value for the badge
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetAttributeValue( BadgeCache badge, string key )
        {
            return badge.GetAttributeValue( key );
        }

        /// <summary>
        /// Renders the badge HTML content that should be inserted into the DOM.
        /// </summary>
        /// <param name="badge">The badge cache that describes this badge.</param>
        /// <param name="entity">The entity to render the badge for.</param>
        /// <param name="writer">The writer to render the HTML into.</param>
        public virtual void Render( BadgeCache badge, IEntity entity, TextWriter writer )
        {
        }

        /// <summary>
        /// Gets the java script that will be used for any dynamic funcionality
        /// required by the badge.
        /// </summary>
        /// <param name="badge">The badge cache that describes this badge.</param>
        /// <param name="entity">The entity to render the badge for.</param>
        /// <returns>A string that contains the plain JavaScript without a &lt;script&gt; tag.</returns>
        protected virtual string GetJavaScript( BadgeCache badge, IEntity entity )
        {
            return null;

            /*
             * BJW 2020-09-18
             * When working on the connections board, I discovered that the badges with <script> tags in the 
             * Render method were not working correctly when added dynamically to the page. It turns out that
             * some DOM manipulation methods such as .innerHtml do not allow <script> tags to be evaluated by 
             * the browser. The postback must be using one of these methods. The solution was to register the 
             * scripts using the ScriptManager.RegisterClientScriptBlock which ensures that the script is 
             * evaluated even when added after the initial page load. This is happening in the BadgeControl.
             */
        }

        /// <summary>
        /// Adds the java script within a standardized function wrapper. This can be overridden when a
        /// different wrapper function is needed.
        /// </summary>
        /// <param name="badge">The badge cache that describes this badge.</param>
        /// <param name="entity">The entity to render the badge for.</param>
        /// <returns>A string that represents the JavaScript to be inserted into a script tag.</returns>
        public virtual string GetWrappedJavaScript( BadgeCache badge, IEntity entity )
        {
            var script = GetJavaScript( badge, entity );

            if ( script.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return
$@";(function () {{
    {script}
}})();";
        }

        /// <summary>
        /// Generates a key unique to this instance of the badge/entity combination. This
        /// is suitable for use as an HTML id.
        /// </summary>
        /// <returns>A string that can be used in the id attribute of an HTML element.</returns>
        public virtual string GenerateBadgeKey( BadgeCache badge, IEntity entity )
        {
            var entityKey = entity != null ? entity.Guid.ToString() : "no-entity";
            return $"{GetType().Name}-{badge.Guid}-{entityKey}";
        }

        #region Obsolete Methods

        /// <summary>
        /// Gets the attribute value for the badge
        /// </summary>
        /// <param name="personBadgeCache">The badge.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use the BadgeCache param instead.", true )]
        protected string GetAttributeValue( PersonBadgeCache personBadgeCache, string key )
        {
            var badgeCache = BadgeCache.Get( personBadgeCache.Id );
            return GetAttributeValue( badgeCache, key );
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="badge">The badge.</param>
        /// <param name="writer">The writer.</param>
        [RockObsolete( "1.14" )]
        [Obsolete( "Use the Render method that takes an IEntity and TextWriter parameters instead." )]
        public virtual void Render( BadgeCache badge, HtmlTextWriter writer ) { }

        /// <summary>
        /// Gets the java script.
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.14" )]
        [Obsolete( "Use the GetJavaScript method that takes an IEntity instead." )]
        protected virtual string GetJavaScript( BadgeCache badge )
        {
            return null;

            /*
             * BJW 2020-09-18
             * When working on the connections board, I discovered that the badges with <script> tags in the 
             * Render method were not working correctly when added dynamically to the page. It turns out that
             * some DOM manipulation methods such as .innerHtml do not allow <script> tags to be evaluated by 
             * the browser. The postback must be using one of these methods. The solution was to register the 
             * scripts using the ScriptManager.RegisterClientScriptBlock which ensures that the script is 
             * evaluated even when added after the initial page load. This is happening in the BadgeControl.
             */
        }

        /// <summary>
        /// Adds the java script within a standardized function wrapper. This can be overridden when a
        /// different wrapper function is needed.
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.14" )]
        [Obsolete( "Use the GetWrappedJavaScript method that takes an IEntity instead." )]
        public virtual string GetWrappedJavaScript( BadgeCache badge )
        {
            var script = GetJavaScript( badge );

            if ( script.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return
$@"(function () {{
    {script}
}})();";
        }

        /// <summary>
        /// Generates a key unique to this instance of the badge/entity combination. This
        /// is suitable for use as an HTML id.
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.14" )]
        [Obsolete( "Use the GenerateBadgeKey that takes IEntity instead." )]
        public virtual string GenerateBadgeKey( BadgeCache badge )
        {
            var entityKey = Entity != null ? Entity.Guid.ToString() : "no-entity";
            return $"{GetType().Name}-{badge.Guid}-{entityKey}";
        }

        /// <summary>
        /// Renders the specified writer.
        /// </summary>
        /// <param name="personBadgeCache">The badge.</param>
        /// <param name="writer">The writer.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use the BadgeCache param instead.", true )]
        public virtual void Render( PersonBadgeCache personBadgeCache, HtmlTextWriter writer ) { }

        #endregion
    }
}
