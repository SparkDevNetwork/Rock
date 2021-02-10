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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Newtonsoft.Json;
namespace RockWeb.Blocks.Examples
{
    [DisplayName( "Model Map" )]
    [Category( "Examples" )]
    [Description( "Displays details about each model classes in Rock.Model." )]
    [KeyValueListField( "Category Icons", "The Icon Class to use for each category.", false, "", "Category", "Icon Css Class" )]
    public partial class ModelMap : RockBlock
    {

        #region Properties

        protected List<MCategory> EntityCategories { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            EntityCategories = ViewState["EntityCategories"] as List<MCategory>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rptCategory.ItemCommand += rptCategory_ItemCommand;
            rptModel.ItemCommand += rptModel_ItemCommand;
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
                Guid? categoryGuid = null;
                int? entityTypeId = null;
                string categoryName = PageParameter( "Category" );

                LoadCategories();
                BindFilter( true );

                if ( !string.IsNullOrWhiteSpace( categoryName ) )
                {
                    var category = EntityCategories.Where( c => c.Name.Equals( categoryName, StringComparison.CurrentCultureIgnoreCase ) ).FirstOrDefault();
                    if ( category != null )
                    {
                        categoryGuid = category.Guid;
                    }
                }

                if ( !string.IsNullOrWhiteSpace( PageParameter( "EntityType" ) ) )
                {
                    entityTypeId = PageParameter( "EntityType" ).AsIntegerOrNull();

                    if ( entityTypeId == null )
                    {
                        var entityType = EntityTypeCache.Get( PageParameter( "EntityType" ).AsGuid() );

                        if ( entityType != null )
                        {
                            entityTypeId = entityType.Id;
                            categoryGuid = EntityCategories.Where( c => c.RockEntityIds.Contains( entityType.Id ) ).Select( c => c.Guid ).FirstOrDefault();
                        }
                    }
                }

                ShowData( categoryGuid, entityTypeId );
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["EntityCategories"] = EntityCategories;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemCommand event of the rptCategory control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCategory_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            ShowData( e.CommandArgument.ToString().AsGuidOrNull(), null );
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptModel control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptModel_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            ShowData( hfSelectedCategoryGuid.Value.AsGuid(), e.CommandArgument.ToString().AsInteger() );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "IsRequired", ddlIsRequired.SelectedValue );
            gfSettings.SaveUserPreference( "IsDatabase", ddlIsDatabase.SelectedValue );
            gfSettings.SaveUserPreference( "IsLava", ddlIsLava.SelectedValue );

