using System;
using System.Collections.Generic;

namespace QboxNext.Qserver.Core.Interfaces
{
    /// <summary>
    /// Interface for simple type save repository
    /// </summary>
    /// <typeparam name="T">POCO class with argumentless constructor</typeparam>
    public interface IRepository
    {
        /// <summary>
        /// Retrieves exactly one object based on the key 
        /// </summary>
        /// <param name="id">The key for the object</param>
        /// <returns>Either the object or null if not found</returns>
        T GetById<T>(object id) where T: class;
        
		/// <summary>
		/// Retrieves all objects
		/// </summary>
		/// <returns>An enumerator for all found objects</returns>
		IEnumerable<T> GetAll<T>() where T : class;

        /// <summary>
        /// Saves the object into the repository
        /// </summary>
        /// <param name="item">The object to save</param>
        /// <exceptions>Can raise DB Exceptions</exceptions>
		void Save<T>(T item) where T : class;

        /// <summary>
        /// Deletes exactly one object from the Repository based on the key
        /// </summary>
        /// <param name="id">The key for the object to delete</param>
        /// <exception>Can throw DB Exceptions</exception>
        void Delete(object id);

        /// <summary>
        /// Overload that specifies the object type to delete.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        void Delete<T>(object id);
        
        /// <summary>
        /// Finds the objects that adhere to the restrictions found in expression
        /// </summary>
        /// <param name="expression">an expression that filters the result</param>
        /// <param name="maxHits">The maximum number of items in the returned list</param>
        /// <returns>An IEnumerable that holds all items upto maxHits, filtered by the expression</returns>
        IEnumerable<T> Find<T>(Func<T, bool> expression, int maxHits = 100) where T: class;
    }
}
