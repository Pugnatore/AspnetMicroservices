using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ordering.Application.Behaviours
{
    public class UnhandledExceptionBehaviour<TRequest,TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger _logger;

        public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
               return await next();
            }
            catch (Exception e)
            {
                var requestName = typeof(TRequest).Name;
                _logger.LogError(e,"Application Request: An unhandled exception for request {name} {@Request}", requestName,request);
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
