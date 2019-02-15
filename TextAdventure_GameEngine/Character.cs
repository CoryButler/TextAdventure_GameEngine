﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace TextAdventure_GameEngine
{
    public class Character : InteractableObject
    {
        public string DisplayName { get; private set; }
        public Room CurrentRoom { get; set; }

        public string WantsItemId
        {
            get
            {
                try
                {
                    var i = _dataBlocks.IndexOf(_dataBlocks.Where(db => db.Id == CurrentRoom.Id).ToList()[0]);
                    return _dataBlocks[i].WantsItemId;
                }
                catch { return ""; }
            }
        }

        public string OnUse
        {
            get
            {
                try
                {
                    var i = _dataBlocks.IndexOf(_dataBlocks.Where(db => db.Id == CurrentRoom.Id).ToList()[0]);
                    return _dataBlocks[i].OnUse;
                }
                catch { return ""; }
            }
        }

        private List<CharacterDataBlock> _dataBlocks;

        public Character(string keyword, Room room)
        {
            Keyword = keyword;
            CurrentRoom = room;

            if (!File.Exists("Characters//" + Keyword + ".xml")) return;

            var characterData = XElement.Load("Characters//" + Keyword + ".xml");
            
            DisplayName = characterData.Element("displayName").Value;

            _dataBlocks = (from dataBlock in characterData.Elements("dataBlock")
                          select new CharacterDataBlock
                          {
                              Id = dataBlock.Element("id").Value,
                              WantsItemId = dataBlock.Elements("wantsItemId").Any() ? dataBlock.Element("wantsItemId").Value : "",
                              OnUse = dataBlock.Elements("onUse").Any() ? dataBlock.Element("onUse").Value : "",
                              NumVisits = dataBlock.Elements("numVisits").Any() ? int.Parse(dataBlock.Element("numVisit").Value) : 0,
                              CurrentDescription = int.Parse(dataBlock.Element("currentDescription").Value),
                              CurrentDetailedDescription = int.Parse(dataBlock.Element("currentDetailedDescription").Value),
                              CurrentDialogue = int.Parse(dataBlock.Element("currentDialogue").Value),

                              Descriptions = (from description in dataBlock.Elements("description")
                                              select new ConditionalData(
                                                description.Attribute("skipIf") != null ?
                                                    description.Attribute("skipIf").Value : "",
                                                description.Value)
                                              ).ToList(),

                              DetailedDescriptions = (from detailedDescription in dataBlock.Elements("detailedDescription")
                                              select new ConditionalData(
                                                detailedDescription.Attribute("skipIf") != null ?
                                                    detailedDescription.Attribute("skipIf").Value : "",
                                                detailedDescription.Value)
                                              ).ToList(),

                              Dialogues = (from dialogue in dataBlock.Elements("dialogue")
                                              select new ConditionalData(
                                                dialogue.Attribute("skipIf") != null ?
                                                    dialogue.Attribute("skipIf").Value : "",
                                                dialogue.Value)
                                              ).ToList()

                          }).ToList();
        }

        public void IncDetailedDescription(Room room)
        {
            try
            {
                var i = _dataBlocks.IndexOf(_dataBlocks.Where(db => db.Id == room.Id).ToList()[0]);
                _dataBlocks[i].CurrentDetailedDescription++;

                if (_dataBlocks[i].CurrentDetailedDescription == _dataBlocks[i].DetailedDescriptions.Count)
                    _dataBlocks[i].CurrentDetailedDescription--;
            }
            catch { /* NOOP */ }
        }

        public void IncDescription(Room room)
        {
            try
            {
                var i = _dataBlocks.IndexOf(_dataBlocks.Where(db => db.Id == room.Id).ToList()[0]);
                _dataBlocks[i].CurrentDescription++;

                if (_dataBlocks[i].CurrentDescription == _dataBlocks[i].Descriptions.Count)
                    _dataBlocks[i].CurrentDescription--;
            }
            catch { /* NOOP */ }
        }

        public void IncDialogue(Room room)
        {
            try
            {
                var i = _dataBlocks.IndexOf(_dataBlocks.Where(db => db.Id == room.Id).ToList()[0]);
                _dataBlocks[i].CurrentDialogue++;

                if (_dataBlocks[i].CurrentDialogue == _dataBlocks[i].Dialogues.Count)
                    _dataBlocks[i].CurrentDialogue--;
            }
            catch { /* NOOP */ }
        }

        public void IncNumVisits(Room room)
        {
            try
            {
                var i = _dataBlocks.IndexOf(_dataBlocks.Where(db => db.Id == room.Id).ToList()[0]);
                _dataBlocks[i].NumVisits++;
            }
            catch { /* NOOP */ }
        }

        public void Save()
        {
            var data = new XElement("root",
                new XElement("displayName", DisplayName));

            foreach (var dataBlock in _dataBlocks)
            {
                var block = new XElement("dataBlock",
                    new XElement("id", dataBlock.Id),
                    new XElement("numVisits", dataBlock.NumVisits),
                    new XElement("currentDescription", dataBlock.CurrentDescription),
                    new XElement("currentDetailedDescription", dataBlock.CurrentDetailedDescription),
                    new XElement("currentDialogue", dataBlock.CurrentDialogue));

                foreach (var description in dataBlock.Descriptions)
                    block.Add(new XElement("description", description));

                foreach (var detailedDescription in dataBlock.DetailedDescriptions)
                    block.Add(new XElement("detailedDescription", detailedDescription));

                foreach (var dialogue in dataBlock.Dialogues)
                    block.Add(new XElement("dialogue", dialogue));

                data.Add(block);
            }

            data.Save("Characters\\" + Keyword + ".xml");
        }

        public string Describe(Room room)
        {
            try
            {
                var i = _dataBlocks.IndexOf(_dataBlocks.First(db => db.Id == room.Id));
                var dataBlock = _dataBlocks[i];
                var currentDatum = dataBlock.CurrentDescription;
                var data = dataBlock.Descriptions;

                for (var j = currentDatum; j < data.Count; j++)
                {
                    if (!data[j].MeetsSkipConditions(dataBlock))
                    {
                        _dataBlocks[i].CurrentDescription = data.IndexOf(data[j]);
                        return data[j].Data;
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public string DescribeDetails(Room room)
        {
            try
            {
                var i = _dataBlocks.IndexOf(_dataBlocks.First(db => db.Id == room.Id));
                var dataBlock = _dataBlocks[i];
                var currentDatum = dataBlock.CurrentDetailedDescription;
                var data = dataBlock.DetailedDescriptions;

                for (var j = currentDatum; j < data.Count; j++)
                {
                    if (!data[j].MeetsSkipConditions(dataBlock))
                    {
                        _dataBlocks[i].CurrentDetailedDescription = data.IndexOf(data[j]);
                        return data[j].Data;
                    }
                }
                return "";
            }
            catch
            {
                return "";
            }
        }

        public string Talk(Room room)
        {
            try
            {
                var i = _dataBlocks.IndexOf(_dataBlocks.First(db => db.Id == room.Id));
                var dataBlock = _dataBlocks[i];
                var currentDatum = dataBlock.CurrentDialogue;
                var data = dataBlock.Dialogues;

                for (var j = currentDatum; j < data.Count; j++)
                {
                    if (!data[j].MeetsSkipConditions(dataBlock))
                    {
                        _dataBlocks[i].CurrentDialogue = data.IndexOf(data[j]);
                        return DisplayName + ": " + data[j].Data;
                    }
                }
                return "";
            }
            catch
            {
                return DisplayName + " has nothing to say.";
            }
        }
    }
}
