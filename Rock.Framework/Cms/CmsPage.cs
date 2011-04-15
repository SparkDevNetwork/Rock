using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.Routing;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Models.Cms;
using Rock.Models.Crm;
using Rock.Services.Cms;
using Rock.Helpers;
using Rock.Cms.Security;

namespace Rock.Cms
{
    /// <summary>
    /// CmsPage is the base abstract class that all page templates should inherit from
    /// </summary>
    public abstract class CmsPage : System.Web.UI.Page
    {
        #region Events

        internal event BlockInstanceAttributesUpdatedEventHandler BlockInstanceAttributesUpdated;

        #endregion

        #region Private Variables

        private Dictionary<string, Control> _zones;
        private PlaceHolder phLoadTime;

        #endregion

        #region Protected Variables

        protected string UserName = string.Empty;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current Rock page instance being requested.  This value is set 
        /// by the RockRouteHandler immediately after instantiating the page
        /// </summary>
        public Rock.Cms.Cached.Page PageInstance { get; set; }

        /// <summary>
        /// The content areas on a layout page that blocks can be added to 
        /// </summary>
        public Dictionary<string, Control> Zones
        {
            get
            {
                if ( _zones == null )
                {
                    _zones = new Dictionary<string, Control>();
                    DefineZones();
                }
                return _zones;
            }
        }

