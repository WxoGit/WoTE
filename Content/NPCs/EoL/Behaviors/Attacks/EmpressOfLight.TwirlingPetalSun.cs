﻿using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// The amount of time the Empress waits before she summons petals.
        /// </summary>
        public static int TwirlingPetalSun_PetalSummonDelay => Utilities.SecondsToFrames(1f);

        /// <summary>
        /// The amount of time petals summoned by the Empress should spend twirling.
        /// </summary>
        public static int TwirlingPetalSun_TwirlTime => Utilities.SecondsToFrames(1.3f);

        /// <summary>
        /// The amount of time petals summoned by the Empress should spend transforming into flames.
        /// </summary>
        public static int TwirlingPetalSun_FlareTransformTime => Utilities.SecondsToFrames(0.8f);

        /// <summary>
        /// The amount of time fire petals summoned by the Empress should spend retracting inward.
        /// </summary>
        public static int TwirlingPetalSun_FlareRetractTime => Utilities.SecondsToFrames(0.24f);

        /// <summary>
        /// The amount of time fire petals summoned by the Empress should spend bursting outward.
        /// </summary>
        public static int TwirlingPetalSun_BurstTime => Utilities.SecondsToFrames(0.1f);

        /// <summary>
        /// The amount of time the Empress should wait after the twirling petals burst to choose a new attack.
        /// </summary>
        public static int TwirlingPetalSun_AttackTransitionDelay => Utilities.SecondsToFrames(2f);

        /// <summary>
        /// The amount of petals the Empress summons during the Twirling Petal Sun attack.
        /// </summary>
        public static int TwirlingPetalSun_PetalCount => 8;

        /// <summary>
        /// The amount of prismatic bolt created during the Twirling Petal Sun attack when the petal flares burst outward.
        /// </summary>
        public static int TwirlingPetalSun_PrismaticBoltCount => 13;

        /// <summary>
        /// The maximum angular spin speed of petals summoned by the Empress.
        /// </summary>
        public static float TwirlingPetalSun_PetalSpinSpeed => MathHelper.ToRadians(1f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_TwirlingPetalSun()
        {
            StateMachine.RegisterTransition(EmpressAIType.TwirlingPetalSun, null, false, () =>
            {
                return AITimer >= TwirlingPetalSun_PetalSummonDelay + TwirlingPetalSun_TwirlTime + TwirlingPetalSun_FlareTransformTime + TwirlingPetalSun_FlareRetractTime + TwirlingPetalSun_BurstTime + TwirlingPetalSun_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.TwirlingPetalSun, DoBehavior_TwirlingPetalSun);
        }

        /// <summary>
        /// Performs the Empress' Twirling Petal Sun attack.
        /// </summary>
        public void DoBehavior_TwirlingPetalSun()
        {
            LeftHandFrame = EmpressHandFrame.HandPressedToChest;
            RightHandFrame = EmpressHandFrame.HandPressedToChest;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;

            if (AITimer == TwirlingPetalSun_PetalSummonDelay)
            {
                SoundEngine.PlaySound(SoundID.Item159 with { MaxInstances = 0 });

                float petalOffsetAngle = NPC.AngleTo(Target.Center) + MathHelper.Pi / TwirlingPetalSun_PetalCount;
                for (int i = 0; i < TwirlingPetalSun_PetalCount; i++)
                {
                    float petalDirection = MathHelper.TwoPi * i / TwirlingPetalSun_PetalCount + petalOffsetAngle;
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DazzlingPetal>(), DazzlingPetalDamage, 0f, -1, petalDirection);
                }
            }

            if (AITimer == TwirlingPetalSun_PetalSummonDelay + TwirlingPetalSun_TwirlTime + TwirlingPetalSun_FlareTransformTime + TwirlingPetalSun_FlareRetractTime + TwirlingPetalSun_BurstTime)
            {
                SoundEngine.PlaySound(SoundID.Item74);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < TwirlingPetalSun_PrismaticBoltCount; i++)
                    {
                        Vector2 boltVelocity = (MathHelper.TwoPi * i / TwirlingPetalSun_PrismaticBoltCount).ToRotationVector2() * 12f + Main.rand.NextVector2Circular(3.5f, 3.5f);
                        Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target);
                    }
                }
            }

            NPC.Center = Vector2.Lerp(NPC.Center, Target.Center, 0.013f);
            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(Target.Center) * 2f, 0.03f);
        }
    }
}
