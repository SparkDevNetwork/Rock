<%@ Control Language="C#" AutoEventWireup="true" CodeFile="InteractionChannelList.ascx.cs" Inherits="RockWeb.Blocks.Reporting.InteractionChannelList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-random"></i>
                    Interaction Channels
                </h1>
            </div>

            <div class="panel-body">
                <div class="list-panel">
                    <Rock:GridFilter ID="gfFilter" runat="server" OnApplyFilterClick="gfFilter_ApplyFilterClick" OnDisplayFilterValue="gfFilter_DisplayFilterValue">
                        <Rock:DefinedValuePicker ID="ddlMediumValue" runat="server" Label="Medium Type" Help="The Medium Type that identify the Content Channel." />
                        <Rock:RockCheckBox ID="cbIncludeInactive" runat="server" Label="Include Inactive Channels" />
                    </Rock:GridFilter>
                    <ul class="list-group">
                        <asp:Repeater ID="rptChannel" runat="server">
                            <ItemTemplate>
                                <asp:Literal ID="lContent" runat="server" Text='<%# Eval("ChannelHtml") %>'></asp:Literal>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
