// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.IO;
using System.Runtime.Caching;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Cached Check-in Label
    /// </summary>
    public class KioskLabel
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="KioskLabel"/> class from being created.
        /// </summary>
        private KioskLabel()
        {
        }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the content of the file.
        /// </summary>
        /// <value>
        /// The content of the file.
        /// </value>
        public string FileContent { get; set; }

        /// <summary>
        /// Gets or sets the merge fields.
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        public Dictionary<string, string> MergeFields { get; set; }

        #region Static Methods

        /// <summary>
        /// Caches the key.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        private static string CacheKey( Guid guid )
        {
            return string.Format( "Rock:CheckIn:KioskLabel:{0}", guid );
        }

        /// <summary>
        /// Reads the specified label by guid.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public static KioskLabel Read( Guid guid )
        {
            string cacheKey = KioskLabel.CacheKey( guid );

            ObjectCache cache = RockMemoryCache.Default;
            KioskLabel label = cache[cacheKey] as KioskLabel;

            if ( label != null )
            {
                return label;
            }
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    var file = new BinaryFileService( rockContext ).Get( guid );
                    if ( file != null )
                    {
                        label = new KioskLabel();

                        label.Guid = file.Guid;
                        label.Url = string.Format( "{0}GetFile.ashx?id={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), file.Id );
                        label.MergeFields = new Dictionary<string, string>();
                        label.FileContent = file.ContentsToString();

                        file.LoadAttributes( rockContext );
                        string attributeValue = file.GetAttributeValue( "MergeCodes" );
                        if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                        {
                            string[] nameValues = attributeValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                            foreach ( string nameValue in nameValues )
                            {
                                string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                                if ( nameAndValue.Length == 2 && !label.MergeFields.ContainsKey( nameAndValue[0] ) )
                                {
                                    label.MergeFields.Add( nameAndValue[0], nameAndValue[1] );

                                    int definedValueId = int.MinValue;
                                    if ( int.TryParse( nameAndValue[1], out definedValueId ) )
                                    {
                                        var definedValue = DefinedValueCache.Read( definedValueId );
                                        if ( definedValue != null )
                                        {
                                            string mergeField = definedValue.GetAttributeValue( "MergeField" );
                                            if ( mergeField != null )
                                            {
                                                label.MergeFields[nameAndValue[0]] = mergeField;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var cachePolicy = new CacheItemPolicy();
                        cachePolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( 60 );
                        cache.Set( cacheKey, label, cachePolicy );

                        return label;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Flushes the specified guid.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public static void Flush( Guid guid)
        {
            ObjectCache cache = RockMemoryCache.Default;
            cache.Remove( KioskLabel.CacheKey( guid ) );
        }

        #endregion
    }
}