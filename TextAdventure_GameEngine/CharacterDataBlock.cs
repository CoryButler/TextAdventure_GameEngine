﻿using System.Collections.Generic;

namespace TextAdventure_GameEngine
{
    public class CharacterDataBlock
    {
        public string Id { get; set; }
        public int CurrentDescription { get; set; }
        public int CurrentDetailedDescription { get; set; }
        public int CurrentDialogue { get; set; }
        public List<string> Descriptions { get; set; }
        public List<string> DetailedDescriptions { get; set; }
        public List<string> Dialogues { get; set; }
    }
}