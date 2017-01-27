<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageZoneBlocksEditor.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageZoneBlocksEditor" %>
<asp:UpdatePanel ID="upPages" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfPageId" runat="server" />

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lZoneIcon" runat="server" />
                    <asp:Literal ID="lZoneTitle" runat="server" /></h1>

                <div class="pull-right">
                    <Rock:RockDropDownList ID="ddlZones" runat="server" Label="" AutoPostBack="true" OnSelectedIndexChanged="ddlZones_SelectedIndexChanged" />
                </div>
            </div>
            <div class="panel-body">
                <legend>Blocks From Layout</legend>
                <asp:Repeater ID="rptLayoutBlocks" runat="server" OnItemDataBound="rptPageOrLayoutBlocks_ItemDataBound">
                    <ItemTemplate>
                        <asp:Panel ID="pnlBlockEditWidget" runat="server" CssClass="panel panel-widget">
                        </asp:Panel>
                    </ItemTemplate>
                </asp:Repeater>

                <hr />

                <legend>Blocks From Page</legend>
                <asp:Repeater ID="rptPageBlocks" runat="server" OnItemDataBound="rptPageOrLayoutBlocks_ItemDataBound">
                    <ItemTemplate>
                        <asp:Panel ID="pnlBlockEditWidget" runat="server" CssClass="panel panel-widget">
                        </asp:Panel>
                    </ItemTemplate>
                </asp:Repeater>

                <div class="actions ">
                    <div class="pull-right">
                        <asp:LinkButton ID="btnAddBlock" runat="server" ToolTip="Add Block" Text="<i class='fa fa-plus'></i>" CssClass="btn btn-default" OnClick="btnAddBlock_Click" />
                    </div>

                </div>
            </div>

            <%--  This will hold blocks that need to be added to the page so that Custom Admin actions will work --%>
            <%-- Display -9999 offscreen. This will hopefully hide everything except for any modals that get shown with the Custom Action --%>
            <asp:Panel ID="pnlBlocksHolder" runat="server" style="position:absolute; left:-9999px">

            </asp:Panel>

        </asp:Panel>

        <Rock:ModalDialog ID="mdBlockMove" runat="server" ValidationGroup="vgBlockMove" OnSaveClick="mdBlockMove_SaveClick" Title="Move Block">
            <Content>
                <asp:HiddenField ID="hfBlockMoveBlockId" runat="server" />
                <legend>New Location</legend>
                <Rock:RockDropDownList ID="ddlMoveToZoneList" runat="server" Label="Zone" />
                <Rock:RockRadioButtonList ID="cblBlockMovePageOrLayout" runat="server" Label="Parent" RepeatDirection="Horizontal" />

            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdAddBlock" runat="server" ValidationGroup="vgAddBlock" OnSaveClick="mdAddBlock_SaveClick" Title="Add Block">
            <Content>
                <Rock:RockTextBox ID="tbNewBlockName" runat="server" Label="Name" />
                <Rock:RockDropDownList ID="ddlBlockType" runat="server" Label="Type" AutoPostBack="true" OnSelectedIndexChanged="ddlBlockType_SelectedIndexChanged" />
                
                <asp:LinkButton ID="btnHtmlContent" runat="server" Text="HTML Content" CssClass="btn btn-default" OnClick="btnHtmlContent_Click" />
                <Rock:RockRadioButtonList ID="cblAddBlockPageOrLayout" runat="server" Label="Parent" RepeatDirection="Horizontal" />


            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>

