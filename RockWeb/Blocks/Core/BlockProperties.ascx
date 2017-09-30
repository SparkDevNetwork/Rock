<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockProperties.ascx.cs" Inherits="RockWeb.Blocks.Core.BlockProperties" %>

<div class="admin-dialog">

    <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Danger" Dismissable="true" />

    <asp:UpdatePanel ID="upBlockProperties" runat="server">
        <ContentTemplate>

            <asp:PlaceHolder ID="phContent" runat="server">

                <ul class="nav nav-pills" >
                    <asp:Repeater ID="rptProperties" runat="server" >
                        <ItemTemplate >
                            <li class='<%# GetTabClass(Container.DataItem) %>'>
                                <asp:LinkButton ID="lbProperty" runat="server" Text='<%# Container.DataItem %>' OnClick="lbProperty_Click" CausesValidation="false">
                                </asp:LinkButton> 
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>

                <div class="margin-t-md" >

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <asp:Panel ID="pnlBasicProperty" runat="server" Visible="true" >
                        <Rock:DataTextBox ID="tbBlockName" runat="server" SourceTypeName="Rock.Model.Block, Rock" PropertyName="Name" Required="true" />
                        <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                    </asp:Panel>

                    <asp:Panel ID="pnlAdvancedSettings" runat="server" Visible="false" >
                        <Rock:RockTextBox ID="tbCssClass" runat="server" Label="CSS Class" Help="An optional CSS class to include with this block's containing div" />
                        <Rock:CodeEditor ID="cePreHtml" runat="server" Label="Pre-HTML" Help="HTML Content to render before the block <span class='tip tip-lava'></span>." EditorMode="Html" EditorTheme="Rock" EditorHeight="400" />
                        <Rock:CodeEditor ID="cePostHtml" runat="server" Label="Post-HTML" Help="HTML Content to render after the block <span class='tip tip-lava'></span>." EditorMode="Html" EditorTheme="Rock" EditorHeight="400" />
                        <Rock:RockTextBox ID="tbCacheDuration" runat="server"  Label="Output Cache Duration (seconds)" Help="Number of seconds to cache the output of this block.  If a value is entered here, this block will only process data when the cache expires." />
                        <asp:PlaceHolder ID="phAdvancedAttributes" runat="server"></asp:PlaceHolder>
                    </asp:Panel>

                    <asp:Panel ID="pnlAdditionalGridColumns" runat="server" Visible="false">
                        <asp:LinkButton ID="lbAddColumns" runat="server" CssClass="btn btn-default" Text="Add Column" OnClick="lbAddColumns_Click" />
                        <asp:Repeater ID="rptAdditionalGridColumns" runat="server" OnItemDataBound="rptAdditionalGridColumns_ItemDataBound">
                            <ItemTemplate>
                                <hr />
                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:RockTextBox ID="tbHeaderText" runat="server" Label="Header Text" />
                                        <Rock:RockTextBox ID="tbHeaderClass" runat="server" Label="Header Class" />
                                        <Rock:RockTextBox ID="tbItemClass" runat="server" Label="Item Class" />
                                    </div>
                                    <div class="col-md-7">
                                        <Rock:CodeEditor ID="ceLavaTemplate" runat="server" Label="Lava Template" EditorMode="Lava" EditorHeight="200" />
                                    </div>
                                    <div class="col-md-1">
                                    <asp:LinkButton ID="btnDeleteColumn" runat="server" CssClass="btn btn-danger btn-sm" OnClick="btnDeleteColumn_Click" ><i class="fa fa-times"></i></asp:LinkButton>    
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>

                </div>

            </asp:PlaceHolder>

        </ContentTemplate>
    </asp:UpdatePanel>

</div>

