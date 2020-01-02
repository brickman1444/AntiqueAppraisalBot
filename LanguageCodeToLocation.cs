namespace AppraisalBot
{
    public static class LanguageCodeToLocation
    {
        public static string LookUp(string languageCode)
        {
            switch (languageCode)
            {
                case "unk": return null; // unknown
                case "zh-Hans": return "China"; //(ChineseSimplified)
                case "zh-Hant": return "China";//(ChineseTraditional)
                case "cs": return "Czechia";//(Czech)
                case "da": return "Denmark"; //(Danish)
                case "nl": return "Netherlands"; //(Dutch)
                case "en": return "United States"; //(English)
                case "fi": return "Finland"; //(Finnish)
                case "fr": return "France"; //(French)
                case "de": return "Germany"; //(German)
                case "el": return "Greece"; //(Greek)
                case "hu": return "Hungary"; //(Hungarian)
                case "it": return "Italy"; //(Italian)
                case "ja": return "Japanese"; //(Japanese)
                case "ko": return "Korea"; //(Korean)
                case "nb": return "Norway"; //(Norwegian)
                case "pl": return "Poland"; //(Polish)
                case "pt": return "Portugal"; //(Portuguese,
                case "ru": return "Russia"; //(Russian)
                case "es": return "Mexico"; //(Spanish)
                case "sv": return "Sweden"; //(Swedish)
                case "tr": return "Turkey"; //(Turkish)
                case "ar": return "Pakistan"; //(Arabic)
                case "ro": return "Romania"; //(Romanian)
                case "sr-Cyrl": return "Serbia"; //(SerbianCyrillic)
                case "sr-Latn": return "Serbia"; //(SerbianLatin)
                case "sk": return "Slovakia"; //(Slovak)
            }

            return null;
        }
    }
}