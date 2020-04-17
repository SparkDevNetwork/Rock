<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionImport.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.Finance.TransactionImport" %>

<style>
    .mb0 {
        margin-bottom: 0px;
    }
</style>

<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-money" aria-hidden="true"> Transaction Import Tool</i></h3>
                </h1>
            </div>
            <div class="panel-body">
                <div class="col-md-12">
                    <asp:Literal ID="lMessages" runat="server"></asp:Literal>
                </div>
                <asp:Panel ID="pEntry" runat="server" Visible="true">
                    <div class="col-md-4">
                        <p>Please use the upload button below to upload a CSV file.</p>
                        <Rock:FileUploader runat="server" 
                            ID="FileUpload" OnFileUploaded="FileUpload_FileUploaded" 
                            BorderColor="Red" BackColor="Red" Required="true" 
                            ShowDeleteButton="true" />
                    </div>
                </asp:Panel>
                <asp:Panel Id="pConfirmation" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-8">
                            <h2>Pending Transactions</h2>
                        </div>
                        <div class="col-md-4">
                            <div class="well">
                                <div class="btn-toolbar">
                                    <Rock:BootstrapButton ID="btnImport" runat="server" CssClass="btn btn-primary" OnClick="btnImport_Click">Import</Rock:BootstrapButton>                                
                                </div>
                            </div>
                        </div>
                        <div class="col-md-12">
                            <table class="table">
                                <thead>
                                    <tr>
                                        <th>Current Status</th>
                                        <th>Imported Name</th>
                                        <th>Matched Person Name</th>
                                        <th>Amount</th>
                                        <th>Account</th>
										<th>Matched Account</th>
                                        <th>Date Time</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rTransactions" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td style="background-color: #edeae6;"><strong><%# Eval("CurrentStatus") %></strong></td>
                                                <td><%# Eval("ImportedName") %></td>
                                                <td><%# Eval("MatchedPersonName") %></td>
                                                <td><%# string.Format("{0:0.00}", Eval("Amount") ) %></td>
                                                <td><%# Eval("FundDescription") %></td>
												<td><%# Eval("MatchedAccount") %></td>
                                                <td><%# Eval("PostDate") %></td>
                                              
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pResults" runat="server" Visible="false">
                    <div class="row">
                        <div class="col-md-8">
                            <h2>Import Summary</h2>
                            <div runat="server" id="lStatus"></div>
                        </div>
                        <div class="col-md-4">
                            <div class="well">
                                <div class="btn-toolbar">
                                    <a href="#" ID="btnViewBatch" runat="server" class="btn btn-primary">View Batch</a>                                
                                </div>
                            </div>
                        </div>
                        <div class="col-md-12">
                            <div class="alert alert-success">
                                <div class="row">
                                    <div class="col-md-3">
                                        <h4 class="progress-label">Matching Contributors</h4>
                                    </div>
                                    <div class="col-md-2" runat="server" id="lMatchingRatio">
                                        75/100
                                    </div>
                                    <div class="col-md-7">
                                         <div class="progress mb0">
                                            <div class="progress-bar progress-bar-success" style="width: 75%;" role="progressbar" runat="server" id="progMatching"></div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                             <div class="alert alert-info">
                                <div class="row">
                                   <div class="col-md-3">
                                        <h4 class="progress-label">Non-Matching Contributors</h4>
                                    </div>
                                    <div class="col-md-2" runat="server" id="lNonMatchingRatio">
                                        24/100
                                    </div>
                                    <div class="col-md-7">
                                         <div class="progress mb0">
                                            <div class="progress-bar progress-bar-info" style="width: 24%;" role="progressbar" runat="server" id="progNotMatching"></div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                             <div class="alert alert-danger">
                                <div class="row">
                                    <div class="col-md-3">
                                        <h4 class="progress-label">Not Imported</h4>
                                    </div>
                                    <div class="col-md-2" runat="server" id="lNotImportedRatio">
                                        1/100
                                    </div>
                                    <div class="col-md-7">
                                         <div class="progress mb0">
                                            <div class="progress-bar progress-bar-danger" style="width: 1%;" role="progressbar" runat="server" id="progExisting"></div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>