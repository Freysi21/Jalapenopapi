
using System.Collections.Generic;
using System.Threading.Tasks;
using JalapenopAPI.Model;
using JalapenopAPI.Helpers;

namespace JalapenopAPI.Repos
{
    //Generic repository interface. States common methods too implement to fit into
    //the generic repository pattern.
    public interface IRepository<T, IDType> where T : IEntity<IDType>
    {
        Task<List<T>> GetAll();
        Task<PagedList<T>> GetAll(PagingQuery page, string orderBy = null, bool asc = false);
        Task<T> Get(IDType id);
        Task<T> GetWithDetails(IDType id);
        Task<T> Add(T entity);
        Task<T> Put(T entity);
        Task<T> Delete(IDType id);
        Task<List<T>> Search(Dictionary<string, string[]> dict, string orderBy = null);

        Task<PagedList<T>> Search(PagingQuery page, Dictionary<string, string[]> dict, string orderBy = null, bool asc = false);
        Task<PagedList<T>> Search(PagingQuery page, Dictionary<string, string[]> dict, string orderBy = null, bool asc = false, Dictionary<string, string[]> dateDict = null);



    }
}