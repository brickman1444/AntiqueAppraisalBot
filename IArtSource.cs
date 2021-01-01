using System.Collections.Generic;

namespace AppraisalBot
{
    public interface IArtSource
    {
        Art.Object GetRandomObject(System.Random random);
    }
}