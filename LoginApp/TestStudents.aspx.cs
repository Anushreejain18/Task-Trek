using System;
using System.Data;
using System.Web.UI.WebControls; // Add this if missing

public partial class TestStudents : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            LoadStudents();
        }
    }

    private void LoadStudents()
    {
        StudentDAL dal = new StudentDAL();
        DataTable dt = dal.GetAllStudents();
        GridView1.DataSource = dt; // Ensure GridView1 matches the ID in the .aspx file
        GridView1.DataBind();
    }
}
