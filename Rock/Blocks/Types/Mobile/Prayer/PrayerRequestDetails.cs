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
using System.ComponentModel;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Content;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Prayer
{
    /// <summary>
    /// Displays custom XAML content on the page.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Prayer Request Details" )]
    [Category( "Mobile > Prayer" )]
    [Description( "Edits an existing prayer request or creates a new one." )]
    [IconCssClass( "fa fa-praying-hands" )]

    #region Block Attributes

    [BooleanField( "Show Category",
        Description = "If disabled, then the user will not be able to select a category and the default category will be used exclusively.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.CategorySelection,
        Key = AttributeKeys.ShowCategory,
        Order = 0 )]

    [CategoryField( "Parent Category",
        Description = "A top level category. This controls which categories the person can choose from when entering their prayer request.",
        IsRequired = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        AllowMultiple = false,
        Category = AttributeCategories.CategorySelection,
        Order = 1,
        Key = AttributeKeys.ParentCategory )]

    [CategoryField( "Default Category",
        Description = "The default category to use for all new prayer requests.",
        IsRequired = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        AllowMultiple = false,
        Category = AttributeCategories.CategorySelection,
        Order = 2,
        Key = AttributeKeys.DefaultCategory )]

    [BooleanField( "Enable Auto Approve",
        Description = "If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.EnableAutoApprove,
        Order = 0 )]

    [IntegerField( "Expires After (Days)",
        Description = "Number of days until the request will expire (only applies when auto-approved is enabled).",
        IsRequired = true,
        DefaultIntegerValue = 14,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.ExpiresAfterDays,
        Order = 1 )]

    [BooleanField( "Show Header",
        Description = "If enabled, a 'Add/Edit Prayer Request' header will be displayed.",
        IsRequired = true,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.ShowHeader,
        Order = 2 )]

    [BooleanField( "Show Urgent Flag",
        Description = "If enabled, requestors will be able to flag prayer requests as urgent.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.ShowUrgentFlag,
        Order = 3 )]

    [BooleanField( "Show Public Display Flag",
        Description = "If enabled, requestors will be able set whether or not they want their request displayed on the public website.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.ShowPublicDisplayFlag,
        Order = 4 )]

    [BooleanField( "Default To Public",
        Description = "If enabled, all prayers will be set to public by default.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.DefaultToPublic,
        Order = 5 )]

    [IntegerField( "Character Limit",
        Description = "If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.",
        IsRequired = false,
        DefaultIntegerValue = 250,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.CharacterLimit,
        Order = 6 )]

    [BooleanField( "Show Campus",
        Description = "Should the campus field be displayed? If there is only one active campus then the campus field will not show.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.ShowCampus,
        Order = 7 )]

    [BooleanField( "Require Campus",
        Description = "Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.RequireCampus,
        Order = 8 )]

    [BooleanField( "Require Last Name",
        Description = "Require that a last name be entered. First name is always required.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.RequireLastName,
        Order = 9 )]

    [BooleanField( "Enable Person Matching",
        Description = "If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Category = AttributeCategories.Features,
        Key = AttributeKeys.EnablePersonMatching,
        Order = 10 )]

    [CustomDropdownListField( "Completion Action",
        description: "What action to perform after saving the prayer request.",
        listSource: "0^Show Completion Xaml,1^Pop Page",
        IsRequired = true,
        DefaultValue = "0",
        Category = AttributeCategories.OnSaveBehavior,
        Key = AttributeKeys.CompletionAction,
        Order = 0 )]

    [CodeEditorField( "Completion Xaml",
        Description = "The XAML markup that will be used if the. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = AttributeDefaults.CompletionXaml,
        Category = AttributeCategories.OnSaveBehavior,
        Key = AttributeKeys.CompletionXaml,
        Order = 1 )]

    [WorkflowTypeField( "Workflow",
        Description = "An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.",
        AllowMultiple = false,
        IsRequired = false,
        Category = AttributeCategories.OnSaveBehavior,
        Order = 2 )]

    #endregion

    public class PrayerRequestDetails : RockMobileBlockType
    {
        /// <summary>
        /// The page parameter keys for the PrayerRequestDetails block.
        /// </summary>
        public static class PageParameterKeys
        {
            /// <summary>
            /// The request identifier
            /// </summary>
            public const string RequestGuid = "RequestGuid";

            /// <summary>
            /// The request
            /// </summary>
            public const string Request = "Request";
        }

        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the PrayerRequestDetails block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The show category
            /// </summary>
            public const string ShowCategory = "EnableCategory";

            /// <summary>
            /// The parent category
            /// </summary>
            public const string ParentCategory = "ParentCategory";

            /// <summary>
            /// The default category
            /// </summary>
            public const string DefaultCategory = "DefaultCategory";

            /// <summary>
            /// The enable automatic approve
            /// </summary>
            public const string EnableAutoApprove = "EnableAutoApprove";

            /// <summary>
            /// The expires after days
            /// </summary>
            public const string ExpiresAfterDays = "ExpiresAfterDays";

            /// <summary>
            /// The show header
            /// </summary>
            public const string ShowHeader = "EnableHeader";

            /// <summary>
            /// The show urgent flag
            /// </summary>
            public const string ShowUrgentFlag = "EnableUrgentFlag";

            /// <summary>
            /// The show public display flag
            /// </summary>
            public const string ShowPublicDisplayFlag = "EnablePublicDisplayFlag";

            /// <summary>
            /// The default to public
            /// </summary>
            public const string DefaultToPublic = "DefaultToPublic";

            /// <summary>
            /// The character limit
            /// </summary>
            public const string CharacterLimit = "CharacterLimit";

            /// <summary>
            /// The show campus
            /// </summary>
            public const string ShowCampus = "EnableCampus";

            /// <summary>
            /// The require campus
            /// </summary>
            public const string RequireCampus = "RequireCampus";

            /// <summary>
            /// The require last name
            /// </summary>
            public const string RequireLastName = "RequireLastName";

            /// <summary>
            /// The enable person matching
            /// </summary>
            public const string EnablePersonMatching = "EnablePersonMatching";

            /// <summary>
            /// The completion action
            /// </summary>
            public const string CompletionAction = "CompletionAction";

            /// <summary>
            /// The completion xaml
            /// </summary>
            public const string CompletionXaml = "CompletionXaml";

            /// <summary>
            /// The workflow
            /// </summary>
            public const string Workflow = "Workflow";
        }

        /// <summary>
        /// The block attribute categories for the PrayerRequestDetails block.
        /// </summary>
        public static class AttributeCategories
        {
            /// <summary>
            /// The category selection
            /// </summary>
            public const string CategorySelection = "Category Selection";

            /// <summary>
            /// The features
            /// </summary>
            public const string Features = "Features";

            /// <summary>
            /// The on save behavior
            /// </summary>
            public const string OnSaveBehavior = "On Save Behavior";
        }

        /// <summary>
        /// The block attribute default values for the PrayerRequestDetails block.
        /// </summary>
        public static class AttributeDefaults
        {
            /// <summary>
            /// The completion xaml
            /// </summary>
            public const string CompletionXaml = @"<Rock:NotificationBox NotificationType=""Success"">
    Thank you for allowing us to pray for you.
</Rock:NotificationBox>";
        }

        /// <summary>
        /// Gets a value indicating whether [show category].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show category]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowCategory => GetAttributeValue( AttributeKeys.ShowCategory ).AsBoolean();

        /// <summary>
        /// Gets the parent category.
        /// </summary>
        /// <value>
        /// The parent category.
        /// </value>
        protected Guid? ParentCategory => GetAttributeValue( AttributeKeys.ParentCategory ).AsGuidOrNull();

        /// <summary>
        /// Gets the default category.
        /// </summary>
        /// <value>
        /// The default category.
        /// </value>
        protected Guid? DefaultCategory => GetAttributeValue( AttributeKeys.DefaultCategory ).AsGuidOrNull();

        /// <summary>
        /// Gets a value indicating whether [enable automatic approve].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable automatic approve]; otherwise, <c>false</c>.
        /// </value>
        protected bool EnableAutoApprove => GetAttributeValue( AttributeKeys.EnableAutoApprove ).AsBoolean();

        /// <summary>
        /// Gets the expires after days.
        /// </summary>
        /// <value>
        /// The expires after days.
        /// </value>
        protected int ExpiresAfterDays => GetAttributeValue( AttributeKeys.ExpiresAfterDays ).AsInteger();

        /// <summary>
        /// Gets a value indicating whether [show header].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show header]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowHeader => GetAttributeValue( AttributeKeys.ShowHeader ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [show urgent flag].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show urgent flag]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowUrgentFlag => GetAttributeValue( AttributeKeys.ShowUrgentFlag ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [show public display flag].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show public display flag]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowPublicDisplayFlag => GetAttributeValue( AttributeKeys.ShowPublicDisplayFlag ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [default to public].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [default to public]; otherwise, <c>false</c>.
        /// </value>
        protected bool DefaultToPublic => GetAttributeValue( AttributeKeys.DefaultToPublic ).AsBoolean();

        /// <summary>
        /// Gets the character limit.
        /// </summary>
        /// <value>
        /// The character limit.
        /// </value>
        protected int CharacterLimit => GetAttributeValue( AttributeKeys.CharacterLimit ).AsInteger();

        /// <summary>
        /// Gets a value indicating whether [show campus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show campus]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowCampus => GetAttributeValue( AttributeKeys.ShowCampus ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [require campus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require campus]; otherwise, <c>false</c>.
        /// </value>
        protected bool RequireCampus => GetAttributeValue( AttributeKeys.RequireCampus ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [require last name].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require last name]; otherwise, <c>false</c>.
        /// </value>
        protected bool RequireLastName => GetAttributeValue( AttributeKeys.RequireLastName ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether [enable person matching].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable person matching]; otherwise, <c>false</c>.
        /// </value>
        protected bool EnablePersonMatching => GetAttributeValue( AttributeKeys.EnablePersonMatching ).AsBoolean();

        /// <summary>
        /// Gets the completion action.
        /// </summary>
        /// <value>
        /// The completion action.
        /// </value>
        protected int CompletionAction => GetAttributeValue( AttributeKeys.CompletionAction ).AsInteger();

        /// <summary>
        /// Gets the completion xaml.
        /// </summary>
        /// <value>
        /// The completion xaml.
        /// </value>
        protected string CompletionXaml => GetAttributeValue( AttributeKeys.CompletionXaml );

        /// <summary>
        /// Gets the workflow.
        /// </summary>
        /// <value>
        /// The workflow.
        /// </value>
        protected Guid? Workflow => GetAttributeValue( AttributeKeys.Workflow ).AsGuidOrNull();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 1;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Prayer.PrayerRequestDetails";

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
            string content;
            string fieldsContent;
            var parameters = new Dictionary<string, string>();

            using ( var rockContext = new RockContext() )
            {
                Guid? requestGuid = RequestContext.GetPageParameter( PageParameterKeys.RequestGuid ).AsGuidOrNull();
                PrayerRequest request = null;

                if ( requestGuid.HasValue )
                {
                    if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return "<Rock:NotificationBox HeaderText=\"Error\" Text=\"You are not authorized to edit prayer requests.\" NotificationType=\"Error\" />";
                    }

                    request = new PrayerRequestService( rockContext ).Get( requestGuid.Value );

                    if ( request == null )
                    {
                        return "<Rock:NotificationBox HeaderText=\"Error\" Text=\"We couldn't find that prayer request.\" NotificationType=\"Error\" />";
                    }
                }

                content = $@"
<StackLayout StyleClass=""prayerdetail"">
    ##HEADER##

    ##FIELDS##
    
    <Rock:Validator x:Name=""vForm"">
        ##VALIDATORS##
    </Rock:Validator>
    
    <Rock:NotificationBox x:Name=""nbError"" NotificationType=""Error"" />
    
    <Button StyleClass=""btn,btn-primary"" Text=""Save"" Margin=""24 0 0 0"" Command=""{{Binding Callback}}"">
        <Button.CommandParameter>
            <Rock:CallbackParameters Name=""Save"" Validator=""{{x:Reference vForm}}"" Notification=""{{x:Reference nbError}}"">
                ##PARAMETERS##
            </Rock:CallbackParameters>
        </Button.CommandParameter>
    </Button>

    <Button StyleClass=""btn,btn-link"" Text=""Cancel"" ##CANCEL## />
</StackLayout>";

                if ( ShowHeader )
                {
                    content = content.Replace( "##HEADER##", $@"<Label StyleClass=""h2"" Text=""{( request == null ? "Add" : "Edit" )} Prayer Request"" />
    <Rock:Divider />" );
                }
                else
                {
                    content = content.Replace( "##HEADER##", "" );
                }

                fieldsContent = BuildCommonFields( request, parameters );
            }

            var validatorsContent = parameters.Keys.Select( a => $"<x:Reference>{a}</x:Reference>" );
            var parametersContent = parameters.Select( a => $"<Rock:Parameter Name=\"{a.Key}\" Value=\"{{Binding {a.Value}, Source={{x:Reference {a.Key}}}}}\" />" );

            //
            // On cancel, pop to the parent page.
            //
            content = content.Replace( "##CANCEL##", "Command=\"{Binding PopPage}\"" );

            return content.Replace( "##FIELDS##", fieldsContent )
                .Replace( "##VALIDATORS##", string.Join( string.Empty, validatorsContent ) )
                .Replace( "##PARAMETERS##", string.Join( string.Empty, parametersContent ) );
        }

        /// <summary>
        /// Builds the common fields.
        /// </summary>
        /// <param name="request">The prayer request.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>A string containing the XAML that represents the common Group fields.</returns>
        private string BuildCommonFields( PrayerRequest request, Dictionary<string, string> parameters )
        {
            var sb = new StringBuilder();
            string field;

            string firstName = request != null ? request.FirstName : RequestContext.CurrentPerson?.FirstName;
            string lastName = request != null ? request.LastName : RequestContext.CurrentPerson?.LastName;
            string email = request != null ? request.Email : RequestContext.CurrentPerson?.Email;

            field = MobileHelper.GetTextEditFieldXaml( "firstName", "First Name", firstName, true );
            sb.AppendLine( MobileHelper.GetSingleFieldXaml( field ) );
            parameters.Add( "firstName", "Text" );

            field = MobileHelper.GetTextEditFieldXaml( "lastName", "Last Name", lastName, RequireLastName );
            sb.AppendLine( MobileHelper.GetSingleFieldXaml( field ) );
            parameters.Add( "lastName", "Text" );

            field = MobileHelper.GetEmailEditFieldXaml( "email", "Email", email, false );
            sb.AppendLine( MobileHelper.GetSingleFieldXaml( field ) );
            parameters.Add( "email", "Text" );

            if ( ShowCampus && CampusCache.All().Where( a => a.IsActive ?? false ).Count() > 1 )
            {
                field = $"<Rock:CampusPicker x:Name=\"campus\" Label=\"Campus\" IsRequired=\"{RequireCampus}\" SelectedValue=\"{request?.Campus?.Guid.ToStringSafe()}\" />";
                sb.AppendLine( MobileHelper.GetSingleFieldXaml( field ) );
                parameters.Add( "campus", "SelectedValue" );
            }

            if ( ShowCategory && ParentCategory.HasValue )
            {
                var items = CategoryCache.Get( ParentCategory.Value )
                    .Categories
                    .Select( a => new KeyValuePair<string, string>( a.Guid.ToString(), a.Name ) );

                var categoryGuid = request?.Category?.Guid;
                if ( !categoryGuid.HasValue && DefaultCategory.HasValue )
                {
                    categoryGuid = CategoryCache.Get( DefaultCategory.Value ).Guid;
                }

                field = MobileHelper.GetDropDownFieldXaml( "category", "Category", categoryGuid.ToStringSafe(), true, items );
                sb.AppendLine( MobileHelper.GetSingleFieldXaml( field ) );
                parameters.Add( "category", "SelectedValue" );
            }

            field = MobileHelper.GetTextEditFieldXaml( "request", "Request", request?.Text, true, true, CharacterLimit );
            sb.AppendLine( MobileHelper.GetSingleFieldXaml( field ) );
            parameters.Add( "request", "Text" );

            if ( ShowPublicDisplayFlag )
            {
                field = MobileHelper.GetCheckBoxFieldXaml( "allowPublication", "Allow Publication", request?.IsPublic ?? DefaultToPublic );
                sb.AppendLine( MobileHelper.GetSingleFieldXaml( field ) );
                parameters.Add( "allowPublication", "IsChecked" );
            }

            if ( ShowUrgentFlag )
            {
                field = MobileHelper.GetCheckBoxFieldXaml( "urgent", "Urgent", request?.IsUrgent ?? false );
                sb.AppendLine( MobileHelper.GetSingleFieldXaml( field ) );
                parameters.Add( "urgent", "IsChecked" );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Saves the prayer request.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The response to send back to the client.</returns>
        private CallbackResponse SaveRequest( Dictionary<string, object> parameters )
        {
            using ( var rockContext = new RockContext() )
            {
                var prayerRequestService = new PrayerRequestService( rockContext );
                PrayerRequest prayerRequest;
                var requestGuid = RequestContext.GetPageParameter( PageParameterKeys.RequestGuid ).AsGuidOrNull();

                if ( requestGuid.HasValue )
                {
                    prayerRequest = prayerRequestService.Get( requestGuid.Value );

                    if ( prayerRequest == null || !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        return new CallbackResponse
                        {
                            Error = "You are not authorized to edit prayer requests."
                        };
                    }
                }
                else
                {
                    int? categoryId = null;

                    if ( DefaultCategory.HasValue )
                    {
                        categoryId = CategoryCache.Get( DefaultCategory.Value ).Id;
                    }

                    prayerRequest = new PrayerRequest
                    {
                        Id = 0,
                        IsActive = true,
                        IsApproved = EnableAutoApprove,
                        AllowComments = false,
                        EnteredDateTime = RockDateTime.Now,
                        CategoryId = categoryId
                    };
                    prayerRequestService.Add( prayerRequest );

                    if ( EnableAutoApprove )
                    {
                        prayerRequest.ApprovedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                        prayerRequest.ApprovedOnDateTime = RockDateTime.Now;
                        prayerRequest.ExpirationDate = RockDateTime.Now.AddDays( ExpiresAfterDays );
                    }
                }

                prayerRequest.FirstName = ( string ) parameters["firstName"];
                prayerRequest.LastName = ( string ) parameters["lastName"];
                prayerRequest.Email = ( string ) parameters["email"];
                prayerRequest.Text = ( string ) parameters["request"];

                if ( ShowCampus )
                {
                    if ( parameters.ContainsKey( "campus" ) )
                    {
                        var campusGuid = ( ( string ) parameters["campus"] ).AsGuidOrNull();

                        if ( campusGuid.HasValue )
                        {
                            prayerRequest.CampusId = CampusCache.Get( campusGuid.Value ).Id;
                        }
                    }
                    else
                    {
                        prayerRequest.CampusId = CampusCache.All().FirstOrDefault( a => a.IsActive ?? false )?.Id;
                    }
                }

                if ( ShowCategory && parameters.ContainsKey( "category" ) )
                {
                    var categoryGuid = ( ( string ) parameters["category"] ).AsGuidOrNull();

                    if ( categoryGuid.HasValue )
                    {
                        prayerRequest.CategoryId = CategoryCache.Get( categoryGuid.Value ).Id;
                    }
                    else if ( prayerRequest.Id > 0 )
                    {
                        prayerRequest.CategoryId = null;
                    }
                }

                if ( ShowPublicDisplayFlag )
                {
                    prayerRequest.IsPublic = ( bool ) parameters["allowPublication"];
                }

                if ( ShowUrgentFlag )
                {
                    prayerRequest.IsUrgent = ( bool ) parameters["urgent"];
                }

                if ( RequestContext.CurrentPerson != null )
                {
                    //
                    // If there is a logged in person and the names still match, meaning they are not
                    // entering a prayer request for somebody else, then set the requested by property.
                    //
                    var person = RequestContext.CurrentPerson;
                    if ( prayerRequest.FirstName == person.FirstName && prayerRequest.LastName == person.LastName )
                    {
                        prayerRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
                    }
                }
                else
                {
                    //
                    // If there is not a logged in person, try to match to an existing person.
                    //
                    var person = MatchPerson( prayerRequest, rockContext );

                    if ( person != null )
                    {
                        prayerRequest.RequestedByPersonAliasId = person.PrimaryAliasId;
                    }
                }

                //
                // Save all changes to database.
                //
                rockContext.SaveChanges();

                StartWorkflow( prayerRequest, rockContext );
            }

            if ( CompletionAction == 0 )
            {
                return new CallbackResponse
                {
                    Content = CompletionXaml ?? string.Empty
                };
            }
            else if ( CompletionAction == 1 )
            {
                return new CallbackResponse
                {
                    Command = "PopPage",
                    CommandParameter = "true"
                };
            }
            else
            {
                return new CallbackResponse
                {
                    Content = AttributeDefaults.CompletionXaml
                };
            }
        }

        /// <summary>
        /// Matches the person if possible.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Person MatchPerson( PrayerRequest prayerRequest, RockContext rockContext )
        {
            if ( EnablePersonMatching )
            {
                var personService = new PersonService( rockContext );
                var person = personService.FindPerson( new PersonService.PersonMatchQuery( prayerRequest.FirstName, prayerRequest.LastName, prayerRequest.Email, null ), false, true, false );

                if ( person == null && prayerRequest.Email.IsNotNullOrWhiteSpace() )
                {
                    var personRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                    var personStatusPending = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid() ).Id;

                    person = new Person
                    {
                        IsSystem = false,
                        RecordTypeValueId = personRecordTypeId,
                        RecordStatusValueId = personStatusPending,
                        FirstName = prayerRequest.FirstName,
                        LastName = prayerRequest.LastName,
                        Gender = Gender.Unknown,
                        Email = prayerRequest.Email,
                        IsEmailActive = true,
                        EmailPreference = EmailPreference.EmailAllowed
                    };

                    PersonService.SaveNewPerson( person, rockContext, prayerRequest.CampusId );
                }

                return person;
            }

            return null;
        }

        /// <summary>
        /// Starts the workflow if one was defined in the block setting.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <param name="rockContext">The rock context.</param>
        private void StartWorkflow( PrayerRequest prayerRequest, RockContext rockContext )
        {
            if ( Workflow.HasValue )
            {
                var workflowType = WorkflowTypeCache.Get( Workflow.Value );
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    try
                    {
                        //
                        // Reload the request to make sure all navigation properties are correct.
                        //
                        prayerRequest = new PrayerRequestService( rockContext ).Get( prayerRequest.Id );

                        var workflow = Model.Workflow.Activate( workflowType, prayerRequest.Name );
                        new WorkflowService( rockContext ).Process( workflow, prayerRequest, out var workflowErrors );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex );
                    }
                }
            }
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the current configuration for this block.
        /// </summary>
        /// <returns>A collection of string/string pairs.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            return new CallbackResponse
            {
                Content = BuildContent()
            };
        }

        /// <summary>
        /// Gets the dynamic XAML content that should be rendered based upon the request.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [BlockAction]
        public object GetCallbackContent( string command, Dictionary<string, object> parameters )
        {
            if ( command == "Save" )
            {
                return SaveRequest( parameters );
            }
            else
            {
                return new CallbackResponse
                {
                    Error = "Invalid command received."
                };
            }
        }

        #endregion
    }
}
