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
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

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

             this.CssClass += " picker-mergefield picker-novalue";
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
                //this.CssClass += " picker-mergefield picker-novalue";
            }
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
            }
        }

        /// <summary>
        /// Gets or sets the initial item parent ids.
        /// </summary>
        /// <value>
        /// The initial item parent ids.
        /// </value>
        public override string InitialItemParentIds
        {
            get
            {
                return base.InitialItemParentIds.Split( ',' ).ToList().Select( i => i.Quoted() ).ToList().AsDelimited( "," );
            }
            set
            {
                base.InitialItemParentIds = value;
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
                // Get any prefixes that were defined with the mergefield
                var prefixedMergeFieldIds = GetPrefixedMergeFieldIds();

                var nodes = new List<string>();
                string workingPath = nodePath;

                // Check to see if the nodepath starts with one of the prefixedMergeFields.  If so
                // the node path should not split its items on the first pipe character
                foreach ( string fieldId in prefixedMergeFieldIds )
                {
                    if ( nodePath.StartsWith( fieldId ) )
                    {
                        nodes.Add( fieldId );
                        workingPath = nodePath.Substring( fieldId.Length );
                        break;
                    }
                }

                workingPath.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList()
                    .ForEach( n => nodes.Add( n ) );
                
                if ( nodes.Count > 0 )
                {
                    ItemId = nodePath;
                    ItemName = nodes[nodes.Count -1];
                    
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

                // Get any prefixes that were defined with the mergefield
                var prefixedMergeFieldIds = GetPrefixedMergeFieldIds();

                foreach ( string nodePath in nodePathsList )
                {
                    var nodes = new List<string>();
                    string workingPath = nodePath;

                    // Check to see if the nodepath starts with one of the prefixedMergeFields.  If so
                    // the node path should not split its items on the first pipe character
                    foreach ( string fieldId in prefixedMergeFieldIds )
                    {
                        if ( nodePath.StartsWith( fieldId ) )
                        {
                            nodes.Add( fieldId );
                            workingPath = nodePath.Substring( fieldId.Length );
                            break;
                        }
                    }

                    workingPath.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList()
                        .ForEach( n => nodes.Add( n ) );

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
        /// Gets the prefixed merge field ids.
        /// </summary>
        /// <returns></returns>
        private List<string> GetPrefixedMergeFieldIds()
        {
            var prefixedMergeFieldIds = new List<string>();

            foreach ( string mergefield in MergeFields )
            {
                string[] parts = mergefield.Split( '|' );
                if ( parts.Length > 2 )
                {
                    prefixedMergeFieldIds.Add( parts[2] + "|" + parts[0] );
                }
            }

            return prefixedMergeFieldIds;
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

        /// <summary>
        /// The selected merge field.
        /// </summary>
        /// <returns></returns>
        public string SelectedMergeField
        {
            get
            {
                return MergeFieldPicker.FormatSelectedValue( this.SelectedValue );
            }
        }

        /// <summary>
        /// Formats the selected value (node path) into a liquid merge field.
        /// </summary>
        /// <param name="selectedValue">The selected value.</param>
        /// <returns></returns>
        public static string FormatSelectedValue(string selectedValue)
        {
            var idParts = selectedValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            if ( idParts.Count > 0 )
            {
                if ( idParts.Count == 2 && idParts[0] == "GlobalAttribute" )
                {
                    return string.Format( "{{{{ 'Global' | Attribute:'{0}' }}}}", idParts[1] );
                }

                if ( idParts.Count == 1 && idParts[0].StartsWith( "AdditionalMergeField" ) ) 
                {
                    string mFields = idParts[0].Replace( "AdditionalMergeField_", "" ).Replace( "AdditionalMergeFields_", "" );
                    if ( mFields.IsNotNullOrWhiteSpace() )
                    {
                        string beginFor = "{% for field in AdditionalFields %}";
                        string endFor = "{% endfor %}";
                        var mergeFields = String.Join( "", mFields.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries )
                            .Select( f => "{{ field." + f + "}}" ) );

                        return $"{beginFor}{mergeFields}{endFor}";
                    }
                }

                if ( idParts.Count == 1 )
                {
                    if ( idParts[0] == "Campuses" )
                    {
                        return @"
{% for campus in Campuses %}
<p>
    Name: {{ campus.Name }}<br/>
    Description: {{ campus.Description }}<br/>
    Is Active: {{ campus.IsActive }}<br/>
    Short Code: {{ campus.ShortCode }}<br/>
    Url: {{ campus.Url }}<br/>
    Phone Number: {{ campus.PhoneNumber }}<br/>
    Service Times:
    {% for serviceTime in campus.ServiceTimes %}
        {{ serviceTime.Day }} {{ serviceTime.Time }},
    {% endfor %}
    <br/>
{% endfor %}
";
                    }

                    if ( idParts[0] == "Date" )
                    {
                        return "{{ 'Now' | Date:'MM/dd/yyyy' }}";
                    }

                    if ( idParts[0] == "Time" )
                    {
                        return "{{ 'Now' | Date:'hh:mm:ss tt' }}";
                    }

                    if ( idParts[0] == "DayOfWeek" )
                    {
                        return "{{ 'Now' | Date:'dddd' }}";
                    }

                    if ( idParts[0] == "PageParameter" )
                    {
                        return "{{ PageParameter.[Enter Page Parameter Name Here] }}";
                    }

                }

                var workingParts = new List<string>();

                // Get the root type
                int pathPointer = 0;
                EntityTypeCache entityType = null;
                while ( entityType == null && pathPointer < idParts.Count() )
                {
                    string item = idParts[pathPointer];
                    string[] itemParts = item.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                    string itemName = itemParts.Length > 1 ? itemParts[0] : string.Empty;
                    string itemType = itemParts.Length > 1 ? itemParts[1] : item;

                    entityType = EntityTypeCache.Get( itemType, false );

                    workingParts.Add( entityType != null ? 
                        ( itemName != string.Empty ? itemName : entityType.FriendlyName.Replace( " ", string.Empty) ) :
                        idParts[pathPointer] );
                    pathPointer++;
                }
                
                if ( entityType != null )
                {
                    Type type = entityType.GetEntityType();

                    var formatString = "{0}";

                    // Traverse the Property path
                    bool itemIsCollection = false;
                    bool lastItemIsProperty = true;

                    while ( idParts.Count > pathPointer )
                    {
                        string propertyName = idParts[pathPointer];
                        workingParts.Add( propertyName );

                        var childProperty = type.GetProperty( propertyName );
                        if ( childProperty != null )
                        {
                            lastItemIsProperty = true;
                            type = childProperty.PropertyType;

                            if ( type.IsGenericType &&
                                type.GetGenericTypeDefinition() == typeof( ICollection<> ) &&
                                type.GetGenericArguments().Length == 1 )
                            {
                                string propertyNameSingularized = propertyName.Singularize();
                                string forString = string.Format( "<% for {0} in {1} %> {{0}} <% endfor %>", propertyNameSingularized, workingParts.AsDelimited( "." ) );
                                workingParts.Clear();
                                workingParts.Add( propertyNameSingularized );
                                formatString = string.Format( formatString, forString );

                                type = type.GetGenericArguments()[0];

                                itemIsCollection = true;
                            }
                            else
                            {
                                itemIsCollection = false;
                            }
                        }
                        else
                        {
                            lastItemIsProperty = false;
                        }

                        pathPointer++;
                    }

                    string itemString = string.Empty;
                    if ( !itemIsCollection )
                    {
                        if ( lastItemIsProperty )
                        {
                            itemString = string.Format( "<< {0} >>", workingParts.AsDelimited( "." ) );
                        }
                        else
                        {
                            string partPath = workingParts.Take( workingParts.Count - 1 ).ToList().AsDelimited( "." );
                            var partItem = workingParts.Last();
                            if ( type == typeof( Rock.Model.Person ) && partItem == "Campus" )
                            {
                                itemString = string.Format( "{{{{ {0} | Campus | Property:'Name' }}}}", partPath );
                            }
                            else
                            {
                                
                                itemString = string.Format( "{{{{ {0} | Attribute:'{1}' }}}}", partPath, partItem );
                            }
                            
                        }

                    }

                    return string.Format( formatString, itemString ).Replace( "<", "{" ).Replace( ">", "}" );
                }

                return string.Format( "{{{{ {0} }}}}", idParts.AsDelimited( "." ) );

            }

            return string.Empty;

        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            ItemRestUrlExtraParams = "?additionalFields=" + HttpUtility.UrlPathEncode(MergeFields.AsDelimited( "," ));
            base.RenderControl( writer );
        }
    }
}