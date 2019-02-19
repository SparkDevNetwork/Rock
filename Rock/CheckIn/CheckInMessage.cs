﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Rock.CheckIn
{
    [DataContract]
    public class CheckInMessage : Lava.ILiquidizable
    {
        public string MessageText { get; set; }

        public MessageType MessageType { get; set; }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [Rock.Data.LavaIgnore]
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
        [Rock.Data.LavaIgnore]
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
        public bool ContainsKey( object key )
        {
            var additionalKeys = new List<string> { "MessageText", "MessageType" };
            if (additionalKeys.Contains( key.ToStringSafe() ))
            {
                return true;
            }
            return false;
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
    }

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