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
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Events
{
    /// <summary>
    /// Displays a page to configure and prepare a prayer session.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Prayer Session Setup" )]
    [Category( "Mobile > Prayer" )]
    [Description( "Displays a page to configure and prepare a prayer session." )]
    [IconCssClass( "fa fa-pray" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [TextField( "Title Text",
        Description = "The title to display at the top of the block. Leave blank to hide.",
        IsRequired = false,
        Key = AttributeKeys.TitleText,
        Order = 0 )]

    [MemoField( "Instruction Text",
        Description = "Instructions to help the individual know how to use the block.",
        IsRequired = false,
        Key = AttributeKeys.InstructionText,
        Order = 1 )]

    [LinkedPage( "Prayer Page",
        Description = "The page to push onto the navigation stack to begin the prayer session.",
        IsRequired = true,
        Key = AttributeKeys.PrayerPage,
        Order = 2 )]

    [CategoryField( "Parent Category",
        Description = "The parent category to use as the root category available for the user to pick from.",
        IsRequired = true,
        EntityType = typeof( Rock.Model.PrayerRequest ),
        Key = AttributeKeys.ParentCategory,
        Order = 3)]

    [BooleanField( "Show Campus Filter",
        Description = "If enabled and the user has a primary campus, then the user will be offered to limit prayer requests to just their campus.",
        IsRequired = true,
        Key = AttributeKeys.ShowCampusFilter,
        Order = 4 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_EVENTS_PRAYER_SESSION_SETUP_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "4A3B0D13-FC32-4354-A224-9D450F860BE9")]
    public class PrayerSessionSetup : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="PrayerSessionSetup"/> block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The prayer page key.
            /// </summary>
            public const string PrayerPage = "PrayerPage";

            /// <summary>
            /// The parent category key.
            /// </summary>
            public const string ParentCategory = "ParentCategory";

            /// <summary>
            /// The show campus filter key.
            /// </summary>
            public const string ShowCampusFilter = "ShowCampusFilter";

            /// <summary>
            /// The title text
            /// </summary>
            public const string TitleText = "TitleText";

            /// <summary>
            /// The instruction text
            /// </summary>
            public const string InstructionText = "InstructionText";
        }

        /// <summary>
        /// Gets the prayer page.
        /// </summary>
        /// <value>
        /// The prayer page.
        /// </value>
        protected Guid? PrayerPage => GetAttributeValue( AttributeKeys.PrayerPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the parent category.
        /// </summary>
        /// <value>
        /// The parent category.
        /// </value>
        protected Guid ParentCategory => GetAttributeValue( AttributeKeys.ParentCategory ).AsGuid();

        /// <summary>
        /// Gets the event template.
        /// </summary>
        /// <value>
        /// The event template.
        /// </value>
        protected bool ShowCampusFilter => GetAttributeValue( AttributeKeys.ShowCampusFilter ).AsBoolean();

        /// <summary>
        /// Gets the title text.
        /// </summary>
        /// <value>
        /// The title text.
        /// </value>
        protected string TitleText => GetAttributeValue( AttributeKeys.TitleText );

        /// <summary>
        /// Gets the instruction text.
        /// </summary>
        /// <value>
        /// The instruction text.
        /// </value>
        protected string InstructionText => GetAttributeValue( AttributeKeys.InstructionText );

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

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
                DynamicContent = true
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the content to be shown.
        /// </summary>
        /// <returns>A string containing the XAML content.</returns>
        protected virtual string BuildContent()
        {
            var sb = new StringBuilder();
            var categories = GetCategories();

            var categoryPickerItems = categories
                .Select( a => $"<Rock:PickerItem Value=\"{a.Guid}\" Text=\"{a.Name.EncodeXml( true )}\" />" )
                .JoinStrings( string.Empty );

            var campusPicker = string.Empty;
            var campusValue = "False";
            var title = string.Empty;
            var instructions = string.Empty;

            // Create control
            if ( this.TitleText.IsNotNullOrWhiteSpace() )
            {
                title = $@"<Label StyleClass=""h2,title-text"" Text=""{this.TitleText}"" />";
            }

            // Create instructions
            if ( this.InstructionText.IsNotNullOrWhiteSpace() )
            {
                instructions = $@"<Rock:ParagraphText StyleClass=""instruction-text"">{this.InstructionText}</Rock:ParagraphText>";
            }

            if ( ShowCampusFilter && ( RequestContext.CurrentPerson?.PrimaryCampusId.HasValue ?? false ) )
            {
                campusPicker = @"<Rock:SegmentPicker x:Name=""pCampus"" Margin=""0,12,0,0"">
        <Rock:PickerItem Text=""My Campus"" Value=""True"" />
        <Rock:PickerItem Text=""All Campuses"" Value=""False"" />
    </Rock:SegmentPicker>";
                campusValue = "{Binding Path=SelectedValue, Source={x:Reference pCampus}}";
            }

            sb.Append( $@"<StackLayout>
    {title}
    {instructions}
    <Rock:FieldContainer>
        <Rock:Picker x:Name=""pCategory"" IsRequired=""True"" SelectedValue="""">
            <Rock:PickerItem Value=""{ParentCategory}"" Text=""All Requests"" />
            {categoryPickerItems}
        </Rock:Picker>
    </Rock:FieldContainer>
    {campusPicker}
    <Button Text=""Start Praying"" StyleClass=""btn, btn-primary"" Margin=""0,12,0,0"" Command=""{{Binding PushPage}}"">
        <Button.CommandParameter>
            <Rock:PushPageParameters PageGuid=""{PrayerPage}"">
                <Rock:Parameter Name=""Category""
                                Value=""{{Binding Path=SelectedValue, Source={{x:Reference pCategory}}}}"" />
                <Rock:Parameter Name=""MyCampus""
                                Value=""{campusValue}"" />
            </Rock:PushPageParameters>
        </Button.CommandParameter>
    </Button>
</StackLayout>
" );

            return sb.ToString();
        }

        /// <summary>
        /// Gets the categories to be included in the picker.
        /// </summary>
        /// <returns>A collection of <see cref="CategoryCache"/> objects.</returns>
        protected virtual IEnumerable<CategoryCache> GetCategories()
        {
            var categories = new List<CategoryCache>();
            var parentCategory = CategoryCache.Get( ParentCategory );

            if ( parentCategory != null )
            {
                categories.AddRange( parentCategory.Categories.OrderBy( a => a.Name ) );
            }

            return categories;
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the initial content for this block.
        /// </summary>
        /// <returns>The content to be displayed.</returns>
        [BlockAction]
        public object GetInitialContent()
        {
            return new CallbackResponse
            {
                Content = BuildContent()
            };
        }

        #endregion
    }
}
