using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Portfol.io.Identity.ViewModels.ResponseModels;

namespace Portfol.io.Identity.Common.Attributes
{
    /// <summary>
    /// Checks if the file being loaded matches the given extensions.
    /// </summary>
    public class ValidateFileExtensionAttribute : Attribute, IActionFilter
    {
        private readonly List<string> _extensions;

        public ValidateFileExtensionAttribute(string extensions)
        {
            _extensions = extensions.Split(',').Select(y => y.Trim().Trim('.').ToLower()).ToList();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            //Without implementation
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var file = context.ActionArguments.FirstOrDefault(u => u.Value!.GetType() == typeof(FormFile)).Value as FormFile;

            if (!_extensions.Contains(Path.GetExtension(file!.FileName).Trim('.').ToLower())) context.Result = new BadRequestObjectResult(new Error { Message = "Wrong file extension." });
        }
    }
}
