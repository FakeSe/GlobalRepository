using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace YourProject.Repositories.Global
{
    public interface IGlobalRepository
    {
        
        /// <summary>
        /// Will Add the given entity
        /// </summary>
        /// <param name="itemToAdd">Entity</param>
        /// <returns>The added ENTITY</returns>
        object Add<T>(T itemToAdd) where T : class;
        
        /// <summary>
        /// Will Add the given list of entities
        /// </summary>
        /// <param name="itemsToAdd">A list of entities</param>
        /// <returns>The added list</returns>
        object AddRange<T>(List<T> itemsToAdd) where T : class;
        
        /// <summary>
        /// Will simply update any Model without any effort
        /// </summary>
        /// <param name="itemToUpdate">Send any Model here the method will determinate it type and update it then it will svae the changes</param>
        /// <returns>The updated object</returns>
        object SimpleUpdate<T>(T itemToUpdate) where T : class;

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
