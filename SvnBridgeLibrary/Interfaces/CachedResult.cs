namespace SvnBridge.Interfaces
{
    /// <summary>
    /// We need this helper class to get
    /// null caching
    /// </summary>
    public class CachedResult
    {
        public object Value;

        public CachedResult(object value)
        {
            Value = value;
        }
    }
}