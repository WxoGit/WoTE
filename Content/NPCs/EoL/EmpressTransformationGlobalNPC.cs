﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public class EmpressTransformationGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override bool PreAI(NPC npc)
        {
            if (npc.type == NPCID.HallowBoss)
            {
                npc.Transform(ModContent.NPCType<EmpressOfLight>());
                return false;
            }

            return true;
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {

        }
    }
}
