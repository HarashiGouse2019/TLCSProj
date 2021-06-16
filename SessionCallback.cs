namespace TLCSProj.Core
{
    internal delegate void SessionCallbackMethods();

    internal class SessionCallback
    {
        internal string Code { get; private set; }
        internal int ArgLength { get; private set; }
        SessionCallbackMethods _CallbackMethods;

        SessionCallback(string code, params SessionCallbackMethods[] methods)
        {
            Code = code;

            foreach (var method in methods)
            {
                _CallbackMethods += method;
            }
        }
        internal static SessionCallback Create(string code, params SessionCallbackMethods[] methods)
        {
            return  new SessionCallback(code, methods);
        }

        internal void Trigger() => _CallbackMethods.Invoke();
    }

    
}
