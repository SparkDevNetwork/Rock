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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Workflow;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Used to enter information for a workflow form entry action.
    /// </summary>
    [DisplayName( "Workflow Entry" )]
    [Category( "Core" )]
    [Description( "Used to enter information for a workflow form entry action." )]
    public partial class WorkflowEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

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
                // added for your convenience
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        void lbAction_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        private void BuildForm(WorkflowActionForm form, Workflow workflow, bool setValue)
        {
            lheadingText.Text = form.Header;

            phAttributes.Controls.Clear();
            foreach(var formAttribute in form.FormAttributes.OrderBy( a => a.Order))
            {
                if (formAttribute.IsVisible)
                {
                    var attribute = AttributeCache.Read( formAttribute.AttributeId );

                    string value = attribute.DefaultValue;
                    if (workflow.AttributeValues.ContainsKey(attribute.Key) && workflow.AttributeValues[attribute.Key].Any())
                    {
                        value = workflow.AttributeValues[attribute.Key][0].Value;
                    }

                    if (formAttribute.IsReadOnly)
                    {
                        RockLiteral lAttribute = new RockLiteral();
                        lAttribute.ID = "lAttribute_" + formAttribute.Id.ToString();
                        lAttribute.Label = formAttribute.Attribute.Name;
                        lAttribute.Text = attribute.FieldType.Field.FormatValue( phAttributes, value, attribute.QualifierValues, false );
                    }
                    else
                    {
                        attribute.AddControl(phAttributes.Controls, value, BlockValidationGroup, setValue, true);
                    }
                }
            }

            lFootingText.Text = form.Footer;

            phActions.Controls.Clear();
            foreach( var action in form.Actions.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var details = action.Split(new char[] {'^'}, StringSplitOptions.RemoveEmptyEntries);
                if (details.Length == 2)
                {
                    LinkButton lb = new LinkButton();
                    lb.Text = details[0];
                    lb.Click += lbAction_Click;
                    lb.CssClass = "btn btn-primar";
                    lb.Attributes.Add("data-action", details[1]);
                    phActions.Controls.Add(lb);
                }
            }
        }

        #endregion
    }

}