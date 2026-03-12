using TinCan;

namespace Xasu.HighLevel
{
    public abstract class AbstractSeriousGameHighLevelTracker<T> : AbstractHighLevelTracker<T> where T : class, new()
    {
        protected override StatementPromise Enqueue(Statement statement)
        {
            return base.Enqueue(statement).CreateAndAddContextCategoryProfileActivity(ContextActivityIds["SeriousGames"]);
        }
    }
}
