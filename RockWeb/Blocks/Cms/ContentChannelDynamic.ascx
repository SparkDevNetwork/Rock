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

        <asp:HiddenField ID="hfChannelGuid" runat="server" />

        <div class="row">
            <div class="col-md-5">
                <Rock:RockDropDownList ID="ddlChannel" runat="server" Label="Channel" 
                    DataTextField="Name" DataValueField="Guid" AutoPostBack="true" OnSelectedIndexChanged="ddlChannel_SelectedIndexChanged"
                    help="The channel to display items from." />
            </div>
            <div class="col-md-7">
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <Rock:CodeEditor ID="ceQuery" runat="server" EditorHeight="400" EditorMode="Liquid" EditorTheme="Rock" Label="Format"
                    Help="The template to use when formatting the list of items." />
            </div>
        </div>

        <div class="row">
            <div class="col-md-5">
                <Rock:RockCheckBox ID="cbDebug" runat="server" Label="Enable Debug" Text="Yes"
                    Help="Enabling debug will display the fields of the first 5 items to help show you wants available for your template." />
                <Rock:NumberBox ID="nbCount" runat="server" Label="Number of Items"
                    help="The maximum number of items to display per page." />
                <Rock:NumberBox ID="nbCacheDuration" runat="server" Label="Cache Duration"
                    help="The number of seconds to cache the content for." />
            </div>
            <div class="col-md-7">
                <Rock:RockCheckBoxList ID="cblStatus" runat="server" Label="Status" RepeatDirection="Horizontal"
                    help="Include items with the following status." />
                <Rock:KeyValueList ID="kvlFilter" runat="server" Label="Attribute Filters"  KeyPrompt="Field" ValuePrompt="Equal To" 
                    help="The field values to filter items by."/>
                <Rock:KeyValueList ID="kvlOrder" runat="server" Label="Order Items By"  KeyPrompt="Field" ValuePrompt="Direction" 
                    help="The field value and direction that items should be ordered by."/>
            </div>
        </div>

        <div class="actions">
            <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
        </div>

    </asp:Panel>

</ContentTemplate>
</asp:UpdatePanel>