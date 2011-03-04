namespace Wordbox
{
    public interface IWordboxDictionary
    {
        string Name { get; }
        byte[] ProbabilityTable { get; }
    }
}
