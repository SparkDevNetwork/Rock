<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelDynamic.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelDynamic" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
<ContentTemplate>

    <%-- View Panel --%>
    <asp:Panel ID="pnlView" runat="server">
        <asp:PlaceHolder ID="phContent" runat="server" />
        <asp:Literal ID="lDebug" runat="server" />
    </asp:Panel>

    <%-- Edit Panel --%>
    <asp:Panel ID="pnlEdit" CssClass="well margin-t-md" runat="server" Visible="false">

        <Rock:NotificationBox ID="nbError" runat="server" Heading="Error" Title="Query Error!" NotificationBoxType="Danger" Visible="false" />

        <div class="row">
            <div class="col-md-5">
                <Rock:RockDropDownList ID="ddlChannel" runat="server" Label="Channel" 
                    DataTextField="Name" DataValueField="Guid" AutoPostBack="true" OnSelectedIndexChanged="ddlChannel_SelectedIndexChanged"
                    help="The channel to display items from." />
            </div>
            <div class="col-md-7">
                <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" RepeatDirection="Horizontal"
                    help="Include items with the following status." />
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:CodeEditor ID="ceQuery" runat="server" EditorHeight="400" EditorMode="Liquid" EditorTheme="Rock" Label="Format"
                    Help="The template to use when formatting the list of items." />
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <Rock:NumberBox ID="nbCount" runat="server" CssClass="input-width-sm" Label="Items per Page"
                    help="The maximum number of items to display per page (0 means unlimited)." />
                <Rock:NumberBox ID="nbCacheDuration" runat="server" CssClass="input-width-sm" Label="Cache Duration"
                    help="The number of seconds to cache the content for (use '0' for no caching)." />
                <Rock:RockCheckBox ID="cbSetPageTitle" runat="server" Label="Set Page Title" Text="Yes"
                    Help="When enabled will update the page title with the channel's name unless there is a item id in the query string then it will display the item's title." />
            </div>
            <div class="col-md-6">
                <Rock:RockCheckBox ID="cbDebug" runat="server" Label="Enable Debug" Text="Yes"
                    Help="Enabling debug will display the fields of the first 5 items to help show you wants available for your template." />
                <Rock:RockCheckBox ID="cbMergeContent" runat="server" Label="Merge Content" Text="Yes"
                    Help="Enabling will result in the content data and attribute values to be merged using the liquid template engine." />
            </div>
        </div>

        <div class="form-group">
            <label class="control-label">
                Filter
            </label>
            <asp:HiddenField ID="hfDataFilterId" runat="server" />
            <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>
        </div>


        <div class="row">
            <div class="col-md-6">
                <Rock:KeyValueList ID="kvlOrder" runat="server" Label="Order Items By"  KeyPrompt="Field" ValuePrompt="Direction" 
                    help="The field value and direction that items should be ordered by."/>
            </div>
            <div class="col-md-6">
                <fieldset>
                    <legend>Social Media Settings</legend>
                        
                        <Rock:RockCheckBox ID="cbSetRssAutodiscover" runat="server" Label="Set RSS Autodiscover Link" Text="Yes"
                                Help="Set's an RSS autodiscover link to the header section of the page." />

                        <Rock:RockDropDownList ID="ddlMetaDescriptionAttribute" runat="server" Label="Meta Description Attribute" 
                                Help="Attribute to use for the page's meta description." />

                        <Rock:RockDropDownList ID="ddlMetaImageAttribute" runat="server" Label="Meta Image Attribute" 
                                Help="Attribute to use for the page's og:image and image_src meta tags." />

                </fieldset>
            </div>

        </div>

        <div class="actions">
            <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>