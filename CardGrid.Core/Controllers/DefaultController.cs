using CardGrid.Core.Services;
using CardGrid.Models;
using Microsoft.AspNetCore.Mvc;

namespace CardGrid.Core.Controllers
{
    /// <summary>
    /// Serves the card grid page and its JSON data endpoint. Port of the legacy MVC 5
    /// <c>DefaultController</c>; routes are unchanged (<c>/</c>, <c>/Default/Index</c>, <c>/Default/GetData</c>).
    /// </summary>
    public class DefaultController : Controller
    {
        /// <summary>Grid query service.</summary>
        private readonly EmployeeGridService gridService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultController"/> class.
        /// </summary>
        /// <param name="gridService">Grid query service.</param>
        public DefaultController(EmployeeGridService gridService)
        {
            this.gridService = gridService ?? throw new ArgumentNullException(nameof(gridService));
        }

        /// <summary>Renders the card grid page.</summary>
        /// <returns>The <c>Index</c> view.</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>Returns one page of employees for the card grid.</summary>
        /// <param name="request">Search, sort and paging parameters from the query string.</param>
        /// <returns>A JSON <see cref="GridResponse{T}"/> of employees.</returns>
        [HttpGet]
        public JsonResult GetData([FromQuery] GridRequest request)
        {
            return Json(this.gridService.GetPage(request));
        }
    }
}
