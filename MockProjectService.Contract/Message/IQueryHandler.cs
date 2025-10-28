using MediatR;
using MockProjectService.Contract.Shared;

namespace MockProjectService.Contract.Message
{
    public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>;
}
