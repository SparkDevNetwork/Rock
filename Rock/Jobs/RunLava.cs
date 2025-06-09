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
//S
using System.ComponentModel;

using Rock.Attribute;
using Rock.Lava;
using Rock.Web.UI.Controls;

namespace Rock.Jobs
{
    /// <summary>
    /// This job runs a Lava template on a schedule.
    /// </summary>
    [DisplayName( "Run Lava" )]
    [Description( "This job runs a Lava template on a schedule." )]

    #region Job Attributes

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The Lava template to be run on a schedule. Common merge fields are available. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 800,
        Order = 0,
        IsRequired = true )]

    [LavaCommandsField( "Enabled Lava Commands",
        Key = AttributeKey.EnabledLavaCommands,
        Description = "The Lava commands that should be enabled for this Job.",
        Order = 1,
        IsRequired = false )]

    #endregion Job Attributes

    public class RunLava : RockJob
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LavaTemplate = "LavaTemplate";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
        }

        #endregion Key

        #region Constructors

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public RunLava()
        {
        }

        #endregion Constructors

        #region RockJob Implementation

        /// <inheritdoc/>
        public override void Execute()
        {
            var lavaTemplate = GetAttributeValue( AttributeKey.LavaTemplate );
            if ( lavaTemplate.IsNullOrWhiteSpace() )
            {
                throw new LavaException( "Lava Template is empty." );
            }

            var commonMergeFields = LavaHelper.GetCommonMergeFields( null );
            var enabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

            // Exceptions will be automatically logged to the result (and the "Exception" status will be set) by way of
            // the rock job listener. Otherwise, we'll simply set the job's result as the resolved output.
            this.Result = lavaTemplate.ResolveMergeFields( commonMergeFields, enabledLavaCommands, throwExceptionOnErrors: true )?.Trim();
        }

        #endregion RockJob Implementation
    }
}
