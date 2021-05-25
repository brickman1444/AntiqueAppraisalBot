using System;

namespace AppraisalBot
{
    class Comment
    {
        // Coat that if touched loses all value $0-$0
        // Silverware stolen from Red Lobster
            // silverware
        // Fiddle used to beat devil in fiddle-off
            // instrument
        // Bowl Jesus threw up in twice
        // A cabinet with $2,000 inside it
        // Adam and Eve's pizza cutters
            // old objects
        // The worst owl
            // simple descriptions
        // Sassiest statue on earth (you can't afford it, honey) (if you have to ask, you can't afford it)
            // Expensive objects
        // The worst thing we could find ($0.05 - $0.06)
        // Sword from the past
            // old items
        // World's worst umbrella ($20-$30)
            // simple descriptions
        // Doll that will kill again
            // simple descriptions of people
        // Chair that just has to be different
            // simple description
        // Painting that kills you if you look at it
            // paintings
        // What you see before you die
            // paintings or statues

        static string[] comments = {
            //"If it were to appear at auction it would have an estimate of x to y dollars.",
            //"I could see a collector being quite willing to pay between x and y dollars for this.",
            //"the price doesn't reflect the magic in this item but if it came up for auction it would still make the best part of x dollars.",
            //"in this condition it's going to be worth about x to y dollars.",
            //"of course it's got a value but tastes do change. Now I would say about x at auction.",
            //"a convservative number would be between x and y dollars.",
            //"this was a side of the road acquisition, but it's probably worth about x to y dollars.",
            //"in today's market, this item should retail between x and y dollars.",
            //"Because there are only 12 of these in the world, they are very collectible and a collector would pay up to X dollars.",
            //"Originally bought at a yard sale, this investment will end up going for x to y dollars.",
            //"These items are very popular at auction, as you can imagine. This one I would estimate at x to y dollars.",
            //"I think a dealer in this sort of thing would sell it for x to y dollars.",
            //"There's a possibility this was once part of a three piece set. I'd value this one at x to y dollars.",
            //"If you had to put a replacement value on this, I would insure it for x to y dollars.",
            //"This is a clear facsimile only worth x to y dollars.",
            //"This piece is hard to describe, but I would estimate its value at x to y dollars.",
            //"The epitome of avant-garde art, this item could be worth between x and y dollars.",
            "This example is in fantastic condition.",
            "This item is in terrible condition.",
            "What is that smell?",
            "This is a shoddy replica, but if it were genuine it'd be worth five times as much.",
            "I'd be thrilled to buy this right now.",
            "I never thought one of these would surface in my lifetime.",
            "This is extremely rare.",
            "The Duchess of Windsor used to fancy these.",
            "There are many uses to items like these.",
            "These have come up before at auction before, but never with the designer's signature like this one has.",
            "I don't even understand why you would bring this here.",
            "This is to valuable to move around. You need to insure this immediately.",
            "Collectors go mad for these. I recommend selling it online.",
            "This item deserves its own social media account",
            "The price here doesn't communicate the magic present in this item",
            "Since this item was cleaned, it's only worth half of what it could be worth",
            "You should be proud of picking this up at a garage sale for $20.",
            "The price of these items goes down every day.",
            "There are only 12 of these in the world so these are highly collectible.",
            "I'm sorry but this item is basically worthless",
            "These items are very popular at auction, as you can imagine.",
            "There's a possibility this was once part of a three piece set.",
            "Can you explain the hidden meaning of the item to me? I'm just not getting it.",
            "I'll give this item an appraisal but it would be worthless to anyone besides you.",
            "This is a clear facsimile. You can tell by the poor workmanship.",
            "This piece defies description.",
            "Simultaneously the epitome of baroque, avant-garde and post-modern art.",
            "They say 'One man's trash is another man's treasure', but this is simply garbage.",
            "After hearing you describe it I was expecting something larger.",
            "How is this legal?",
            "Please keep this dangerous item locked up.",
            "I didn't think any of these survived the Great Disaster of 2017.",
            "You truly have a marvel here. You should be proud.",
            "Your kids will be happy selling this after you die.",
            "You decidedly got the better end of the divorce.",
            "These colors speak to me.",
            "I thought this piece was worthless but it's actually a masterpiece.",
            "I think I'm going to faint.",
            "The evaluation of this item is largely dependent on how much cocaine is left in it.",
            "This item is priceless. Now let's chop it in half and see what's inside.",
            "You won't believe what I found in the secret compartment underneath.",
            "If you inspect it closely you'll see there are still some unidentified stains.",
            "Is it true that you used this as a shoe?",
            "It is an incredible feat that you smuggled this across the border undetected.",
            "Don't stare at it too long, it will slowly your life force.",
            "Golly it's just like in the song!",
            "The real value of this piece is the friends you made along the way.",
            "I had a little trouble identifying this one.",
            "I knew exactly what this piece was as soon as I saw it.",
            "Items like this from this era are quite common.",
            "Other appraisors wouldn't be able to get such an accurate evaluation.",
            "I'm literally guessing here.",
            "Did you know this was my specialty?",
            "I'm pretty sure it's broken.",
            "The color is a key part in finding the value of items like this.",
            "The colors here are staggering.",
            "This item appears to be one thing, but I was able to figure out what it truly is.",
        };

        public static string Get(Random random)
        {
            int index = random.Next(0, comments.Length);
            return comments[index];
        }
    }
}