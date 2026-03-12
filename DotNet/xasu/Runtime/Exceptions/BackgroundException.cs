namespace Xasu.Exceptions
{
    public class BackgroundException : System.Exception
    {
        public BackgroundException(string message) : base(message)
        {
        }

        public BackgroundException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
