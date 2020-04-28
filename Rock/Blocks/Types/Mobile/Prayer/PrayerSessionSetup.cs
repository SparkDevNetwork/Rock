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
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Prayer Session Setup" )]
    [Category( "Mobile > Prayer" )]
    [Description( "Displays a page to configure and prepare a prayer session." )]
    [IconCssClass( "fa fa-pray" )]

    #region Block Attributes

    [LinkedPage( "Prayer Page",
        Description = "The page to push onto the navigation stack to begin the prayer session.",
        IsRequired = true,
        Key = AttributeKeys.PrayerPage,
        Order = 0 )]

    [CategoryField( "Parent Category",
        Description = "The parent category to use as the root category available for the user to pick from.",
        IsRequired = true,
        EntityType = typeof( Rock.Model.PrayerRequest ),
        Key = AttributeKeys.ParentCategory,
        Order = 1)]

    [BooleanField( "Show Campus Filter",
        Description = "If enabled and the user has a primary campus, then the user will be offered to limit prayer requests to just their campus.",
        IsRequired = true,
        Key = AttributeKeys.ShowCampusFilter,
        Order = 2 )]

    #endregion

    public class PrayerSessionSetup : RockMobileBlockType
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
        public override string MobileBlockType => "Rock.Mobile.Blocks.Prayer.PrayerSessionSetup";

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

            string campusPicker = string.Empty;
            string campusValue = "False";

            if ( ShowCampusFilter && ( RequestContext.CurrentPerson?.PrimaryCampusId.HasValue ?? false ) )
            {
                campusPicker = @"<Rock:SegmentPicker x:Name=""pCampus"" Margin=""0,12,0,0"">
        <Rock:PickerItem Text=""My Campus"" Value=""True"" />
        <Rock:PickerItem Text=""All Campuses"" Value=""False"" />
    </Rock:SegmentPicker>";
                campusValue = "{Binding Path=SelectedValue, Source={x:Reference pCampus}}";
            }

            sb.Append( $@"<StackLayout>
    <Label StyleClass=""heading1"" Text=""Pray For"" />
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
