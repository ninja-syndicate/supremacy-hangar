namespace SupremacyData.Editor
{
    public interface ILogInterface
    {
        void LogNormal(string text);
        void LogError(string text);
        void LogWarning(string text);
        void LogStyled(string text);
    }
}