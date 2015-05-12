﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.DynamicData;
using System.Web.Helpers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ReteaSocialaMDS.Models;
using ReteaSocialaMDS.Models.HomeViewModels;

namespace ReteaSocialaMDS.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Search(string userName)
        {
            return View();
        }

        public ActionResult NewPost()
        {
            var model = new PostViewModel();
            return PartialView("_NewPost", model);
        }

        [Authorize]
        public ActionResult CreateUserImage()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUserImage(ImageViewModel imageModel)
        {
            if (imageModel.ImageUpload == null)
            {
                ModelState.AddModelError("ImageUpload", "You have to upload an image");
            }
            else if (imageModel.ImageUpload.ContentLength == 0)
            {
                ModelState.AddModelError("ImageUpload", "You have to select a valid image");
            }
            if (imageModel.ImageUpload != null)
            {
                switch (imageModel.ImageUpload.ContentType)
                {
                    case "image/png":
                    case "image/jpeg":
                    case "image/pjpeg":
                    case "image/gif":
                        break;
                    default:
                        {
                            ModelState.AddModelError("ImageUpload", "Your file is not a valid MYME type");
                            break;
                        }

                }
            }


            if (ModelState.IsValid)
            {
                var uploadDir = "~/Images/Users/";

                var imageName = User.Identity.GetUserId() + imageModel.ImageUpload.FileName;
                var imagePath = Path.Combine(Server.MapPath(uploadDir), imageName);
                imageModel.ImageUpload.SaveAs(imagePath);
                var imageUrl = Path.Combine(uploadDir, imageName);

                var newImage = new UserImage()
                {
         
                    OwnerId = User.Identity.GetUserId(),
                    Title = imageModel.Title,
                    Description = imageModel.Description,
                    ImageUrl = imageUrl

                };



                db.UserImage.Add(newImage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(imageModel);
        }
        [Authorize]
        [HttpGet]
        public ActionResult UserImagesList()
        {
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            ApplicationUser user = userManager.FindById(User.Identity.GetUserId());

            IEnumerable<string> imgs = (from image in user.UserImages select Url.Content(image.ImageUrl)).Take(5).ToList();

            return View(imgs);
            
        }

        [Authorize]
        [HttpPost]
        public JsonResult UserImagesList(int skip)
        {
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            ApplicationUser user = userManager.FindById(User.Identity.GetUserId());

            IEnumerable<string> imgs = (from image in user.UserImages select Url.Content(image.ImageUrl)).Skip(skip).Take(5).ToList();
            
            return Json(imgs);

        }

        [Authorize]
        [HttpGet]
        public ActionResult UserProfile(string id)
        {
            if (id == null)
                id = User.Identity.GetUserId();
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            ApplicationUser user = userManager.FindById(id);
            string userId = User.Identity.GetUserId();
            List<string> friends = (from fr in db.Friend where (fr.UserId.CompareTo(id)==0 && fr.OtherUserId.CompareTo(userId)==0) select fr.UserId).ToList();
            
            List<String> friendRequest = (from fr in db.FriendRequest where (fr.UserId.CompareTo(id)==0 && fr.FutureFriendUserId.CompareTo(userId)==0) select fr.UserId).ToList();
            UserProfileViewModel model = new UserProfileViewModel()
            {
                firstName = user.FirstName,
                lastName = user.LastName,
                numberOfFriends = user.Friends.Count,
                numberOfImages = user.UserImages.Count,
                friend = friends.Count !=0,
                friendReq = friendRequest.Count != 0,
                User = id
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public JsonResult FriendRequestSend(string otherUserId)
        {
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            ApplicationUser friend = userManager.FindById(otherUserId);
            if (friend == null || otherUserId ==User.Identity.GetUserId())
            {
                return Json("-1");
            }
            FriendRequest newFriendRequest = new FriendRequest()
            {
                UserId = otherUserId,
                FutureFriendUserId = User.Identity.GetUserId()
            };
            db.FriendRequest.Add(newFriendRequest);
            db.SaveChanges();

            return Json("0");
        }

        [Authorize]
        public JsonResult FriendRequests()
        {
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);

            
            string userId = User.Identity.GetUserId();
            List<string> friendReq = (from fr in db.FriendRequest where (fr.UserId.CompareTo(userId) ==0 ) select fr.FutureFriendUserId).ToList();
            List<FriendRequestViewModel> futureFriends = new List<FriendRequestViewModel>();
            foreach (string futureFriendUserId in friendReq)
            {
                futureFriends.Add(new FriendRequestViewModel()
                {
                  FutureFriendId  = futureFriendUserId,
                  FullName =  userManager.FindById(futureFriendUserId).FirstName
                });
            }
            
            return Json(futureFriends);
        }

        [Authorize]
        [HttpGet]
        public ActionResult AddPost()
        {
            return View(new Post());
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddPost(Post newPost)
        {

            newPost.Id = int.Parse(User.Identity.GetUserId());
            newPost.PostDate = System.DateTime.Now;

            db.Post.Add(newPost);

            //string userId = User.Identity.GetUserId();
            //DateTime now = System.DateTime.Now;

            //db.Post.Add(new Post()
            //{
            //    UserId = userId,
            //    PostDate = now,
            //    PostMessage = postComment

            //});
            db.SaveChanges();

            //List<string> postAuthors  = (from post in db.Post where (post.UserId.CompareTo(userId) == 0) select post.UserId).ToList();
            //List<DateTime> postDates = (from post in db.Post where (post.UserId.CompareTo(userId) == 0) select post.PostDate).ToList();
            //List<string> postContent = (from post in db.Post where (post.UserId.CompareTo(userId) == 0) select post.PostMessage).ToList();

            return Redirect("/");

        }
        [Authorize]
        public ActionResult Posts()
        {

            IEnumerable<Post> allPosts = db.Post.ToList();

            return View(allPosts);
        }

    }
}