using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace YourProject.Repositories.Global
{
    public class GlobalRepository : IGlobalRepository
    {
        ApplicationDbContext _dbContext;
     
        public GlobalRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes) where T : class
        {
            var query = _dbContext.Set<T>().AsNoTracking();
            if(includes != null)
                if (includes.Length > 0)
                    foreach (var include in includes)
                        query = query.Include(include);
            return query.Where(predicate);
        }

        public IQueryable<T> ForceGet<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes) where T : class
        {
            var query = _dbContext.Set<T>().IgnoreQueryFilters();
            if (includes != null)
                if (includes.Length > 0)
                    foreach (var include in includes)
                        query = query.Include(include);
            return query.Where(predicate);
        }

        public object Restore(BaseEntity itemToRestore)
        {
            itemToRestore.IsDeleted = false;
            SmartUpdate(itemToRestore);
            return itemToRestore;
        }

        public object SoftDelete(BaseEntity itemToDelete)
        {
            itemToDelete.IsDeleted = true;
            itemToDelete.DeletedAt = DateTime.Now;
            SmartUpdate(itemToDelete);
            return itemToDelete;
        }

        public object SmartUpdate(BaseEntity itemToUpdate)
        {
            var proprityInfos = itemToUpdate.GetType().GetProperties().Where(p => p.Name.Contains("Id"));
            PropertyInfo keyProperty = null;
            foreach (var item in proprityInfos)
            {
                var keyAttr = item
                            .GetCustomAttributes(typeof(KeyAttribute), false)
                            .Cast<KeyAttribute>().FirstOrDefault();
                if (keyAttr != null)
                {
                    keyProperty = item;
                    break;
                }
            }
            if (keyProperty != null)
            {
                var aux = _dbContext.Find(itemToUpdate.GetType(), keyProperty.GetValue(itemToUpdate));

                foreach (PropertyInfo propertyInfo in aux.GetType().GetProperties())
                {
                    var itemProprity = itemToUpdate.GetType().GetProperties().Where(p => p.Name.Equals(propertyInfo.Name)).First();
                    var currentItem = itemProprity.GetValue(itemToUpdate);
                    if (currentItem != null)
                    {
                        var test = currentItem.ToString();

                        if (!(itemProprity.Name.Contains("Id") && currentItem.ToString() == "0"))
                            propertyInfo.SetValue(aux, itemProprity.GetValue(itemToUpdate));
                    }
                      
                }
                _dbContext.SaveChanges();
                return aux;
            }
            return "The sent object does not much any table in the database";
        }

    }
}
