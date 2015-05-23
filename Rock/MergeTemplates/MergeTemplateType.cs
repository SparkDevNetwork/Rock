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
using System.Text.RegularExpressions;
using Rock.Extension;
using Rock.Model;

namespace Rock.MergeTemplates
{
    /// <summary>
    /// Base class for merge template types (i.e. Word Document, HTML, etc) 
    /// </summary>
    public abstract class MergeTemplateType : Component
    {
        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
        public abstract List<Exception> Exceptions { get; set; }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <returns></returns>
        public abstract BinaryFile CreateDocument( MergeTemplate mergeTemplate, List<object> mergeObjectList, Dictionary<string, object> globalMergeFields );

        /// <summary>
        /// Gets the lava debug information.
        /// </summary>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <returns></returns>
        public virtual string GetLavaDebugInfo( List<object> mergeObjectList, Dictionary<string, object> globalMergeFields )
        {
            return GetDefaultLavaDebugInfo( mergeObjectList, globalMergeFields );
        }

        /// <summary>
        /// Gets the supported file extensions 
        /// Returns NULL if the file extension doesn't matter or doesn't apply
        /// Rock will use this to warn the user if the file extension isn't supported
        /// </summary>
        /// <value>
        /// The supported file extensions.
        /// </value>
        public virtual IEnumerable<string> SupportedFileExtensions
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the default lava debug information.
        /// </summary>
        /// <param name="mergeObjectList">The merge object list.</param>
        /// <param name="globalMergeFields">The global merge fields.</param>
        /// <param name="preText">The pre text.</param>
        /// <returns></returns>
        public static string GetDefaultLavaDebugInfo( List<object> mergeObjectList, Dictionary<string, object> globalMergeFields, string preText = null )
        {
            DotLiquid.Hash debugMergeFields = new DotLiquid.Hash();

            if ( mergeObjectList.Count >= 1 )
            {
                debugMergeFields.Add( "Row", mergeObjectList[0] );
            }

            foreach ( var mergeField in globalMergeFields )
            {
                debugMergeFields.Add( mergeField.Key, mergeField.Value );
            }

            return debugMergeFields.lavaDebugInfo( null, preText );
        }

        /// <summary>
        /// The RegEx for finding the "next" delimiter/indicator
        /// </summary>
        protected Regex nextRecordRegEx = new Regex( @"{%\s*\bnext\b\s*%}", RegexOptions.IgnoreCase );

        /// <summary>
        /// The RegEx of "." that matches anything
        /// </summary>
        protected Regex regExDot = new Regex( "." );

        /// <summary>
        /// The RegEx to detect if the text has {{ }} tags in it
        /// </summary>
        protected Regex lavaRegEx = new Regex( @"\{\{.+?\}\}", RegexOptions.Multiline );
    }
}
