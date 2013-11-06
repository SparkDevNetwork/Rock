<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockProperties.ascx.cs" Inherits="RockWeb.Blocks.Core.BlockProperties" %>

<div class="admin-dialog">

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-danger block-message error" />

    <asp:UpdatePanel ID="upBlockProperties" runat="server">
        <ContentTemplate>

            <asp:PlaceHolder ID="phContent" runat="server">

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger block-message error" />

                <fieldset>
                    <legend>Settings</legend>
                    <Rock:DataTextBox ID="tbBlockName" runat="server" SourceTypeName="Rock.Model.Block, Rock" PropertyName="Name" Required="true" />
                    <asp:PlaceHolder ID="phAttributes" runat="server"></asp:PlaceHolder>
                    <fieldset>
                        <legend>Advanced Settings</legend>
                        <Rock:DataTextBox ID="tbCacheDuration" runat="server" SourceTypeName="Rock.Model.Block, Rock" PropertyName="OutputCacheDuration" Label="Output Cache Duration" />
                    </fieldset>
                </fieldset>

                <asp:ValidationSummary ID="valSummaryBottom" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger block-message error" />

            </asp:PlaceHolder>
        </ContentTemplate>

    </asp:UpdatePanel>

</div>

