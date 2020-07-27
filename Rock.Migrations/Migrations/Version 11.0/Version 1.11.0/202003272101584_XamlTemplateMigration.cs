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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class XamlTemplateMigration : Rock.Migrations.RockMigration
    {
        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
    "6207AF10-B6C9-40B5-8AA5-4C11FA6D0966",
    Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_VIEW,
    "Default",
    @"<StackLayout Spacing=""0"">
    <StackLayout Orientation=""Horizontal"" Spacing=""20"">
        <StackLayout Orientation=""Vertical"" Spacing=""0"" HorizontalOptions=""FillAndExpand"">
            <Label StyleClass=""heading1"" Text=""{{ Group.Name | Escape }} Group"" />
            <Label Text=""{{ Group.Members | Size }} members"" LineHeight=""0.8"" />
        </StackLayout>
        {% if GroupEditPage != '' and AllowedActions.Edit == true %}
        <Rock:Icon IconClass=""Ellipsis-v"" FontSize=""24"" TextColor=""#ccc"" Command=""{Binding ShowActionPanel}"">
            <Rock:Icon.CommandParameter>
                <Rock:ShowActionPanelParameters Title=""Group Actions"" CancelTitle=""Cancel"">
                    <Rock:ActionPanelButton Title=""Edit Group"" Command=""{Binding PushPage}"" CommandParameter=""{{ GroupEditPage }}?GroupId={{ Group.Id }}"" />
                </Rock:ShowActionPanelParameters>
            </Rock:Icon.CommandParameter>
        </Rock:Icon>
        {% endif %}
    </StackLayout>

    <BoxView Color=""#ccc"" HeightRequest=""1"" Margin=""0, 30, 0, 10"" />

    <!-- Handle Group Attributes -->
    {% if VisibleAttributes != empty %}
        <Rock:ResponsiveLayout>
        {% for attribute in VisibleAttributes %}
            <Rock:ResponsiveColumn ExtraSmall=""6"">
                <Rock:SingleField Wrapped=""false"">
                    <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
                </Rock:SingleField>
            </Rock:ResponsiveColumn>
        </Rock:ResponsiveLayout>
        {% endfor %}
    {% endif %}

    <!-- Handle displaying of leaders -->
    {% if ShowLeaderList == true %}
        <Label Text=""Leaders"" StyleClass=""field-title"" Margin=""0, 40, 0, 0"" />
        <Grid RowSpacing=""0"" ColumnSpacing=""20"">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width=""Auto"" />
                <ColumnDefinition Width=""*"" />
            </Grid.ColumnDefinitions>
        {% assign row = 0 %}
        {% assign members = Group.Members | OrderBy:'Person.FullName' %}
        {% for member in members %}
            {% if member.GroupRole.IsLeader == false %}{% continue %}{% endif %}
            <Label Grid.Row=""{{ row }}"" Grid.Column=""0"" Text=""{{ member.Person.FullName }}"" />
            <Label Grid.Row=""{{ row }}"" Grid.Column=""1"" Text=""{{ member.GroupRole.Name }}"" />
            {% assign row = row | Plus:1 %}
        {% endfor %}
        </Grid>
    {% endif %}
</StackLayout>",
    STANDARD_ICON_SVG,
    "standard-template.svg",
    "image/svg+xml" );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "F093A516-6D95-429E-8EEB-1DFB0303DF71",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_VIEW,
                "Default",
                @"<StackLayout Spacing=""0"">
    <Label StyleClass=""heading1"" Text=""{{ Member.Group.Name | Escape }} Group"" />
    <Label Text=""{{ Member.Group.Members | Size }} members"" LineHeight=""0.8"" />

    <StackLayout Orientation=""Horizontal"" Spacing=""20"" Margin=""0, 20, 0, 40"">
            <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% if Member.Person.PhotoId != null %}{{ Member.Person.PhotoUrl | Append:'&width=120' | Escape }}{% else %}{{ Member.Person.PhotoUrl | Escape }}{% endif %}"" WidthRequest=""80"">
                <Rock:CircleTransformation />
            </Rock:Image>
            <StackLayout Spacing=""0"" VerticalOptions=""Center"">
                <Label FontSize=""20"" FontAttributes=""Bold"" Text=""{{ Member.Person.FullName | Escape }}"" />
                {% if Member.Person.BirthDate != null %}
                    <Label LineHeight=""0.85"" TextColor=""#888"" Text=""Age: {{ Member.Person.AgePrecise | Floor }}"" />
                    <Label LineHeight=""0.85"" TextColor=""#888"" Text=""Birthdate: {{ Member.Person.BirthDate | Date:'MMMM' }} {{ Member.Person.BirthDate | Date:'d' | NumberToOrdinal }}"" />
                {% endif %}
            </StackLayout>
    </StackLayout>

    <!-- Handle Member Attributes -->
    {% if VisibleAttributes != empty %}
        {% for attribute in VisibleAttributes %}
        <Rock:SingleField Wrapped=""false"" Margin=""0, 0, 0, {% if forloop.last %}40{% else %}10{% endif %}"">
            <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
        </Rock:SingleField>
        {% endfor %}
    {% endif %}

    <!-- Contact options -->
    {% assign hasContact = false %}
    {% if Member.Person.Email != '' %}
        {% assign hasContact = true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        <StackLayout Orientation=""Horizontal"" Padding=""12"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ Member.Person.Email | Escape }}"" />
                <Label Text=""Email"" />
            </StackLayout>
            <Rock:Icon IconClass=""Envelope"" FontSize=""36"" Command=""{Binding SendEmail}"" CommandParameter=""{{ Member.Person.Email | Escape }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% assign phoneNumber = Member.Person | PhoneNumber:'Mobile' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Mobile',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        <StackLayout Orientation=""Horizontal"" Padding=""12"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Mobile"" />
            </StackLayout>
            <Rock:Icon IconClass=""Comment"" FontSize=""36"" Command=""{Binding SendSms}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
            <Rock:Icon IconClass=""Phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% assign phoneNumber = Member.Person | PhoneNumber:'Home' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Home',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        <StackLayout Orientation=""Horizontal"" Padding=""12"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Home"" />
            </StackLayout>
            <Rock:Icon IconClass=""Phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% if hasContact == true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
    {% endif %}

    {% if GroupMemberEditPage != '' %}
        <Button StyleClass=""btn,btn-primary"" Text=""Edit"" Margin=""0, 40, 0, 0"" WidthRequest=""200"" HorizontalOptions=""Center"" Command=""{Binding PushPage}"" CommandParameter=""{{ GroupMemberEditPage }}?GroupMemberId={{ Member.Id }}"" />
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "E3A4AA4E-2A61-4E63-B636-93B86E493D95",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_LIST,
                "Default",
                @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
    <StackLayout Spacing=""0"">
        <Label StyleClass=""calendar-event-title"" Text=""{Binding Name}"" />
        {% if Item.EndDateTime == null %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
        {% else %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }} - {{ Item.EndDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
        {% endif %}
        <StackLayout Orientation=""Horizontal"">
            <Label HorizontalOptions=""FillAndExpand"" StyleClass=""calendar-event-audience"" Text=""{{ Item.Audiences | Select:'Name' | Join:', ' | Escape }}"" />
            <Label StyleClass=""calendar-event-campus"" Text=""{{ Item.Campus | Escape }}"" HorizontalTextAlignment=""End"" LineBreakMode=""NoWrap"" />
        </StackLayout>
    </StackLayout>
</Frame>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "2B0F4548-8DA7-4236-9BF9-5FA3C07D762F",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_PRAYER_SESSION,
                "Default",
                @"{% if Request != null %}
<StackLayout Spacing=""0"">
    {% if Request.RequestedByPersonAlias != null %}
        {% if Request.RequestedByPersonAlias.Person.PhotoId != null %}
            {% assign photoUrl = Request.RequestedByPersonAlias.Person.PhotoUrl | Append:'&width=120' | Escape %}
        {% else %}
            {% assign photoUrl = Request.RequestedByPersonAlias.Person.PhotoUrl %}
        {% endif %}
    {% else %}
        {% assign photoUrl = 'Assets/Images/person-no-photo-unknown.svg' %}
    {% endif %}

    <StackLayout Orientation=""Horizontal"">
        <Label Text=""{{ Request.Category.Name | Escape }}""
                    HorizontalOptions=""StartAndExpand"" />
        <Rock:Icon IconFamily=""FontAwesomeRegular""
                           IconClass=""Times-Circle""
                          FontSize=""28""
                          TextColor=""#afafaf""
                          Command=""{Binding PopPage}"" />
    </StackLayout>

    <StackLayout Orientation=""Horizontal"">
        <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' | Append:photoUrl | Escape}}""
                              HeightRequest=""80""
                              BackgroundColor=""#afafaf""
                              HorizontalOptions=""StartAndExpand"">
            <Rock:CircleTransformation />
        </Rock:Image>
        <Rock:FollowingIcon HorizontalOptions=""End""
                                          VerticalOptions=""Center""
                                          EntityTypeId=""{{ Request.TypeId }}""
                                          EntityId=""{{ Request.Id }}""
                                          FontSize=""28""
                                          IsFollowed=""{{ Request | IsFollowed }}""
                                          FollowingIconClass=""Heart""
                                          FollowingIconFamily=""FontAwesomeSolid""
                                          FollowingIconColor=""#ff3434""
                                          NotFollowingIconClass=""Heart""
                                          NotFollowingIconFamily=""FontAwesomeRegular""
                                          NotFollowingIconColor=""#afafaf"" />
    </StackLayout>

    <Label StyleClass=""heading1""
                Text=""Pray for {{ Request.FirstName | Escape }}"" />
    <Label Text=""{{ Request.Campus.Name }}"" />
    <Label Margin=""0,30,0,60"">{{ Request.Text | XamlWrap }}</Label>

    <Button StyleClass=""btn,btn-primary""
                  Text=""{{ PrayedButtonText | Escape }}""
                  Command=""{Binding Callback}""
                  CommandParameter=""{Rock:CallbackParameters Name=':NextRequest', Parameters={Rock:Parameter Name='SessionContext', Value='{{ SessionContext }}'}}"" />

    {% if ShowInappropriateButton %}
    <Button StyleClass=""btn,btn-link""
                  Text=""Report prayer as inappropriate""
                  Command=""{Binding ShowActionPanel}"">
        <Button.CommandParameter>
            <Rock:ShowActionPanelParameters Title=""Report Prayer Request""
                                            CancelTitle=""Cancel"">
                <Rock:ShowActionPanelParameters.DestructiveButton>
                    <Rock:ActionPanelButton Title=""Report""
                                            Command=""{Binding Callback}""
                                            CommandParameter=""{Rock:CallbackParameters Name=':FlagRequest', Parameters={Rock:Parameter Name='SessionContext', Value='{{ SessionContext }}'}}"" />
                </Rock:ShowActionPanelParameters.DestructiveButton>
            </Rock:ShowActionPanelParameters>
        </Button.CommandParameter>
    </Button>
    {% endif %}
</StackLayout>
{% else %}
<StackLayout Spacing=""0"">
    <Label Text=""You have completed your prayer session."" />
    <Button StyleClass=""btn,btn-primary""
                  Text=""Ok""
                  Command=""{Binding PopPage}"" />
</StackLayout>
{% endif %}",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
