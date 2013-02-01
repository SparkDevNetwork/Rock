<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DynamicQuery.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DynamicQuery" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>

        <div class="dynamic-report">
            
            <div class="dynamic-report-details" >
                <asp:Literal ID="lDesc" runat="server"></asp:Literal>
                <asp:PlaceHolder ID="phEdit" runat="server">
                    <p><a id="edit-report" class="report-add btn" onclick="return toggleDetails();" ><i class="icon-edit"></i></a></p>
                </asp:PlaceHolder>
            </div>

            <asp:PlaceHolder ID="phDetails" runat="server">
                
                <div class="dynamic-report-details row-fluid well" style="display: none;">
                    
                    <div class="span6">
                        <fieldset>
                            <Rock:LabeledTextBox ID="tbName" runat="server" LabelText="Page Name" CssClass="input-large" />
                            <Rock:LabeledTextBox ID="tbDesc" runat="server" LabelText="Page Description" TextMode="MultiLine" Rows="2" CssClass="input-xlarge" />
                            <Rock:LabeledTextBox ID="tbQuery" runat="server" LabelText="Query" TextMode="MultiLine" Rows="5" CssClass="input-xlarge"/>
                        </fieldset>
                    </div>

                    <div class="span6">
                        <fieldset>
                            <Rock:LabeledTextBox ID="tbUrlMask" runat="server" LabelText="Selection Url" CssClass="input-large" />
                            <Rock:LabeledTextBox ID="tbParams" runat="server" LabelText="Parameters" TextMode="MultiLine" Rows="2" CssClass="input-xlarge" />

                            <div class="control-group">
                                <div class="control-label">
                                    <asp:DropDownList ID="ddlHideShow" runat="server" CssClass="input-small">
                                        <asp:ListItem Text="Hide" Value="False"></asp:ListItem>
                                        <asp:ListItem Text="Show" Value="True"></asp:ListItem>
                                    </asp:DropDownList>
                                    Columns
                                </div>
		                        <div class="controls">
                                    <asp:TextBox ID="tbHide" runat="server" TextMode="MultiLine" Rows="2" CssClass="input-xlarge" />
                                </div>
                            </div>
                        </fieldset>
                    </div>

                    <div class="actions span12">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <a onclick="return toggleDetails();" class="btn">Cancel</a>
                    </div>

                </div>

            </asp:PlaceHolder>

            <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="alert alert-error">
                <strong>Query Error!</strong> <asp:Literal ID="lError" runat="server"></asp:Literal>
            </asp:Panel>

            <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results"></Rock:Grid>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
