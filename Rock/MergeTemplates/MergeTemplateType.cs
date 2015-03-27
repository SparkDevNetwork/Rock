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
using Rock.Attribute;
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
        /// <param name="mergeObjectsList">The merge objects list.</param>
        /// <returns></returns>
        public abstract BinaryFile CreateDocument( MergeTemplate mergeTemplate, List<Dictionary<string, object>> mergeObjectsList );

        /// <summary>
        /// The RegEx for finding the "next" delimiter/indicator
        /// </summary>
        internal Regex nextRecordRegEx = new Regex( @"{%\s*\bnext\b\s*%}", RegexOptions.IgnoreCase );

        /// <summary>
        /// The RegEx of "." that matches anything
        /// </summary>
        internal Regex regExDot = new Regex( "." );
    }
}
