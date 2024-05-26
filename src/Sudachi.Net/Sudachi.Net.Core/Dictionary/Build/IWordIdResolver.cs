namespace Sudachi.Net.Core.Dictionary.Build
{
    public interface IWordIdResolver
    {
        int Lookup(string headword, short posId, string reading);

        void Validate(int wordId);

        bool IsUser();
    }
}
