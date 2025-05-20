namespace TestProject;

class AlreadyInitializedException : Exception
{
    public AlreadyInitializedException()
    {
    }

    public AlreadyInitializedException(string message) : base(message)
    {
    }
}