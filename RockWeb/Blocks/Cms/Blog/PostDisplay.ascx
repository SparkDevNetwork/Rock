<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PostDisplay.ascx.cs" Inherits="Rock.Web.Blocks.Cms.Blog.PostDisplay" %>

<article>
    <header>
        <h1><asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
    </header>

    <asp:Literal ID="lContents" runat="server"></asp:Literal>
    
    <footer>
        <div class="post-details"><asp:Literal ID="lPostDetails" runat="server"></asp:Literal></div>

        <div class="post-comments"><asp:Literal ID="lPostComments" runat="server"></asp:Literal></div>
    </footer>
</article>

<asp:Panel ID="pnlAddComment" runat="server">
    <fieldset>
        <legend>Leave a Comment</legend>
        <ol>
            <li>
                <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName">Name:
                    <asp:TextBox ID="txtName" runat="server"></asp:TextBox></asp:Label>
                
                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName" 
                        CssClass="failureNotification" ErrorMessage="Name is required." ToolTip="Name is required." 
                        ValidationGroup="CommentValidationGroup">*</asp:RequiredFieldValidator>
            </li>
            <li>
                <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail">Email:
                    <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox></asp:Label>
                
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" 
                        CssClass="failureNotification" ErrorMessage="Email address is required." ToolTip="Email address is required." 
                        ValidationGroup="CommentValidationGroup">*</asp:RequiredFieldValidator>
            </li>
            <li>
                <asp:TextBox ID="txtComment" Rows="12" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvComment" runat="server" ControlToValidate="txtComment" 
                        CssClass="failureNotification" ErrorMessage="A comment is required." ToolTip="A comment is required." 
                        ValidationGroup="CommentValidationGroup">*</asp:RequiredFieldValidator>
            </li>
        </ol>
    </fieldset>

</asp:Panel>

