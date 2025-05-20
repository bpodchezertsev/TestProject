namespace TestProject;

class UninitializedException : Exception
{
    public UninitializedException()
    {
    }

    public UninitializedException(string message) : base(message)
    {
    }
}