using System;
using System.Linq;
using System.Web.Mvc;
using HeyChat.Models;
using PusherServer;
using SecureChat.TripleDES;

namespace HeyChat.Controllers
{
    public class AuthController : Controller
    {
    	private readonly Pusher pusher;

	    public AuthController() 
	    {

	        var options = new PusherOptions();
	        options.Cluster = "eu";

	        pusher = new Pusher(
               "757440",
               "2f3520b0b549573f4208",
               "7271b8e8cd40b328c83a",
	           options
	        );
	    }
		[HttpPost]
		public ActionResult Login()
		{
            
			string user_name = Request.Form["username"];

			if (user_name.Trim() == "") {
				return Redirect("/");
			}


            using (var db = new Models.ChatContext()) {

                User user = db.Users.FirstOrDefault(u => u.Name == user_name);

                if (user == null) {
                    user = new User { Name = user_name, CreatedAt = DateTime.Now };

                    db.Users.Add(user);
                    db.SaveChanges();
                }

                Session["user"] = user;
            }

			return Redirect("/chat");
		}

        public JsonResult AuthForChannel(string channel_name, string socket_id)
        {
            if (Session["user"] == null)
            {
                return Json(new { status = "error", message = "User is not logged in" });
            }

            var currentUser = (Models.User)Session["user"];

            if ( channel_name.IndexOf("presence") >= 0 ) {

				var channelData = new PresenceChannelData()
				{
					user_id = currentUser.Id.ToString(),
					user_info = new
					{
						id = currentUser.Id,
						name = currentUser.Name
					},
				};

				var presenceAuth = pusher.Authenticate(channel_name, socket_id, channelData);

				return Json(presenceAuth);

            }

	    if (channel_name.IndexOf(currentUser.Id.ToString()) == -1)
	    {
		return Json(new { status = "error", message = "User cannot join channel" });
	    }

	    var auth = pusher.Authenticate(channel_name, socket_id);

	    return Json(auth);


        }
    }
}
