﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace TextAdventure_GameEngine
{
    public class Room
    {
        private string _filePath;
        private string _description;
        private List<Exit> _exits;
        private List<Item> _items;

        public string FilePath { get { return _filePath; } }

        public Room(string filePath)
        {
            _filePath = filePath;

            var data = XElement.Load(_filePath);
            _description = data.Element("description").Value;
            _exits = (from exit in data.Elements("exit")
                     select new Exit
                     {
                         Keyword = exit.Element("keyword").Value,
                         Description = exit.Element("description").Value,
                         Destination = exit.Element("destination").Value
                     }).ToList();

            _items = (from item in data.Elements("item")
                     select new Item
                     {
                         Keyword = item.Element("keyword").Value,
                         Description = item.Element("description").Value,
                         DetailedDescription = item.Element("detailedDescription").Value,
                         IsTakeable = item.Element("isTakeable").Value == "true"
                     }).ToList();

            Console.WriteLine(Describe());
        }

        public string Describe()
        {
            string combinedText = _description + "\n";

            foreach (Exit exit in _exits)
            {
                combinedText += exit.Description + "\n";
            }

            foreach (Item item in _items)
            {
                combinedText += item.Description + "\n";
            }

            return combinedText;
        }

        public void AddItem(Item item)
        {
            _items.Add(item);
            Save();
        }

        public bool HasExit(string keyword)
        {
            foreach (Exit exit in _exits)
            {
                if (exit.Keyword == keyword) return true;
            }
            return false;
        }

        public bool HasItem(string keyword)
        {
            foreach (Item item in _items)
            {
                if (item.Keyword == keyword) return true;
            }
            return false;
        }

        public Item GetItem(string keyword)
        {
            foreach (Item item in _items)
            {
                if (item.Keyword == keyword) return item;
            }
            return null;
        }

        public void RemoveItem(Item item)
        {
            _items.Remove(item);
            Save();
        }

        public Room Exit(string keyword)
        {
            foreach (Exit exit in _exits)
            {
                if (exit.Keyword == keyword) return new Room(exit.Destination);
            }
            return this;
        }

        private void Save()
        {
            var data = new XElement("root", new XElement("description", _description));
            data = AddExitsToXElement(data);
            data = AddItemsToXElement(data);
            data.Save(_filePath);
        }

        private XElement AddExitsToXElement(XElement xElement)
        {
            foreach (Exit exit in _exits)
            {
                xElement.Add(new XElement("exit",
                    new XElement("keyword", exit.Keyword),
                    new XElement("description", exit.Description),
                    new XElement("destination", exit.Destination)
                    ));
            }

            return xElement;
        }

        private XElement AddItemsToXElement(XElement xElement)
        {
            foreach (Item item in _items)
            {
                xElement.Add(new XElement("item",
                    new XElement("keyword", item.Keyword),
                    new XElement("description", item.Description),
                    new XElement("detailedDescription", item.DetailedDescription),
                    new XElement("isTakeable", item.IsTakeable.ToString().ToLower())
                    ));
            }

            return xElement;
        }
    }
}
