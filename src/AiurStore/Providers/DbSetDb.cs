using AiurStore.Models;
using AiurStore.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AiurStore.Providers
{
    public class DbSetDb<TValue, TEntity, TContext> : InOutDatabase<TValue> 
        where TValue : class
        where TEntity :class
        where TContext : DbContext
    {
        private readonly Func<TContext> _contextFactory;
        private readonly Func<TContext, DbSet<TEntity>> _dbSetFactory;
        private readonly Func<DbSet<TEntity>, IQueryable<TValue>> _queryFactory;
        private readonly Action<TValue, DbSet<TEntity>> _addAction;

        public DbSetDb(
            Func<TContext> contextFactory,
            Func<TContext, DbSet<TEntity>> dbSetFactory,
            Func<DbSet<TEntity>, IQueryable<TValue>> queryFactory,
            Action<TValue, DbSet<TEntity>> addAction)
        {
            this._contextFactory = contextFactory;
            _dbSetFactory = dbSetFactory;
            _queryFactory = queryFactory;
            _addAction = addAction;
        }

        public override IEnumerable<TValue> GetAll()
        {
            var context = _contextFactory();
            var dbSet = _dbSetFactory(context);
            return _queryFactory(dbSet);
        }

        public override void Add(TValue newItem)
        {
            var context = _contextFactory();
            var dbSet = _dbSetFactory(context);
            _addAction(newItem, dbSet);
            context.SaveChanges();
        }

        public override void Clear()
        {
            throw new NotSupportedException("You can't clear if you are using QueryStoreProvider!");
        }

        public override void Insert(int index, TValue newItem)
        {
            throw new NotSupportedException("You can't insert if you are using QueryStoreProvider!");
        }
    }
}
