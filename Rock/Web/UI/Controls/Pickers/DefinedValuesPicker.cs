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
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class DefinedValuesPicker : RockCheckBoxList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValuesPicker"/> class.
        /// </summary>
        public DefinedValuesPicker()
            : base()
        {
            this.RepeatDirection = System.Web.UI.WebControls.RepeatDirection.Horizontal;
            this.AddCssClass( "checkboxlist-group" );
        }

        /// <summary>
        /// Gets or sets the defined type identifier ( Required )
        /// </summary>
        /// <value>
        /// The defined type identifier.
        /// </value>
        public int? DefinedTypeId
        {
            get
            {
                return _definedTypeId;
            }

            set
            {
                _definedTypeId = value;
                LoadCheckBoxItems();
            }
        }

        /// <summary>
        /// The _defined type identifier
        /// </summary>
        private int? _definedTypeId;

        /// <summary>
        /// Defined value descriptions will be displayed instead of the values (defaults to false)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [display descriptions]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayDescriptions { get; set; }

        /// <summary>
        /// Loads the CheckBox items.
        /// </summary>
        private void LoadCheckBoxItems()
        {
            this.Items.Clear();

            if ( _definedTypeId.HasValue )
            {
                Rock.Model.DefinedValueService definedValueService = new Model.DefinedValueService( new RockContext() );
                var definedValues = definedValueService.GetByDefinedTypeId( _definedTypeId.Value );
                if ( definedValues.Any() )
                {
                    foreach ( var definedValue in definedValues )
                    {
                        this.Items.Add( new ListItem( this.DisplayDescriptions ? definedValue.Description : definedValue.Value, definedValue.Id.ToString() ) );
                    }
                }
            }
        }
    }
}