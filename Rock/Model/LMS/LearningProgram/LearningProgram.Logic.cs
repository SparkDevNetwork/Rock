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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Lms;

namespace Rock.Model
{
    public partial class LearningProgram
    {
      
    }

    /// <summary>
    /// POCO for encapsulating KPIs for a given <see cref="LearningProgram"/> .
    /// </summary>
    public class LearningProgramKpis
    {
        /// <summary>
        /// Gets or sets the number of active classes in the Program.
        /// </summary>
        public int ActiveClasses { get; set; }

        /// <summary>
        /// Gets or sets the number of active students in the Program.
        /// </summary>
        public int ActiveStudents{ get; set; }

        /// <summary>
        /// Gets or sets the number of students who have completed this Program.
        /// </summary>
        public int Completions { get; set; }
    }
}
