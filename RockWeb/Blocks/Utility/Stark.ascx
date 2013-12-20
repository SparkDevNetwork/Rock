<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Stark.ascx.cs" Inherits="RockWeb.Blocks.Utility.Stark" %>

<asp:UpdatePanel ID="upStark" runat="server">
    <ContentTemplate>

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

    </ContentTemplate>
</asp:UpdatePanel>
