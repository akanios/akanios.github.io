using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Owin;
using PurchasingRequisitionApp.Models;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace PurchasingRequisitionApp.Admin
{
    public partial class AdminPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM AspNetRoles WHERE Name IN ('Business & Accounting Chair', 'Education Chair', 'Extended Learning Chair', 'Humanities Chair', 'Nursing Chair', 'STEM Chair', 'admin', 'Chief Financial Officer')"))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = con;
                        con.Open();
                        ChooseDivision.DataSource = cmd.ExecuteReader();
                        ChooseDivision.DataTextField = "Name";
                        ChooseDivision.DataValueField = "Name";
                        ChooseDivision.DataBind();
                        con.Close();
                    }
                }
                ChooseDivision.Items.Insert(0, new ListItem("--Select Division--", "0"));
            }
        }

        protected void CreateUser_Click(object sender, EventArgs e)
        {
            var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var signInManager = Context.GetOwinContext().Get<ApplicationSignInManager>();



            var user = new ApplicationUser() { UserName = Email.Text, Email = Email.Text, FirstName = FirstName.Text, LastName = LastName.Text };
            IdentityResult result = manager.Create(user, Password.Text);
            if (result.Succeeded)
            {
                var context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(context));
                result = userManager.AddToRole(user.Id, ChooseDivision.SelectedValue);

                string code = manager.GenerateEmailConfirmationToken(user.Id);
                string callbackUrl = IdentityHelper.GetUserConfirmationRedirectUrl(code, user.Id, Request);
                manager.SendEmail(user.Id, "Your Requisition Account has been Created", "Hello,\n Your new requisiton account has been created.\n\n  To confirm please click <a href=\"" + callbackUrl + "\">here</a>.");

                if (user.EmailConfirmed)
                {
                    signInManager.SignIn(user, isPersistent: false, rememberBrowser: false);
                    IdentityHelper.RedirectToReturnUrl(Request.QueryString["ReturnUrl"], Response);
                }
                else
                {
                    ErrorMessage.Text = "An email has been sent to the user.";
                }
            }
            else
            {
                ErrorMessage.Text = result.Errors.FirstOrDefault();
            }
        }
    }
}