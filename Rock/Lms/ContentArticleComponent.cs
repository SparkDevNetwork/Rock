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
using System.ComponentModel.Composition;

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Data;
using Rock.Enums.Lms;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Utility;

namespace Rock.Lms
{
    /// <summary>
    /// The acknowledgment activity is a learning activity that requires a participant to read an article.
    /// </summary>
    [Description( "A Learning Activity that requires a participant to to read an article." )]
    [Export( typeof( LearningActivityComponent ) )]
    [ExportMetadata( "ComponentName", "ContentArticle" )]

    [RockInternal( "17.0" )]
    [Rock.SystemGuid.EntityTypeGuid( "760FB9B3-8052-4704-A790-4A61B14F0C60" )]
    public class ContentArticleComponent : LearningActivityComponent
    {
        #region Keys

        private class SettingKey
        {
            public const string Header = "header";

            public const string Items = "items";
        }

        private class CompletionKey
        {
            public const string CompletedItems = "completedItems";

            public const string PointsPossibleAtCompletion = "pointsPossibleAtCompletion";
        }

        #endregion

        /// <inheritdoc/>
        public override string HighlightColor => "#644a88"; // TODO - Ask PO

        /// <inheritdoc/>
        public override string IconCssClass => "ti ti-article"; // TODO - Maybe news icon?

        /// <inheritdoc/>
        public override string Name => "Content Article";

        /// <inheritdoc/>
        public override string ComponentUrl => @"/Obsidian/Controls/Internal/LearningActivity/contentArticleLearningActivity.obs";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetActivityConfiguration( LearningClassActivity activity, Dictionary<string, string> componentData, PresentedFor presentation, RockContext rockContext, RockRequestContext requestContext )
        {
            if ( presentation == PresentedFor.Configuration )
            {
                return new Dictionary<string, string>();
            }
            else
            {
                var content = componentData.GetValueOrNull( SettingKey.Header );

                var headerHtml = content.IsNotNullOrWhiteSpace()
                    ? new StructuredContentHelper( content ).Render()
                    : string.Empty;

                if ( headerHtml.IsNotNullOrWhiteSpace() )
                {
                    var mergeFields = requestContext.GetCommonMergeFields();

                    headerHtml = headerHtml.ResolveMergeFields( mergeFields );
                }

                var items = componentData.GetValueOrNull( SettingKey.Items ).FromJsonOrNull<List<ContentArticleItem>>()
                    ?? new List<ContentArticleItem>();

                foreach ( var item in items )
                {
                    if ( item.Type == ContentArticleItemType.Text && item.Text.IsNotNullOrWhiteSpace() )
                    {
                        item.Text = new StructuredContentHelper( item.Text ).Render();
                    }
                }

                return new Dictionary<string, string>
                {
                    [SettingKey.Header] = headerHtml,
                    [SettingKey.Items] = items.ToCamelCaseJson( false, false ),
                };
            }
        }

        #region Support Classes

        private enum ContentArticleItemType
        {
            Text = 0,
            Section = 1,
            Video = 2,
            Note = 3
        }

        private class ContentArticleItem
        {
            public ContentArticleItemType Type { get; set; }

            public Guid UniqueId { get; set; }

            public bool? HasBeenGraded { get; set; }

            public int Order { get; set; }

            public string Text { get; set; }

            public string Title { get; set; }

            public string Summary { get; set; }

            public ListItemBag Video { get; set; }

            public string Label { get; set; }

            public int? InputRows { get; set; }

            public bool? IsRequired { get; set; }

            public string Note { get; set; }
        }

        #endregion
    }
}
