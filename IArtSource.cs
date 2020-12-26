using System.Collections.Generic;

namespace AppraisalBot
{
    public interface IArtSource
    {
        IEnumerable<Art.Object> GetRandomObjects(int numObjects);
    }
}