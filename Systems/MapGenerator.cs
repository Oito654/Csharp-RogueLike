using RogueSharp;
using System;
using System.Collections.Generic;
using System.Text;
using RLNET;
using System.Linq;
using RogueSharp.DiceNotation;
using First_Rogue.Monsters;

namespace First_Rogue.Core.Systems
{
    public class MapGenerator
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxRooms;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;

        private readonly DungeonMap _map;

        public MapGenerator(int width, int height, 
            int maxRooms, int roomMaxSize, int roomMinSIze)
        {
            _width = width;
            _height = height;
            _maxRooms = maxRooms;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSIze;
            _map = new DungeonMap();
        }

        public DungeonMap CreateMap()
        {
            _map.Initialize(_width, _height);
           
            for(int r = _maxRooms; r > 0; r--)
            {
                int roomWidth = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomHeight = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomXPosition = Game.Random.Next(0, _width - roomWidth - 1);
                int roomYPosition = Game.Random.Next(0, _height - roomHeight - 1);

                var newRoom = new Rectangle(roomXPosition, roomYPosition,
                    roomWidth, roomHeight);

                bool newRoomIntersects = _map.Rooms.Any(room => newRoom.Intersects(room));

                if (!newRoomIntersects)
                {
                    _map.Rooms.Add(newRoom);
                }
            }

            foreach (Rectangle room in _map.Rooms)
            {
                CreateRoom(room);
            }

            PlacePlayer();
            PlaceMonsters();

            for(int c = 1; c < _map.Rooms.Count; c++)
            {
                int previousRoomCenterX = _map.Rooms[c - 1].Center.X;
                int previousRoomCenterY = _map.Rooms[c - 1].Center.Y;
                int currentRoomCenterX = _map.Rooms[c].Center.X;
                int currentRoomCenterY = _map.Rooms[c].Center.Y;

                if(Game.Random.Next(1,2) == 1)
                {
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY);
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX);
                }
                else
                {
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX);
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY);
                }
            }

            return _map;
        }

        private void CreateRoom(Rectangle room)
        {
            for(int x = room.Left + 1; x < room.Right; x++)
            {
                for(int y = room.Top + 1; y < room.Bottom; y++)
                {
                    _map.SetCellProperties(x, y, true, true, false);
                }
            }
        }

        private void PlacePlayer()
        {
            Player player = Game.Player;
            if (player == null)
            {
                player = new Player();
            }

            player.X = _map.Rooms[0].Center.X;
            player.Y = _map.Rooms[0].Center.Y;

            _map.AddPlayer(player);
        }

        private void CreateHorizontalTunnel(int xStart, int xEnd, int yPosition)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                _map.SetCellProperties(x, yPosition, true, true);
            }
        }

        private void CreateVerticalTunnel(int yStart, int yEnd, int xPosition)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                _map.SetCellProperties(xPosition, y, true, true);
            }
        }

        private void PlaceMonsters()
        {
            foreach( var room in _map.Rooms)
            {
                if(Dice.Roll("1D10") < 7)
                {
                    var numberOfMonsters = Dice.Roll("1D4");
                    for(int i = 0; i < numberOfMonsters; i++)
                    {
                        Point randomLocation = _map.GetRandomWalkableSpace(room);

                        if(randomLocation != null)
                        {
                            var monster = Kobold.Create(1);
                            monster.X = randomLocation.X;
                            monster.Y = randomLocation.Y;
                            _map.AddMonster(monster);
                        }
                    }
                }
            }
        }

    }
}
