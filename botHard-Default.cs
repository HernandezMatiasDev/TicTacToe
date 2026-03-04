using System;

namespace TaTeTi_1._0
{
    public class BotHardDefault : BotExpert
    {

        public override byte[] playing(bool player)
        {
            // This is a bot to test the defaultTurn function (a function of the expert bot) on its own

            // bot botTestDefault (83,00%) vs BotEasy (8,70%)
            // bot botTestDefault (41,50%) vs BotMedium (14,50%)
            // bot botTestDefault (29,90%) vs BotHard (25,60%)
            // bot botTestDefault (00,00%) vs BotExpert (38,00%)

            return defaultTurn(player);
        }
    }
}
