
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using JalapenopAPI.Model;
using System.Linq;
using System.Linq.Expressions;
using JalapenopAPI.Helpers;
using SearchService = JalapenopAPI.Expressions.Search;
using IncludeService = JalapenopAPI.Expressions.Include;
using OrderbyService = JalapenopAPI.Expressions.OrderBy;
using RangeService = JalapenopAPI.Expressions.Range;
using System;
using System.Reflection;
//Abstract implementation of a data repository relying on the Entity framework.
//Implements common procedures by utilizing linq query language.
namespace JalapenopAPI.Repos
{
    public abstract class EFCoreRepository<TEntity, TContext, IDType> : IRepository<TEntity, IDType>
        where TEntity : IEntity<IDType>
        where TContext : DbContext
        where IDType : IEquatable<IDType>
    {
        protected readonly TContext context;
        private readonly string[] searchIncludes;
        public EFCoreRepository(TContext context)
        {
            this.context = context;
        }

        public EFCoreRepository(TContext context, string[] searchIncludes)
        {
            this.context = context;
            this.searchIncludes = searchIncludes;
        }

        public virtual async Task<TEntity> Add(TEntity entity)
        {
            await context.Set<TEntity>().AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<TEntity> Delete(IDType id)
        {
            var entity = await context.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                return entity;
            }

            context.Set<TEntity>().Remove(entity);
            await context.SaveChangesAsync();

            return entity;
        }

        public virtual async Task<TEntity> Get(IDType id)
        {
            return await context.Set<TEntity>().FindAsync(id);
        }
        public virtual async Task<TEntity> GetWithDetails(IDType id)
        {
            var ent = await context.Set<TEntity>().FindAsync(id);
            var singles = IncludeService.GetSingleNavigationsAsStrings<TEntity>();
            var collections = IncludeService.GetCollectionNavigationsAsStrings<TEntity>();
            foreach (var prop in singles)
            {
                await context.Entry(ent).Reference(prop).LoadAsync();
            }
            foreach (var prop in collections)
            {
                await context.Entry(ent).Collection(prop).LoadAsync();
            }
            return ent;

            /*Func<IQueryable<TEntity>, IQueryable<TEntity>> includes = IncludeService.GetNavigations<TEntity>();
            IQueryable<TEntity> query = context.Set<TEntity>();
            if (includes != null)
            {
                query = includes(query);
            }

            return await query.FirstOrDefaultAsync(entity => entity.Id.Equals(id));*/
        }
        public virtual async Task<List<TEntity>> GetAll()
        {
            return await context.Set<TEntity>().ToListAsync();
        }

        public virtual async Task<PagedList<TEntity>> GetAll(PagingQuery page, string orderBy = null, bool asc = false)
        {
            var query = context.Set<TEntity>().Select(val => val);
            var type = typeof(TEntity);
            var properties = type.GetProperties();
            var sortByDefaultPropertyInfoList = properties.Where(
                prop =>
                    prop.IsDefined(typeof(DefaultSortPropertyAttribute))
            ).ToList();
            if(orderBy != null && OrderbyService.PropertyExists<TEntity>(query, orderBy)){
                return asc ? await PagedList<TEntity>.ToPagedListAsync(OrderbyService.OrderByProperty(query, orderBy), page.PageNumber, page.PageSize)
                : await PagedList<TEntity>.ToPagedListAsync(OrderbyService.OrderByPropertyDescending(query, orderBy), page.PageNumber, page.PageSize);
            }
            else if(sortByDefaultPropertyInfoList.Count > 0 && OrderbyService.PropertyExists<TEntity>(query, sortByDefaultPropertyInfoList[0].Name)) {
                return await PagedList<TEntity>.ToPagedListAsync(OrderbyService.OrderByPropertyDescending(query, sortByDefaultPropertyInfoList[0].Name), page.PageNumber, page.PageSize);
            }
            return await PagedList<TEntity>.ToPagedListAsync(context.Set<TEntity>(), page.PageNumber, page.PageSize);
        }

        public virtual async Task<TEntity> Put(TEntity entity)
        {
            context.Update(entity);
            //context.Entry(entity).State = EntityState.Modified;
            /*try
            {
                Type t = entity.GetType();

                var properties = t.GetProperties();
                var navigationPropertyInfoList = properties.Where(prop =>
                    prop.IsDefined(typeof(NavigationPropertyAttribute)) && !prop.IsDefined(typeof(DoNotModifyAttribute))
                );
                foreach (var prop in navigationPropertyInfoList)
                {
                    var values = (IEnumerable<object>)prop.GetValue(entity);
                    foreach (var value in values)
                    {
                        context.Entry(value).State = EntityState.Modified;
                    }
                }
                var singleNavigationPropertyInfoList = properties.Where(prop =>
                    prop.IsDefined(typeof(SingleNavigationPropertyAttribute)) && !prop.IsDefined(typeof(DoNotModifyAttribute))
                );
                foreach (var prop in singleNavigationPropertyInfoList)
                {
                    var value = prop.GetValue(entity);
                    context.Entry(value).State = EntityState.Modified;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }*/


            await context.SaveChangesAsync();
            return entity;
        }
        public virtual async Task<List<TEntity>> Search(Dictionary<string, string[]> dict, string orderBy = null)
        {
            var expr = SearchService.ContainsValues<TEntity>(dict);

             var query = GetQueryWithInclude(expr).Select(entity => entity);
            if(orderBy != null && OrderbyService.PropertyExists<TEntity>(query, orderBy))
            {
                return await OrderbyService.OrderByPropertyDescending(query, orderBy).ToListAsync();
            }
            return await query.ToListAsync();
        }
        public virtual async Task<PagedList<TEntity>> Search(PagingQuery page, Dictionary<string, string[]> dict, string orderBy = null, bool asc = false)
        {
            var expr = SearchService.ContainsValues<TEntity>(dict);

            var query = GetQueryWithInclude(expr).Select(entity => entity);
            
            if(orderBy != null && OrderbyService.PropertyExists<TEntity>(query, orderBy))
            {
                return asc ? await PagedList<TEntity>.ToPagedListAsync(OrderbyService.OrderByProperty(query, orderBy), page.PageNumber, page.PageSize)
                : await PagedList<TEntity>.ToPagedListAsync(OrderbyService.OrderByPropertyDescending(query, orderBy), page.PageNumber, page.PageSize);
            }
            return await PagedList<TEntity>.ToPagedListAsync(query, page.PageNumber, page.PageSize);
        }

        public virtual async Task<PagedList<TEntity>> Search(PagingQuery page, Dictionary<string, string[]> dict, string orderBy = null, bool asc = false, Dictionary<string, string[]> dateDict = null)
        {
            var query = SearchQuery(page, dict, orderBy, asc, dateDict);
            return await PagedList<TEntity>.ToPagedListAsync(query, page.PageNumber, page.PageSize);
        }

        protected IQueryable<TEntity> SearchQuery(PagingQuery page, Dictionary<string, string[]> dict, string orderBy = null, bool asc = false, Dictionary<string, string[]> dateDict = null)
        {
            var expr = SearchService.ContainsValues<TEntity>(dict);
            var dateExpr = RangeService.RangeExpression<TEntity>(dateDict);

            var query = dateExpr != null ? GetQueryWithInclude(expr).Where(dateExpr).Select(entity => entity) : GetQueryWithInclude(expr).Select(entity => entity);
            if (orderBy != null && OrderbyService.PropertyExists<TEntity>(query, orderBy))
            {
                return asc ? OrderbyService.OrderByProperty(query, orderBy)
                : OrderbyService.OrderByPropertyDescending(query, orderBy);
            }
            return query;
        }
        public IQueryable<TEntity> GetQueryWithInclude(Expression<Func<TEntity, bool>> expr = null)
        {
            var query = expr != null ? context.Set<TEntity>().Where(expr) : context.Set<TEntity>();

            if (this.searchIncludes != null)
            {
                foreach (var include in this.searchIncludes)
                {
                    query = query.Include(include);
                }
            }

            return query;
        }
    }
}