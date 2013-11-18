//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class MergeFieldPicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
 	         base.OnInit(e);
             base.DefaultText = "Add Merge Field";
        }

        /// <summary>
        /// Gets or sets the merge fields.
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        public List<string> MergeFields
        {
            get
            {
                var mergeFields = ViewState["MergeFields"] as List<string>;
                if ( mergeFields == null )
                {
                    mergeFields = new List<string>();
                    ViewState["MergeFields"] = mergeFields;
                }
                return mergeFields;
            }

            set 
            {
                ViewState["MergeFields"] = value;
                ItemRestUrlExtraParams = "/" + value.AsDelimited(",");
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="nodePath">The node path.</param>
        public void SetValue( string nodePath )
        {
            if ( ! string.IsNullOrWhiteSpace(nodePath))
            {
                var nodes = nodePath.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                
                if ( nodes.Count > 0 )
                {
                    ItemId = nodePath;
                    ItemName = nodes[nodes.Count - 1];
                    
                    if ( nodes.Count > 1 )
                    {
                        InitialItemParentIds = nodes.Take( nodes.Count - 1 ).ToList().AsDelimited( "," );
                    }
                }
            }
            else
            {
                ItemId = "0";
                ItemName = "Add Merge Field";
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="nodePaths">The node paths.</param>
        public void SetValues( IEnumerable<string> nodePaths )
        {
            var nodePathsList = nodePaths.ToList();

            if ( nodePathsList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                InitialItemParentIds = string.Empty;

                foreach ( string nodePath in nodePathsList )
                {
                    var nodes = nodePath.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                    if ( nodes.Count > 0 )
                    {
                        ItemId = nodePath;
                        ItemName = nodes[nodes.Count - 1];

                        if ( InitialItemParentIds == string.Empty && nodes.Count > 1 )
                        {
                            InitialItemParentIds = nodes.Take( nodes.Count - 1 ).ToList().AsDelimited( "," );
                        }
                    }
                }

                ItemIds = ids;
                ItemNames = names;

            }
            else
            {
                ItemId = "0";
                ItemName = "Add Merge Field";
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            this.SetValue( ItemId );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void SetValuesOnSelect()
        {
            this.SetValues( ItemIds );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/MergeFields/GetChildren/"; }
        }

    }
}