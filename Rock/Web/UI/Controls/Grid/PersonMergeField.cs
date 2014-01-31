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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// The ASP:CheckBoxField doesn't work very well for retrieving changed values, especially when the value is changed from True to False (weird)
    /// This CheckBoxEditableField works like the ASP:CheckBoxField except it gives the CheckBox's IDs so their changed values will consistantly persist on postbacks
    /// </summary>
    public class PersonMergeField : SelectField, INotRowSelectedField
    {

        #region Properties

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonId
        {
            get { return ViewState["PersonId"] as int? ?? 0; }
            set { ViewState["PersonId"] = value; }
        }

        public string PersonName
        {
            get { return ViewState["PersonName"] as string; }
            set { ViewState["PersonName"] = value; }
        }

        public DateTime? DateCreated
        {
            get { return ViewState["DateCreated"] as DateTime?; }
            set { ViewState["DateCreated"] = value; }
        }

        public string CreatedBy
        {
            get { return ViewState["CreatedBy"] as string; }
            set { ViewState["CreatedBy"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is primary person].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is primary person]; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimaryPerson
        {
            get { return ViewState["IsPrimaryPerson"] as bool? ?? false; }
            set { ViewState["IsPrimaryPerson"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Performs basic instance initialization for a data control field.
        /// </summary>
        /// <param name="sortingEnabled">A value that indicates whether the control supports the sorting of columns of data.</param>
        /// <param name="control">The data control that owns the <see cref="T:System.Web.UI.WebControls.DataControlField" />.</param>
        /// <returns>
        /// Always returns false.
        /// </returns>
        public override bool Initialize( bool sortingEnabled, Control control )
        {
            this.HeaderTemplate = new PersonMergeFieldHeaderTemplate();
            return base.Initialize( sortingEnabled, control );
        }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class PersonMergeFieldHeaderTemplate : ITemplate
    {
        /// <summary>
        /// Gets the header text.
        /// </summary>
        /// <value>
        /// The header text.
        /// </value>
        public string HeaderText { get; private set; }

        /// <summary>
        /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
        /// </summary>
        /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
        public void InstantiateIn( Control container )
        {
            var cell = container as DataControlFieldCell;
            if ( cell != null )
            {
                var mergeField = cell.ContainingField as PersonMergeField;
                if ( mergeField != null )
                {
                    HeaderText = mergeField.HeaderText;

                    var rb = new RadioButton();
                    rb.Text = mergeField.PersonName;
                    rb.ID = "rbSelectPrimary_" + mergeField.ColumnIndex.ToString();
                    rb.Checked = mergeField.IsPrimaryPerson;
                    rb.GroupName = "PrimaryPerson";
                    cell.Controls.Add( rb );

                    string created = (mergeField.DateCreated.HasValue ? mergeField.DateCreated.ToElapsedString() + " " : "") +
                        (!string.IsNullOrWhiteSpace(mergeField.CreatedBy) ? "by " + mergeField.CreatedBy : "");

                    if ( created != string.Empty )
                    {
                        cell.Controls.Add( new LiteralControl( string.Format( "<br/><span>Created {0}</span>", created ) ) );
                    }
                }
            }
        }

    }
}
