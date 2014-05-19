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
using System.Linq;
using System.Web.UI;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "CkEditor MergeField" )]
    [Category( "Utility" )]
    [Description( "Block to be used as part of the RockMergeField CKEditor Plugin" )]
    public partial class CkEditorMergeFieldPicker : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string mergeFields = PageParameter( "mergeFields" );

            if ( !string.IsNullOrEmpty( mergeFields ) )
            {
                mfpCkEditor.MergeFields = mergeFields.Split( ',' ).ToList();
            }
        }

        #endregion

        #region Events
        
        /// <summary>
        /// Handles the SelectItem event of the mfpCkEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mfpCkEditor_SelectItem( object sender, EventArgs e )
        {
            // set the result of the dialog so that the CkEditor plugin can grab it
            hfResultValue.Value = mfpCkEditor.SelectedValue;
        }

        #endregion
    }
}