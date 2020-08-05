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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 125, "1.10.2" )]
    public class MobileTemplateUpdatesPostAlpha : Migration
    {
        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //UpdateMobileBlockTemplates();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// JE: Update mobile block templates for final polish.
        /// </summary>
        private void UpdateMobileBlockTemplates()
        {
            //
            // --- Group View
            //

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "6207AF10-B6C9-40B5-8AA5-4C11FA6D0966",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_VIEW,
                "Default",
                @"<StackLayout Spacing=""0"">
    {% assign groupMemberCount = Group.Members | Size %}

    <StackLayout Orientation=""Horizontal"" Spacing=""20"">
        <StackLayout Orientation=""Vertical"" Spacing=""0"" HorizontalOptions=""FillAndExpand"">
            <Label StyleClass=""h1"" Text=""{{ Group.Name | Escape }} Group"" />
            <Label StyleClass=""text"" Text=""{{ 'member' | ToQuantity:groupMemberCount }}"" />
        </StackLayout>
        
        {% if GroupEditPage != '' and AllowedActions.Edit == true %}
            <Rock:Icon IconClass=""Ellipsis-v"" FontSize=""24"" TextColor=""#ccc"" Command=""{Binding ShowActionPanel}"" Margin=""0,8,4,0"">
                <Rock:Icon.CommandParameter>
                    <Rock:ShowActionPanelParameters Title=""Group Actions"" CancelTitle=""Cancel"">
                        <Rock:ActionPanelButton Title=""Edit Group"" 
                            Command=""{Binding PushPage}"" CommandParameter=""{{ GroupEditPage }}?GroupGuid={{ Group.Guid }}"" />
                    </Rock:ShowActionPanelParameters>
                </Rock:Icon.CommandParameter>
            </Rock:Icon>
        {% endif %}
    </StackLayout>
    
    <Rock:Divider StyleClass=""my-24"" />
    
    <!-- Handle Group Attributes -->
    {% if VisibleAttributes != empty %}
        <Rock:ResponsiveLayout ColumnSpacing=""0"">
            {% for attribute in VisibleAttributes %}
                {% if attribute.FormattedValue != '' %}
                    <Rock:ResponsiveColumn ExtraSmall=""6"">
                        <Rock:FieldContainer>
                            <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
                        </Rock:FieldContainer>
                    </Rock:ResponsiveColumn>
                {% endif %}
            {% endfor %}
        </Rock:ResponsiveLayout>
    {% endif %}
    
    <!-- Handle displaying of leaders -->
    {% if ShowLeaderList == true %}
        <Label StyleClass=""title, mt-24"" Text=""Leaders"" />
        <Rock:ResponsiveLayout ColumnSpacing=""0"">
            {% assign members = Group.Members | OrderBy:'Person.FullName' %}
            {% for member in members %}
                {% if member.GroupRole.IsLeader == false %}{% continue %}{% endif %}
                
                    <Rock:ResponsiveColumn ExtraSmall=""6"">
                        <Label Text=""{{ member.Person.FullName }}"" />
                    </Rock:ResponsiveColumn>
                    <Rock:ResponsiveColumn ExtraSmall=""6"">
                        <Label StyleClass=""o-60"" Text=""{{ member.GroupRole.Name }}"" />
                    </Rock:ResponsiveColumn>
                
            {% endfor %}
        </Rock:ResponsiveLayout>
    {% endif %}
    
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            //
            // --- Group Member View
            //

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "F093A516-6D95-429E-8EEB-1DFB0303DF71",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_VIEW,
                "Default",
                @"<StackLayout Spacing=""0"">
    {% assign groupMemberCount = Member.Group.Members | Size %}
    
    <Label StyleClass=""h1"" Text=""{{ Member.Group.Name | Escape }} Group"" />
    <Label StyleClass=""text"" Text=""{{ 'member' | ToQuantity:groupMemberCount }}"" />

    <StackLayout Orientation=""Horizontal"" Spacing=""20"" Margin=""0, 20, 0, 40"">
            <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% if Member.Person.PhotoId != null %}{{ Member.Person.PhotoUrl | Append:'&width=300' | Escape }}{% else %}{{ Member.Person.PhotoUrl | Escape }}{% endif %}"" WidthRequest=""80"">
                <Rock:CircleTransformation />
            </Rock:Image>
            <StackLayout Spacing=""0"" VerticalOptions=""Center"">
                <Label StyleClass=""h4"" Text=""{{ Member.Person.FullName | Escape }}"" />
                {% if Member.Person.BirthDate != null %}
                    <Label StyleClass=""text, o-60"" Text=""Age: {{ Member.Person.AgePrecise | Floor }}"" />
                    <Label StyleClass=""text, o-60"" Text=""Birthdate: {{ Member.Person.BirthDate | Date:'MMMM' }} {{ Member.Person.BirthDate | Date:'d' | NumberToOrdinal }}"" />
                {% endif %}
            </StackLayout>
    </StackLayout>

    <!-- Handle Member Attributes -->
    {% if VisibleAttributes != empty %}
        <Rock:ResponsiveLayout ColumnSpacing=""0"">
            {% for attribute in VisibleAttributes %}
                {% if attribute.FormattedValue != '' %}
                    <Rock:ResponsiveColumn ExtraSmall=""6"">
                        <Rock:FieldContainer Margin=""0, 0, 0, {% if forloop.last %}40{% else %}10{% endif %}"">
                            <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
                        </Rock:FieldContainer>
                    </Rock:ResponsiveColumn>
                {% endif %}
            {% endfor %}
        </Rock:ResponsiveLayout>
    {% endif %}

    <!-- Contact options -->
    {% assign hasContact = false %}
    {% if Member.Person.Email != '' %}
        {% assign hasContact = true %}
        <Rock:Divider />
        <StackLayout Orientation=""Horizontal"" Padding=""0,16"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ Member.Person.Email | Escape }}"" />
                <Label Text=""Email"" />
            </StackLayout>
            <Rock:Icon IconClass=""envelope"" FontSize=""36"" Command=""{Binding SendEmail}"" CommandParameter=""{{ Member.Person.Email | Escape }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% assign phoneNumber = Member.Person | PhoneNumber:'Mobile' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Mobile',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <Rock:Divider />
        <StackLayout Orientation=""Horizontal"" Padding=""0,16"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Mobile"" />
            </StackLayout>
            <Rock:Icon IconClass=""comment"" FontSize=""36"" Command=""{Binding SendSms}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
            <Rock:Icon IconClass=""phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% assign phoneNumber = Member.Person | PhoneNumber:'Home' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Home',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <Rock:Divider />
        <StackLayout Orientation=""Horizontal"" Padding=""0,16"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Home"" />
            </StackLayout>
            <Rock:Icon IconClass=""phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% if hasContact == true %}
        <Rock:Divider />
    {% endif %}

    {% if GroupMemberEditPage != '' %}
        <Button StyleClass=""btn,btn-primary,mt-32"" Text=""Edit"" Command=""{Binding PushPage}"" CommandParameter=""{{ GroupMemberEditPage }}?GroupMemberGuid={{ Member.Guid }}"" />
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            //
            // --- Group Member List
            //

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "674CF1E3-561C-430D-B4A8-39957AC1BCF1",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST,
                "Default",
                @"<StackLayout>
    {% assign groupMemberCount = Members | Size %}
    
    <Label StyleClass=""h1"" Text=""{{ Title | Escape }}"" />
    <Label StyleClass=""text"" Text=""{{ 'member' | ToQuantity:groupMemberCount }}"" />

    {% if Members != empty %}
        <StackLayout Spacing=""0"" Margin=""0,20,0,0"">
            <Rock:Divider />
            
            {% for member in Members %}
				<StackLayout Orientation=""Horizontal"" Padding=""0,16"" Spacing=""16"">
					<StackLayout.GestureRecognizers>
						<TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?GroupMemberGuid={{ member.Guid }}"" />
					</StackLayout.GestureRecognizers>
					
					{%- if member.PhotoId != null -%}
						<Rock:Image Source=""{{ member.PhotoUrl | Append:'&width=60' | Escape }}"" HeightRequest=""64"" WidthRequest=""64"" Aspect=""Fill"" BackgroundColor=""#ccc"">
							<Rock:RoundedTransformation CornerRadius=""8"" />
						</Rock:Image>
					{%- else -%}
						<Rock:Image Source=""{{ member.PhotoUrl | Append:'&width=600' | Escape }}"" HeightRequest=""64"" WidthRequest=""64"" Aspect=""Fill"" BackgroundColor=""#ccc"">
							<Rock:RoundedTransformation CornerRadius=""8"" />
						</Rock:Image>
					{%- endif -%}
		
		            <StackLayout Spacing=""0"" HorizontalOptions=""FillAndExpand"" VerticalOptions=""Center"">
						<Label StyleClass=""h4"" Text=""{{ member.FullName | Escape }}"" />
						<Label StyleClass=""text, o-60"" Text=""{{ member.GroupRole | Escape }}"" />
					</StackLayout>
					<Rock:Icon IconClass=""chevron-right"" Margin=""0,0,20,0"" VerticalOptions=""Center"" />
				</StackLayout>
				<Rock:Divider />	
			{% endfor %}
        </StackLayout>
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }
    }
}
