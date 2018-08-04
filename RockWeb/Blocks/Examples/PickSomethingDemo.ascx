<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PickSomethingDemo.ascx.cs" Inherits="RockWeb.Blocks.Examples.PickSomethingDemo" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <h1>Picker in Modal</h1>
        button container stuff begin
        <Rock:ItemFromBlockPicker Id="pModalExample" runat="server" BlockTypePath="~/Blocks/Utility/PickSomething.ascx" SelectedValue="Cheese" OnSelectItem="pModalExample_SelectItem" CssClass="btn btn-xs btn-default" ShowInModal="true" ModalSaveButtonText="Select"  >
           Button Text is {{ SelectedText }}
        </Rock:ItemFromBlockPicker>
        button container stuff end

        <Rock:RockLiteral ID="lModalPickedResult" runat="server" />


        <h1>Picker inline</h1>
        <Rock:ItemFromBlockPicker Id="pInlineExample" runat="server" BlockTypePath="~/Blocks/Utility/PickSomething.ascx" SelectedValue="Lettuce" OnSelectItem="pInlineExample_SelectItem" CssClass="" ShowInModal="false"  />



        <h1>Picker HtmlFileBrowser</h1>
        <Rock:ItemFromBlockPicker Id="pPickFromFileBrowser" runat="server" BlockTypePath="~/Blocks/Utility/HtmlEditorFileBrowser.ascx" CssClass="btn btn-xs btn-default" ShowInModal="true" ModalSaveButtonText="Select" OnSelectItem="pPickFromFileBrowser_SelectItem"  >
            Click Me
        </Rock:ItemFromBlockPicker>

        <Rock:RockLiteral ID="lFileBrowserPickedResult" runat="server" />



        <Rock:ItemFromBlockPicker Id="ItemFromBlockPicker1" runat="server" BlockTypePath="~/Blocks/Core/BinaryFileTypeList.ascx" CssClass="" ShowInModal="false"  />
        
    </ContentTemplate>
</asp:UpdatePanel>
