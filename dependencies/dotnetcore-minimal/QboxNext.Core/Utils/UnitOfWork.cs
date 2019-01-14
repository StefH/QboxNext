using System;

namespace QboxNext.Core
{
    /// <summary>
    /// Defines an interface for a Unit Of Work type implementation.
    /// This is primarily used in conjunction with the EcoContext. It helps to
    /// keep the EcoContext seperated between requests but alive for the duration of the request.
    /// </summary>
    public interface IUnitOfWork
    {
        object this[string key] { get; set; }
    }

    /// <summary>
    /// The Unit Of Work Helper is a holder that allows access to the UnitOfWork interface
    /// There are now two implementations of the Unit Of Work datastores:
    /// - in a normal application the Thread variables are used
    /// - In a web application the HttpContext is used 
    /// for storage of the EcoSpace.
    /// </summary>
    public static class UnitOfWorkHelper
    {
        public static IUnitOfWork CurrentDataStore;
    }


	public static class DataStoreName
	{
		public const string cLogger = "logger";
		public const string cQboxes = "Qboxes";
	}
}

