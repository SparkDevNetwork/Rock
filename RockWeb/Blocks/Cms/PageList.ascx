<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageList.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:HiddenField runat="server" ID="hfSiteId" />
            <div id="pnlPages" runat="server">
                <h4>Pages</h4>
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                
                <div class="grid">
                    <Rock:GridFilter ID="gPagesFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlLayoutFilter" runat="server" Label="Layout" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gPages" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gPages_Edit">
                        <Columns>
                            <asp:BoundField DataField="InternalName" HeaderText="Name" SortExpression="InternalName" />
                            <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <asp:BoundField DataField="Layout" HeaderText="Layout" SortExpression="Layout" />
                            <Rock:DeleteField OnClick="gPages_Delete"/>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
