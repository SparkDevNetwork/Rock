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
using System.IO;

using Rock.Enums.CheckIn.Labels;

namespace Rock.CheckIn.v2.Labels.Renderers
{
    /// <summary>
    /// <para>
    /// Renders a <see cref="CheckInLabel"/> of type <see cref="LabelFormat.Designed"/>
    /// to the output that can be sent to a printer.
    /// </para>
    /// <para>
    /// A renderer may be used multiple times. Implementations must cleanup any
    /// instance data in the <see cref="EndLabel"/> method.
    /// </para>
    /// </summary>
    internal interface ILabelRenderer : IDisposable
    {
        /// <summary>
        /// Starts writing a label to the stream. This is called at the start
        /// of each label to be printed.
        /// </summary>
        /// <param name="outputStream">The stream to write the contents of the label into.</param>
        /// <param name="printRequest">The object that describes the request to print the label.</param>
        void BeginLabel( Stream outputStream, PrintLabelRequest printRequest );

        /// <summary>
        /// Writes a single field out to the stream that was passed to
        /// <see cref="BeginLabel(Stream, PrintLabelRequest)"/>.
        /// </summary>
        /// <param name="field">The field to be written.</param>
        void WriteField( LabelField field );

        /// <summary>
        /// Called after all fields have been written. The renderer should
        /// perform any final output to the stream and then cleanup any
        /// instance data.
        /// </summary>
        void EndLabel();
    }
}
