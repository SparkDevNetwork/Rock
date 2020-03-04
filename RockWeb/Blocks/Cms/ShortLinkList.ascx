﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ShortLinkList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ShortLinkList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-link"></i> Shortened Links</h1>
                </div>
                <div class="panel-body">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="gfShortLink" runat="server">
                            <Rock:RockDropDownList ID="ddlSite" runat="server" Label="Site" />
                            <Rock:RockTextBox ID="txtToken" runat="server" Label="Token" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gShortLinks" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gShortLinks_Edit">
                            <Columns>
                                <Rock:RockBoundField DataField="ShortLink" HeaderText="Shortened URL" SortExpression="ShortLink" />
                                <Rock:RockBoundField DataField="SiteName" HeaderText="Site" SortExpression="SiteName" ColumnPriority="Tablet" />
                                <Rock:RockBoundField DataField="Token" HeaderText="Token" SortExpression="Token" />
                                <Rock:RockTemplateFieldUnselected HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand" ColumnPriority="Tablet">
                                    <ItemTemplate>
                                        <button
                                            data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copy to Clipboard"
                                            class="btn btn-sm btn-square btn-default js-copy-clipboard" data-clipboard-text='<%# Eval( "ShortLink" ) %>'
                                            onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy to Clipboard');return false;">
                                            <i class='fa fa-clipboard'></i>
                                        </button>
                                    </ItemTemplate>
                                </Rock:RockTemplateFieldUnselected>
                                <Rock:DeleteField OnClick="DeleteShortLink_Click" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
