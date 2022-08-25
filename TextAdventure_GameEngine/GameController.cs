using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextAdventure_GameEngine
{
    public class GameController
    {
        private Player _player;
        private TextInput _textInput;
        private Room _room;
        private bool _isGameOver = false;
        private GameLog _gameLog;

        public List<UserAction> AvailableActions = new List<UserAction>() { new Add(), new Check(), new Describe(), new Destroy(), new Discard(), new Drop(), new DropSilent(), new Go(), new Hint(), new Help(), new Inventory(), new JoinParty(), new LeaveParty(), new Open(), new Restart(), new Take(), new Talk(), new Use() };

        public GameController()
        {
            var save_dir = "Save";

            if (Directory.Exists(save_dir)) Directory.Delete(save_dir, true);

            _gameLog = new GameLog();
            _player = new Player() { Name = "Geralt", Gender = Genders.MALE, Gold = 12 };
            _player.Save();
            _textInput = new TextInput();
            _room = new Room("Village.xml", _gameLog, _player);
            _player.UpdateLocation(_room);

            while (!_isGameOver)
            {
                _gameLog.Write("\n >>");
                var input = Console.ReadLine();
                _gameLog.WriteSilent(input);
                var response = _textInput.Accept(input, this);
                if (response != "") _gameLog.Write(response);
            }
        }

        public void Add(string keyword)
        {
            if (!_room.HasCharacter(keyword))
            {
                var character = new Character(keyword, _room);
                character.IncNumVisits(_room);
                _room.AddCharacter(character);
            }
        }

        public void Check(string keyword)
        {
            if (_room.HasExit(keyword))
            {
                Exit exit = _room.GetExit(keyword);
                _gameLog.Write(exit.OnCheck);
            }
            else if (_room.HasCharacter(keyword))
            {
                Character character = _room.GetCharacter(keyword);
                _gameLog.Write(character.DescribeDetails(_room));
            }
            else if (_room.HasContainer(keyword))
            {
                Container container = _room.GetContainer(keyword);
                _gameLog.Write(container.OnCheck);
            }
            else if (_room.HasProp(keyword))
            {
                Prop prop = _room.GetProp(keyword);
                _gameLog.Write(prop.OnCheck);
            }
            else if (_room.HasItem(keyword))
            {
                Item item = _room.GetItem(keyword);
                _gameLog.Write(item.OnCheck);
            }
            else if (keyword != "")
            {
                _gameLog.Write("There is no " + keyword + " to examine.");
            }
            else
            {
                _gameLog.Write("You cannot check nothing.  Specify something to check.");
            }
        }

        public void Describe(string keyword)
        {
            if (_room.HasCharacter(keyword))
            {
                Character character = _room.GetCharacter(keyword);
                _gameLog.Write(character.Describe(_room));
            }
        }

        public void Destroy(string keyword)
        {
            if (_room.HasCharacter(keyword))
            {
                var character = _room.GetCharacter(keyword);
                LeaveParty(keyword);
                _room.DestroyCharacter(character);
            }
            if (_room.HasProp(keyword))
            {
                var prop = _room.GetProp(keyword);
                LeaveParty(keyword);
                _room.DestroyProp(prop);
            }
        }

        public void Discard(string keyword)
        {
            if (_player.HasItem(keyword))
            {
                Item item = _player.GetItem(keyword);
                _player.RemoveItem(item);
            }
            else if (keyword != "")
            {
                _gameLog.Write("There is no " + keyword + " in your inventory.");
            }
            else
            {
                _gameLog.Write("You cannot discard nothing.  Specify something to discard.");
            }
        }

        public void Drop(string keyword)
        {
            if (_player.HasItem(keyword))
            {
                Item item = _player.GetItem(keyword);
                _room.AddItem(item);
                _player.RemoveItem(item);
                _gameLog.Write("You drop the " + keyword + ".");
            }
            else if (keyword != "")
            {
                _gameLog.Write("There is no " + keyword + " in your inventory.");
            }
            else
            {
                _gameLog.Write("You cannot drop nothing.  Specify something to drop.");
            }
        }

        public void DropSilent(string keyword)
        {
            if (_player.HasItem(keyword))
            {
                Item item = _player.GetItem(keyword);
                _room.AddItem(item);
                _player.RemoveItem(item);
            }
        }

        public void Go(string keyword)
        {
            if (_room.HasExit(keyword))
            {
                if (_room.GetExit(keyword).IsLocked)
                {
                    _gameLog.Write("The " + keyword + " is locked.");
                }
                else
                {
                    bool isRestarting = false;
                    var actions = _room.GetExit(keyword).OnGo.Split('|');
                    foreach (var action in actions)
                    {
                        if (action == "") continue;
                        var response = _textInput.Accept(action, this);
                        if (response != "") _gameLog.Write(response);
                        if (action.StartsWith("restart")) isRestarting = true;
                    }

                    foreach (var character in _player.Party) character.Save();

                    if (isRestarting)
                    {
                        //TODO: save game log
                        return;
                    }

                    _gameLog.Write("");
                    Console.Clear();

                    _room = _room.Exit(keyword, _player);
                    _player.UpdateLocation(_room);
                }
            }
            else if (keyword != "")
            {
                _gameLog.Write("You cannot go to the " + keyword + ".");
            }
            else
            {
                _gameLog.Write("You cannot go nowhere.  Specify somewhere to go.");
            }
        }

        public void Hint()
        {
            _gameLog.Write(_room.GetHint());
        }

        public void Help()
        {
            _gameLog.Write(_room.GetHint());
        }

        public void IncDescription(string keyword)
        {
            if (_room.HasCharacter(keyword))
            {
                var character = _room.GetCharacter(keyword);
                character.IncDescription(_room);
            }
        }

        public void IncDetailedDescription(string keyword)
        {
            if (_room.HasCharacter(keyword))
            {
                var character = _room.GetCharacter(keyword);
                character.IncDetailedDescription(_room);
            }
        }

        public void IncDialogue(string keyword)
        {
            if (_room.HasCharacter(keyword))
            {
                var character = _room.GetCharacter(keyword);
                character.IncDialogue(_room);
            }
        }

        public void Inventory()
        {
            string inventory = "Name: " + _player.Name + "\n";// + "Gold: " + _player.Gold + "";
            foreach (Item item in _player.Items)
            {
                inventory += item.Keyword + ": " + item.OnCheck + "\n";
            }

            _gameLog.Write(inventory);
        }

        public void JoinParty(string keyword)
        {
            if (_room.HasCharacter(keyword))
            {
                var character = _room.GetCharacter(keyword);
                if (!_player.Party.Contains(character))
                    _player.JoinParty(character);
            }
        }

        public void LeaveParty(string keyword)
        {
            if (_room.HasCharacter(keyword))
            {
                var character = _room.GetCharacter(keyword);
                if (_player.Party.Contains(character))
                    _player.LeaveParty(character);
            }
        }

        public void Open(string keyword)
        {
            if (!_room.HasContainer(keyword))
            {
                _gameLog.Write("There is no " + keyword + ".");
                return;
            }
            else if (_room.GetContainer(keyword).Inventory.Count == 0)
            {
                _gameLog.Write("The " + keyword + " is empty.");
                return;
            }
            _gameLog.Write("Opening the " + keyword + " reveals:\n" + _room.GetContainer(keyword).ListInventory());
        }

        public void Restart()
        {
            Directory.Delete("Save", true);
            Console.Clear();
            _isGameOver = true;
        }

        public void Take(string keyword)
        {
            if (_room.HasItem(keyword))
            {
                Item item = _room.GetItem(keyword);
                _player.AddItem(item);
                _room.RemoveItem(item);
                _gameLog.Write("You take the " + keyword + ".");
                var actions = item.OnTake.Split('|');
                foreach (var action in actions)
                {
                    if (action == "") continue;
                    var response = _textInput.Accept(action, this);
                    if (response != "") _gameLog.Write(response);
                }
            }
            else if (_room.Containers.Any(c => c.Inventory.Any(i => i.Keyword == keyword)))
            {
                Container container = _room.Containers.Where(c => c.Inventory.Any(i => i.Keyword == keyword)).ToList()[0];
                Item item = container.GetItem(keyword);
                _player.AddItem(item);
                container.RemoveItem(item);
                _gameLog.Write("You take the " + keyword + " from the " + container.Keyword + ".");
                var actions = item.OnTake.Split('|');
                foreach (var action in actions)
                {
                    if (action == "") continue;
                    var response = _textInput.Accept(action, this);
                    if (response != "") _gameLog.Write(response);
                }
            }
            else if (_room.HasProp(keyword))
            {
                _gameLog.Write("You cannot take the " + keyword + ".");
            }
            else if (_room.HasCharacter(keyword))
            {
                _gameLog.Write("You cannot take " + keyword + ".");
            }
            else if (keyword != "")
            {
                _gameLog.Write("There is no " + keyword + " to take.");
            }
            else
            {
                _gameLog.Write("You cannot take nothing.  Specify something to take.");
            }
        }

        public void Talk(string keyword)
        {
            if (_room.HasCharacter(keyword))
            {
                Character character = _room.GetCharacter(keyword);
                _gameLog.Write(character.Talk(_room));
            }
            else if (keyword != "")
            {
                _gameLog.Write("You cannot talk to the " + keyword + ".");
            }
            else
            {
                _gameLog.Write("You cannot talk to no one.  Specify someone to talk to.");
            }
        }

        public void Use(string keyword, string targetKeyword)
        {
            Item item;
            if (_player.HasItem(keyword))
            {
                item = _player.GetItem(keyword);
            }
            else if (keyword != "")
            {
                _gameLog.Write("There is no " + keyword + " in your inventory.");
                return;
            }
            else
            {
                _gameLog.Write("You cannot use nothing.  Specify something to use.");
                return;
            }            
            
            InteractableObject target;
            if (_room.HasExit(targetKeyword))
            {
                target = _room.GetExit(targetKeyword);
            }
            else if (_room.HasCharacter(targetKeyword))
            {
                target = _room.GetCharacter(targetKeyword);
            } else if (_room.HasProp(targetKeyword))
            {
                target = _room.GetProp(targetKeyword);
            }
            else if (_room.HasItem(targetKeyword))
            {
                target = _room.GetItem(targetKeyword);
            }
            else if (targetKeyword != "")
            {
                _gameLog.Write("There is no " + targetKeyword + ".");
                return;
            }
            else
            {
                _gameLog.Write("Yor cannot use the " + keyword + " on nothing.  Specify a target: use " + keyword + " tagret");
                return;
            }

            if (target.WantsItemId != item.Id)
            {
                _gameLog.Write("You cannot use the " + keyword + " on the " + targetKeyword + ".");
                return;
            }

            _gameLog.Write("You used the " + keyword + " on the " + targetKeyword + ".");
            var actions = target.OnUse.Split('|');
            foreach (var action in actions)
            {
                if (action == "") continue;
                var response = _textInput.Accept(action, this);
                if (response != "") _gameLog.Write(response);
            }
        }
    }
}