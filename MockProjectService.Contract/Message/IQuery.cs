using MediatR;
using MockProjectService.Contract.Shared;

namespace MockProjectService.Contract.Message
{
    public interface IQuery<TResponse> : IRequest<TResponse>;
}
