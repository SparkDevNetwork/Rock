<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:panel ID="pnlContent" runat="server" CssClass="panel panel-block">
            
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bullhorn"></i> Content Channels
                </h1>
            </div>
            
            <div class="panel-body">
                <div class="grid grid-panel">

                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlType" runat="server" Label="Type" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gContentChannels" runat="server" EmptyDataText="No Channels Found" RowItemText="Channel" AllowSorting="true" TooltipField="Description" OnRowSelected="gContentChannels_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Channel" SortExpression="Name" />
                            <Rock:RockBoundField DataField="ContentChannelType" HeaderText="Type" SortExpression="ContentChannelType" />
                            <asp:HyperLinkField DataNavigateUrlFields="ChannelUrl" DataNavigateUrlFormatString="{0}" DataTextField="ChannelUrl" SortExpression="ChannelUrl" HeaderText="Channel Url" />
                            <Rock:BadgeField InfoMin="1" DataField="TotalItems" HeaderText="Total Items" SortExpression="TotalItems" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:BadgeField InfoMin="1" DataField="ActiveItems" HeaderText="Active Items" SortExpression="ActiveItems" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                            <Rock:DateField DataField="ItemLastCreated" HeaderText="Last Item Created" SortExpression="ItemLastCreated" FormatAsElapsedTime="true" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gContentChannels_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:panel>

    </ContentTemplate>
</asp:UpdatePanel>
