namespace TestProject;

class UnsupportedTypeException : Exception
{
    public UnsupportedTypeException()
    {
    }

    public UnsupportedTypeException(string message) : base(message)
    {
    }
}