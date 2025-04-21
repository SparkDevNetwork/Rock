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
using System.ComponentModel;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Prayer
{
    /// <summary>
    /// Displays an existing prayer request and allows the user to enter the answer to prayer.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Answer To Prayer" )]
    [Category( "Mobile > Prayer" )]
    [Description( "Displays an existing prayer request and allows the user to enter the answer to prayer." )]
    [IconCssClass( "fa fa-reply" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage( "Return Page",
        Description = "If set then the current page will be replaced with the Return Page on Save. If not set then a Pop Page is performed instead.",
        IsRequired = false,
        Key = AttributeKeys.ReturnPage,
        Order = 0 )]

    [BooleanField( "Enforce Security",
        Description = "Ensures that the person editing the request is the owner of the request.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Key = AttributeKeys.EnforceSecurity,
        Order = 1 )]

    [BlockTemplateField( "Template",
        Description = "The template for how to display the prayer request.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_ANSWER_TO_PRAYER,
        DefaultValue = "91C29610-1D77-49A8-A46B-5A35EC67C551",
        IsRequired = true,
        Key = AttributeKeys.Template,
        Order = 2 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_ANSWER_TO_PRAYER_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "324D5295-72E6-42DF-B111-E428E811B786" )]
    public class AnswerToPrayer : RockBlockType
    {
        /// <summary>
        /// The page parameter keys for the <see cref="AnswerToPrayer"/> block.
        /// </summary>
        public static class PageParameterKeys
        {
            /// <summary>
            /// The request identifier
            /// </summary>
            public const string RequestGuid = "RequestGuid";
        }

        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="AnswerToPrayer"/> block.
        /// </summary>
        protected static class AttributeKeys
        {
            /// <summary>
            /// The return page key.
            /// </summary>
            public const string ReturnPage = "ReturnPage";

            /// <summary>
            /// The enforce security key.
            /// </summary>
            public const string EnforceSecurity = "EnforceSecurity";

            /// <summary>
            /// The template key attribute key.
            /// </summary>
            public const string Template = "Template";
        }

        /// <summary>
        /// Gets the return page unique identifier.
        /// </summary>
        /// <value>
        /// The return page unique identifier.
        /// </value>
        protected Guid? ReturnPageGuid => GetAttributeValue( AttributeKeys.ReturnPage ).AsGuidOrNull();

        /// <summary>
        /// Gets a value indicating whether security should be enforced.
        /// </summary>
        /// <value>
        ///   <c>true</c> if security should be enforced; otherwise, <c>false</c>.
        /// </value>
        protected bool EnforceSecurity => GetAttributeValue( AttributeKeys.EnforceSecurity ).AsBoolean();

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        protected string Template => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKeys.Template ) );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 2 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            //
            // Indicate that we are a dynamic content providing block.
            //
            return new Rock.Common.Mobile.Blocks.Content.Configuration
            {
                Content = null,
                DynamicContent = true
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the content to be displayed on the block.
        /// </summary>
        /// <returns>A string containing the XAML content to be displayed.</returns>
        private string BuildContent()
        {
            using ( var rockContext = new RockContext() )
            {
                Guid? requestGuid = RequestContext.GetPageParameter( PageParameterKeys.RequestGuid ).AsGuidOrNull();
                PrayerRequest request = null;

                if ( requestGuid.HasValue )
                {
                    request = new PrayerRequestService( rockContext ).Get( requestGuid.Value );
                }

                if ( request == null )
                {
                    return "<Rock:NotificationBox HeaderText=\"Not Found\" Text=\"We couldn't find that prayer request.\" NotificationType=\"Error\" />";
                }

                var canEdit = request.RequestedByPersonAlias != null && request.RequestedByPersonAlias.PersonId == RequestContext.CurrentPerson?.Id;

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && !canEdit )
                {
                    return "<Rock:NotificationBox HeaderText=\"Error\" Text=\"You are not authorized to edit prayer requests.\" NotificationType=\"Error\" />";
                }

                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.AddOrReplace( "PrayerRequest", request );
                var prayerRequestXaml = Template.ResolveMergeFields( mergeFields, RequestContext.CurrentPerson );

                return $@"
<StackLayout StyleClass=""prayerdetail, spacing-24"">
    {prayerRequestXaml}

    <Rock:FieldContainer>
        <Rock:TextEditor x:Name=""tbAnswer"" IsRequired=""True"" MinimumHeightRequest=""80"" AutoSize=""TextChanges"" Placeholder=""My answer to prayer is...""
            Text=""{request.Answer.ToStringSafe().EncodeXml( true )}"" />
    </Rock:FieldContainer>

    <Rock:Validator x:Name=""vForm"">
        <x:Reference>tbAnswer</x:Reference>
    </Rock:Validator>
    
    <Rock:NotificationBox x:Name=""nbError"" NotificationType=""Warning"" />

    <StackLayout>
        <Button StyleClass=""btn,btn-primary,save-button"" Text=""Save"" Command=""{{Binding Callback}}"">
            <Button.CommandParameter>
                <Rock:CallbackParameters Name="":SaveAnswer"" Validator=""{{x:Reference vForm}}"" Notification=""{{x:Reference nbError}}"">
                    <Rock:Parameter Name=""answer"" Value=""{{Binding Text, Source={{x:Reference tbAnswer}}}}"" />
                </Rock:CallbackParameters>
            </Button.CommandParameter>
        </Button>

        <Button StyleClass=""btn,btn-link,cancel-button"" Text=""Cancel"" Command=""{{Binding PopPage}}"" />
    </StackLayout>
</StackLayout>";
            }
        }

        /// <summary>
        /// Saves the prayer request.
        /// </summary>
        /// <param name="answer">The answer text.</param>
        /// <returns>
        /// The response to send back to the client.
        /// </returns>
        private CallbackResponse SaveRequest( string answer )
        {
            using ( var rockContext = new RockContext() )
            {
                var prayerRequestService = new PrayerRequestService( rockContext );
                PrayerRequest prayerRequest = null;
                var requestGuid = RequestContext.GetPageParameter( PageParameterKeys.RequestGuid ).AsGuidOrNull();

                if ( requestGuid.HasValue )
                {
                    prayerRequest = new PrayerRequestService( rockContext ).Get( requestGuid.Value );
                }

                if ( prayerRequest == null )
                {
                    return new CallbackResponse
                    {
                        Error = "We couldn't find that prayer request."
                    };
                }

                var canEdit = prayerRequest.RequestedByPersonAlias != null && prayerRequest.RequestedByPersonAlias.PersonId == RequestContext.CurrentPerson?.Id;

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && !canEdit )
                {
                    return new CallbackResponse
                    {
                        Error = "You are not authorized to edit prayer requests."
                    };
                }

                prayerRequest.Answer = answer;

                //
                // Save all changes to database.
                //
                rockContext.SaveChanges();
            }

            if ( ReturnPageGuid.HasValue )
            {
                return new CallbackResponse
                {
                    Command = "ReplacePage",
                    CommandParameter = ReturnPageGuid.Value.ToString()
                };
            }
            else
            {
                return new CallbackResponse
                {
                    Command = "PopPage",
                    CommandParameter = "true"
                };
            }
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the current configuration for this block.
        /// </summary>
        /// <returns>The initial content to display.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            return new CallbackResponse
            {
                Content = BuildContent()
            };
        }

        /// <summary>
        /// Saves the answer.
        /// </summary>
        /// <param name="answer">The answer.</param>
        /// <returns></returns>
        [BlockAction]
        public object SaveAnswer( string answer )
        {
            return SaveRequest( answer );
        }

        #endregion
    }
}