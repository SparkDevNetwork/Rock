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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// A schedule options for the current check-in
    /// </summary>
    [DataContract]
    public class CheckInSchedule : ILavaDataDictionary
    {
        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        [DataMember]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the last time person checked into this schedule for the selected group type, location and group 
        /// </summary>
        /// <value>
        /// The last check-in.
        /// </value>
        [DataMember]
        public DateTime? LastCheckIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [excluded by filter].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [excluded by filter]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ExcludedByFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInPerson" /> was pre-selected by a check-in action.
        /// </summary>
        /// <value>
        ///   <c>true</c> if preselected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PreSelected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInSchedule" /> is selected for check-in
        /// </summary>
        /// <value>
        ///   <c>true</c> if selected; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Selected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CheckInSchedule"/> is processed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool Processed { get; set; }

        /// <summary>
        /// Gets the campus current date time.
        /// </summary>
        /// <value>
        /// The campus current date time.
        /// </value>
        public DateTime CampusCurrentDateTime
        {
            get
            {
                if ( CampusId.HasValue )
                {
                    var campus = CampusCache.Get( CampusId.Value );
                    if ( campus != null )
                    {
                        return campus.CurrentDateTime;
                    }
                }
                return RockDateTime.Now;
            }
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Schedule != null ? Schedule.Name : string.Empty;
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
                var availableKeys = new List<string> { "StartTime", "LastCheckIn" };
                if ( this.Schedule != null )
                {
                    availableKeys.AddRange( this.Schedule.AvailableKeys );
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
            switch ( key )
            {
                case "StartTime": return StartTime;
                case "LastCheckIn": return LastCheckIn;
                default: return Schedule[key];
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
                switch ( key.ToStringSafe() )
                {
                    case "StartTime": return StartTime;
                    case "LastCheckIn": return LastCheckIn;
                    default: return Schedule[key];
                }
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( string key )
        {
            var additionalProperties = new List<string> { "StartTime", "LastCheckIn" };
            if ( additionalProperties.Contains( key.ToStringSafe() ) )
            {
                return true;
            }
            else
            {
                return Schedule.ContainsKey( key );
            }
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
            var additionalProperties = new List<string> { "StartTime", "LastCheckIn" };
            var propertyKey = key.ToStringSafe();
            if ( additionalProperties.Contains( propertyKey ) )
            {
                return true;
            }
            else
            {
                return Schedule.ContainsKey( propertyKey );
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
}