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
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using Rock.Data;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A web control that allows the user to select a single Entity from a list of available Entities of a specified type.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class SingleEntityPicker<TEntity> : RockDropDownList
        where TEntity : IEntity
    {
        private SingleEntityPickerKeySpecifier _Key = SingleEntityPickerKeySpecifier.Id;

        /// <summary>
        /// The type of key value used to uniquely identify the Entities in the selection list.
        /// </summary>
        public SingleEntityPickerKeySpecifier KeyType
        {
            get
            {
                return _Key;
            }
        }

        /// <summary>
        /// Initializes the list of available items.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="itemDescriptionExpression"></param>
        /// <param name="keyType"></param>
        /// <param name="allowEmptySelection"></param>
        public void InitializeListItems( IEnumerable<TEntity> entities, Expression<Func<TEntity,string>> itemDescriptionExpression, SingleEntityPickerKeySpecifier keyType = SingleEntityPickerKeySpecifier.Id, bool allowEmptySelection = false )
        {
            var mexpr = itemDescriptionExpression.Body as MemberExpression;

            if ( mexpr == null )
            {
                return;
            }

            var func = itemDescriptionExpression.Compile();

            this.Items.Clear();

            _Key = keyType;

            if ( allowEmptySelection )
            {
                this.Items.Add( new ListItem() );
            }

            foreach ( var entity in entities )
            {
                var description = func(entity);

                string key;

                if ( _Key == SingleEntityPickerKeySpecifier.Guid )
                {
                    key = entity.Guid.ToString();
                }
                else
                {
                    key = entity.Id.ToString();
                }

                this.Items.Add( new ListItem( description, key ) );
            }
        }

        /// <summary>
        /// Gets or sets the selected StepProgram Guid (only works when UseGuidAsValue = true) 
        /// </summary>
        /// <value>
        /// The selected StepProgram guids.
        /// </value>
        public Guid? SelectedGuid
        {
            get
            {
                VerifyRequiredKeyTypeOrThrow( SingleEntityPickerKeySpecifier.Guid );

                return this.SelectedValue.AsGuidOrNull();
            }

            set
            {
                VerifyRequiredKeyTypeOrThrow( SingleEntityPickerKeySpecifier.Guid );

                string itemValue = value.HasValue ? value.ToString() : string.Empty;
                var li = this.Items.FindByValue( itemValue );
                if ( li != null )
                {
                    li.Selected = true;
                    this.SelectedValue = itemValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected StepProgram ids  (only works when UseGuidAsValue = false)
        /// </summary>
        /// <value>
        /// The selected StepProgram ids.
        /// </value>
        public int? SelectedId
        {
            get
            {
                VerifyRequiredKeyTypeOrThrow( SingleEntityPickerKeySpecifier.Id );

                return this.SelectedValueAsId();
            }

            set
            {
                VerifyRequiredKeyTypeOrThrow( SingleEntityPickerKeySpecifier.Id );

                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                    this.SelectedValue = id.ToString();
                }
                else
                {
                    // if setting Id to NULL or 0, just default to the first item in the list (which should be nothing)
                    if ( this.Items.Count > 0 )
                    {
                        this.SelectedIndex = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Verify that the current Key Type is set to a specific value, or throw an Exception.
        /// </summary>
        /// <param name="keyType"></param>
        private void VerifyRequiredKeyTypeOrThrow( SingleEntityPickerKeySpecifier keyType )
        {
            if ( keyType == SingleEntityPickerKeySpecifier.Guid
                 && _Key != SingleEntityPickerKeySpecifier.Guid)
            {
                throw new Exception( "Invalid Operation. This operation is only available when KeyType is set to \"Guid\"." );
            }
            else if ( keyType == SingleEntityPickerKeySpecifier.Id
                 && _Key != SingleEntityPickerKeySpecifier.Id )
            {
                throw new Exception( "Invalid Operation. This operation is only available when KeyType is set to \"Id\"." );
            }
        }

        /// <summary>
        /// Gets the text of the Label associated with this control.
        /// Override this method to set a custom label for the control.
        /// </summary>
        /// <returns></returns>
        protected virtual string OnGetLabelText()
        {
            return typeof( TEntity ).Name;
        }
    }

    #region Support Classes and Enums

    /// <summary>
    /// Specifies the type of key value used to uniquely identify an Entity in a picker control.
    /// </summary>
    public enum SingleEntityPickerKeySpecifier
    {
        /// <summary>
        /// Key Specifier Id
        /// </summary>
        Id = 0,

        /// <summary>
        /// Key Specifier Guid
        /// </summary>
        Guid = 1
    }

    #endregion
}