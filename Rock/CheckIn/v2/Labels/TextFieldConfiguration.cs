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

using System.Collections.Generic;

using Rock.Enums.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels
{
    /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag"/>
    internal class TextFieldConfiguration : IFieldConfiguration
    {
        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.SourceKey" path="/summary"/>
        public string SourceKey { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.FormatterOptionKey" path="/summary"/>
        public string FormatterOptionKey { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.CollectionFormat" path="/summary"/>
        public TextCollectionFormat CollectionFormat { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.PlaceholderText" path="/summary"/>
        public string PlaceholderText { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.IsDynamicText" path="/summary"/>
        public bool IsDynamicText { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.StaticText" path="/summary"/>
        public string StaticText { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.DynamicTextTemplate" path="/summary"/>
        public string DynamicTextTemplate { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.FontSize" path="/summary"/>
        public double FontSize { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.AdaptiveFontSize" path="/summary"/>
        public Dictionary<int, double> AdaptiveFontSize { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.HorizontalAlignment" path="/summary"/>
        public HorizontalTextAlignment HorizontalAlignment { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.IsBold" path="/summary"/>
        public bool IsBold { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.IsColorInverted" path="/summary"/>
        public bool IsColorInverted { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.IsCondensed" path="/summary"/>
        public bool IsCondensed { get; set; }

        /// <inheritdoc cref="Rock.ViewModels.CheckIn.Labels.TextFieldConfigurationBag.MaxLength" path="/summary"/>
        public int MaxLength { get; set; }

        /// <inheritdoc/>
        public void Initialize( Dictionary<string, string> values )
        {
            SourceKey = values.GetValueOrNull( "sourceKey" );
            FormatterOptionKey = values.GetValueOrNull( "formatterOptionKey" );
            CollectionFormat = values.GetValueOrNull( "collecitonFormat" ).ConvertToEnumOrNull<TextCollectionFormat>() ?? TextCollectionFormat.FirstItemOnly;
            PlaceholderText = values.GetValueOrNull( "placeholderText" );
            IsDynamicText = values.GetValueOrNull( "isDynamicText" ).AsBoolean();
            StaticText = values.GetValueOrNull( "staticText" );
            DynamicTextTemplate = values.GetValueOrNull( "dynamicTextTemplate" );
            FontSize = ( double ) ( values.GetValueOrNull( "fontSize" ).AsDecimalOrNull() ?? 12 );
            // TODO: AdaptiveFontSize
            HorizontalAlignment = values.GetValueOrNull( "horizontalAlignment" ).ConvertToEnumOrNull<HorizontalTextAlignment>() ?? HorizontalTextAlignment.Left;
            IsBold = values.GetValueOrNull( "isBold" ).AsBoolean();
            IsColorInverted = values.GetValueOrNull( "isColorInverted" ).AsBoolean();
            IsCondensed = values.GetValueOrNull( "isCondensed" ).AsBoolean();
            MaxLength = values.GetValueOrNull( "maxLength" ).AsInteger();
        }
    }
}
