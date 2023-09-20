<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BookView.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.Rockumentation.BookView" %>
<%@ Register Namespace="com.blueboxmoon.Rockumentation.UI" Assembly="com.blueboxmoon.Rockumentation" TagPrefix="RM" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:Panel ID="pnlView" runat="server">
                <asp:Literal ID="ltPageHeader" runat="server" />
                <asp:Literal ID="ltPanelHeader" runat="server" />

                <div class="doc-container">
                    <div class="left-container">
                        <div class="book-toc">
                            <div class="book-toc-container">
                                <asp:Panel ID="pnlVersionPicker" runat="server" CssClass="dropdown version-dropdown">
                                    <button class="btn btn-sm btn-default dropdown-toggle" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
                                        Version <asp:Literal ID="lCurrentVersion" runat="server" /> <span class="caret"></span>
                                    </button>
                                    <ul class="dropdown-menu">
                                        <asp:Repeater ID="rptrVersionPicker" runat="server">
                                            <ItemTemplate>
                                                <li>
                                                    <a href='<%# Eval( "Url" ) %>'><%# Eval( "Title" ) %></a>
                                                </li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                </asp:Panel>
                
                                <asp:Panel ID="pnlSearch" runat="server" CssClass="margin-b-sm js-search-field">
                                </asp:Panel>
                
                                <asp:Literal ID="lTocContent" runat="server" />
                            </div>
                        </div>
                    </div>
                
                    <div class="center-container">
                        <asp:Panel ID="pnlCanEdit" runat="server" Visible="true" CssClass="edit-bar-container">
                            <div class="edit-bar">
                                <div class="btn-group">
                                    <a id="lbCopySlug" runat="server" class="btn btn-default btn-xs js-copy-slug" title="Copy article slug" onclick="return false">
                                        <i class="fa fa-clipboard"></i>
                                    </a>
                
                                    <a id="lbSecurity" runat="server" class="btn btn-default btn-xs" title="Edit the security of this article">
                                        <i class="fa fa-lock"></i>
                                    </a>
                
                                    <asp:LinkButton ID="lbChildren" runat="server" CssClass="btn btn-default btn-xs" OnClick="lbChildren_Click" ToolTip="Child articles">
                                        <i class="fa fa-sitemap"></i>
                                    </asp:LinkButton>
                
                                    <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-default btn-xs" OnClick="lbEdit_Click" ToolTip="Edit this article">
                                        <i class="fa fa-pencil"></i>
                                    </asp:LinkButton>
                                </div>
                
                                <span class="btn btn-xs"><i class="fa fa-cog"></i></span>
                            </div>
                        </asp:Panel>
                
                        <asp:Panel ID="Panel1" runat="server">
                            <asp:Literal ID="lArticleContent" runat="server" />
                        </asp:Panel>
                    </div>
                
                    <div class="right-container">
                        <nav id="navArticle" runat="server" class="article-toc" data-spy="affix"></nav>
                    </div>
                </div>

                <div class="end-of-doc-marker" style="height: 0px;"></div>

                <asp:Literal ID="ltPanelFooter" runat="server" />
                <asp:Literal ID="ltPageFooter" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlEdit" runat="server" CssClass="article-edit-panel">
                <asp:HiddenField ID="hfEditId" runat="server" />
            
                <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" ValidationGroup="Edit" Required="true" />

                <Rock:RockDropDownList ID="ddlParentArticle" runat="server" Label="Parent Article" ValidationGroup="Edit" Required="true" />
            
                <Rock:AttributeValuesContainer ID="avcEditAttributes" runat="server" />

                <RM:MarkdownEditor ID="mdContent" runat="server" Visible="false" />
                <RM:StructuredEditor ID="seContent" runat="server" Visible="false" />
            
                <div class="actions margin-t-md">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" ValidationGroup="Edit" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false" />
                </div>
            </asp:Panel>

            <Rock:ModalDialog ID="mdlChildren" runat="server" Title="Child Articles">
                <Content>
                    <asp:Panel ID="pnlChildrenList" runat="server">
                        <Rock:Grid ID="gArticle" runat="server" RowItemText="Article" OnGridRebind="gArticle_GridRebind" OnGridReorder="gArticle_GridReorder" OnRowSelected="gArticle_RowSelected" OnRowDataBound="gArticle_RowDataBound">
                            <Columns>
                                <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                                <Rock:RockBoundField DataField="Slug" HeaderText="Slug" SortExpression="Slug" />
                                <Rock:ReorderField />
                                <Rock:DeleteField OnClick="gArticle_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </asp:Panel>

                    <asp:Panel ID="pnlChildrenAdd" runat="server">
                        <Rock:RockTextBox ID="tbChildrenAddTitle" runat="server" Label="Title" Required="true" ValidationGroup="AddArticle" />

                        <div class="actions margin-t-md">
                            <asp:LinkButton ID="lbChildrenAddSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbChildrenAddSave_Click" ValidationGroup="AddArticle" />
                            <asp:LinkButton ID="lbChildrenAddCancel" runat="server" CssClass="btn btn-link" Text="Cancel" OnClick="lbChildrenAddCancel_Click" CausesValidation="false" />
                        </div>

                    </asp:Panel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
