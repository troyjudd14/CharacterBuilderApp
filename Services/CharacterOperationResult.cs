namespace CharacterBuilderApp.Services
{
    public enum CharacterUpdateResult
    {
        Updated,
        NotFound,
        Unchanged,
        Failed
    }

    public enum CharacterDeleteResult
    {
        Deleted,
        NotFound,
        Failed
    }
}