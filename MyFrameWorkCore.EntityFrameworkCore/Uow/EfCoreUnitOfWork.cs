using Microsoft.EntityFrameworkCore;
using VaCantFrameWorkCore.Domain.Uow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Linq;

namespace VaCantFrameWorkCore.EntityFrameworkCore.Uow
{
   public class EfCoreUnitOfWork: IUnitOfWork
    {
        protected IDictionary<string, DbContext> ActiveDbContexts { get; }

       // protected List<DbContext> DbContexts { get; }

        public EfCoreUnitOfWork()
        {
            ActiveDbContexts=new Dictionary<string, DbContext>(StringComparer.OrdinalIgnoreCase);

            //DbContexts = new List<DbContext>();
        }

        public void Commit()
        {
            GetAllActiveDbContexts().ForEach(dbContext => dbContext.SaveChanges());
        }

        public Task CommitAsync()
        {
            var tasks = new List<Task<int>>();
            GetAllActiveDbContexts().ForEach(dbContext => tasks.Add(dbContext.SaveChangesAsync()));
            return Task.WhenAll(tasks);
        }
        //IReadOnlyList
        public List<DbContext> GetAllActiveDbContexts()
        {
            return ActiveDbContexts.Values.ToImmutableList().ToList();
        }
    }
}
