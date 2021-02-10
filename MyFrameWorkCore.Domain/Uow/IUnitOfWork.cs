using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VaCantFrameWorkCore.Domain.Uow
{
    public interface IUnitOfWork
    {
        void Commit();
        Task CommitAsync();
    }
}
