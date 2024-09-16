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
using System.Linq;

using RestSharp;

using Rock.ViewModels.Utility;

namespace Rock.Field.Types
{
    /// <summary>
    /// Picker for selecting a single model for the OpenAI AI Provider.
    /// </summary>
    /// <remarks>
    ///     WARNING! This pattern is not performant for most use-cases.
    ///     Typically we'd want to call to an API only on an as-needed basis;
    ///     while keeping a cached copy to prevent numerous unnecessary API calls.
    ///     In this particular use-case the picker should only be used when configuring an AI Provider
    ///     which is not a task which needs to be completed often.
    /// </remarks>
    public class OpenAIModelPickerFieldType : UniversalItemPickerFieldType
    {
        private const string modelListBaseUrl = "https://rockrms.blob.core.windows.net";
        private const string modelListPath = "/resources/ai/open-ai-models.json";

        /// <inheritdoc/>
        protected sealed override List<ListItemBag> GetItemBags( IEnumerable<string> values, Dictionary<string, string> privateConfigurationValues )
        {
            return GetListItems( privateConfigurationValues )
                .Where( bag => values.Contains( bag.Value ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected sealed override List<ListItemBag> GetListItems( Dictionary<string, string> privateConfigurationValues )
        {
            try
            {
                var client = new RestClient( modelListBaseUrl );
                var request = new RestRequest( modelListPath );
                var response = client.Execute<List<ListItemBag>>( request );

                return response.Data ?? new List<ListItemBag>();

            } catch
            {
                return new List<ListItemBag>();
            }
        }
    }
}
