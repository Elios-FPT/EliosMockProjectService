using MediatR;
using MockProjectService.Contract.Shared;

namespace MockProjectService.Contract.Message
{
    public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>;
}
