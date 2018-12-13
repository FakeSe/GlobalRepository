using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace YourProject.Repositories.Global
{
    public interface IGlobalRepository
    {
         //Add an item to the database
        T Add<T>(T itemToAdd) where T : class;
        //Add an item, if "mustSave" is false, the method will not save the changes in the database, used when you have many items to add in different faces of your method so you want to save the changes one time at the end
        T Add<T>(T itemToAdd, bool mustSave) where T : class; 
        //Add a list of items
        List<T> AddRange<T>(List<T> itemsToAdd) where T : class;
        //Update an entity
        T SimpleUpdate<T>(T itemToUpdate) where T : class;
        //Detach all tracked entities : I used this once so I'll let it here, sometimes you know what you need better then c#
        void DetachAllEntities<T>() where T : class;

        /// <summary>
        /// Will update any Model without replacing data with null or foreign keys with 0 in case the user didn't add them
        /// </summary>
        /// <param name="itemToUpdate">Send any Model here the method will determinate it type and update it then it will svae the changes</param>
        /// <returns>The updated object</returns>
        object SmartUpdate(BaseEntity itemToUpdate);

        /// <summary>
        /// will SoftDelete the indicated object
        /// </summary>
        /// <param name="itemToDelete">Send any Model here the method will determinate it type and delete it then it will save the changes</param>
        /// <returns>The deleted object</returns>
        object SoftDelete(BaseEntity itemToDelete);
        object SoftDeleteRange(List<BaseEntity> itemsToDelete);

        /// <summary>
        /// will SoftDelete the indicated object in a secure way : whenever you want to delete an entity that is used by other entities, to avoid problems you can pass the Item that you want to delete
        /// and the entity that use that item in the database, the method will return the deleted object in case you want to cancel or something, you can adapt it to return true or false if it deleted
        /// the entity or not
        /// </summary>
        /// <param name="itemToDelete">Send any Model here the method will determinate it type and delete it then it will save the changes</param>
        /// <param name="toCheckIn">The type of the Model (entity) which use the Item that you want to delete</param>
        object SecuredSoftDelete(BaseEntity itemToDelete, Type toCheckIn);
        /// <summary>
        /// Same as SecuredSoftDelete(BaseEntity itemToDelete, Type toCheckIn) but you can send a list of types in case your entity is used by multiple entities
        /// </summary>
        /// <param name="itemToDelete">Send any Model here the method will determinate it type and delete it then it will save the changes</param>
        /// <param name="toCheckIn">The type of the Model (entity) which use the Item that you want to delete</param>
        object SecuredSoftDelete(BaseEntity itemToDelete, List<Type> toCheckIn);
        /// <summary>
        /// Same as SecuredSoftDelete(BaseEntity itemToDelete, Type toCheckIn) but for a list of items to delete and one type
        /// </summary>
        /// <param name="itemsToDelete">Send any Model here the method will determinate it type and delete it then it will save the changes</param>
        /// <param name="toCheckIn">The type of the Model (entity) which use the Item that you want to delete</param>
        object SecuredSoftDeleteRange(List<BaseEntity> itemsToDelete, Type toCheckIn);
        /// <summary>
        /// Same as  SecuredSoftDeleteRange(List<BaseEntity> itemsToDelete, Type toCheckIn) but for a list of items to delete and a list of types that use those entities
        /// </summary>
        /// <param name="itemsToDelete">Send any Model here the method will determinate it type and delete it then it will save the changes</param>
        /// <param name="toCheckIn">The type of the Model (entity) which use the Item that you want to delete</param>
        object SecuredSoftDeleteRange(List<BaseEntity> itemsToDelete, List<Type> toCheckIn);




        void HardDelete<T>(T itemToDelete) where T : class;
        void HardDeleteRange<T>(List<T> itemsToDelete) where T : class;

        //SecuredHardDelete is the same as SecuredSoftDelete but it will delete the entity permanently
        void SecuredHardDelete<T>(T itemToDelete, Type toCheckIn) where T : class;
        void SecuredHardDelete<T>(T itemToDelete, List<Type> toCheckIn) where T : class;
        void SecuredHardDeleteRange<T>(List<T> itemsToDelete, Type toCheckIn) where T : class;
        void SecuredHardDeleteRange<T>(List<T> itemsToDelete, List<Type> toCheckIn) where T : class;

        /// <summary>
        /// Restore the soft deleted elements ! this method will not update the DeletedAt property
        /// </summary>
        /// <param name="itemToRestore"></param>
        /// <returns></returns>
        object Restore(BaseEntity itemToRestore);

        /// <summary>
        /// This method will return an IQueryable object containing all the data that match the specified model and the condition expression with the indicated includes
        /// </summary>
        /// <typeparam name="T">Represent the entity that you want to extract the data from it</typeparam>
        /// <param name="predicate">The expression of the condition</param>
        /// <param name="includes">The entities that you want to include (using lambda expressions)</param>
        /// <returns>IQueryable object that respect the indicated conditions</returns>
        IQueryable<T> Get<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes) where T : class;

        /// <summary>
        /// Will ignore the global query filters! can be used to get data that will be restored
        /// </summary>
        /// <typeparam name="T">Represent the entity that you want to extract the data from it</typeparam>
        /// <param name="predicate">The expression of the condition</param>
        /// <param name="includes">The entities that you want to include (using lambda expressions)</param>
        /// <returns>IQueryable object that respect the indicated conditions</returns>
        IQueryable<T> ForceGet<T>(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes) where T : class;
    }
}
