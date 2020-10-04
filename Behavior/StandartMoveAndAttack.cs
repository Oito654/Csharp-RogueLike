using First_Rogue.Core;
using First_Rogue.Interfaces;
using First_Rogue.Systems;
using RogueSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace First_Rogue.Behavior
{
    public class StandardMoveAndAttack : IBehavior
    {
        public bool Act(Monster monster, CommandSystem commandSystem)
        {
            DungeonMap dungeonMap = Game.DungeonMap;
            Player player = Game.Player;
            FieldOfView monsterFov = new FieldOfView(dungeonMap);

            if (!monster.TurnsAlerted.HasValue)
            {
                monsterFov.ComputeFov(monster.X, monster.Y, monster.Awareness, true);
                if (monsterFov.IsInFov(player.X, player.Y))
                {
                    Game.MessageLog.Add($"{monster.Name} está em busca do {player.Name} para lutar ");
                    monster.TurnsAlerted = 1;
                }
            }

            if (monster.TurnsAlerted.HasValue)
            {
                dungeonMap.SetIsWalkable(monster.X, monster.Y, true);
                dungeonMap.SetIsWalkable(player.X, player.Y, true);

                PathFinder pathFinder = new PathFinder(dungeonMap);
                Path path = null;

                try
                {
                    path = pathFinder.ShortestPath(
                        dungeonMap.GetCell(monster.X, monster.Y),
                        dungeonMap.GetCell(player.X, player.Y));
                }
                catch (PathNotFoundException)
                {
                    Game.MessageLog.Add($"{monster.Name} espera uma oportunidade para avançar");

                }

                dungeonMap.SetIsWalkable(monster.X, monster.Y, false);
                dungeonMap.SetIsWalkable(player.X, player.Y, false);

                if(path != null)
                {
                    try
                    {
                        //TODO: path.SetpFoward() RogueSharo 4.0
                        commandSystem.MoveMonster(monster, path.Steps.First());
                    }
                    catch (NoMoreStepsException)
                    {
                        Game.MessageLog.Add($"{monster.Name} grita em frustração");
                    }
                }

                monster.TurnsAlerted++;

                if(monster.TurnsAlerted > 15)
                {
                    monster.TurnsAlerted = null;
                }
            }
            return true;
        }
    }
}