            ShowData( hfSelectedCategoryGuid.Value.AsGuidOrNull(), hfSelectedEntityId.Value.AsIntegerOrNull() );
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ClearFilterClick( object sender, EventArgs e )
        {
            gfSettings.DeleteUserPreferences();
            BindFilter();

            ShowData( hfSelectedCategoryGuid.Value.AsGuidOrNull(), hfSelectedEntityId.Value.AsIntegerOrNull() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter( bool useQueryString = false )
        {
            if ( useQueryString )
            {
                if ( !string.IsNullOrWhiteSpace( PageParameter( "IsRequired" ) ) )
                {
                    gfSettings.SaveUserPreference( "IsRequired", PageParameter( "IsRequired" ) );
                }

                if ( !string.IsNullOrWhiteSpace( PageParameter( "IsDatabase" ) ) )
                {
                    gfSettings.SaveUserPreference( "IsDatabase", PageParameter( "IsDatabase" ) );
                }

                if ( !string.IsNullOrWhiteSpace( PageParameter( "IsLava" ) ) )
                {
                    gfSettings.SaveUserPreference( "IsLava", PageParameter( "IsLava" ) );
                }
            }

            ddlIsRequired.SelectedValue = gfSettings.GetUserPreference( "IsRequired" );
            ddlIsDatabase.SelectedValue = gfSettings.GetUserPreference( "IsDatabase" );
            ddlIsLava.SelectedValue = gfSettings.GetUserPreference( "IsLava" );
        }

        private void LoadCategories()
        {
            var entityCategories = new List<MCategory>();

            var categoryIcons = new Dictionary<string, string>();
            var categoryIconValues = GetAttributeValue( "CategoryIcons" );
            if ( !string.IsNullOrWhiteSpace( categoryIconValues ) )
            {
                categoryIconValues = categoryIconValues.TrimEnd( '|' );
                foreach ( var keyVal in categoryIconValues.Split( '|' )
                                .Select( s => s.Split( '^' ) )
                                .Where( s => s.Length == 2 ) )
                {
                    categoryIcons.AddOrIgnore( keyVal[0], keyVal[1] );
                }
            }

            foreach ( var entity in EntityTypeCache.All().Where( t => t.IsEntity ) )
            {
                var type = entity.GetEntityType();
                if ( type != null && type.InheritsOrImplements( typeof( Rock.Data.Entity<> ) ) )
                {
                    string category = "Other";
                    var domainAttr = type.GetCustomAttribute<RockDomainAttribute>( false );
                    if ( domainAttr != null && domainAttr.Name.IsNotNullOrWhiteSpace() )
                    {
                        category = domainAttr.Name;
                    }

                    var entityCategory = entityCategories
                        .Where( c => c.Name == category )
                        .FirstOrDefault();
                    if ( entityCategory == null )
                    {
                        entityCategory = new MCategory { Guid = Guid.NewGuid(), Name = category, RockEntityIds = new List<int>() };
                        entityCategory.IconCssClass = categoryIcons.ContainsKey( category ) ? categoryIcons[category] : string.Empty;
                        entityCategories.Add( entityCategory );
                    }
                    entityCategory.RockEntityIds.Add( entity.Id );
                }
            }

            EntityCategories = new List<MCategory>( entityCategories.Where( c => c.Name != "Other" ).OrderBy( c => c.Name ) );
            EntityCategories.AddRange( entityCategories.Where( c => c.Name == "Other" ) );
        }

        private void ShowData( Guid? categoryGuid, int? entityTypeId )
        {
            if ( EntityCategories == null )
            {
                LoadCategories();
            }

            hfSelectedCategoryGuid.Value = categoryGuid.ToString();
            hfSelectedEntityId.Value = null;

            // Bind Categories
            rptCategory.DataSource = EntityCategories;
            rptCategory.DataBind();

            pnlModels.Visible = false;
            pnlKey.Visible = false;
            lCategoryName.Text = string.Empty;

            EntityTypeCache entityType = null;
            var entityTypeList = new List<EntityTypeCache>();
            if ( categoryGuid.HasValue )
            {
                var category = EntityCategories.Where( c => c.Guid.Equals( categoryGuid ) ).FirstOrDefault();
                if ( category != null )
                {
                    lCategoryName.Text = category.Name + " Models";
                    pnlModels.Visible = true;

                    entityTypeList = category
                        .RockEntityIds
                        .Select( a => EntityTypeCache.Get( a ) )
                        .Where( a => a != null )
                        .OrderBy( et => et.FriendlyName )
                        .ToList();
                    if ( entityTypeId.HasValue )
                    {
                        entityType = entityTypeList.Where( t => t.Id == entityTypeId.Value ).FirstOrDefault();
                        hfSelectedEntityId.Value = entityType != null ? entityType.Id.ToString() : null;
                    }
                    else
                    {
                        entityType = entityTypeList.FirstOrDefault();
                        hfSelectedEntityId.Value = entityTypeList.Any() ? entityTypeList.First().Id.ToString() : null;
                    }
                }
            }

            // Bind Models
            rptModel.DataSource = entityTypeList;
            rptModel.DataBind();

            string details = string.Empty;
            nbClassesWarning.Visible = false;
            pnlClassDetail.Visible = false;
            if ( entityType != null )
            {
                try
                {
                    var type = entityType.GetEntityType();
                    if ( type != null )
                    {
                        pnlKey.Visible = true;

                        var xmlComments = GetXmlComments();

                        var mClass = new MClass();
                        mClass.Name = type.Name;
                        mClass.Comment = GetComments( type, xmlComments );

                        PropertyInfo[] properties = type.GetProperties( BindingFlags.Public | ( BindingFlags.Instance ) )
                            .Where( m => ( m.MemberType == MemberTypes.Method || m.MemberType == MemberTypes.Property ) )
                            .ToArray();
                        foreach ( PropertyInfo p in properties.OrderBy( i => i.Name ).ToArray() )
                        {
                            mClass.Properties.Add( new MProperty
                            {
                                Name = p.Name,
                                IsInherited = p.DeclaringType != type,
                                IsVirtual = p.GetGetMethod() != null && p.GetGetMethod().IsVirtual && !p.GetGetMethod().IsFinal,
                                IsLavaInclude = p.IsDefined( typeof( LavaIncludeAttribute ) ) || p.IsDefined( typeof( DataMemberAttribute ) ),
                                IsObsolete = p.IsDefined( typeof( ObsoleteAttribute ) ),
                                ObsoleteMessage = GetObsoleteMessage( p ),
                                NotMapped = p.IsDefined( typeof( NotMappedAttribute ) ),
                                Required = p.IsDefined( typeof( RequiredAttribute ) ),
                                Id = p.MetadataToken,
                                Comment = GetComments( p, xmlComments )
                            } );
                        }

                        MethodInfo[] methods = type.GetMethods( BindingFlags.Public | ( BindingFlags.Instance ) )
                            .Where( m => !m.IsSpecialName && ( m.MemberType == MemberTypes.Method || m.MemberType == MemberTypes.Property ) )
                            .ToArray();
                        foreach ( MethodInfo m in methods.OrderBy( i => i.Name ).ToArray() )
                        {
                            // crazy, right?
                            var param = string.Join( ", ", m.GetParameters().Select( pi => { var x = pi.ParameterType + " " + pi.Name; return x; } ) );

                            mClass.Methods.Add( new MMethod
                            {
                                Name = m.Name,
                                IsInherited = m.DeclaringType != type,
                                Id = m.MetadataToken,
                                Signature = string.Format( "{0}({1})", m.Name, param ),
                                Comment = GetComments( m, xmlComments ),
                                IsObsolete = m.IsDefined( typeof( ObsoleteAttribute ) ),
                                ObsoleteMessage = GetObsoleteMessage( m )
                            } );
                        }

                        var pageReference = new Rock.Web.PageReference( CurrentPageReference );
                        pageReference.QueryString = new System.Collections.Specialized.NameValueCollection();
                        pageReference.QueryString["EntityType"] = entityType.Guid.ToString();

                        lClassName.Text = mClass.Name;
                        hlAnchor.NavigateUrl = pageReference.BuildUrl();
                        lClassDescription.Text = mClass.Comment != null ? mClass.Comment.Summary : string.Empty;
                        lClasses.Text = ClassNode( mClass );

                        pnlClassDetail.Visible = true;
                    }
                    else
                    {
                        nbClassesWarning.Text = "Unable to get class details for " + entityType.FriendlyName;
                        nbClassesWarning.Details = entityType.AssemblyName;
                        nbClassesWarning.Dismissable = true;
                        nbClassesWarning.Visible = true;
                    }
                }
                catch ( Exception ex )
                {
                    nbClassesWarning.Text = string.Format( "Error getting class details for <code>{0}</code>", entityType );
                    nbClassesWarning.Details = ex.Message;
                    nbClassesWarning.Dismissable = true;
                    nbClassesWarning.Visible = true;
                }
            }
        }

        /// <summary>
        /// Build a "node" of the class/model with its properties and methods inside.
        /// </summary>
        /// <param name="aClass"></param>
        /// <returns></returns>
        private string ClassNode( MClass aClass )
        {
            var sb = new StringBuilder();

            if ( aClass.Properties.Any() || aClass.Methods.Any() )
            {
                if ( aClass.Properties.Any() )
                {
                    sb.AppendLine( "<h4>Properties</h4><ul class='list-unstyled'>" );
                    foreach ( var property in aClass.Properties.OrderBy( p => p.Name ) )
                    {
                        bool? isRequired = gfSettings.GetUserPreference( "IsRequired" ).AsBooleanOrNull();
                        bool? isDatabase = gfSettings.GetUserPreference( "IsDatabase" ).AsBooleanOrNull();
                        bool? isLava = gfSettings.GetUserPreference( "IsLava" ).AsBooleanOrNull();

                        if ( isRequired.HasValue && isRequired.Value != property.Required )
                        {
                            continue;
                        }

                        if ( isDatabase.HasValue && isDatabase.Value != ( !property.NotMapped && !property.IsVirtual ) )
                        {
                            continue;
                        }

                        if ( isLava.HasValue && isLava.Value != property.IsLavaInclude )
                        {
                            continue;
                        }

                        sb.AppendFormat( "<li data-id='p{0}' class='{6}'><strong>{9}<tt>{1}</tt></strong>{3}{4}{5}{10}{2}{7}</li>{8}",
                            property.Id, // 0
                            HttpUtility.HtmlEncode( property.Name ), // 1
                            ( property.Comment != null && !string.IsNullOrWhiteSpace( property.Comment.Summary ) ) ? " - " + property.Comment.Summary : string.Empty, // 2
                            property.Required ? " <strong class='text-danger'>*</strong> " : string.Empty, // 3
                            property.IsLavaInclude ? " <i class='fa fa-bolt fa-fw text-warning'></i> " : string.Empty, // 4
                            string.Empty, // 5
                            property.IsInherited ? " js-model hidden " : " ", // 6
                            property.IsInherited ? " (inherited)" : string.Empty, // 7
                            Environment.NewLine, // 8
                            property.NotMapped || property.IsVirtual ? "<i class='fa fa-square-o fa-fw'></i> " : "<i class='fa fa-database fa-fw'></i> ", // 9
                            property.IsObsolete ? "<i class='fa fa-ban fa-fw text-danger' title='no longer supported'></i> <i>" + property.ObsoleteMessage + " </i> " : string.Empty // 10
                            );
                    }
                    sb.AppendLine( "</ul>" );
                }

                if ( aClass.Methods.Any() )
                {
                    sb.AppendLine( "<h4>Methods</h4><ul>" );

                    if ( aClass.Methods.Where( m => m.IsInherited == false ).Count() == 0 )
                    {
                        sb.AppendLine( "<small class='text-muted'><i>all inherited</i></small>" );
                    }

                    foreach ( var method in aClass.Methods.OrderBy( m => m.Name ) )
                    {
                        sb.AppendFormat( "<li data-id='m{0}' class='{3}'><strong><tt>{1}</tt></strong> {2}{4} {6}</li>{5}",
                            method.Id,
                            HttpUtility.HtmlEncode( method.Signature ),
                            ( method.Comment != null && !string.IsNullOrWhiteSpace( method.Comment.Summary ) ) ? " - " + method.Comment.Summary : "",
                            method.IsInherited ? " js-model hidden " : " ",
                            method.IsInherited ? " (inherited)" : "",
                            Environment.NewLine, // 5
                            method.IsObsolete ? "<i class='fa fa-ban fa-fw text-danger' title='no longer supported'></i> <i>" + method.ObsoleteMessage + " </i> " : string.Empty // 6
                            );
                    }

                    sb.AppendLine( "</ul>" );
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Reads the XML comments from the Rock assembly XML file.
        /// </summary>
        /// <param name="rockDll">The rock DLL.</param>
        private Dictionary<string, XElement> GetXmlComments()
        {
            var rockDll = typeof( Rock.Model.EntityType ).Assembly;

            string rockDllPath = rockDll.Location;

            string docuPath = rockDllPath.Substring( 0, rockDllPath.LastIndexOf( "." ) ) + ".XML";

            if ( !File.Exists( docuPath ) )
            {
                docuPath = HttpContext.Current.Server.MapPath( "~" ) + @"bin\Rock.XML";
            }

            if ( File.Exists( docuPath ) )
            {
                var _docuDoc = XDocument.Load( docuPath );
                return _docuDoc.Descendants( "member" ).ToDictionary( a => a.Attribute( "name" ).Value, v => v );
            }

            return null;
        }

        /// <summary>
        /// Gets the comments from the data in the assembly's XML file for the 
        /// given member object.
        /// </summary>
        /// <param name="p">The MemberInfo instance.</param>
        /// <returns>an XmlComment object</returns>
        private XmlComment GetComments( MemberInfo p, Dictionary<string, XElement> xmlComments )
        {
            XmlComment xmlComment = new XmlComment();

            try
            {
                var prefix = string.Empty;

                if ( p.MemberType == MemberTypes.Property )
                {
                    prefix = "P:";
                }
                else if ( p.MemberType == MemberTypes.Method )
                {
                    prefix = "M:";
                }
                else if ( p.MemberType == MemberTypes.TypeInfo )
                {
                    prefix = "T:";
                }
                else
                {
                    return null;
                }

                string path = string.Format( "{0}{1}.{2}", prefix, ( p.DeclaringType != null ) ? p.DeclaringType.FullName : "Rock.Model", p.Name );

                var name = xmlComments != null && xmlComments.ContainsKey( path ) ? xmlComments[path] : null;
                if ( name != null )
                {
                    // Read the InnerXml contents of the summary Element.
                    var reader = name.Element( "summary" ).CreateReader();
                    reader.MoveToContent();
                    xmlComment.Summary = MakeSummaryHtml( reader.ReadInnerXml() );
                    xmlComment.Value = name.Element( "value" ).ValueSafe();
                    xmlComment.Remarks = name.Element( "remarks" ).ValueSafe();
                    xmlComment.Returns = name.Element( "returns" ).ValueSafe();
                }
            }
            catch { }
            return xmlComment;

        }


        /// <summary>
        /// Gets the comments from the data in the assembly's XML file for the 
        /// given member object.
        /// </summary>
        /// <param name="p">The MemberInfo instance.</param>
        /// <returns>an XmlComment object</returns>
        private string GetObsoleteMessage( MemberInfo p )
        {
            if ( !p.IsDefined( typeof( ObsoleteAttribute ) ) )
            {
                return null;
            }

            string message = string.Empty;

            try
            {
                var msg = p.CustomAttributes.Where( a => a.AttributeType == typeof( ObsoleteAttribute ) ).Select( r => r.ConstructorArguments.FirstOrDefault() ).FirstOrDefault();
                if ( msg != null )
                {
                    message = msg.Value.ToStringSafe();
                }

            }
            catch { }
            return message;

        }
        /// <summary>
        /// Makes the summary HTML; converting any type/class cref (ex. <see cref="T:Rock.Model.Campus" />) 
        /// references to HTML links (ex <a href="#Campus">Campus</a>)
        /// </summary>
        /// <param name="innerXml">The inner XML.</param>
        /// <returns></returns>
        private string MakeSummaryHtml( string innerXml )
        {
            innerXml = System.Text.RegularExpressions.Regex.Replace( innerXml, @"\s+", " " );
            innerXml = System.Text.RegularExpressions.Regex.Replace( innerXml, @"<see cref=""T:(.*)\.([^.]*)"" />", "<a href=\"#$2\">$2</a>" );
            return innerXml;
        }

        protected string GetCategoryClass( object obj )
        {
            if ( obj is Guid )
            {
                Guid? selectedCategoryGuid = hfSelectedCategoryGuid.Value.AsGuidOrNull();
                Guid categoryGuid = ( Guid ) obj;
                return selectedCategoryGuid.HasValue && selectedCategoryGuid.Value == categoryGuid ? "active" : string.Empty;
            }
            return string.Empty;
        }

        protected string GetEntityClass( object obj )
        {
            if ( obj is int )
            {
                int? selectedEntityId = hfSelectedEntityId.Value.AsIntegerOrNull();
                int entityId = ( int ) obj;
                return selectedEntityId.HasValue && selectedEntityId.Value == entityId ? "active" : string.Empty;
            }
            return string.Empty;
        }

        #endregion
    }

    #region Helper Classes

    [Serializable]
    public class MCategory
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string IconCssClass { get; set; }
        public List<int> RockEntityIds { get; set; }
    }

    class MClass
    {
        public MCategory Category { get; set; }
        public string Name { get; set; }
        public XmlComment Comment { get; set; }
        public Guid Guid { get; set; }
        public List<MProperty> Properties { get; set; }
        public List<MMethod> Methods { get; set; }

        public MClass()
        {
            Properties = new List<MProperty>();
            Methods = new List<MMethod>();
        }
    }

    class MProperty
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool IsInherited { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsLavaInclude { get; set; }
        public bool IsObsolete { get; set; }
        public string ObsoleteMessage { get; set; }
        public bool NotMapped { get; set; }
        public bool Required { get; set; }
        public XmlComment Comment { get; set; }
    }

    class MMethod
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool IsInherited { get; set; }
        public bool IsObsolete { get; set; }
        public string ObsoleteMessage { get; set; }
        public string Signature { get; set; }
        public XmlComment Comment { get; set; }
    }

    class XmlComment
    {
        public string Summary { get; set; }
        public string Value { get; set; }
        public string Remarks { get; set; }
        public string[] Params { get; set; }
        public string Returns { get; set; }
    }

    #endregion

    #region Extension Methods

    public static class Extensions
    {
        public static string ValueSafe( this XElement element, string defaultValue = "" )
        {
            if ( element != null )
            {
                return element.Value;
            }

            return defaultValue;
        }

        // from @fir3rpho3nixx at http://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class
        public static bool InheritsOrImplements( this Type child, Type parent )
        {
            parent = ResolveGenericTypeDefinition( parent );

            var currentChild = child.IsGenericType
                                ? child.GetGenericTypeDefinition()
                                : child;

            while ( currentChild != typeof( object ) )
            {
                if ( parent == currentChild || HasAnyInterfaces( parent, currentChild ) )
                {
                    return true;
                }

                currentChild = currentChild.BaseType != null
                            && currentChild.BaseType.IsGenericType
                                ? currentChild.BaseType.GetGenericTypeDefinition()
                                : currentChild.BaseType;

                if ( currentChild == null )
                {
                    return false;
                }
            }
            return false;
        }

        private static bool HasAnyInterfaces( Type parent, Type child )
        {
            return child.GetInterfaces()
                .Any( childInterface =>
                {
                    var currentInterface = childInterface.IsGenericType
                        ? childInterface.GetGenericTypeDefinition()
                        : childInterface;

                    return currentInterface == parent;
                } );
        }

        private static Type ResolveGenericTypeDefinition( Type parent )
        {
            var shouldUseGenericType = true;
            if ( parent.IsGenericType && parent.GetGenericTypeDefinition() != parent )
            {
                shouldUseGenericType = false;
            }

            if ( parent.IsGenericType && shouldUseGenericType )
            {
                parent = parent.GetGenericTypeDefinition();
            }
            return parent;
        }
    }

    #endregion

}