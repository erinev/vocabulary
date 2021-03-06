﻿using System;
using SilverLinguo.Enums;

namespace SilverLinguo.Repositories.Models
{
    public class WordPair
    {
        public int Id { get; set; }
        public string FirstLanguageWord { get; set; }
        public string SecondLanguageWord { get; set; }
        public LanguagePair LanguagePair { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
