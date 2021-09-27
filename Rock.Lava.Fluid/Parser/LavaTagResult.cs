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
using Fluid.Parser;
using Parlot;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// Extends the Fluid TagResult to include the content and position of the source text.
    /// </summary>
    /// <remarks>
    /// Although it would be preferrable to derive from Fluid.TagResult, that Type is sealed.
    /// </remarks>
    public class LavaTagResult
    {
        public LavaTagResult()
        {
            //
        }
        public LavaTagResult( TagResult result, TextSpan text, LavaTagFormatSpecifier format )
        {
            TagResult = result;
            Text = text;
        }

        public TagResult TagResult;
        public TextSpan Text;
        public LavaTagFormatSpecifier TagFormat;
    }
}
