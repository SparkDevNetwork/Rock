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
using System.Collections.Generic;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls.Communication
{
    /// <summary>
    /// abstract class for controls used to render a communication medium
    /// </summary>
    public abstract class MediumControl : CompositeControl
    {
        /// <summary>
        /// Gets or sets a value indicating whether the communication is a template.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is template]; otherwise, <c>false</c>.
        /// </value>
        public bool IsTemplate
        {
            get { return ViewState["IsTemplate"] as bool? ?? false; }
            set { ViewState["IsTemplate"] = value; }
        }
    
        /// <summary>
        /// Gets or sets the medium data.
        /// </summary>
        /// <value>
        /// The medium data.
        /// </value>
        public abstract Dictionary<string, string> MediumData { get; set; }

        /// <summary>
        /// Gets or sets any additional merge fields.
        /// </summary>
        public List<string> AdditionalMergeFields
        {
            get
            {
                var mergeFields = ViewState["AdditionalMergeFields"] as List<string>;
                if ( mergeFields == null )
                {
                    mergeFields = new List<string>();
                    ViewState["AdditionalMergeFields"] = mergeFields;
                }
                return mergeFields;
            }

            set { ViewState["AdditionalMergeFields"] = value; }
        }

        /// <summary>
        /// On new communicaiton, initializes controls from sender values
        /// </summary>
        /// <param name="sender">The sender.</param>
        public abstract void InitializeFromSender( Person sender );

        /// <summary>
        /// Gets the data value.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetDataValue( Dictionary<string, string> data, string key )
        {
            if ( data != null && data.ContainsKey( key ) )
            {
                return data[key];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public virtual string ValidationGroup 
        { 
            get { return string.Empty; }
            set {}
        }

        /// <summary>
        /// Called when [communication save].
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        public virtual void OnCommunicationSave( Rock.Data.RockContext rockContext )
        {
        }

    }
}