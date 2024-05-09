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
using Rock.ViewModels.Blocks.Reporting.InteractionDetail;
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
    [DisplayName( "Interaction Detail" )]
    [Category( "Reporting" )]
    [Description( "Presents the details of a interaction using Lava" )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Default Template",
        Description = "The Lava template to use as default.",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorTheme = Rock.Web.UI.Controls.CodeEditorTheme.Rock,
        EditorHeight = 300,
        IsRequired = false,
        Order = 2,
        DefaultValue = DEFAULT_LAVA_TEMPLATE,
        Key = AttributeKey.DefaultTemplate )]

    #endregion Block Attribute

    [Rock.SystemGuid.EntityTypeGuid( "a2a1c452-6916-4c91-ab96-df744512032a" )]
    [Rock.SystemGuid.BlockTypeGuid( "011aede7-b036-4f4a-bf3e-4c284dc45de8" )]
    public class InteractionDetail : RockBlockType
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
            public const string InteractionId = "InteractionId";
        }

        private static class MergeFieldKeys
        {
            public const string Person = "Person";
            public const string InteractionDetailPage = "InteractionDetailPage";
            public const string InteractionChannel = "InteractionChannel";
            public const string InteractionComponent = "InteractionComponent";
            public const string InteractionEntity = "InteractionEntity";
            public const string InteractionEntityName = "InteractionEntityName";
            public const string Interaction = "Interaction";
        }

        #endregion

        #region Attribute Field Constants

        protected const string DEFAULT_LAVA_TEMPLATE = @"<div class='panel panel-block'>
        <div class='panel-heading'>
	        <h1 class='panel-title'>
                <i class='fa fa-user'></i>
                Interaction Detail
            </h1>
        </div>
        <div class='panel-body'>
            <div class='row'>
                <div class='col-md-6'>
                    <dl>
                        <dt>Channel</dt><dd>{{ InteractionChannel.Name }}<dd/>
                        <dt>Date / Time</dt><dd>{{ Interaction.InteractionDateTime }}<dd/>
                        <dt>Operation</dt><dd>{{ Interaction.Operation }}<dd/>

                        {% if InteractionEntityName != '' %}
                            <dt>Entity Name</dt><dd>{{ InteractionEntityName }}<dd/>
                        {% endif %}
                    </dl>
                </div>
                <div class='col-md-6'>
                    <dl>
                        <dt> Component</dt><dd>{{ InteractionComponent.Name }}<dd/>
                        {% if Interaction.PersonAlias.Person.FullName != '' %}
                            <dt>Person</dt><dd>{{ Interaction.PersonAlias.Person.FullName }}<dd/>
                        {% endif %}

                        {% if Interaction.InteractionSummary and Interaction.InteractionSummary != '' %}
                            <dt>Interaction Summary</dt><dd>{{ Interaction.InteractionSummary }}<dd/>
                        {% endif %}

                        {% if Interaction.InteractionData and Interaction.InteractionData != '' %}
                            <dt>Interaction Data</dt><dd>{{ Interaction.InteractionData }}<dd/>
                        {% endif %}
                    </dl>
                </div>
            </div>
        </div>
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
        private InteractionDetailInitializationBox GetInitializationBox( RockContext rockContext )
        {
            var interactionId = PageParameter( PageParameterKey.InteractionId ).AsInteger();
            var interaction = new InteractionService( rockContext ).Get( interactionId );
            var box = new InteractionDetailInitializationBox();

            if ( interaction != null )
            {
                IEntity interactionEntity = null;
                if ( interaction.EntityId.HasValue )
                {
                    interactionEntity = GetInteractionEntity( rockContext, interaction );
                }

                if ( BlockCache.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) || interaction.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                {
                    var mergeFields = RequestContext.GetCommonMergeFields( GetCurrentPerson() );
                    mergeFields.TryAdd( MergeFieldKeys.Person, GetCurrentPerson() );
                    mergeFields.Add( MergeFieldKeys.InteractionDetailPage,  LinkedPageRoute( MergeFieldKeys.InteractionDetailPage ) );
                    mergeFields.Add( MergeFieldKeys.InteractionChannel, interaction.InteractionComponent.InteractionChannel );
                    mergeFields.Add( MergeFieldKeys.InteractionComponent, interaction.InteractionComponent );
                    mergeFields.Add( MergeFieldKeys.InteractionEntity, interactionEntity );

                    if ( interactionEntity != null )
                    {
                        mergeFields.Add( MergeFieldKeys.InteractionEntityName, interactionEntity.ToString() );
                    }
                    else
                    {
                        mergeFields.Add( MergeFieldKeys.InteractionEntityName, string.Empty );
                    }

                    mergeFields.Add( MergeFieldKeys.Interaction, interaction );

                    box.Content = interaction.InteractionComponent.InteractionChannel.InteractionDetailTemplate.IsNotNullOrWhiteSpace() ?
                        interaction.InteractionComponent.InteractionChannel.InteractionDetailTemplate.ResolveMergeFields( mergeFields ) :
                        GetAttributeValue( AttributeKey.DefaultTemplate ).ResolveMergeFields( mergeFields );
                }
            }
            else
            {
                box.ErrorMessage = "<strong>Missing Interaction Information</strong> <span> <p> Make sure you have navigated to this page correctly. </p> </span>";
            }

            return box;
        }

        /// <summary>
        /// Gets the Component Entity
        /// </summary>
        /// <param name="rockContext">The db context.</param>
        /// <param name="interaction">The interaction .</param>
        private IEntity GetInteractionEntity( RockContext rockContext, Interaction interaction )
        {
            IEntity interactionEntity = null;
            var interactionEntityType = EntityTypeCache.Get( interaction.InteractionComponent.InteractionChannel.InteractionEntityTypeId.Value ).GetEntityType();
            IService serviceInstance = Reflection.GetServiceForEntityType( interactionEntityType, rockContext );
            if ( serviceInstance != null )
            {
                System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                interactionEntity = getMethod.Invoke( serviceInstance, new object[] { interaction.EntityId.Value } ) as Rock.Data.IEntity;
            }

            return interactionEntity;
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