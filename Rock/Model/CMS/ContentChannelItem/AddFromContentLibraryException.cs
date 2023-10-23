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

namespace Rock.Model
{
    /// <summary>
    /// Represents an error that occurs while adding a <see cref="ContentChannelItem"/> from the Content Library.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class AddFromContentLibraryException : Exception
    {
        /// <summary>
        /// The default message, <c>"An unexpected error occurred while downloading the item from the Content Library."</c>
        /// </summary>
        public static readonly string DefaultMessage = "An unexpected error occurred while downloading the item from the Content Library.";

        /// <summary>
        /// Gets or sets the content library item unique identifier.
        /// </summary>
        /// <value>
        /// The content library item unique identifier.
        /// </value>
        public Guid ContentLibraryItemGuid { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddFromContentLibraryException"/> class.
        /// </summary>
        /// <param name="contentLibraryItemGuid">The content library item unique identifier.</param>
        public AddFromContentLibraryException( Guid contentLibraryItemGuid )
            : this( contentLibraryItemGuid, DefaultMessage, null )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddFromContentLibraryException"/> class.
        /// </summary>
        /// <param name="contentLibraryItemGuid">The content library item unique identifier.</param>
        /// <param name="message">The message that describes the error.</param>
        public AddFromContentLibraryException( Guid contentLibraryItemGuid, string message )
            : this( contentLibraryItemGuid, message, null )
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddFromContentLibraryException"/> class.
        /// </summary>
        /// <param name="contentLibraryItemGuid">The content library item unique identifier.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public AddFromContentLibraryException( Guid contentLibraryItemGuid, string message, Exception innerException )
            : base( message, innerException )
        {
            ContentLibraryItemGuid = contentLibraryItemGuid;
        }
    }
}
