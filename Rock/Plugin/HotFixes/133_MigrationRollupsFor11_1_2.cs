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
    [MigrationNumber( 133, "1.11.0" )]
    public class MigrationRollupsFor11_1_2 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //FixExpiredContentChannelItems();
            //MobileTemplateUpdate();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// NA: Fix Expired Content Channel Items
        /// </summary>
        private void FixExpiredContentChannelItems()
        {
            // This will fix the 20 well-known Content Channel Items that were added 
            // in Rock v1.0 with an expiration date of 2020-08-02, if they still exist and
            // still have the same expiration date value
            Sql( @"
                UPDATE 
                    [ContentChannelItem] 
                SET
                    [ExpireDateTime] = '2050-08-02 00:00:00.000' 
                WHERE 
                    [ExpireDateTime] = '2020-08-02 00:00:00.000' 
                    AND [Guid] IN ( 
                         '3b8e1859-e42f-4f01-9007-d3e04429f17d'
                        ,'41dc3262-1588-4142-ac09-3266fdd6aa17'
                        ,'462fdda7-2fb6-41a7-9671-30865d991a33'
                        ,'6b5e7f2b-f55b-42e1-bca4-64206bc9e276'
                        ,'8c5d4d99-7ff0-4cbf-ae5c-b7c9075ebe05'
                        ,'699c7c21-5798-4150-ae28-379d335fabe4'
                        ,'e78148ea-35c8-43a8-b4c2-3fab7925df53'
                        ,'a9a15e0e-f736-4f84-9527-50217b4e9091'
                        ,'e214f2a7-cb53-4640-acd5-cead2190c99d'
                        ,'095466ed-029c-48bc-bcbb-fcf79cf8a68b'
                        ,'f946a31a-59cf-43a8-a595-b01ab2b69d6e'
                        ,'4d9a94be-33d9-4a38-8f36-393de13ccfb7'
                        ,'4134a71f-80df-4482-bd33-3db4db9ec409'
                        ,'9aa0e16d-fe65-4164-b3a9-406e53a40f99'
                        ,'5a95642a-a181-4043-a596-bd47d65b8654'
                        ,'c9029380-7295-4a0b-9cb6-2c134ac12c72'
                        ,'08e0e677-1f98-487f-accd-7b0ec5ad50d1'
                        ,'8a3d944e-ad82-479f-89d5-9729cdb5d4c8'
                        ,'006d5288-fe49-4c5d-82b6-bac36445cd83'
                        ,'d5592927-2faf-4131-8d6d-e9ea58165522')" );

        }

        /// <summary>
        /// JE - Mobile Template Update
        /// </summary>
        private void MobileTemplateUpdate()
        {
            string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

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
					
					
					<Rock:Image Source=""{{ member.PhotoUrl | Append:'&width=400' | Escape }}"" HeightRequest=""64"" WidthRequest=""64"" Aspect=""AspectFill"" BackgroundColor=""#ccc"">
						<Rock:RoundedTransformation CornerRadius=""8"" />
					</Rock:Image>
					
		
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
