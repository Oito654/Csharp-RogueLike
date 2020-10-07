using RogueSharp;
using System;
using System.Collections.Generic;
using System.Text;
using RLNET;
using System.Linq;
using First_Rogue.Interfaces;

namespace First_Rogue.Core
{
    public class DungeonMap : Map
    {
        private readonly List<Monster> _monsters;
        private readonly List<TreasurePile> _treasurePiles;
        public List<Rectangle> Rooms;
        public List<Door> Doors { get; set; }
        public Stairs StairsUp { get; set; }
        public Stairs StairsDown { get; set; }

        public DungeonMap()
        {
            Game.SchedulingSystem.Clear();
            Rooms = new List<Rectangle>();
            _monsters = new List<Monster>();
            _treasurePiles = new List<TreasurePile>();
            Doors = new List<Door>();
        }

        public void UpdatePlayerFieldOfView()
        {
            Player player = Game.Player;
            ComputeFov(player.X, player.Y, player.Awareness, true);

            foreach (Cell cell in GetAllCells())
            {
                if (IsInFov(cell.X, cell.Y))
                {
                    SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                }
            }
        }

        public void Draw(RLConsole mapConsole, RLConsole statConsole)
        {
            foreach (Cell cell in GetAllCells())
            {
                SetConsoleSymbolForCell(mapConsole, cell);
            }

            int i = 0;

            foreach (Monster monster in _monsters)
            {
                monster.Draw(mapConsole, this);

                if (IsInFov(monster.X, monster.Y))
                {
                    monster.DrawStats(statConsole, i);
                    i++;
                }
            }

            foreach (Door door in Doors)
            {
                door.Draw(mapConsole, this);
            }
            StairsUp.Draw(mapConsole, this);
            StairsDown.Draw(mapConsole, this);

            foreach(TreasurePile treasurePile in _treasurePiles)
            {
                IDrawable drawableTreasure = treasurePile.Treasure as IDrawable;
                drawableTreasure?.Draw(mapConsole, this);
            }
        }

        private void SetConsoleSymbolForCell(RLConsole console, Cell cell)
        {
            if (!cell.IsExplored)
            {
                return;
            }

            if (IsInFov(cell.X, cell.Y))
            {
                if (cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.FloorFov, Colors.FloorBackgroundFov, '.');
                }
                else
                {
                    console.Set(cell.X, cell.Y, Colors.WallFov, Colors.WallBackgroundFov, '#');
                }
            }
            else
            {
                if (cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.Floor, Colors.FloorBackground, '.');
                }
                else
                {
                    console.Set(cell.X, cell.Y, Colors.Wall, Colors.WallBackground, '#');
                }
            }
        }

        public bool SetActorPosition(Actor actor, int x, int y)
        {
            if (GetCell(x, y).IsWalkable)
            {
                SetIsWalkable(actor.X, actor.Y, true);
                OpenDoor(actor, x, y);

                actor.X = x;
                actor.Y = y;

                SetIsWalkable(actor.X, actor.Y, false);
                OpenDoor(actor, x, y);

                if (actor is Player)
                {
                    UpdatePlayerFieldOfView();
                }
                return true;
            }
            return false;
        }

        public void SetIsWalkable(int x, int y, bool isWalkable)
        {
            Cell cell = GetCell(x, y);
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
        }

        public void AddGold(int x, int y, int amount)
        {
            if(amount > 0)
            {
                AddTreasure(x, y, new Gold(amount));
            }
        }

        public void AddTreasure(int x, int y, ITreasure treasure)
        {
            _treasurePiles.Add(new TreasurePile(x, y, treasure));
        }

        public void AddPlayer(Player player)
        {
            Game.Player = player;
            SetIsWalkable(player.X, player.Y, false);
            UpdatePlayerFieldOfView();
            Game.SchedulingSystem.Add(player);
        }

        public void AddMonster(Monster monster)
        {
            _monsters.Add(monster);

            SetIsWalkable(monster.X, monster.Y, false);
            Game.SchedulingSystem.Add(monster);
        }

        public Point GetRandomWalkableSpace(Rectangle room)
        {
            if (DoesRoomHaveWalkableSpacec(room))
            {
                for (int i = 0; i < 100; i++)
                {
                    int x = Game.Random.Next(1, room.Width - 2) + room.X;
                    int y = Game.Random.Next(1, room.Height - 2) + room.Y;

                    if (IsWalkable(x, y))
                    {
                        return new Point(x, y);
                    }
                }
            }

            return null;
        }

        public bool DoesRoomHaveWalkableSpacec(Rectangle room)
        {
            for (int x = 1; x <= room.Width - 2; x++)
            {
                for (int y = 1; y <= room.Height - 2; y++)
                {
                    if (IsWalkable(x + room.X, y + room.Y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void RemoveMonster(Monster monster)
        {
            _monsters.Remove(monster);
            SetIsWalkable(monster.X, monster.Y, true);
            Game.SchedulingSystem.Remove(monster);
        }

        public Monster GetMonsterAt(int x, int y)
        {
            return _monsters.FirstOrDefault(m => m.X == x && m.Y == y);
        }

        public Door GetDoor(int x, int y)
        {
            return Doors.SingleOrDefault(d => d.X == x && d.Y == y);
        }

        private void OpenDoor(Actor actor, int x, int y)
        {
            Door door = GetDoor(x, y);
            if (door != null && !door.IsOpen)
            {
                door.IsOpen = true;
                if (door != null && !door.IsOpen)
                {
                    door.IsOpen = true;
                    var cell = GetCell(x, y);

                    SetCellProperties(x, y, true, cell.IsWalkable, cell.IsExplored);

                    Game.MessageLog.Add($"{actor.Name} abriu a porta");
                }

            }
        }

        private void PickUpTreasure(Actor actor, int x, int y)
        {
            List<TreasurePile> treasureAtLocation = _treasurePiles.Where(g => g.X == x && g.Y == y).ToList();
            foreach(TreasurePile treasurePile in treasureAtLocation)
            {
                if (treasurePile.Treasure.PickUp(actor))
                {
                    _treasurePiles.Remove(treasurePile);
                }
            }
        }

        public bool CanMoveDownToNextLevel()
        {
            Player player = Game.Player;
            return StairsDown.X == player.X && StairsDown.Y == player.Y;
        }
    }
}
