<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockProperties.ascx.cs" Inherits="RockWeb.Blocks.Core.BlockProperties" %>

<div class="admin-dialog">

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-danger block-message error" />

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

                <div class="tabContent" >

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <asp:Panel ID="pnlBasicProperty" runat="server" Visible="true" >
                        <Rock:DataTextBox ID="tbBlockName" runat="server" SourceTypeName="Rock.Model.Block, Rock" PropertyName="Name" Required="true" />
                        <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                    </asp:Panel>

                    <asp:Panel ID="pnlAdvancedSettings" runat="server" Visible="false" >
                        <Rock:RockTextBox ID="tbCssClass" runat="server" Label="CSS Class" Help="An optional CSS class to include with this blocks containg div" />
                        <Rock:RockTextBox ID="tbCacheDuration" runat="server"  Label="Output Cache Duration (seconds)" Help="Number of seconds to cache the output of this block.  If a value is entered here, this block will only process data when the cache expires." />
                    </asp:Panel>

                </div>

            </asp:PlaceHolder>

        </ContentTemplate>
    </asp:UpdatePanel>

</div>

