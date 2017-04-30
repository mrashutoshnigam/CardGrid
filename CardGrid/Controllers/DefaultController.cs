using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CardGrid.Database;
using CardGrid.Models;

namespace CardGrid.Controllers
{
    public class DefaultController : Controller
    {
        private CardGridContext dbContext;

        public DefaultController()
        {
            dbContext = new CardGridContext();
        }

        // GET: Default
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetData(string searchParam, string sidx = "Id", string sord = "asc", int page = 1, int rows = 10)
        {
            try
            {
                int nextRow = ((page-1) * rows);

                var employees = from emp in this.dbContext.Employees
                                where emp.Id.ToString() == searchParam
                                || emp.Email.Contains(searchParam)
                                || emp.ContactNo.Contains(searchParam)
                                || emp.BirthDate.ToString().Contains(searchParam)
                                || emp.JobTitle.Contains(searchParam)
                                || emp.Name.Contains(searchParam)
                                select emp;
                switch (sidx.Trim().ToLower())
                {
                    case "email":
                        employees = sord == "asc" ? employees.OrderBy(x => x.Email) : employees.OrderByDescending(x => x.Email);
                        break;

                    case "contactno":
                        employees = sord == "asc" ? employees.OrderBy(x => x.ContactNo) : employees.OrderByDescending(x => x.ContactNo);
                        break;

                    case "birthdate":
                        employees = sord == "asc" ? employees.OrderBy(x => x.BirthDate) : employees.OrderByDescending(x => x.BirthDate);
                        break;

                    case "jobtitle":
                        employees = sord == "asc" ? employees.OrderBy(x => x.JobTitle) : employees.OrderByDescending(x => x.JobTitle);
                        break;

                    case "name":
                        employees = sord == "asc" ? employees.OrderBy(x => x.Name) : employees.OrderByDescending(x => x.Name);
                        break;

                    case "gender":
                        employees = sord == "asc" ? employees.OrderBy(x => x.Gender) : employees.OrderByDescending(x => x.Gender);
                        break;

                    case "hiredate":
                        employees = sord == "asc" ? employees.OrderBy(x => x.HireDate) : employees.OrderByDescending(x => x.HireDate);
                        break;

                    default:
                        employees = sord == "asc" ? employees.OrderBy(x => x.Id) : employees.OrderByDescending(x => x.Id);
                        break;
                }
                GridResponse<Models.Employee> employeesGridResponse = new GridResponse<Employee>() {
                    rows = rows,
                    searchParam = searchParam,
                    sidx = sidx,
                    page = page,
                    total= employees.Count(),
                    sord = sord == "asc" ? SortDirection.asc : SortDirection.desc
                };                
                employeesGridResponse.Data = employees.Skip(nextRow).Take(rows).ToList();
                return Json(employeesGridResponse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                var people = dbContext.Employees.ToList();
                throw ex;
            }
        }
    }
}