﻿using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// How long the Empress waits before exploding into butterflies during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_ButterflyTransitionDelay => Utilities.SecondsToFrames(0.33f);

        /// <summary>
        /// How long the Empress' butterflies spend redirecting during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_RedirectTime => Utilities.SecondsToFrames(0.9f);

        /// <summary>
        /// How long the Empress' butterflies spend repositioning for the dash during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_DashRepositionTime => Utilities.SecondsToFrames(0.1f);

        /// <summary>
        /// How long the Empress' butterflies spend dashing during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_DashTime => Utilities.SecondsToFrames(0.22f);

        /// <summary>
        /// How long the Empress' butterflies spend slowing down after a dash during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_DashSlowdownTime => Utilities.SecondsToFrames(0.3f);

        /// <summary>
        /// The amount of dashes that should be performed during the Empress' Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_DashCount => 4;

        /// <summary>
        /// The amount of butterflies the Empress explodes into during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_ButterflyCount => 40;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_ButterflyBurstDashes()
        {
            StateMachine.RegisterTransition(EmpressAIType.ButterflyBurstDashes, null, false, () =>
            {
                return AITimer >= ButterflyBurstDashes_ButterflyTransitionDelay + 5 && !NPC.AnyNPCs(ModContent.NPCType<Lacewing>());
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.ButterflyBurstDashes, DoBehavior_ButterflyBurstDashes);
        }

        /// <summary>
        /// Performs the Empress' Butterfly Burst Dashes attack.
        /// </summary>
        public void DoBehavior_ButterflyBurstDashes()
        {
            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;

            if (AITimer >= ButterflyBurstDashes_ButterflyTransitionDelay)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == ButterflyBurstDashes_ButterflyTransitionDelay)
                {
                    for (int i = 0; i < ButterflyBurstDashes_ButterflyCount; i++)
                    {
                        float offsetAngle = Main.rand.NextFloatDirection() * 0.5f;
                        if (i >= ButterflyBurstDashes_ButterflyCount / 2)
                            offsetAngle += MathHelper.Pi;

                        int lacewingIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Lacewing>(), NPC.whoAmI, offsetAngle, i);
                        if (lacewingIndex >= 0 && lacewingIndex < Main.maxNPCs)
                            Main.npc[lacewingIndex].velocity = Main.rand.NextVector2Circular(38f, 24f);
                    }
                }

                NPC.Opacity = Utilities.InverseLerp(32f, 0f, NPC.CountNPCS(ModContent.NPCType<Lacewing>()));
                if (NPC.Opacity <= 0f)
                    NPC.Center = Target.Center - Vector2.UnitY * 300f;

                NPC.hide = NPC.Opacity <= 0f;
                NPC.ShowNameOnHover = NPC.Opacity >= 0.4f;
                NPC.dontTakeDamage = NPC.Opacity <= 0.5f;
            }

            NPC.velocity *= 0.9f;
            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }
    }
}
