﻿// <copyright>
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
using System.Runtime.Serialization;
using Rock.Lava;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class CheckInMessage : ILavaDataDictionary
    {
        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        /// <value>
        /// The message text.
        /// </value>
        public string MessageText { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object GetValue( string key )
        {
            switch ( key )
            {
                case "MessageText": return MessageText;
                case "MessageType": return this.MessageType.ToStringSafe().ToLower();
                default: return MessageText;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [LavaHidden]
        [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
        [Rock.RockObsolete( "18.0" )]
        public object this[object key]
        {
            get
            {
                switch (key.ToStringSafe())
                {
                    case "MessageText": return MessageText;
                    case "MessageType": return this.MessageType.ToStringSafe().ToLower();
                    default: return MessageText;
                }
            }
        }

        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaHidden]
        public List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string> { "MessageText", "MessageType"};
                return availableKeys;
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( string key )
        {
            var additionalKeys = new List<string> { "MessageText", "MessageType" };
            if (additionalKeys.Contains( key.ToStringSafe() ))
            {
                return true;
            }
            return false;
        }

        #region ILiquidizable

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
        [Rock.RockObsolete( "18.0" )]
        public bool ContainsKey( object key )
        {
            var additionalKeys = new List<string> { "MessageText", "MessageType" };
            if ( additionalKeys.Contains( key.ToStringSafe() ) )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
        [Rock.RockObsolete( "18.0" )]
        public object GetValue( object key )
        {
            return this[key];
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
        [Rock.RockObsolete( "18.0" )]
        public object ToLiquid()
        {
            return this;
        }

        #endregion
    }

    /// <summary>
    ///Message Enumerations
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// The success
        /// </summary>
        Success,

        /// <summary>
        /// The information
        /// </summary>
        Info,

        /// <summary>
        /// The warning
        /// </summary>
        Warning,

        /// <summary>
        /// The danger
        /// </summary>
        Danger
    }
}