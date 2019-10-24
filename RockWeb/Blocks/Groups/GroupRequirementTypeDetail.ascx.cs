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
using Rock.Constants;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Group Requirement Type Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of the given group requirement type for editing." )]
    public partial class GroupRequirementTypeDetail : RockBlock, IDetailBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
                ShowDetail( PageParameter( "GroupRequirementTypeId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupRequirementTypeId">The group requirement type identifier.</param>
        public void ShowDetail( int groupRequirementTypeId )
        {
            RockContext rockContext = new RockContext();
            GroupRequirementType groupRequirementType = null;
            GroupRequirementTypeService groupRequirementTypeService = new GroupRequirementTypeService( rockContext );
            
            if ( !groupRequirementTypeId.Equals( 0 ) )
            {
                groupRequirementType = groupRequirementTypeService.Get( groupRequirementTypeId );
                lActionTitle.Text = ActionTitle.Edit( GroupRequirementType.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( groupRequirementType, ResolveRockUrl( "~" ) );
            }

            if ( groupRequirementType == null )
            {
                groupRequirementType = new GroupRequirementType { Id = 0 };
                groupRequirementType.RequirementCheckType = RequirementCheckType.Manual;
                lActionTitle.Text = ActionTitle.Add( GroupRequirementType.FriendlyTypeName ).FormatAsHtmlTitle();
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfGroupRequirementTypeId.Value = groupRequirementType.Id.ToString();
            tbName.Text = groupRequirementType.Name;
            tbDescription.Text = groupRequirementType.Description;
            tbPositiveLabel.Text = groupRequirementType.PositiveLabel;
            tbNegativeLabel.Text = groupRequirementType.NegativeLabel;
            tbWarningLabel.Text = groupRequirementType.WarningLabel;
            tbCheckboxLabel.Text = groupRequirementType.CheckboxLabel;
            cbCanExpire.Checked = groupRequirementType.CanExpire;
            nbExpireInDays.Text = groupRequirementType.ExpireInDays.ToString();

            nbSQLHelp.InnerHtml = @"A SQL expression that returns a list of Person Ids that meet the criteria. Example:
<pre>
SELECT [Id] FROM [Person]
WHERE [LastName] = 'Decker'</pre>
</pre>
The SQL can include Lava merge fields:

<ul>
   <li>Group</i>
   <li>GroupRequirementType</i>
</ul>

TIP: When calculating for a specific Person, a <strong>Person</strong> merge field will also be included. This can improve performance in cases when the system is checking requirements for a specific person. Example:

<pre>
    SELECT [Id] FROM [Person]
        WHERE [LastName] = 'Decker'
    {% if Person != empty %}
        AND [Id] = {{ Person.Id }}
    {% endif &}
</pre>
";

            nbSQLHelp.InnerHtml += groupRequirementType.GetMergeObjects( new Group(), this.CurrentPerson ).lavaDebugInfo();

            ceSqlExpression.Text = groupRequirementType.SqlExpression; 

            ceWarningSqlExpression.Text = groupRequirementType.WarningSqlExpression;

            dpDataView.EntityTypeId = EntityTypeCache.Get<Person>().Id;
            dpDataView.SetValue( groupRequirementType.DataViewId );

            dpWarningDataView.EntityTypeId = EntityTypeCache.Get<Person>().Id;
            dpWarningDataView.SetValue( groupRequirementType.WarningDataViewId );

            hfRequirementCheckType.Value = groupRequirementType.RequirementCheckType.ConvertToInt().ToString();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            GroupRequirementType groupRequirementType;

            var rockContext = new RockContext();
            GroupRequirementTypeService groupRequirementTypeService = new GroupRequirementTypeService( rockContext );

            int groupRequirementTypeId = hfGroupRequirementTypeId.Value.AsInteger();

            if (groupRequirementTypeId == 0)
            {
                groupRequirementType = new GroupRequirementType();
            }
            else
            {
                groupRequirementType = groupRequirementTypeService.Get( groupRequirementTypeId );
            }

            groupRequirementType.Name = tbName.Text;
            groupRequirementType.Description = tbDescription.Text;
            groupRequirementType.CanExpire = cbCanExpire.Checked;
            groupRequirementType.ExpireInDays = groupRequirementType.CanExpire ? nbExpireInDays.Text.AsIntegerOrNull() : null;
            groupRequirementType.RequirementCheckType = hfRequirementCheckType.Value.ConvertToEnum<RequirementCheckType>( RequirementCheckType.Manual );

            if ( groupRequirementType.RequirementCheckType == RequirementCheckType.Sql)
            {
                groupRequirementType.SqlExpression = ceSqlExpression.Text;
                groupRequirementType.WarningSqlExpression = ceWarningSqlExpression.Text;
            }
            else
            {
                groupRequirementType.SqlExpression = null;
                groupRequirementType.WarningSqlExpression = null;
            }

            if (groupRequirementType.RequirementCheckType == RequirementCheckType.Dataview)
            {
                groupRequirementType.DataViewId = dpDataView.SelectedValue.AsIntegerOrNull() == 0 ? null : dpDataView.SelectedValue.AsIntegerOrNull();
                groupRequirementType.WarningDataViewId = dpWarningDataView.SelectedValue.AsIntegerOrNull() == 0 ? null : dpWarningDataView.SelectedValue.AsIntegerOrNull();
            }
            else
            {
                groupRequirementType.DataViewId = null;
                groupRequirementType.WarningDataViewId = null;
            }
            
            groupRequirementType.PositiveLabel = tbPositiveLabel.Text;
            groupRequirementType.NegativeLabel = tbNegativeLabel.Text;
            groupRequirementType.WarningLabel = tbWarningLabel.Text;
            groupRequirementType.CheckboxLabel = tbCheckboxLabel.Text;

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !groupRequirementType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            if ( groupRequirementType.Id == 0 )
            {
                groupRequirementTypeService.Add( groupRequirementType );
            }

            rockContext.SaveChanges();

            NavigateToParentPage();
        }

        #endregion
    }
}