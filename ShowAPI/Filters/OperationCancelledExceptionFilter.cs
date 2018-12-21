using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ShowAPI.Filters
{
    public class OperationCancelledExceptionFilter : ExceptionFilterAttribute
    {
        private const int ClientClosedRequestStatus = 499;

        private readonly ILogger<OperationCancelledExceptionFilter> _logger;

        public OperationCancelledExceptionFilter(ILogger<OperationCancelledExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is OperationCanceledException)
            {
                _logger.LogInformation("Request cancelled.");
                context.ExceptionHandled = true;
                context.Result = new StatusCodeResult(ClientClosedRequestStatus);
            }
        }
    }
}