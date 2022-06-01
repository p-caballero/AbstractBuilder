namespace AbstractBuilder.Internal
{
    internal static class CtorConstants
    {
        internal const string MethodName = ".ctor";

        internal const System.Reflection.BindingFlags BindingFlags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;
    }
}
