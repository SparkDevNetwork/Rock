<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditLabel.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Config.EditLabel" %>

<asp:UpdatePanel ID="upnlDevice" runat="server">
    <ContentTemplate>

        <div class="panel panel-block" >

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-print"></i> <asp:Literal ID="lTitle" runat="server" Text="Edit Label"></asp:Literal></h1>
            </div>

            <div class="panel-body">

                <asp:HiddenField ID="hfBinaryFileId" runat="server" />
                <asp:Panel ID="pnlOpenFile" runat="server" Visible="false" class="row">
                    <div class="col-md-6">
                        <Rock:RockControlWrapper ID="rcwLabelFile" runat="server" Label="Open Label File">
                            <Rock:ButtonDropDownList ID="ddlLabel" runat="server" />
                            <asp:LinkButton ID="lbOpenLabel" runat="server" Text="Open" CssClass="btn btn-default margin-l-sm" OnClick="btnOpen_Click" />
                        </Rock:RockControlWrapper>
                    </div>
                    <div class="col-md-6">
                    </div>
                </asp:Panel>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:CodeEditor ID="ceLabel" runat="server" Label="Label Contents" EditorMode="Text" EditorTheme="Rock" EditorHeight="500" />
                        <div class="p-t-md">
                            <Rock:RockCheckBox ID="cbForceUTF8" runat="server" Checked="true" Label="Force Expanded Character set (UTF-8)"
                                Help="By default Rock will force uploaded and saved ZPL templates to use UTF-8 encoding. This allows characters from any language to print on label for ZPL printers version .14 and up. Changes ^CI0 to ^CI28. Uncheck this box to save the label as-is." />
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" Text="Save" Visible="false" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" Visible="false" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                        </div>
                    </div>
                    <div class="col-md-6">

                        <Rock:RockControlWrapper ID="rcwPrint" runat="server" Label="Test Print to Device">
                            <Rock:ButtonDropDownList ID="ddlDevice" runat="server" />
                            <asp:LinkButton ID="btnPrint" runat="server" Text="Print" CssClass="btn btn-default margin-l-sm" OnClick="btnPrint_Click" />
                        </Rock:RockControlWrapper>

                        <div class="well">
                            <h4>Label Viewer</h4>
                            <div class="form-inline">
                                <Rock:RockDropDownList ID="ddlPrintDensity" runat="server" Label="Print Density">
                                    <asp:ListItem Text="6 dpmm (152 dpi)" Value="6"></asp:ListItem>
                                    <asp:ListItem Selected="True" Text="8 dpmm (203 dpi)" Value="8"></asp:ListItem>
                                    <asp:ListItem Text="12 dpmm (300 dpi)" Value="12"></asp:ListItem>
                                    <asp:ListItem Text="24 dpmm (600 dpi)" Value="24"></asp:ListItem>
                                </Rock:RockDropDownList>
                                <Rock:RockControlWrapper ID="rcwLabelSize" runat="server" Label="Label Size" FormGroupCssClass="margin-l-md">
                                    <Rock:NumberBox ID="nbLabelWidth" runat="server" CssClass="input-width-xs" Text="4" NumberType="Double"></Rock:NumberBox> X
                                    <Rock:NumberBox ID="nbLabelHeight" runat="server" CssClass="input-width-xs" Text="2" NumberType="Double"></Rock:NumberBox>
                                </Rock:RockControlWrapper>
                                <Rock:RockControlWrapper ID="rcwShowLabel" runat="server" Label="Show Label" Help="(0 = first label, 1 = second label, etc.)" FormGroupCssClass="margin-l-md">
                                    <Rock:NumberBox ID="nbShowLabel" runat="server" CssClass="input-width-xs" Text="0" NumberType="Integer" ></Rock:NumberBox>
                                    <asp:LinkButton ID="lbRedraw" runat="server" Text="Redraw" CssClass="btn btn-default margin-l-lg" OnClick="btnRedraw_Click" />
                                </Rock:RockControlWrapper>
                            </div>
                            <div class="margin-t-md">
                                <asp:Image ID="imgLabelary" runat="server" Style="display: block; max-height: 600px; max-width: 400px"  />
                            </div>
                        </div>


                    </div>
                </div>


            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
