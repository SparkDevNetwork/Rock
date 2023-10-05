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

namespace Rock.Lava
{
    /// <summary>
    /// A context that holds the configuration and data used by the Lava Engine to resolve a Lava template.    
    /// </summary>
    /// <remarks>
    /// This simple context implementation can be used to provide parameters to a render method for any Lava engine implementation,
    /// but the data will be transferred to an engine-specific context to execute the render operation.
    /// </remarks>
    /// <inheritdoc cref="LavaRenderContextBase"/>
    public class LavaRenderContext : LavaRenderContextBase
    {
        #region Factory methods

        /// <summary>
        /// Create a new instance with the specified merge fields.
        /// </summary>
        /// <param name="mergeFields"></param>
        /// <returns></returns>
        public static LavaRenderContext FromMergeValues( LavaDataDictionary mergeFields )
        {
            var newContext = new LavaRenderContext();

            newContext.SetMergeFields( mergeFields );

            return newContext;
        }

        #endregion

        private const string _notSupportedMessage = "The current context does not support this feature. It can only be used as a data container to initiate processing.";

        private List<string> _enabledCommands = new List<string>();
        private LavaDataDictionary _internalFields = new LavaDataDictionary();
        private LavaDataDictionary _mergeFields = new LavaDataDictionary();

        public override void EnterChildScope()
        {
            throw new NotSupportedException( _notSupportedMessage );
        }

        public override void ExitChildScope()
        {
            throw new NotSupportedException( _notSupportedMessage );
        }

        public override List<string> GetEnabledCommands()
        {
            return _enabledCommands;
        }

        public override object GetInternalField( string key, object defaultValue = null )
        {
            return _internalFields.GetValue( key, defaultValue );
        }

        public override LavaDataDictionary GetInternalFields()
        {
            return _internalFields;
        }

        public override object GetMergeField( string key, object defaultValue = null )
        {
            return _mergeFields.GetValue( key, defaultValue );
        }

        public override LavaDataDictionary GetMergeFields()
        {
            return _mergeFields;
        }

        public override void SetEnabledCommands( IEnumerable<string> commands )
        {
            _enabledCommands.Clear();
            _enabledCommands.AddRange( commands );
        }

        public override void SetInternalField( string key, object value )
        {
            _internalFields.SetValue( key, value );
        }

        public override void SetMergeField( string key, object value, LavaContextRelativeScopeSpecifier scope = LavaContextRelativeScopeSpecifier.Default )
        {
            _mergeFields.SetValue( key, value );
        }
    }
}