        /// <summary>
        /// The Person ID of the currently logged in user.  Returns null if there is not a user logged in
        /// </summary>
        public int? CurrentPersonId
        {
            get
            {
                MembershipUser user = Membership.GetUser();
                if ( user != null )
                {
                    if ( user.ProviderUserKey != null )
                        return ( int )user.ProviderUserKey;
                    else
                        return null;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the currently logged in person.  Returns null if there is not a user logged in
        /// </summary>
        public Person CurrentPerson
        {
            get
            {
                int? personId = CurrentPersonId;
                if ( personId != null )
                {
                    if ( Context.Items.Contains( "CurrentPerson" ) )
                    {
                        return ( Person )Context.Items["CurrentPerson"];
                    }
                    else
                    {
                        Rock.Services.Crm.PersonService personService = new Services.Crm.PersonService();
                        Person person = personService.GetPerson( personId.Value );
                        Context.Items.Add( "CurrentPerson", person );
                        return person;
                    }
                }
                return null;
            }

            private set
            {
                Context.Items.Add( "CurrentPerson", value );
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Each layout page should define it's content zones in this method
        /// <code>
        ///     Zones.Add( "FirstColumn", FirstColumn );
        /// </code>
        /// </summary>
        protected abstract void DefineZones();

        #endregion

        #region Protected Methods

        protected virtual Control FindZone( string zoneName )
        {
            // First look in the Zones dictionary
            if ( Zones.ContainsKey( zoneName ) )
                return Zones[zoneName];

            // Then try to find a control with the zonename as the id
            Control zone = RecurseControls( this, zoneName );
            if ( zone != null )
                return zone;

            // If still no match, just add module to the form
            return this.Form;
        }

        /// <summary>
        /// Recurses the page's control heirarchy looking for any control who's id ends
        /// with the conrolId property
        /// </summary>
        /// <param name="parentControl"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected virtual Control RecurseControls( Control parentControl, string controlId )
        {
            if ( parentControl.ID != null && parentControl.ID.ToLower().EndsWith( controlId.ToLower() ) )
                return parentControl;

            foreach ( Control childControl in parentControl.Controls )
            {
                Control zoneControl = RecurseControls( childControl, controlId.ToLower() );
                if ( zoneControl != null )
                    return zoneControl;
            }

            return null;
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Initializes the page's culture to use the culture specified by the browser ("auto")
        /// </summary>
        protected override void InitializeCulture()
        {
            base.UICulture = "auto";
            base.Culture = "auto";

            base.InitializeCulture();
        }

        /// <summary>
        /// Loads all of the configured blocks for the current page into the control tree
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit( EventArgs e )
        {
            Trace.Write( "Begin page init" );

            if ( ScriptManager.GetCurrent( this.Page ) == null )
            {
                ScriptManager sm = new ScriptManager();
                sm.ID = "sManager";
                Page.Form.Controls.AddAt( 0, sm );
            }
            Trace.Write( "Added Script Manager" );

            // Get current user/person info
            MembershipUser user = Membership.GetUser();
            Trace.Write( "Got current user" );

            if ( user != null )
            {
                UserName = user.UserName;

                if ( user.ProviderUserKey != null && user.ProviderUserKey is int)
                {
                    int personId = ( int )user.ProviderUserKey;
                    string personNameKey = "PersonName_" + personId.ToString();
                    if ( Session[personNameKey] != null )
                    {
                        UserName = Session[personNameKey].ToString();
                        Trace.Write( "Read username from session variable" );
                    }
                    else
                    {
                        Rock.Services.Crm.PersonService personService = new Services.Crm.PersonService();
                        Rock.Models.Crm.Person person = personService.GetPerson( personId );
                        if ( person != null )
                        {
                            UserName = person.FullName;
                            CurrentPerson = person;
                        }

                        Session[personNameKey] = UserName;
                        Trace.Write( "Read username from database and saved to session" );
                    }
                }
            }

            // If a PageInstance exists
            if ( PageInstance != null )
            {
                if ( !PageInstance.Authorized( "View", user ) )
                {
                    if ( user == null || !user.IsApproved )
                        FormsAuthentication.RedirectToLoginPage();
                }
                else
                {
                    Trace.Write( "Checked if user is authorized to view page" );

                    // Cache object used for block output caching
                    ObjectCache cache = MemoryCache.Default;
                    Trace.Write( "Created Cache Object" );

                    // Load the blocks and insert them into page zones
                    Trace.Write("Getting Block Instances");
                    foreach ( Rock.Cms.Cached.BlockInstance blockInstance in PageInstance.BlockInstances )
                    {
                        Trace.Write( string.Format( "Begin Block Instance: {0}", blockInstance.Id ) );

                        // Get current user's permissions for the block instance
                        bool canView = blockInstance.Authorized( "View", user );
                        bool canEdit = blockInstance.Authorized( "Edit", user );
                        bool canConfig = blockInstance.Authorized( "Configure", user );
                        Trace.Write( "Read user's permissions for block instance" );

                        // If user can't view and they haven't logged in, redirect to the login page
                        if ( !canView )
                        {
                            if ( user == null || !user.IsApproved )
                                FormsAuthentication.RedirectToLoginPage();
                        }
                        else
                        {
                            // Create block wrapper control (implements INamingContainer so child control IDs are unique for
                            // each block instance
                            Rock.Controls.HtmlGenericContainer blockWrapper = new Rock.Controls.HtmlGenericContainer( "div" );
                            blockWrapper.ID = string.Format("block-instance-id-{0}", blockInstance.Id);
                            blockWrapper.ClientIDMode = ClientIDMode.Static;
                            FindZone( blockInstance.Zone ).Controls.Add( blockWrapper );
                            blockWrapper.Attributes.Add( "class", "block-instance " +
                                ( canEdit || canConfig ? "can-edit " : "" ) +
                                HtmlHelper.CssClassFormat( blockInstance.Block.Name ) );
                            Trace.Write( "Created block wrapper control" );

                            // Check to see if block is configured to use a "Cache Duration'
                            string blockCacheKey = string.Format( "Rock:BlockInstanceOutput:{0}", blockInstance.Id );
                            Trace.Write( "Checked if block output has been cached" );
                            if ( blockInstance.OutputCacheDuration > 0 && cache.Contains( blockCacheKey ) )
                            {
                                // If the current block exists in our custom output cache, add the cached output instead of adding the control
                                blockWrapper.Controls.Add( new LiteralControl( cache[blockCacheKey] as string ) );
                            }
                            else
                            {
                                // Load the control and add to the control tree
                                Control control = TemplateControl.LoadControl( blockInstance.Block.Path );
                                Trace.Write( string.Format( "Loaded {0}", blockInstance.Block.Path ) );
                                control.ClientIDMode = ClientIDMode.AutoID;

                                CmsBlock cmsBlock = null;

                                // Check to see if the control was a PartialCachingControl or not
                                if ( control is CmsBlock )
                                {
                                    cmsBlock = control as CmsBlock;
                                }
                                else
                                {
                                    if ( control is PartialCachingControl && ( ( PartialCachingControl )control ).CachedControl != null )
                                        cmsBlock = ( CmsBlock )( ( PartialCachingControl )control ).CachedControl;
                                }

                                // If the current control is a cmsBlock, set it's properties
                                if ( cmsBlock != null )
                                {
                                    cmsBlock.PageInstance = PageInstance;
                                    cmsBlock.BlockInstance = blockInstance;
                                    Trace.Write( "Set block cached instance properties" );

                                    // If user has edit or configuration rights add the configure buttons and attribute
                                    // edit panel in addition to the user block
                                    if ( canConfig )
                                    {
                                        // Add the attribute edit script
                                        string blockConfigScript = string.Format( @"
    $(document).ready(function () {{

        $('div.attributes-{0}').dialog({{
            autoOpen: false,
            draggable: true,
            width: 530,
            title: 'Module Instance Properties',
            closeOnEscape: true,
            modal: true,
            open: function (type, data) {{
                $(this).parent().appendTo(""form"");
            }}
        }});

        $('a.attributes-{0}-show').click(function () {{
            $('div.attributes-{0}').dialog('open');
            return false;
        }});

    }});

    Sys.Application.add_load(function () {{
        $('.attributes-{0}-hide').click(function () {{
            $('div.attributes-{0}').dialog('close');
        }});
    }});
", blockInstance.Id );

                                        this.Page.ClientScript.RegisterStartupScript( this.GetType(),
                                            string.Format( "block-config-script-{0}", blockInstance.Id ),
                                            blockConfigScript, true );
                                        Trace.Write( "Added the configuration script" );

                                    }

                                    if (canEdit || canConfig)
                                    {
                                        // Add the config buttons
                                        HtmlGenericControl blockConfig = new HtmlGenericControl( "div" );
                                        blockConfig.ClientIDMode = ClientIDMode.AutoID;
                                        blockConfig.Attributes.Add( "class", "block-configuration" );
                                        blockConfig.Attributes.Add( "style", "display: none" );
                                        blockWrapper.Controls.Add( blockConfig );

                                        foreach ( Control configControl in cmsBlock.GetConfigurationControls( canConfig, canEdit ) )
                                        {
                                            configControl.ClientIDMode = ClientIDMode.AutoID;
                                            blockConfig.Controls.Add( configControl );
                                        }
                                        Trace.Write( "Added the configuration controls" );
                                    }

                                    // Add the block
                                    blockWrapper.Controls.Add( control );
                                    Trace.Write( "Added the block instance control" );

                                    if (canConfig)
                                    {
                                        // Add the attribute update panel
                                        HtmlGenericControl attributePanel = new HtmlGenericControl( "div" );
                                        attributePanel.ClientIDMode = ClientIDMode.AutoID;
                                        attributePanel.Attributes.Add( "class",
                                            string.Format( "attributes-{0}", blockInstance.Id ) );
                                        attributePanel.Attributes.Add( "style", "text-align: left" );
                                        blockWrapper.Controls.Add( attributePanel );

                                        UpdatePanel upPanel = new UpdatePanel();
                                        upPanel.ClientIDMode = ClientIDMode.AutoID;
                                        upPanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
                                        upPanel.ChildrenAsTriggers = true;
                                        attributePanel.Controls.Add( upPanel );

                                        PlaceHolder phAttributes = new PlaceHolder();
                                        phAttributes.ClientIDMode = ClientIDMode.AutoID;
                                        upPanel.ContentTemplateContainer.Controls.Add( phAttributes );

                                        HtmlGenericControl fieldset = new HtmlGenericControl( "fieldset" );
                                        fieldset.ClientIDMode = ClientIDMode.AutoID;
                                        phAttributes.Controls.Add( fieldset );

                                        HtmlGenericControl ol = new HtmlGenericControl( "ol" );
                                        ol.ClientIDMode = ClientIDMode.AutoID;
                                        fieldset.Controls.Add( ol );

                                        foreach ( Rock.Cms.Cached.Attribute attribute in blockInstance.Attributes )
                                        {
                                            HtmlGenericControl li = new HtmlGenericControl( "li" );
                                            li.ID = string.Format( "attribute-{0}", attribute.Id );
                                            li.ClientIDMode = ClientIDMode.AutoID;
                                            ol.Controls.Add( li );


                                            Label lbl = new Label();
                                            lbl.ClientIDMode = ClientIDMode.AutoID;
                                            lbl.Text = attribute.Name;
                                            lbl.AssociatedControlID = string.Format("attribute-field-{0}", attribute.Id);
                                            li.Controls.Add( lbl );

                                            Control attributeControl = attribute.CreateControl( blockInstance.AttributeValues[attribute.Name] );
                                            attributeControl.ID = string.Format("attribute-field-{0}", attribute.Id);
                                            attributeControl.ClientIDMode = ClientIDMode.AutoID;
                                            li.Controls.Add( attributeControl );

                                            if ( !string.IsNullOrEmpty( attribute.Description ) )
                                            {
                                                HtmlAnchor a = new HtmlAnchor();
                                                a.ClientIDMode = ClientIDMode.AutoID;
                                                a.Attributes.Add( "class", "attribute-description tooltip" );
                                                a.InnerHtml = "<span>" + attribute.Description + "</span>";

                                                li.Controls.Add( a );
                                            }

                                        }


                                        Button btnSaveAttributes = new Button();
                                        btnSaveAttributes.ID = string.Format( "attributes-{0}-hide", blockInstance.Id );
                                        btnSaveAttributes.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                                        btnSaveAttributes.Text = "Save";
                                        btnSaveAttributes.CssClass = btnSaveAttributes.ID;
                                        btnSaveAttributes.Click += new EventHandler( btnSaveAttributes_Click );
                                        upPanel.ContentTemplateContainer.Controls.Add( btnSaveAttributes );

                                        Trace.Write( "Added the attribute update panel" );
                                    }

                                }
                                else
                                    // add the generic control
                                    blockWrapper.Controls.Add( control );
                            }
                        }

                        Trace.Write( string.Format( "End Block Instance: {0}", blockInstance.Id ) );
                    }

                    if ( PageInstance.IncludeAdminFooter && PageInstance.Authorized( "Edit", user ) )
                    {
                        HtmlGenericControl adminFooter = new HtmlGenericControl( "div" );
                        adminFooter.ID = "cms-admin-footer";
                        adminFooter.ClientIDMode = System.Web.UI.ClientIDMode.Static;
                        this.Form.Controls.Add( adminFooter );

                        phLoadTime = new PlaceHolder();
                        adminFooter.Controls.Add( phLoadTime );

                        HtmlGenericControl buttonBar = new HtmlGenericControl( "div" );
                        adminFooter.Controls.Add( buttonBar );
                        buttonBar.Attributes.Add( "class", "button-bar" );

                        HtmlGenericControl aBlockConfig = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aBlockConfig );
                        aBlockConfig.Attributes.Add( "class", "block-config icon-button" );
                        aBlockConfig.Attributes.Add( "href", "#" );
                        aBlockConfig.Attributes.Add( "Title", "Show Block Configuration" );
                        aBlockConfig.InnerText = "Block Settings";

                        HtmlGenericControl aAttributes = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aAttributes );
                        aAttributes.Attributes.Add( "class", "attributes icon-button" );
                        aAttributes.Attributes.Add( "href", "#" );
                        aAttributes.Attributes.Add( "Title", "Show Page Attributes" );
                        aAttributes.InnerText = "Page Attributes";

                        HtmlGenericControl aChildPages = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aChildPages );
                        aChildPages.Attributes.Add( "class", "page-child-pages icon-button" );
                        aChildPages.Attributes.Add( "href", "#" );
                        aChildPages.Attributes.Add( "Title", "Show Child Pages" );
                        aChildPages.InnerText = "Child Pages";

                        HtmlGenericControl aPageZones = new HtmlGenericControl( "a" );
                        buttonBar.Controls.Add( aPageZones );
                        aPageZones.Attributes.Add( "class", "page-zones icon-button" );
                        aPageZones.Attributes.Add( "href", "#" );
                        aPageZones.Attributes.Add( "Title", "Show Page Zones" );
                        aPageZones.InnerText = "Page Zones";

                        string footerScript = @"
    $(document).ready(function () {
        $('#cms-admin-footer .block-config').click(function (ev) {
            $('.block-configuration').toggle();
            $('.block-instance').toggleClass('outline');
            return false;
        });
    });
";

                        this.ClientScript.RegisterClientScriptBlock( this.GetType(), "cms-admin-footer", footerScript, true );

                        Trace.Write( "Added the admin footer" );
                    }

                    // Check to see if page output should be cached.  The RockRouteHandler
                    // saves the PageCacheData information for the current page to memorycache 
                    // so it should always exist
                    if ( PageInstance.OutputCacheDuration > 0 )
                    {
                        Response.Cache.SetCacheability( System.Web.HttpCacheability.Public );
                        Response.Cache.SetExpires( DateTime.Now.AddSeconds( PageInstance.OutputCacheDuration ) );
                        Response.Cache.SetValidUntilExpires( true );
                    }
                }
            }

            Trace.Write( "End page init" );
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( phLoadTime != null  )
            {
                TimeSpan tsDuration = DateTime.Now.Subtract( ( DateTime )Context.Items["Request_Start_Time"] );
                phLoadTime.Controls.Add( new LiteralControl( string.Format( "Page Load Time: {0:N2}s", tsDuration.TotalSeconds ) ) );
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the current page's value(s) for the selected attribute
        /// If the attribute doesn't exist an empty string is returned.  If there
        /// is more than one value for the attribute, the values are returned delimited
        /// by a bar character (|).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string AttributeValue( string name )
        {
            if ( PageInstance == null )
                return string.Empty;

            if ( PageInstance.AttributeValues == null )
                return string.Empty;

            if ( !PageInstance.AttributeValues.ContainsKey( name ) )
                return string.Empty;

            return string.Join( "|", PageInstance.AttributeValues[name] );
        }

        #endregion

        #region Event Handlers

        void btnSaveAttributes_Click( object sender, EventArgs e )
        {
            Button btnSave = ( Button )sender;
            int blockInstanceId = Convert.ToInt32( btnSave.ID.Replace( "attributes-", "" ).Replace( "-hide", "" ) );

            Cached.BlockInstance blockInstance = PageInstance.BlockInstances.Where( b => b.Id == blockInstanceId ).FirstOrDefault();
            if ( blockInstance != null )
            {
                // Find the container control
                Control blockWrapper = RecurseControls(this, string.Format("block-instance-id-{0}", blockInstance.Id));
                if ( blockWrapper != null )
                {
                    foreach ( Rock.Cms.Cached.Attribute attribute in blockInstance.Attributes )
                    {
                        //HtmlGenericControl editCell = ( HtmlGenericControl )blockWrapper.FindControl( string.Format( "attribute-{0}", attribute.Id.ToString() ) );
                        Control control = blockWrapper.FindControl( string.Format( "attribute-field-{0}", attribute.Id.ToString() ) );
                        if ( control != null )
                            blockInstance.AttributeValues[attribute.Name] = attribute.FieldType.Field.ReadValue( control );
                    }

                    blockInstance.SaveAttributeValues( CurrentPersonId );

                    if ( BlockInstanceAttributesUpdated != null )
                        BlockInstanceAttributesUpdated( sender, new BlockInstanceAttributesUpdatedEventArgs( blockInstanceId ) );
                }
            }
        }

        #endregion
    }

    #region Event Argument Classes

    internal class BlockInstanceAttributesUpdatedEventArgs : EventArgs
    {
        public int BlockInstanceID { get; private set; }

        public BlockInstanceAttributesUpdatedEventArgs( int blockInstanceID )
        {
            BlockInstanceID = blockInstanceID;
        }
    }

    #endregion

    #region Delegates 

    internal delegate void BlockInstanceAttributesUpdatedEventHandler( object sender, BlockInstanceAttributesUpdatedEventArgs e );

    #endregion

}
