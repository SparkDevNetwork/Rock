<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RequestFilterList.ascx.cs" Inherits="RockWeb.Blocks.Cms.RequestFilterList" %>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i> 
                    Request Filters
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gFilter" runat="server" OnApplyFilterClick="gfList_ApplyFilterClick" OnClearFilterClick="gfList_ClearFilterClick" >
                        <Rock:RockTextBox ID="tbNameFilter" runat="server" Label="Name" />
                        <Rock:RockCheckBox ID="cbShowInactive" runat="server" Checked="false" Label="Include Inactive" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="SiteName" HeaderText="Site" SortExpression="SiteName" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:DeleteField onClick="gList_DeleteClick"/>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
