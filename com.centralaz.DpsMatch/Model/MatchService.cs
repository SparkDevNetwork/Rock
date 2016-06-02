// <copyright>
// Copyright by Central Christian Church
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
using System.Text;
using System.Threading.Tasks;

using com.centralaz.DpsMatch.Data;
using Rock.Model;

namespace com.centralaz.DpsMatch.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class MatchService : DpsMatchService<Match>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public MatchService( DpsMatchContext context ) : base( context ) { }

    }
}
