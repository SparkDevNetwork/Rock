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
    [MigrationNumber( 145, "1.12.6" )]
    public class UpdateWistiaVideosToVimeo : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141373"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '0DEC1A4C-F60F-4391-AFF8-EB93DD94177C');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141373"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'FF502AB8-EBC2-4329-8CA1-2F8207BAF368');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141423"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'A16C6B04-F9AE-4660-A23B-131BF1660417');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141423"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'DF73F414-C0AE-45CC-B86F-AD768D56931D');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141483"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'E8015334-C624-48BC-A08E-74EA557D24FA');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141483"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'E330D555-FFD7-49E9-B99D-84F3FC50E398');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140493"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'BA279A1C-1EC9-4F9C-8AF9-313C6995A925');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140493"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'E60565FB-B43F-4B19-BB7F-7D9D05BD9BC5');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142783"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '0D0882DF-EE3D-4196-9A76-26E432A21B4F');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140393"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '5F7A87C6-38B8-4C28-9228-70DE55D456B7');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140450"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '9ABEE2F3-03E5-427A-A052-F3F5811F39E3');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142805"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'D7F52FDA-D015-436A-9817-2DF7D95FA824');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142633"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '29E81661-3CF3-4691-BA23-DA7B039AE30E');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140515"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '120EB145-1705-454A-B9A5-AD6697A55D09');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583140515"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '87EA36D1-8911-4E79-A5EE-B584A03B48E5');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141519"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '36BBE6BC-2BC8-4B94-86CA-CE1D483C30FC');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583141519"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '0741524D-11A1-484C-B233-F43426C2AC9B');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142241"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = '9119897F-1634-4C1A-86B7-8BC9A109C754');
                UPDATE [AttributeValue] SET [Value]=N'<div class=""embed-responsive embed-responsive-16by9""><iframe src=""https://player.vimeo.com/video/583142241"" width=""640"" height=""564"" frameborder=""0"" allow=""autoplay; fullscreen"" allowfullscreen></iframe></div>' WHERE ([Guid] = 'BB6F6B72-EB80-4A7F-BADE-150F8826616D');" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

       
    }
}
