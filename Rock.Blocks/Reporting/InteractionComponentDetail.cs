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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Reporting.InteractionComponentDetail;
using Rock.Web;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Presents the details of a interaction using Lava
    /// </summary>
    [DisplayName( "Interaction Component Detail" )]
    [Category( "Reporting" )]
    [Description( "Presents the details of a interaction channel using Lava" )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Default Template",
        Description = "The Lava template to use as default.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        Order = 0,
        DefaultValue = DEFAULT_LAVA_TEMPLATE,
        Key = AttributeKey.DefaultTemplate )]

    #endregion Block Attribute

    [Rock.SystemGuid.EntityTypeGuid( "29e5a6bf-fe7f-406e-afc1-64eab506ddb0" )]
    [Rock.SystemGuid.BlockTypeGuid( "bc2034d1-416b-4fb4-9fff-e202fa666203" )]
    public class InteractionComponentDetail : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DefaultTemplate = "DefaultTemplate";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class PageParameterKey
        {
            public const string ComponentId = "ComponentId";
        }

        private static class MergeFieldKeys
        {
            public const string Person = "Person";
            public const string InteractionChannel = "InteractionChannel";
            public const string InteractionComponent = "InteractionComponent";
            public const string InteractionComponentEntity = "InteractionComponentEntity";
            public const string InteractionComponentEntityName = "InteractionComponentEntityName";
        }

        #endregion

        #region Attribute Field Constants

        protected const string DEFAULT_LAVA_TEMPLATE = @"<div class='row'>
        <div class='col-md-6'>
            <dl><dt>Name</dt><dd>{{ InteractionComponent.Name }}<dd/></dl>
        </div>
        {% if InteractionComponentEntity != '' %}
            <div class='col-md-6'>
                <dl>
                    <dt>Entity Name</dt><dd>{{ InteractionComponentEntity }}<dd/>
                </dl>
            </div>
        {% endif %}
    </div>";

        #endregion Attribute Field Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = GetInitializationBox( rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();

                return box;
            }
        }

        /// <summary>
        /// Gets the initialization box.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private InteractionComponentDetailInitializationBox GetInitializationBox( RockContext rockContext )
        {
            var interactionId = PageParameter( PageParameterKey.ComponentId ).AsInteger();
            var interactionComponent = new InteractionComponentService( rockContext ).Get( interactionId );
            var box = new InteractionComponentDetailInitializationBox();

            if ( interactionComponent != null )
            {
                IEntity interactionEntity = null;
                if ( interactionComponent.EntityId.HasValue )
                {
                    interactionEntity = GetComponentEntity( rockContext, interactionComponent );
                }

                if ( BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) || interactionComponent.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                {
                    var mergeFields = RequestContext.GetCommonMergeFields( GetCurrentPerson() );
                    mergeFields.TryAdd( MergeFieldKeys.Person, GetCurrentPerson() );
                    mergeFields.Add( MergeFieldKeys.InteractionChannel, interactionComponent.InteractionChannel );
                    mergeFields.Add( MergeFieldKeys.InteractionComponent, interactionComponent );
                    mergeFields.Add( MergeFieldKeys.InteractionComponentEntity, interactionEntity );

                    if ( interactionEntity != null )
                    {
                        mergeFields.Add( MergeFieldKeys.InteractionComponentEntityName, interactionEntity.ToString() );
                    }
                    else
                    {
                        mergeFields.Add( MergeFieldKeys.InteractionComponentEntityName, string.Empty );
                    }

                    box.ComponentName = interactionComponent.Name;
                    box.Content = interactionComponent.InteractionChannel.ComponentDetailTemplate.IsNotNullOrWhiteSpace() ?
                        interactionComponent.InteractionChannel.ComponentDetailTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( AttributeKey.DefaultTemplate ).ResolveMergeFields( mergeFields );
                }
            }
            else
            {
                box.ErrorMessage = "<strong>Missing Component Information</strong> <span> <p> Make sure you have navigated to this page correctly. </p> </span>";
            }

            return box;
        }

        /// <summary>
        /// Gets the Component Entity
        /// </summary>
        /// <param name="rockContext">The db context.</param>
        /// <param name="interactionComponent">The interaction component.</param>
        private IEntity GetComponentEntity( RockContext rockContext, InteractionComponent interactionComponent )
        {
            IEntity componentEntity = null;
            var componentEntityType = EntityTypeCache.Get( interactionComponent.InteractionChannel.ComponentEntityTypeId.Value ).GetEntityType();
            IService serviceInstance = Reflection.GetServiceForEntityType( componentEntityType, rockContext );
            if ( serviceInstance != null )
            {
                System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                componentEntity = getMethod.Invoke( serviceInstance, new object[] { interactionComponent.EntityId.Value } ) as Rock.Data.IEntity;
            }

            return componentEntity;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// If this Attribute is a reference to a PageRoute, this will return the Route, otherwise it will return the normal URL
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        public string LinkedPageRoute( string attributeKey )
        {
            return new PageReference( GetAttributeValue( attributeKey ) ).Route;
        }

        #endregion
    }
}