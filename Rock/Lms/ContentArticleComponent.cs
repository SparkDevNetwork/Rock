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
using System.Linq;

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

        #region Properties

        /// <inheritdoc/>
        public override string HighlightColor => "#e6c229";

        /// <inheritdoc/>
        public override string IconCssClass => "ti ti-article";

        /// <inheritdoc/>
        public override string Name => "Content Article";

        /// <inheritdoc/>
        public override string ComponentUrl => @"/Obsidian/Controls/Internal/LearningActivity/contentArticleLearningActivity.obs";

        #endregion

        #region Methods

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

        /// <inheritdoc/>
        public override Dictionary<string, string> GetComponentData( LearningClassActivity activity, Dictionary<string, string> componentSettings, RockContext rockContext, RockRequestContext requestContext )
        {
            // This is a cheat, we shouldn't really be trying to access the original
            // JSON this way, but we don't have a better way to do it.
            var oldData = activity.LearningActivity?.ActivityComponentSettingsJson?.FromJsonOrNull<Dictionary<string, string>>();

            // Apply any database changes to the header content.
            new StructuredContentHelper( componentSettings?.GetValueOrNull( SettingKey.Header ) )
                .DetectAndApplyDatabaseChanges( oldData?.GetValueOrNull( SettingKey.Header ), rockContext );

            // Get the text items from the old component settings.
            var oldTextItems = oldData?.GetValueOrNull( SettingKey.Items )
                ?.FromJsonOrNull<List<ContentArticleItem>>()
                ?.Where( i => i.Type == ContentArticleItemType.Text )
                .ToList()
                ?? new List<ContentArticleItem>();

            // Get the text items from the new component settings.
            var newTextItems = componentSettings?.GetValueOrNull( SettingKey.Items )
                ?.FromJsonOrNull<List<ContentArticleItem>>()
                ?.Where( i => i.Type == ContentArticleItemType.Text )
                .ToList()
                ?? new List<ContentArticleItem>();

            var deletedItems = oldTextItems
                .Where( i => !newTextItems.Any( ni => ni.UniqueId == i.UniqueId ) )
                .ToList();

            // Apply any database changes to the text items that were deleted.
            foreach ( var item in deletedItems )
            {
                new StructuredContentHelper( string.Empty )
                    .DetectAndApplyDatabaseChanges( item.Text, rockContext );
            }

            // Apply any database changes to the text items that were added or updated.
            foreach ( var item in newTextItems )
            {
                var oldItem = oldTextItems.FirstOrDefault( i => i.UniqueId == item.UniqueId );

                new StructuredContentHelper( item.Text )
                    .DetectAndApplyDatabaseChanges( oldItem?.Text, rockContext );
            }

            return base.GetComponentData( activity, componentSettings, rockContext, requestContext );
        }

        #endregion

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
