using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace VaCantFrameWorkCore.EntityFrameworkCore
{
    public class BaseDbContext : DbContext
    {
        public BaseDbContext(DbContextOptions<BaseDbContext> options):base(options)
        { 
        
        }

        /// <summary>
        /// 重写 OnConfiguring 方法，指定数据库
        /// </summary>
        /// <param name="optionsBuilder"></param>
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    //连接数据库-一般不用
        //    optionsBuilder.UseSqlServer(ConnectionString);
        //}


        public void ChangeConnection(string connection, string schema)
        {
            Database.GetDbConnection().ConnectionString = connection;
            ChangeAllSchema(schema);
        }

        public void ChangeDatabase(string database, string schema)
        {
            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentNullException(nameof(database));
            }
            var connection = Database.GetDbConnection();
            if (connection.State.HasFlag(ConnectionState.Open))
            {
                connection.ChangeDatabase(database);
            }
            else
            {
                var connectionString = Regex.Replace(connection.ConnectionString, @"(?<=[Dd]atabase=)\w+(?=;)", database, RegexOptions.Singleline);
                connection.ConnectionString = connectionString;
            }
            ChangeAllSchema(schema);
        }

        public void ChangeAllSchema(string schema)
        {
            var model = (Model)Model;
            if (model.ValidateModelIsReadonly())
                return;
            var items = Model.GetEntityTypes();
            foreach (var item in items)
            {
                if (item is IMutableEntityType entityType)
                {
                    entityType.SetSchema(schema);
                }
            }
        }

        public void ChangeSchema<TEntity>(string schema)
        {
            var model = (Model)Model;
            if (model.ValidateModelIsReadonly())
                return;
            if (Model.FindEntityType(typeof(TEntity)) is IMutableEntityType relational)
            {
                relational.SetSchema(schema);
            }
        }

        public void ChangeTable<TEntity>(string tableName)
        {
            var model = (Model)Model;
            if (model.ValidateModelIsReadonly())
                return;
            if (Model.FindEntityType(typeof(TEntity)) is IMutableEntityType relational)
            {
                relational.SetTableName(tableName);
            }
        }


        /// <summary>
        /// public void Detach() 方法是在SaveChanges后更改EF追踪实体标记的，用来取消追踪的 => Added、Detached、UnChanged… 将所有Save后的对象的标记改成Detached。
        /// </summary>
        public void Detach()
        {
            ChangeTracker.Entries().ToList().ForEach(aEntry =>
            {
                var temp = aEntry;
                if (aEntry.State != EntityState.Detached)
                    aEntry.State = EntityState.Detached;
            });
        }

        /// <summary>
        /// 重写 SaveChanges() 方法，此处EF在保存前都会进行变更追踪，所以优化是在保存前先关闭，在保存后开启。ChangeTracker.AutoDetectChangesEnabled = false/true;
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
            int count = base.SaveChanges();
            Detach();
            ChangeTracker.AutoDetectChangesEnabled = true;
            return count;
        }

        /// <summary>
        /// 重写 SaveChanges() 方法，此处EF在保存前都会进行变更追踪，所以优化是在保存前先关闭，在保存后开启。ChangeTracker.AutoDetectChangesEnabled = false/true;
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeTracker.AutoDetectChangesEnabled = false;
            int count = await base.SaveChangesAsync(cancellationToken);
            Detach();
            ChangeTracker.AutoDetectChangesEnabled = true;
            return count;
        }

    }
}
