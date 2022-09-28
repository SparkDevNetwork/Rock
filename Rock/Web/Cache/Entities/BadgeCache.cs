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
using System.Linq;
using System.Runtime.Serialization;
using Rock.Badge;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Crm;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a badge that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class BadgeCache : ModelCache<BadgeCache, Rock.Model.Badge>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; private set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; private set; }

        /// <summary>
        /// Gets or sets the badge component entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? BadgeComponentEntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the subject entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets the Entity Type.
        /// </summary>
        public EntityTypeCache BadgeComponentEntityType
        {
            get
            {
                if ( BadgeComponentEntityTypeId.HasValue )
                {
                    return EntityTypeCache.Get( BadgeComponentEntityTypeId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the Entity Type.
        /// </summary>
        public EntityTypeCache EntityType
        {
            get
            {
                if ( EntityTypeId.HasValue )
                {
                    return EntityTypeCache.Get( EntityTypeId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the badge component.
        /// </summary>
        /// <value>
        /// The badge component.
        /// </value>
        public virtual BadgeComponent BadgeComponent => BadgeComponentEntityType != null ? BadgeContainer.GetComponent( BadgeComponentEntityType.Name ) : null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns all of the badges that apply to the given type (for example Person or Group).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static List<BadgeCache> All( Type type )
        {
            var entityTypeCache = EntityTypeCache.Get( type );

            if ( entityTypeCache == null )
            {
                return new List<BadgeCache>();
            }

            return All( entityTypeCache.Id );
        }

        /// <summary>
        /// Returns all of the badges that apply to the given type (for example Person or Group).
        /// </summary>
        /// <param name="entityTypeId">The entity type id.</param>
        /// <returns></returns>
        public static List<BadgeCache> All( int entityTypeId )
        {
            var allBadges = All();
            return allBadges.Where( b => !b.EntityTypeId.HasValue || b.EntityTypeId.Value == entityTypeId ).ToList();
        }

        /// <summary>
        /// Renders the badge for the specified entity. No security checks are
        /// performed.
        /// </summary>
        /// <param name="entity">The entity to use when rendering the badge.</param>
        /// <returns>An instance of <see cref="RenderedBadgeBag"/> that contains the HTML and JavaScript rendered by the badge.</returns>
        internal RenderedBadgeBag RenderBadge( IEntity entity )
        {
            // If the entity is null or the badge does not apply to it then
            // return an empty result.
            if ( entity == null || !BadgeService.DoesBadgeApplyToEntity( this, entity ) )
            {
                return new RenderedBadgeBag();
            }

            try
            {
                var component = BadgeComponent;
                var textWriter = new StringWriter();

                component.Render( this, entity, textWriter );

                using ( var htmlTextWriter = new System.Web.UI.HtmlTextWriter( textWriter ) )
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    component.ParentContextEntityBlock = null;
                    component.Entity = entity;
                    component.Render( this, htmlTextWriter );
#pragma warning restore CS0618 // Type or member is obsolete
                }

                var script = component.GetWrappedJavaScript( this, entity );

                if ( script.IsNullOrWhiteSpace() )
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    script = component.GetWrappedJavaScript( this );
#pragma warning restore CS0618 // Type or member is obsolete
                }

                return new RenderedBadgeBag
                {
                    Html = textWriter.ToString(),
                    JavaScript = script
                };
            }
            catch ( Exception ex )
            {
                var errorMessage = $"An error occurred rendering badge: {Name }, badge-id: {Id}";
                ExceptionLogService.LogException( new Exception( errorMessage, ex ) );
                var badgeNameClass = Name.ToLower().RemoveAllNonAlphaNumericCharacters() ?? "error";

                return new RenderedBadgeBag
                {
                    Html = $@"<div class='rockbadge rockbadge-{badgeNameClass} rockbadge-id-{Id} badge-error' data-toggle='tooltip' data-original-title='{errorMessage}'>
    <i class='fa fa-exclamation-triangle badge-icon text-warning'></i>
</div>"
                };
            }
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var badge = entity as Model.Badge;
            if ( badge == null )
            {
                return;
            }

            Name = badge.Name;
            Description = badge.Description;
            BadgeComponentEntityTypeId = badge.BadgeComponentEntityTypeId;
            EntityTypeId = badge.EntityTypeId;
            Order = badge.Order;
            EntityTypeQualifierColumn = badge.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = badge.EntityTypeQualifierValue;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

    }
}