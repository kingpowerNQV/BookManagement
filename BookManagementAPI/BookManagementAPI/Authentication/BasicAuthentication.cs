using System;  
using System.Collections.Generic;  
using System.Linq;  
using System.Net;  
using System.Net.Http;  
using System.Security.Principal;  
using System.Threading;  
using System.Web;  
using System.Web.Http.Controllers;  
using System.Web.Http.Filters;  
  
namespace BookManagementAPI.Authentication
{
    public class BasicAuthentication : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                if (actionContext.Request.Headers.Authorization != null)
                {
                    //Taking the parameter from the header  
                    var authToken = actionContext.Request.Headers.Authorization.Parameter;
                    //decode the parameter  
                    var decoAuthToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authToken));
                    //split by colon : and store in variable  
                    var UserNameAndPassword = decoAuthToken.Split(':');
                    //Passing to a function for authorization  
                    if (IsAuthorizedUser(UserNameAndPassword[0], UserNameAndPassword[1], Int32.Parse(UserNameAndPassword[2])))
                    {
                        // setting current principle  
                        Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(UserNameAndPassword[0]), null);
                    }
                    else
                    {
                        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    }
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }
        public static bool IsAuthorizedUser(string Username, string Password, int Role)
        {
            // In this method we can handle our database logic here...  
            //Here we have given the hard-coded values   
            return Username == "admin" && Password == "123456" && Role == 1;
        }
    }
}