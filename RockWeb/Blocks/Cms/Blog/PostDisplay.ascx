<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PostDisplay.ascx.cs" Inherits="RockWeb.Blocks.Cms.Blog.PostDisplay" %>

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
                <asp:Label ID="lblName" runat="server" AssociatedControlID="txtName">Name:</asp:Label>
                    <asp:TextBox ID="txtName" runat="server"></asp:TextBox>
                
                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName" 
                        CssClass="failureNotification" ErrorMessage="Name is required." ToolTip="Name is required." 
                        ValidationGroup="CommentValidationGroup">*</asp:RequiredFieldValidator>
            </li>
            <li>
                <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail">Email:</asp:Label>
                    <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox>
                
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" 
                        CssClass="failureNotification" ErrorMessage="Email address is required." ToolTip="Email address is required." 
                        ValidationGroup="CommentValidationGroup">*</asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="regexEmail" runat="server" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" 
                        ControlToValidate="txtEmail" ValidationGroup="CommentValidationGroup" ErrorMessage="Invalid email address." ToolTip="Invalid email address.">*</asp:RegularExpressionValidator>

            </li>
            <li>
                <asp:TextBox ID="txtComment" TextMode="MultiLine" Rows="12" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvComment" runat="server" ControlToValidate="txtComment" 
                        CssClass="failureNotification" ErrorMessage="A comment is required." ToolTip="A comment is required." 
                        ValidationGroup="CommentValidationGroup">*</asp:RequiredFieldValidator>
            </li>
            <li>
                <recaptcha:RecaptchaControl ID="rcComment"
                    Theme="clean"
                    PublicKey="6Lf1A8QSAAAAAM1_QtF5xyxgo8IwtA2WJDJDu0D0" PrivateKey="6Lf1A8QSAAAAAIilSEpreSmMP__Bad8EgT6kB81W"
                    runat="server"/>
                <asp:CustomValidator id="valRecaptcha" runat="server" ValidationGroup="CommentValidationGroup"
                    Text="Phases do not match.  Please try again." OnServerValidate="valRecaptcha_Validate" ToolTip="Phases do not match.  Please try again." />
            </li>
            <li>
                <asp:Button ID="btnSubmitComment" Text="Submit Comment" runat="server" OnClick="btnSubmitComment_Click" ValidationGroup="CommentValidationGroup"/>
            </li>
        </ol>
        
    </fieldset>
    <asp:ValidationSummary ID="CommentValidationGroup" runat="server" CssClass="failureNotification" 
                ValidationGroup="CommentValidationGroup"/>
</asp:Panel>



