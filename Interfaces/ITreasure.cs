using System;
using System.Collections.Generic;
using System.Text;

namespace First_Rogue.Interfaces
{
    public interface ITreasure
    {
        bool PickUp(IActor actor);
    }
}
