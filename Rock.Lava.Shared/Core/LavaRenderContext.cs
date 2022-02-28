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
    public class LavaRenderContext : LavaRenderContextBase
    {
        #region Factory methods

        public static LavaRenderContext FromMergeValues( LavaDataDictionary mergeValues )
        {
            var newContext = new LavaRenderContext();

            newContext.SetMergeFields( mergeValues );

            return newContext;  
        }

        #endregion

        public object this[string key]
        {
            get
            {
                return _mergeFields[key];
            }
            set
            {
                _mergeFields[key] = value;
            }
        }

        public void EnterChildScope()
        {
            throw new NotSupportedException( _notSupportedMessage );
        }

        public void ExecuteInChildScope( Action<ILavaRenderContext> callback )
        {
            throw new NotSupportedException( _notSupportedMessage );
        }

        public void ExitChildScope()
        {
            throw new NotSupportedException( _notSupportedMessage );
        }

        private List<string> _enabledCommands = new List<string>();


        public List<string> GetEnabledCommands()
        {
            return _enabledCommands;
        }

        private LavaDataDictionary _internalFields = new LavaDataDictionary();
        private LavaDataDictionary _mergeFields = new LavaDataDictionary();

        public object GetInternalField( string key, object defaultValue = null )
        {
            return _internalFields.GetValue( key, defaultValue );
        }

        public LavaDataDictionary GetInternalFields()
        {
            return _internalFields;
        }

        public object GetMergeField( string key, object defaultValue = null )
        {
            return _mergeFields.GetValue( key, defaultValue );
        }

        public LavaDataDictionary GetMergeFields()
        {
            return _mergeFields;
        }

        public ILavaService GetService( Type serviceType )
        {
            throw new NotImplementedException();
        }

        public void SetEnabledCommands( IEnumerable<string> commands )
        {
            _enabledCommands.Clear();
            _enabledCommands.AddRange( commands );
        }

        public void SetEnabledCommands( string commandList, string delimiter = "," )
        {
            _enabledCommands.Clear();
            //_enabledCommands.AddRange( commandList.SplitDelimited(delimiter) );

            throw new NotImplementedException();
        }

        public void SetInternalField( string key, object value )
        {
            _internalFields.SetValue( key, value );
        }

        public void SetInternalFields( IDictionary<string, object> fieldValues )
        {
            if ( fieldValues == null )
            {
                return;
            }

            foreach ( var key in fieldValues.Keys )
            {
                _mergeFields.SetValue( key, fieldValues[key] );
            }
        }

        public void SetInternalFields( ILavaDataDictionary fieldValues )
        {
            if ( fieldValues == null )
            {
                return;
            }

            foreach ( var key in fieldValues.AvailableKeys )
            {
                _mergeFields.SetValue( key, fieldValues.GetValue( key ) );
            }
        }

        public void SetInternalFields( LavaDataDictionary fieldValues )
        {
            if ( fieldValues == null )
            {
                return;
            }

            foreach ( var key in fieldValues.AvailableKeys )
            {
                _mergeFields.SetValue( key, fieldValues.GetValue( key ) );
            }
        }

        public void SetMergeField( string key, object value, LavaContextRelativeScopeSpecifier scope = LavaContextRelativeScopeSpecifier.Current )
        {
            _mergeFields.SetValue( key, value );
        }

        public void SetMergeFields( ILavaDataDictionary fieldValues )
        {
            if ( fieldValues == null )
            {
                return;
            }

            foreach ( var key in fieldValues.AvailableKeys )
            {
                _mergeFields.SetValue( key, fieldValues.GetValue( key ) );
            }
        }

        public void SetMergeFields( IDictionary<string, object> fieldValues )
        {
            if ( fieldValues == null )
            {
                return;
            }

            foreach ( var key in fieldValues.Keys )
            {
                _mergeFields.SetValue( key, fieldValues[key] );
            }
        }

        public void SetMergeFields( LavaDataDictionary fieldValues )
        {
            if ( fieldValues == null )
            {
                return;
            }

            foreach ( var key in fieldValues.AvailableKeys )
            {
                _mergeFields.SetValue( key, fieldValues.GetValue( key ) );
            }
        }

        TService ILavaRenderContext.GetService<TService>()
        {
            throw new NotSupportedException( _notSupportedMessage );
        }

        private const string _notSupportedMessage = "The current context does not support this feature. It can only be used as a data container to initiate processing.";
    }
}
