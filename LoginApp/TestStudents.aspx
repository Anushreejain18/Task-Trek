<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestStudents.aspx.cs" Inherits="TestStudents" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Student List</title>
</head>
<body>
    <form id="form1" runat="server">
        <h2>Student List</h2>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="True"></asp:GridView>
    </form>
</body>
</html>
