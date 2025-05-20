namespace TestProject;

public interface IBagAppender<in T>
{
    int Add(T item);
}