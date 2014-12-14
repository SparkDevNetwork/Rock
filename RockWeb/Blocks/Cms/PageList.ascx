<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageList.ascx.cs" Inherits="RockWeb.Blocks.Cms.PageList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">
            <asp:HiddenField runat="server" ID="hfSiteId" />
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-files-o"></i> Page List</h1>
            </div>
            <div class="panel-body">

                <div id="pnlPages" runat="server">
                
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gPagesFilter" runat="server">
                            <Rock:RockDropDownList ID="ddlLayoutFilter" runat="server" Label="Layout" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gPages" runat="server" DisplayType="Full" AllowSorting="true" >
                            <Columns>
                                <Rock:RockBoundField DataField="InternalName" HeaderText="Name" SortExpression="InternalName" />
                                <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                                <Rock:RockBoundField DataField="Layout" HeaderText="Layout" SortExpression="Layout" />
                                <Rock:DeleteField OnClick="gPages_Delete"/>
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
