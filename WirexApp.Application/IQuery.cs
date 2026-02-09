using MediatR;
using System;

namespace WirexApp.Application
{
    public interface IQuery<out TResult> : IRequest<TResult>
    {
        Guid Id { get; }
    }
}
