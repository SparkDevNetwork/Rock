// <copyright>
// Copyright by Central Christian Church
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
using System.ComponentModel;
using System.Data.Entity;
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
    [Category( "com_centralaz > Utility" )]
    [Description( "Context aware block that assigns the next sequential number (on an integer Attribute) if none is already set." )]

    [ContextAware( typeof( Person ) )]

    // List of all possible/documented Block Atributes:
    // https://github.com/SparkDevNetwork/Rock/wiki/Block-Attributes

    [TextField( "Attribute Name", "Name of the person attribute where the number is stored.", true, "Envelope Number", "", 0 )]
    [TextField( "Attribute Key", "The Key of the attribute.", true, "EnvelopeNumber", "", 1 )]
    [TextField( "Icon Class", "The icon class string (e.g., fa fa-plus-circle) to use on the assign button.", true, "fa fa-plus-circle", "", 2 )]
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
            string number = _person.GetAttributeValue( GetAttributeValue( "AttributeKey" ) );

            return string.IsNullOrEmpty( number ) ? false : true;
        }

        /// <summary>
        /// Assigns the next sequential unused number for the configured AttributeName.
        /// </summary>
        /// <returns>the number assigned or null</returns>
        private int? AssignNewNumber()
        {
            string attributeKey = GetAttributeValue( "AttributeKey" );
            var thePersonAttribute = _person.Attributes.Values.Where( a => a.Key == attributeKey ).FirstOrDefault();

            if ( thePersonAttribute != null )
            {
                // Get the highest value stored in this attribute.
                int maxValue = 0;
                RockContext rockContext = new RockContext();
                // We're doing those odd order by's below because the Value is a string datatype.
                // That means in descending order, 999 would come before 3200 (for example).
                // ordering by the length first and then the value gets around this problem
                // in a way that's legal for LINQ to Entities.
                var maxValueString = new AttributeValueService( rockContext ).Queryable().AsNoTracking()
                    .Where( a => a.AttributeId == thePersonAttribute.Id )
                    .Where( av => av.Value != null && ! av.Value.Equals( string.Empty ) )
                    .OrderByDescending( av => av.Value.Length )
                    .ThenByDescending( av => av.Value ).FirstOrDefault();

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