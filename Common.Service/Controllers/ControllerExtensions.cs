using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VH.MiniService.Common.Errors;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace VH.MiniService.Common.Service.Controllers
{
    public static class ControllerExtensions
    {
        public static async Task<ActionResult> ToActionResult(this Task<Result> resultTask, ControllerBase controller, HttpStatusCode successCode = HttpStatusCode.OK)
        {
            var result = await resultTask;
            return result.ToActionResult(controller, successCode);
        }

        public static async Task<ActionResult<T>> ToActionResult<T>(this Task<Result<T>> resultTask, ControllerBase controller, HttpStatusCode successCode = HttpStatusCode.OK)
        {
            var result = await resultTask;
            return result.ToActionResult(controller, successCode);
        }

        public static ActionResult ToActionResult(this Result result, ControllerBase controller, HttpStatusCode successCode = HttpStatusCode.OK)
            => ToActionResult(result.ToResult(new object()), controller, successCode);

        public static ActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller, HttpStatusCode successCode = HttpStatusCode.OK)
        {
            if (result.IsSuccess)
                return controller.StatusCode((int)successCode, result.Value);

            var error = result.Reasons
                .OfType<ErrorBase>()
                .FirstOrDefault();

            if (error is null)
                return controller.StatusCode((int)HttpStatusCode.InternalServerError);

            return controller.StatusCode((int)error.Type.ToHttpCode(), error);
        }
    }
}
