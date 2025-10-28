using MediatR;
using MockProjectService.Contract.Shared;

namespace MockProjectService.Contract.Message
{
    public interface ICommand<TResponse> : IRequest<TResponse>;
}
