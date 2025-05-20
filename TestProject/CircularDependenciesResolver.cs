namespace TestProject;

/// <summary>
/// This class allows to resolve circular references in constructors.
/// </summary>
/// <typeparam name="T"></typeparam>
public class LazyValue<T>
{
    private readonly Func<Action<T>, T> _creator;
    private readonly Lock _lock = new Lock();
    private bool _assigned = false;
    private T _value;


    public LazyValue(Func<Action<T>, T> creator)
    {
        _creator = creator;
    }

    public void Set(T value)
    {
        _ = Set(value, _ => true, (_, _) => false);
    }

    public TResult Set<TResult>(T value, Func<T, TResult> okay, Func<T, T, TResult> alreadyInitialized)
    {
        _lock.Enter();
        try
        {
            if (_assigned)
            {
                return alreadyInitialized(value, _value);
            }
            else
            {
                _value = value;
                _assigned = true;
                return okay(_value);
            }
        }
        finally
        {
            _lock.Exit();
        }
    }

    private void Register(T value)
    {
        try
        {
            if (_assigned)
            {
                throw new AlreadyInitializedException();
            }
            else
            {
                _value = value;
                _assigned = true;
            }
        }
        finally
        {
            _lock.Exit();
        }
    }
        
    public T Get()
    {
        _lock.Enter();
        if (_assigned)
        {
            _lock.Exit();
            return _value;
        }
        else
        {
            return _value = _creator(Register);
        }
    }
        
}
