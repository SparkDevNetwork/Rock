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

namespace Rock.AI.Agent;

/// <summary>
/// Represents the implementation of a chat completion response.
/// </summary>
internal class ChatCompletionResponseImplementation : ChatCompletionResponse
{
    #region Fields

    /// <summary>
    /// The text of the chat completion response.
    /// </summary>
    private readonly string _text;

    #endregion

    #region Properties

    /// <inheritdoc/>
    public override bool IsSuccessful { get; }

    /// <inheritdoc/>
    public override string ErrorMessage { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatCompletionResponseImplementation"/> class.
    /// </summary>
    /// <param name="text">The text of the chat completion response.</param>
    /// <param name="isSuccessful">A value indicating whether the chat completion was successful.</param>
    public ChatCompletionResponseImplementation( string text, bool isSuccessful )
    {
        IsSuccessful = isSuccessful;

        if ( isSuccessful )
        {
            _text = text ?? string.Empty;
        }
        else
        {
            _text = string.Empty;
            ErrorMessage = text;
        }
    }

    #endregion

    #region Methods

    /// <inheritdoc/>
    public override string GetText()
    {
        return _text;
    }

    #endregion
}
