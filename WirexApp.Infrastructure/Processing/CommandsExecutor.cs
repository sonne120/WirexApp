using System;
using WirexApp.Application;
using System.Threading.Tasks;
using Autofac;
using MediatR;

namespace WirexApp.Infrastructure.Processing
{
    // TODO: Implement CompositionRoot or refactor to use IServiceProvider
    public static class CommandsExecutor
    {
        // Commented out until CompositionRoot is implemented
        /*
        public static async Task Execute(ICommand command)
        {
            using (var scope = CompositionRoot.BeginLifetimeScope())
            {
                var mediator = scope.Resolve<IMediator>();
                await mediator.Send(command);
            }
        }

        public static async Task<TResult> Execute<TResult>(ICommand<TResult> command)
        {
            using (var scope = CompositionRoot.BeginLifetimeScope())
            {
                var mediator = scope.Resolve<IMediator>();
                return await mediator.Send(command);
            }
        }
        */
    }
}
