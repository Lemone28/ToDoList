﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToDoList.DB;
using ToDoList.Models.Home;
using ToDoList.Servicies;
using ToDoList.Shared.Entity;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _db;
        private readonly TaskManager _taskManager;

        public HomeController(ILogger<HomeController> logger, ApplicationContext context, TaskManager taskManager)
        {
            _logger = logger;
            _db = context;
            _taskManager = taskManager;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
        }

        public async Task<IActionResult> Index()
        {            
            return View(new IndexViewModel(await GetTasksAll()));
        }

		[HttpPost]
		public async Task<IActionResult> Sort(IndexViewModel model)
		{
            return View("Index", new IndexViewModel(await GetTasksAll(), sortBy: model.SortBy, isDescending: model.IsDescending));
		}

		[HttpPost]
		public async Task<IActionResult> Search(IndexViewModel model)
		{
            return View("Index", new IndexViewModel(await GetTasksAll(), searchBy: model.SearchBy, searchValue: model.SearchValue));
		}

        [NonAction]
        private UserEntity GetCurrentUserEntity()
        {
            ClaimsPrincipal claims = HttpContext.User;
            return _db.Users.FirstOrDefault(x => x.Id == int.Parse(claims.FindFirstValue("Id")));
        }

        [NonAction]
        private async Task<IEnumerable<TaskEntity>> GetTasksAll()
        {
            List<TaskEntity> tasks = new List<TaskEntity>();
            if(User.Identity.IsAuthenticated)
            {
                UserEntity user = GetCurrentUserEntity();
                tasks = _taskManager.GetTasks(x => x.UserId == user.Id).ToList();
            }
            return tasks;
        }
    }
}