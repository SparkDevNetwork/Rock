<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Disc.ascx.cs" Inherits="Rockweb.Blocks.Crm.Disc" ViewStateMode="Enabled" EnableViewState="true" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlInstructions" runat="server">
            <asp:Literal ID="lInstructions" runat="server"></asp:Literal>

            <div class="actions">
                <asp:Button ID="btnStart" Text="Start" runat="server" CssClass="btn btn-primary" OnClick="btnStart_Click" />
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlQuestions" runat="server">
            <div class="container">
                <asp:Repeater ID="rQuestions" runat="server" OnItemDataBound="rQuestions_ItemDataBound">
                    <ItemTemplate>
                        <div class="js-disc-questions panel panel-default">
                            <div class="panel-heading">
                                Question <%# Container.ItemIndex + 1 %>
                            </div>
                            <div class="panel-body">
                                <div class="row">
                                    <div class="col-md-1">
                                        MORE
                                    </div>
                                    <div class="col-md-11">
                                        LESS
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-1">
                                        <Rock:RockRadioButtonList ID="rblMore" runat="server" />
                                    </div>
                                    <div class="col-md-11">
                                        <Rock:RockRadioButtonList ID="rblLess" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <div style="display: none" class="alert alert-danger" id="divError">
                    Please answer all questions before scoring.
                </div>
                <div class="actions">
                    <asp:Button ID="btnScoreTest" Text="Score Test" runat="server" CssClass="btn btn-primary" OnClick="btnScoreTest_Click" OnClientClick="if (!isComplete()) { return false; }" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlResults" runat="server">

            <h2>Results</h2>
            <p>Here are the result of your test:</p>

            <style>
                .disc-chart {
                    display: table;
                    table-layout: fixed;
                    width: 70%;
                    max-width: 500px;
                    height: 200px;
                    //background-image: linear-gradient(to top, rgba(0, 0, 0, 0.1) 2%, rgba(0, 0, 0, 0) 2%);
                    background-size: 100% 50px;
                    background-position: left top;
                    padding: 30px;
                    border: 1px solid #e1e1e8;
                    border-radius: 5px;
                }

                    .disc-chart li {
                        position: relative;
                        display: table-cell;
                        vertical-align: bottom;
                        height: 200px;
                    }

                    .disc-chart span {
                        margin: 0 1em;
                        display: block;
                        //background: rgba(209, 236, 250, 0.75);
                        animation: draw 1s ease-in-out;
                    }

                    .disc-chart span .discbar-primary {
                    }
                        .disc-chart span:before {
                            position: absolute;
                            left: 0;
                            right: 0;
                            top: 100%;
                            padding: 5px 1em 0;
                            display: block;
                            text-align: center;
                            content: attr(title);
                            word-wrap: break-word;
                        }

                @keyframes draw {
                    0% {
                        height: 0;
                    }
                }
            </style>

            <h3>Natural Behavior</h3>

            <ul class="disc-chart">
                <li>
                    <span id="discNaturalScore_D" runat="server" title="D" class="label-default discbar-d"></span>
                </li>
                <li>
                    <span id="discNaturalScore_I" runat="server" title="I" class="label-default discbar-i"></span>
                </li>
                <li>
                    <span id="discNaturalScore_S" runat="server" title="S" class="label-default discbar-s"></span>
                </li>
                <li>
                    <span id="discNaturalScore_C" runat="server" title="C" class="label-default discbar-c"></span>
                </li>
            </ul>

            <h3>Adaptive Behavior</h3>

            <ul class="disc-chart">
                <li>
                    <span id="discAdaptiveScore_D" runat="server" title="D" class="label-default discbar-d"></span>
                </li>
                <li>
                    <span id="discAdaptiveScore_I" runat="server" title="I" class="label-default discbar-i"></span>
                </li>
                <li>
                    <span id="discAdaptiveScore_S" runat="server" title="S" class="label-default discbar-s"></span>
                </li>
                <li>
                    <span id="discAdaptiveScore_C" runat="server" title="C" class="label-default discbar-c"></span>
                </li>
            </ul>

            <div class="actions">
                <asp:Button ID="btnRetakeTest" runat="server" Visible="false" Text="Retake Test" CssClass="btn btn-default" OnClick="btnRetakeTest_Click" />
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>

<!--
<div id="tabs">
    <ul id="navTabs" class="nav nav-tabs">
        <li class="active"><a data-toggle="tab" href="#instructions">Instructions</a></li>
        <li><a data-toggle="tab" href="#questions">Questions</a></li>
        <li><a data-toggle="tab" href="#results">Results</a></li>
    </ul>
    <div class="tab-content">

        <div id="questions" class="tab-pane">

        </div>
        <div id="results" class="tab-pane">
            <div class="row">
                <div class="col-md-3">
                    <table class="table table-bordered table-condensed table-striped">
                        <thead>
                            <tr class="info">
                                <th colspan="2">Adaptive Behavior
                                </th>
                                <th colspan="2">Natural Behavior
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>D:
                                </td>
                                <td>
                                    <asp:Label ID="lblABd" Text="" runat="server" />
                                </td>
                                <td>D:
                                </td>
                                <td>
                                    <asp:Label ID="lblNBd" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>I:
                                </td>
                                <td>
                                    <asp:Label ID="lblABi" Text="" runat="server" />
                                </td>
                                <td>I:
                                </td>
                                <td>
                                    <asp:Label ID="lblNBi" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>S:
                                </td>
                                <td>
                                    <asp:Label ID="lblABs" Text="" runat="server" />
                                </td>
                                <td>S:
                                </td>
                                <td>
                                    <asp:Label ID="lblNBs" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>C:
                                </td>
                                <td>
                                    <asp:Label ID="lblABc" Text="" runat="server" />
                                </td>
                                <td>C:
                                </td>
                                <td>
                                    <asp:Label ID="lblNBc" Text="" runat="server" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>

            <hr />
            <h2>Your Saved DISC Assessment Scores
            </h2>
            <div>
                Saved on:
                <asp:Label ID="lblLastAssessmentDate" Text="" runat="server" />
            </div>
            <div class="row">
                <div class="col-md-3">
                    <table class="table table-bordered table-condensed table-striped">
                        <thead>
                            <tr class="info">
                                <th colspan="2">Adaptive Behavior
                                </th>
                                <th colspan="2">Natural Behavior
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td>D:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevABd" Text="" runat="server" />
                                </td>
                                <td>D:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevNBd" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>I:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevABi" Text="" runat="server" />
                                </td>
                                <td>I:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevNBi" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>S:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevABs" Text="" runat="server" />
                                </td>
                                <td>S:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevNBs" Text="" runat="server" />
                                </td>
                            </tr>
                            <tr>
                                <td>C:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevABc" Text="" runat="server" />
                                </td>
                                <td>C:
                                </td>
                                <td>
                                    <asp:Label ID="lblPrevNBc" Text="" runat="server" />
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>
    -->
