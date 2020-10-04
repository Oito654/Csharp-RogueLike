using First_Rogue.Core;
using First_Rogue.Interfaces;
using RogueSharp;
using RogueSharp.DiceNotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace First_Rogue.Systems
{
    public class CommandSystem
    {
        public bool IsPlayerTurn { get; set; }

        public bool MovePlayer(Direction direction)
        {
            int x = Game.Player.X;
            int y = Game.Player.Y;

            switch (direction)
            {
                case Direction.Up:
                    {
                        y = Game.Player.Y - 1;
                        break;
                    }
                   case Direction.Down:
                    {
                        y = Game.Player.Y + 1;
                        break;
                    }
                case Direction.Left:
                    {
                        x = Game.Player.X - 1;
                        break;
                    }
                case Direction.Right:
                    {
                        x = Game.Player.X + 1;
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            if(Game.DungeonMap.SetActorPosition(Game.Player, x, y))
            {
                return true;
            }

            Monster monster = Game.DungeonMap.GetMonsterAt(x, y);

            if(monster != null)
            {
                Attack(Game.Player, monster);
                return true;
            }

            return false;
        }

        private void Attack(Actor attacker, Actor defender)
        {
            StringBuilder attackMessage = new StringBuilder();
            StringBuilder defenseMessage = new StringBuilder();

            int hits = ResolveAttack(attacker, defender, attackMessage);
            int blocks = ResolveDefense(defender, hits, attackMessage, defenseMessage);

            Game.MessageLog.Add(attackMessage.ToString());
            if (!string.IsNullOrWhiteSpace(defenseMessage.ToString()))
            {
                Game.MessageLog.Add(defenseMessage.ToString());
            }

            int damage = hits - blocks;

            ResolveDamage(defender, damage);

        }

        private static int ResolveAttack(Actor attacker, Actor defender, StringBuilder attackMessage)
        {
            int hits = 0;

            attackMessage.AppendFormat("{0} ataca {1} e rola: ", attacker.Name, defender.Name);

            DiceExpression attackDice = new DiceExpression().Dice(attacker.Attack, 100);
            DiceResult attackResult = attackDice.Roll();

            foreach(TermResult termResult in attackResult.Results)
            {
                attackMessage.Append(termResult.Value + ", ");

                if(termResult.Value >= 100 - attacker.AttackChance)
                {
                    hits++;
                }
            }

            return hits;
        }



        private static int ResolveDefense(Actor defender, int hits, StringBuilder attackMessage, StringBuilder defenseMessage)
        {
            int blocks = 0;

            if(hits > 0)
            {
                attackMessage.AppendFormat("atingindo {0} golpes", hits);
                defenseMessage.AppendFormat(" {0} defende e rola: ", defender.Name);

                DiceExpression defenseDice = new DiceExpression().Dice(defender.Defense, 100);
                DiceResult defenseRoll = defenseDice.Roll();

                foreach(TermResult termResult in defenseRoll.Results)
                {
                    defenseMessage.Append(termResult.Value + ", ");

                    if(termResult.Value >= 100 - defender.DefenseChance)
                    {
                        blocks++;
                    }
                }

                defenseMessage.AppendFormat("resultando em {0} bloqueios", blocks);
            }
            else
            {
                attackMessage.Append("e erra completamente");
            }

            return blocks;
        }

        private static void ResolveDamage(Actor defender, int damage)
        {
            if(damage > 0)
            {
                defender.Health = defender.Health - damage;

                Game.MessageLog.Add($" {defender.Name} foi atingido por {damage} de dano");

                if(defender.Health <= 0)
                {
                    ResolveDeath(defender);
                }
            }
            else
            {
                Game.MessageLog.Add($"  {defender.Name} bloqueou todo o dano");
            }
        }

        private static void ResolveDeath(Actor defender)
        {
            if(defender is Player)
            {
                Game.MessageLog.Add($" {defender.Name} foi morto, GAME OVER!");
            }
            else if(defender is Monster)
            {
                Game.DungeonMap.RemoveMonster((Monster)defender);

                Game.MessageLog.Add($" {defender.Name} morreu e derrubou {defender.Gold} ouro");
            }
        }

        public void EndPlayerTurn()
        {
            IsPlayerTurn = false;
        }

        public void ActivateMonster()
        {
            IScheduleable scheduleable = Game.SchedulingSystem.Get();

            if(scheduleable is Player)
            {
                IsPlayerTurn = true;
                Game.SchedulingSystem.Add(Game.Player);
            }
            else
            {
                Monster monster = scheduleable as Monster;

                if(monster != null)
                {
                    monster.PerformAction(this);
                    Game.SchedulingSystem.Add(monster);
                }

                ActivateMonster();
            }
        }

        public void MoveMonster(Monster monster, Cell cell)
        {
            if (!Game.DungeonMap.SetActorPosition(monster, cell.X, cell.Y))
            {
                if (Game.Player.X == cell.X && Game.Player.Y == cell.Y)
                {
                    Attack(monster, Game.Player);
                }
            }
        }
    }
}
