using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _1 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sleep = Request.QueryString["sleep"];
        string msg = Request.QueryString["msg"];
        Response.Clear();
        Response.Write(get2(int.Parse(sleep), msg));
        Response.End();
    }

    public string get2(int sleep, string msg)
    {
        System.Threading.Thread.Sleep(sleep);
        return msg;
    }
}