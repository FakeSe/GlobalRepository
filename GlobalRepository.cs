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
        SmartDeletersService _smartDeleters;

        public GlobalRepository(ApplicationDbContext dbContext, SmartDeletersService smartDeleters)
        {
            _smartDeleters = smartDeleters;
            _dbContext = dbContext;
        }

        public List<T> AddRange<T>(List<T> itemsToAdd) where T : class
        {
            _dbContext.Set<T>().AddRange(itemsToAdd);
            _dbContext.SaveChanges();
            return itemsToAdd;
        }

        public T Add<T>(T itemToAdd) where T : class
        {
            _dbContext.Set<T>().Add(itemToAdd);
            _dbContext.SaveChanges();
            return itemToAdd;
        }



        public T Add<T>(T itemToAdd, bool mustSave) where T : class
        {
            _dbContext.Set<T>().Add(itemToAdd);
            if (mustSave)
                _dbContext.SaveChanges();
            return itemToAdd;
        }

        public IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes) where T : class
        {
            var query = _dbContext.Set<T>().AsNoTracking();
            if (includes != null)
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

        public T SimpleUpdate<T>(T itemToUpdate) where T : class
        {
            _dbContext.Set<T>().Update(itemToUpdate);
            _dbContext.SaveChanges();
            return itemToUpdate;
        }

        //Will detach all the tracked entities
        public void DetachAllEntities<T>() where T : class
        {
            var changedEntriesCopy = _dbContext.ChangeTracker.Entries<T>()
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
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
                        if (!(itemProprity.Name.Contains("Id") && currentItem.ToString() == "0"))
                            if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true).IsPublic)
                                propertyInfo.SetValue(aux, itemProprity.GetValue(itemToUpdate));
                }
                _dbContext.SaveChanges();
                return aux;
            }
            return "The sent object does not match any table in the database. Did you forget to set a Key property to the given entity ?";
        }


        //SOFT DELETE : Here you will find tow types of SoftDelete :
        // - Simple Soft Delete : the one we know, which will change IsDelete to True then leave + SoftDeleteRange another simple SoftDelete but for a range of records
        // - Secured Soft Delete : This one is charged many times :
        // - 2 times : Accept one record and check in one type + a SecuredSoftDeleteRange with the same principe but for a range of records
        // - 2 times : Accept one record and a list of types + one with the same principe but for a range of records

        //A Simple SoftDelete which will change the IsDeleted to true then return the updated record
        public object SoftDelete(BaseEntity itemToDelete)
        {
            itemToDelete.IsDeleted = true;
            itemToDelete.DeletedAt = DateTime.Now;
            SmartUpdate(itemToDelete);
            return itemToDelete;
        }
            //A Simple SoftDelete which will change the IsDeleted to true then return the updated record with the possibility to not save the changes (usefull for SoftDeleteRange)
        public object SoftDelete(BaseEntity itemToDelete, bool saveChanges)
        {
            itemToDelete.IsDeleted = true;
            itemToDelete.DeletedAt = DateTime.Now;
            if(saveChanges)
                SmartUpdate(itemToDelete);
            return itemToDelete;
        }
        //Optimized SoftDeleteRange
        public object SoftDeleteRange(List<BaseEntity> itemsToDelete)
        {
            itemsToDelete.ForEach(item => SoftDelete(item, false));
            _dbContext.SaveChanges();
            return itemsToDelete;
        }
            //A SoftDelete which will check if the "itemToDelete" is used in the database by the "toCheckin" type, if it is used it will not delete the record
        public object SecuredSoftDelete(BaseEntity itemToDelete, Type toCheckIn)
        {
            if (!_smartDeleters.IsUsedBy(itemToDelete, toCheckIn))
            {
                itemToDelete.IsDeleted = true;
                itemToDelete.DeletedAt = DateTime.Now;
                SmartUpdate(itemToDelete);
            }

            return itemToDelete;
        }
        //Same as SecuredSoftDelete(BaseEntity itemToDelete, Type toCheckIn) with the possibility to not save the changes (usefull for SecuredSoftDeleteRange)
        public object SecuredSoftDelete(BaseEntity itemToDelete, Type toCheckIn, bool saveChanges)
        {
            if (!_smartDeleters.IsUsedBy(itemToDelete, toCheckIn))
            {
                itemToDelete.IsDeleted = true;
                itemToDelete.DeletedAt = DateTime.Now;
                if (saveChanges)
                    SmartUpdate(itemToDelete);
            }

            return itemToDelete;
        }
        //SecuredSoftDelete for multiple records with 1 type as a parameter
        public object SecuredSoftDeleteRange(List<BaseEntity> itemsToDelete, Type toCheckIn)
        {
            itemsToDelete.ForEach(item => SecuredSoftDelete(item, toCheckIn, false));
            _dbContext.SaveChanges();
            return itemsToDelete;
        }

        //A SoftDelete which will check if the "itemToDelete" is used in the database by the list of the "toCheckin" type, if it is used by 1 record it will not delete the record
        public object SecuredSoftDelete(BaseEntity itemToDelete, List<Type> toCheckIn)
        {
            if (!_smartDeleters.IsUsedBy(itemToDelete, toCheckIn))
            {
                itemToDelete.IsDeleted = true;
                itemToDelete.DeletedAt = DateTime.Now;
                SmartUpdate(itemToDelete);
            }

            return itemToDelete;
        }
        //same as SoftDelete(BaseEntity itemToDelete, List<Type> toCheckIn) with the possibility to not save the changes (usefull for SecuredSoftDeleteRange)
        public object SecuredSoftDelete(BaseEntity itemToDelete, List<Type> toCheckIn, bool saveChanges)
        {
            if (!_smartDeleters.IsUsedBy(itemToDelete, toCheckIn))
            {
                itemToDelete.IsDeleted = true;
                itemToDelete.DeletedAt = DateTime.Now;
                if (saveChanges)
                    SmartUpdate(itemToDelete);
            }

            return itemToDelete;
        }

        //SecuredSoftDelete for multiple records with many types as a parameter
        public object SecuredSoftDeleteRange(List<BaseEntity> itemsToDelete, List<Type> toCheckIn)
        {
            itemsToDelete.ForEach(item => SecuredSoftDelete(item, toCheckIn, false));
            _dbContext.SaveChanges();
            return itemsToDelete;
        }

        //Hard DELETE : Same logic as SoftDelete but this one will perma delete thje records


        public void HardDelete<T>(T itemToDelete) where T : class
        {
            _dbContext.Remove(itemToDelete);
            _dbContext.SaveChanges();
        }

        public void HardDelete<T>(T itemToDelete, bool saveChanges) where T : class
        {
            _dbContext.Remove(itemToDelete);
            if(saveChanges)
                _dbContext.SaveChanges();
        }

        public void HardDeleteRange<T>(List<T> itemsToDelete) where T : class
        {
            _dbContext.RemoveRange(itemsToDelete);
            _dbContext.SaveChanges();
        }

        public void SecuredHardDelete<T>(T itemToDelete, Type toCheckIn) where T : class
        {
            if (!_smartDeleters.IsUsedBy(itemToDelete, toCheckIn))
                HardDelete(itemToDelete);
        }

        public void SecuredHardDelete<T>(T itemToDelete, Type toCheckIn, bool saveChanges) where T : class
        {
            if (!_smartDeleters.IsUsedBy(itemToDelete, toCheckIn))
                HardDelete(itemToDelete, saveChanges);
        }

        public void SecuredHardDeleteRange<T>(List<T> itemsToDelete, Type toCheckIn) where T : class
        {
            itemsToDelete.ForEach(item => SecuredHardDelete(item, toCheckIn, false));
            _dbContext.SaveChanges();
        }

        public void SecuredHardDelete<T>(T itemToDelete, List<Type> toCheckIn) where T : class
        {
            if (!_smartDeleters.IsUsedBy(itemToDelete, toCheckIn))
                    HardDelete(itemToDelete);
        }

        public void SecuredHardDelete<T>(T itemToDelete, List<Type> toCheckIn, bool saveChanges) where T : class
        {
            if (!_smartDeleters.IsUsedBy(itemToDelete, toCheckIn))
                HardDelete(itemToDelete, saveChanges);
        }

        public void SecuredHardDeleteRange<T>(List<T> itemsToDelete, List<Type> toCheckIn) where T : class
        {
            itemsToDelete.ForEach(item => SecuredHardDelete(item, toCheckIn, false));
            _dbContext.SaveChanges();
        }


    }
}
