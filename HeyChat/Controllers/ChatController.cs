using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PusherServer;
using HeyChat.Models;
using SecureChat.TripleDES;
using System.Data.Entity;

namespace HeyChat.Controllers
{
    public class ChatController : Controller
    {
        private readonly Pusher pusher;
        public ChatController() 
        {
            var options = new PusherOptions();
            options.Cluster = "eu";
            pusher = new Pusher(
              "757440",
              "2f3520b0b549573f4208",
              "7271b8e8cd40b328c83a", options);
        }
        public ActionResult Index()
        {
            if (Session["user"] == null) {
                return Redirect("/");
            }
            var currentUser = (User) Session["user"];
            using ( var db = new ChatContext() ) {
                ViewBag.allUsers = db.Users.Where(u => u.Name != currentUser.Name )
                                 .ToList();
            }
            ViewBag.currentUser = currentUser;
            return View ();
        }
        
        public JsonResult ConversationWithContact(int contact)
        {
            if (Session["user"] == null)
            {
                return Json(new { status = "error", message = "User is not logged in" });
            }
            var currentUser = (User)Session["user"];
            var conversations = new List<Conversation>();
            using (var db = new ChatContext())
            {
                conversations = db.Conversations.
                                  Where(c => (c.ReceiverId == currentUser.Id && c.SenderId == contact) || (c.ReceiverId == contact && c.SenderId == currentUser.Id))
                                  .OrderBy(c => c.CreatedAt)
                                  .ToList();
                foreach(var i in conversations)
                {
                    i.Message = Triple_DES.Decrypt(i.SecretKey, i.Message);
                }
            }
            
            return Json(new { status = "success", data = conversations }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SendMessage() 
        {
            if (Session["user"] == null)
            {
                return Json(new { status = "error", message = "User is not logged in" });
            }
            var currentUser = (User)Session["user"];
            var contact = Convert.ToInt32(Request.Form["contact"]);
            string socket_id = Request.Form["socket_id"];
            string secretKey = Triple_DES.GetRandomKeyHex();
            Conversation convo = new Conversation
            {
                SenderId = currentUser.Id,
                Message = Triple_DES.Encrypt(secretKey, HexConverter.StrToHex(Request.Form["message"])),
                ReceiverId = contact,
                CreatedAt = DateTime.Now,
                SecretKey = secretKey
            };
            using (var db = new ChatContext()) {
                db.Conversations.Add(convo);
                db.SaveChanges();
                convo.Message = Triple_DES.Decrypt(db.Conversations.FirstOrDefault(c => c.Id == convo.Id).SecretKey, convo.Message);
            }

            
            var conversationChannel = getConvoChannel(
                currentUser.Id, contact);
            pusher.TriggerAsync(
              conversationChannel,
              "new_message",
              convo,
              new TriggerOptions() { SocketId = socket_id });
            return Json(convo);
        }
        [HttpPost]
        public JsonResult MessageDelivered(int message_id)
        {
            Conversation convo = null;
            using (var db = new ChatContext())
            {
                convo = db.Conversations.FirstOrDefault(c => c.Id == message_id);
                if ( convo != null) {
                    convo.Status = Conversation.MessageStatus.Delivered;
                    db.Entry(convo).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            string socket_id = Request.Form["socket_id"];
            var conversationChannel = getConvoChannel(convo.SenderId, convo.ReceiverId);
            pusher.TriggerAsync(
              conversationChannel,
              "message_delivered",
              convo,
              new TriggerOptions() { SocketId = socket_id });
            return Json(convo);
        }
        private String getConvoChannel(int user_id, int contact_id)
        {
            if (user_id > contact_id)
            {
                return "private-chat-" + contact_id + "-" + user_id;
            }
            return "private-chat-" + user_id + "-" + contact_id;
        }
    }
}
