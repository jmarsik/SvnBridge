namespace SvnBridge.TfsLibrary
{
    public enum SourceItemStatus
    {
        None,
        Unversioned,
        Unmodified,
        Modified,
        Missing,
        Delete,
        Add,
        Conflict,
        Rename,
    }
}