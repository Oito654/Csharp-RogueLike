using First_Rogue.Core;
using First_Rogue.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace First_Rogue.Interfaces
{
    public interface IBehavior
    {
        bool Act(Monster monster, CommandSystem commandSystem);
    }
}
