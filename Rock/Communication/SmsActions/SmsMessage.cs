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
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Communication.SmsActions
{
    /// <summary>
    /// 
    /// </summary>
    public class SmsMessage : ILavaDataDictionary, Lava.ILiquidizable
    {
        private string _message;

        /// <summary>
        /// Gets or sets the number the message was sent to.
        /// </summary>
        /// <value>
        /// The number the message was sent to.
        /// </value>
        public string ToNumber { get; set; }

        /// <summary>
        /// Gets or sets the number that the message was sent from.
        /// </summary>
        /// <value>
        /// The number that the message was sent from.
        /// </value>
        public string FromNumber { get; set; }

        /// <summary>
        /// Gets or sets the person identified as the sender of the message.
        /// </summary>
        /// <value>
        /// The person identified as the sender of the message.
        /// </value>
        public Person FromPerson { get; set; }

        /// <summary>
        /// Gets or sets the message content.
        /// </summary>
        /// <value>
        /// The message content.
        /// </value>
        public string Message
        {
            get { return _message; }
            set { _message = value?.Trim() ?? string.Empty; }
        }

        /// <summary>
        /// Gets or sets the attachments.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        public List<BinaryFile> Attachments { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmsMessage"/> class.
        /// </summary>
        public SmsMessage()
        {
            Attachments = new List<BinaryFile>();
        }

        #region ILavaDataDictionary implementation

        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaHidden]
        public virtual List<string> AvailableKeys
        {
            get
            {
                var availableKeys = new List<string>();

                foreach ( var propInfo in GetType().GetProperties() )
                {
                    availableKeys.Add( propInfo.Name );
                }

                return availableKeys;
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
        public object GetValue( string key )
        {
            return this[key];
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
        public virtual object this[object key]
        {
            get
            {
                string propertyKey = key.ToStringSafe();
                var propInfo = GetType().GetProperty( propertyKey );

                try
                {
                    object propValue = null;
                    if ( propInfo != null )
                    {
                        propValue = propInfo.GetValue( this, null );
                    }

                    if ( propValue is Guid )
                    {
                        return ( ( Guid ) propValue ).ToString();
                    }
                    else
                    {
                        return propValue;
                    }
                }
                catch
                {
                    // intentionally ignore
                }

                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( string key )
        {
            string propertyKey = key.ToStringSafe();
            var propInfo = GetType().GetProperty( propertyKey );

            return propInfo != null;
        }

        #endregion

        #region ILiquidizable

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual bool ContainsKey( object key )
        {
            string propertyKey = key.ToStringSafe();
            var propInfo = GetType().GetProperty( propertyKey );

            return propInfo != null;
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object GetValue( object key )
        {
            return this[key];
        }

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public object ToLiquid()
        {
            return this;
        }

        #endregion

    }
}
