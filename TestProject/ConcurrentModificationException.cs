namespace TestProject;

class ConcurrentModificationException : Exception
{
    public ConcurrentModificationException()
    {
    }

    public ConcurrentModificationException(string message) : base(message)
    {
    }
}