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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_CentralAZ.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Auto Assign Number" )]
    [Category( "CentralAZ.com > Utility" )]
    [Description( "Context aware block that assigns the next sequential number (on an integer Attribute) if none is already set." )]

    [ContextAware( typeof( Person ) )]

    // List of all possible/documented Block Atributes:
    // https://github.com/SparkDevNetwork/Rock/wiki/Block-Attributes

    [TextField( "Attribute Name", "Name of the person attribute where the number is stored.", true, "Envelope Number", "", 0 )]
    [TextField( "Icon Class", "The icon class string (e.g., fa fa-plus-circle) to use on the assign button.", true, "fa fa-plus-circle", "", 1 )]

    public partial class AutoAssignNumber : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        Person _person = null;

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods
        
        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                _person = (Person)contextEntity;
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( _person != null && ! IsAttributeAssigned() )
                {
                    upnlContent.Visible = true;
                    lbAutoAssign.Text = string.Format( "<i class='{0}'></i> Assign {1}", GetAttributeValue( "IconClass" ), GetAttributeValue( "AttributeName" ) );
                }
                else
                {
                    upnlContent.Visible = false;
                }
            }
        }

        #endregion

        #region Events

        protected void lbAutoAssign_Click( object sender, EventArgs e )
        {
            int? newValue = AssignNewNumber();
            if ( newValue != null )
            {
                nbMessage.Text = string.Format( "Assigned {0} {1}", GetAttributeValue( "AttributeName" ), newValue.ToStringSafe() );
                lbAutoAssign.Enabled = false;
                lbAutoAssign.Visible = false;
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated(object sender, EventArgs e)
        {
            
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to check if a person already has an assigned value.
        /// </summary>
        /// <returns>true if they already have an assigned value; false otherwise</returns>
        private bool IsAttributeAssigned()
        {
            string attributeName = GetAttributeValue( "AttributeName" );
            string number = _person.GetAttributeValue( attributeName.Replace(" ", "" ) );

            return string.IsNullOrEmpty( number ) ? false : true;
        }

        /// <summary>
        /// Assigns the next sequential unused number for the configured AttributeName.
        /// </summary>
        /// <returns>the number assigned or null</returns>
        private int? AssignNewNumber()
        {
            string attributeKey = GetAttributeValue( "AttributeName" ).Replace( " ", "" );
            var thePersonAttribute = _person.Attributes.Values.Where( a => a.Key == attributeKey ).FirstOrDefault();

            if ( thePersonAttribute != null )
            {
                // Get the highest value stored in this attribute.
                int maxValue = 0;
                RockContext rockContext = new RockContext();
                var maxValueString = new AttributeValueService( rockContext ).Queryable().Where( a => a.AttributeId == thePersonAttribute.Id ).OrderByDescending( av => av.Value ).FirstOrDefault();
                if ( maxValueString != null && ! string.IsNullOrEmpty( maxValueString.Value ) )
                {
                    maxValue = maxValueString.Value.AsInteger();
                }

                // Now load the person's attributes, increment the number, then save it to their record.
                _person.LoadAttributes();
                int nextNumber = maxValue + 1;
                _person.SetAttributeValue( attributeKey, nextNumber.ToStringSafe() );
                _person.SaveAttributeValues();
                return nextNumber;
            }
            else
            {
                return null;
            }

        }
        #endregion
}
}