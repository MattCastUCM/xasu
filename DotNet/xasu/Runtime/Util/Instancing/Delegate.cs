namespace Xasu.Util
{
    /// <summary>
    /// Base class for delegate classes for functionalities whose instancing can be replaced in a factory method
    /// </summary>
    /// <typeparam name="BaseImplementation">Base class/interface that is common to all wrappers of the delegated functionality</typeparam>
    /// <typeparam name="DefaultClass">Default class to be instanced if there is no factory method configured for the delegate</typeparam>
    public abstract class Delegate<BaseImplementation, DefaultClass>
        where DefaultClass : class, BaseImplementation, new()
    {
        protected static BaseImplementation _instance;

        /// <summary>
        /// Initialize the instance whose functionality to delegate (MUST BE CALLED IN A STATIC CONSTRUCTOR)
        /// </summary>
        /// <param name="factoryId">Id of the factory method</param>
        protected static void InitInstance(Factories.Id factoryId = Factories.Id.NONE)
        {
            if (_instance == null)
            {
                try
                {
                    _instance = (BaseImplementation)Factories.factories[factoryId]();
                }
                catch
                {
                    _instance = new DefaultClass();
                }
            }
        }
    }
}
