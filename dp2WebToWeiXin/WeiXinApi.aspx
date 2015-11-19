<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WeiXinApi.aspx.cs" Inherits="dp2WebToWeiXin.WeiXinApi" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="btnSend" runat="server" Text="send" OnClick="btnSend_Click" />
        <br />
        <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" Rows="5" Height="126px"  Width="521px"></asp:TextBox>
        <br />
        <br />
        result:<br />
        <asp:TextBox ID="txtResult" runat="server" Height="255px" Width="860px" TextMode="MultiLine" Rows="5"></asp:TextBox>
        <br />
    </div>
    </form>
</body>
</html>
