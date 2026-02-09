using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using MediatR;
using WirexApp.Application;


namespace WirexApp.Infrastructure.UnitOfWork
{
    public class UnitOfWorkCommandHandlerDecorator<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _decorated;
        private readonly IUnitOfWork _unitOfWork;

        public UnitOfWorkCommandHandlerDecorator(
            ICommandHandler<T> decorated,
            IUnitOfWork unitOfWork)
        {
            _decorated = decorated;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(T command, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _decorated.Handle(command, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                // In case of error, UnitOfWork will be disposed without committing
                throw;
            }
        }
    }
}
