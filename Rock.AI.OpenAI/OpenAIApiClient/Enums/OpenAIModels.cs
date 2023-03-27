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

using Rock.AI.OpenAI.OpenAIApiClient.Attributes;

namespace Rock.AI.OpenAI.OpenAIApiClient.Enums
{
    /// <summary>
    /// Enum to hold the various models OpenAI supports with some of the configuration we'll need
    /// to use these models.
    /// </summary>
    public enum OpenAIModel
    {
        [OpenAIModelProperties( "", 0 )]
        Default = 0,

        [OpenAIModelProperties( "text-davinci-002", 4097 )]
        DaVinci2 = 1,

        [OpenAIModelProperties( "text-davinci-003", 4097 )]
        DaVinci3 = 2,

        [OpenAIModelProperties( "gpt-3.5-turbo", 4096 )]
        GPT3_5_Turbo = 3,

        [OpenAIModelProperties( "gpt-4", 8192 )]
        GPT4 = 4,

        [OpenAIModelProperties( "gpt-4-32k", 32768 )]
        GPT4_32 = 5,
    }
}
