using ActPro.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Domain.Repository
{
    public class EfRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        protected DbSet<TEntity> DbSet { get; set; }

        protected ApplicationDbContext Context { get; set; }

        public EfRepository(ApplicationDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DbSet = Context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> All() => DbSet;

        public virtual IQueryable<TEntity> AllAsNoTracking() => DbSet.AsNoTracking();

        public virtual Task AddAsync(TEntity entity) => DbSet.AddAsync(entity).AsTask();

        public virtual void Update(TEntity entity)
        {
            var entry = Context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                DbSet.Attach(entity);
            }

            entry.State = EntityState.Modified;
        }
        public virtual Task DeleteAsync(TEntity entity)
        {
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public virtual void Delete(TEntity entity) => DbSet.Remove(entity);

        public Task<int> SaveChangesAsync() => Context.SaveChangesAsync();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Context?.Dispose();
            }
        }
    }
}