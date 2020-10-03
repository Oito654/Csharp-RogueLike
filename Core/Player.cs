using RLNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace First_Rogue.Core
{
    public class Player : Actor
    {
        public Player()
        {
            Attack = 2;
            AttackChance = 50;
            Awareness = 15;
            Color = Colors.Player;
            Defense = 2;
            DefenseChance = 40;
            Gold = 0;
            Health = 100;
            MaxHealth = 100;
            Name = "Errante";
            Speed = 10;
            Symbol = '@';
        }

        public void DrawStats(RLConsole statConsole)
        {
            statConsole.Print(1, 1, $"Nome:    {Name}", Colors.Text);
            statConsole.Print(1, 3, $"Vida:  {Health}/{MaxHealth}", Colors.Text);
            statConsole.Print(1, 5, $"Ataque:  {Attack} ({AttackChance}%)", Colors.Text);
            statConsole.Print(1, 7, $"Defesa: {Defense} ({DefenseChance}%)", Colors.Text);
            statConsole.Print(1, 9, $"Ouro:    {Gold}", Colors.Gold);
        }
    }
}
