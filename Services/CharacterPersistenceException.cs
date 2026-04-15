namespace CharacterBuilderApp.Services
{
    public class CharacterPersistenceException : Exception
    {
        public CharacterPersistenceException(string message) : base(message)
        {
        }

        public CharacterPersistenceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}