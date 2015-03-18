<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MergeTemplateEntry.ascx.cs" Inherits="RockWeb.Blocks.Core.MergeTemplateEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i> Blank Detail Block</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <div class="panel-body">

                <div class="alert alert-info">
                    <h4>Stark Template Block</h4>
                    <p>This block serves as a starting point for creating new blocks. After copy/pasting it and renaming the resulting file be sure to make the following changes:</p>

                    <strong>Changes to the Codebehind (ascx.cs) File</strong>
                    <ul>
                        <li>Update the namespace to match your directory</li>
                        <li>Update the class name</li>
                        <li>Fill in the DisplayName, Category and Description attributes</li>
                    </ul>

                    <strong>Changes to the Usercontrol (.ascx) File</strong>
                    <ul>
                        <li>Update the Inherhits to match the namespace and class file</li>
                        <li>Remove this text... unless you really like it...</li>
                    </ul>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
