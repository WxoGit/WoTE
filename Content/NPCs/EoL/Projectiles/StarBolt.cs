﻿using System;
using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.Particles;

namespace WoTE.Content.NPCs.EoL.Projectiles
{
    public class StarBolt : ModProjectile, IPixelatedPrimitiveRenderer, IProjOwnedByBoss<EmpressOfLight>
    {
        public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeProjectiles;

        /// <summary>
        /// How long this bolt has existed, in frames.
        /// </summary>
        public ref float Time => ref Projectile.ai[0];

        /// <summary>
        /// How long this bolt should last, in frames.
        /// </summary>
        public static int Lifetime => Utilities.SecondsToFrames(2.4f);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1100;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.timeLeft = Lifetime;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            if (Time >= 120)
                Projectile.velocity *= 0.65f;
            else if (Projectile.velocity.Length() <= 70f)
                Projectile.velocity *= 1.07f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.velocity.Length() >= 12f)
            {
                float sinusoidalAngle = CalculateSinusoidalOffset(0.4f) * 0.7f;
                Vector2 particleVelocity = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(sinusoidalAngle) * Main.rand.NextFloat(2.5f, 3.3f) + Main.rand.NextVector2Circular(1.6f, 1.6f);
                Color particleColor = Main.hslToRgb(Main.rand.NextFloat(0.93f, 1.15f) % 1f, 1f, 0.7f) * 0.8f;
                BloomCircleParticle particle = new(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), particleVelocity, 0.028f, Color.Wheat, particleColor, 60, 1.8f, 1.75f);
                particle.Spawn();
            }

            Time++;
        }

        public float BoltWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width;
            float tipCutFactor = Utilities.InverseLerp(0.02f, 0.134f, completionRatio);
            float slownessFactor = Utils.Remap(Projectile.velocity.Length(), 3f, 9f, 0.18f, 1f);
            return baseWidth * tipCutFactor * slownessFactor;
        }

        public Color BoltColorFunction(float completionRatio)
        {
            float sineOffset = CalculateSinusoidalOffset(completionRatio);
            return Color.Lerp(Color.White, Color.Black, sineOffset * 0.5f + 0.5f);
        }

        public float CalculateSinusoidalOffset(float completionRatio)
        {
            return MathF.Sin(MathHelper.TwoPi * completionRatio + Main.GlobalTimeWrappedHourly * -12f + Projectile.identity) * Utilities.InverseLerp(0.01f, 0.9f, completionRatio);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            ManagedShader trailShader = ShaderManager.GetShader("WoTE.PrismaticBoltShader");
            trailShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly * 1.2f + Projectile.identity * 1.9f);
            trailShader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(TextureAssets.Extra[ExtrasID.FlameLashTrailShape], 2, SamplerState.LinearWrap);
            trailShader.SetTexture(TextureAssets.Projectile[Type], 3, SamplerState.LinearWrap);
            trailShader.Apply();

            float perpendicularOffset = Utils.Remap(Projectile.velocity.Length(), 4f, 20f, 14f, 56f);
            Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * perpendicularOffset;
            Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
            for (int i = 0; i < trailPositions.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float sine = CalculateSinusoidalOffset(i / (float)trailPositions.Length);
                trailPositions[i] = Projectile.oldPos[i] + perpendicular * sine;
            }

            PrimitiveSettings settings = new(BoltWidthFunction, BoltColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(trailPositions, settings, 25);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float hue = (Projectile.identity * 0.23f + Main.GlobalTimeWrappedHourly * 0.5f).Modulo(1f);
            Color baseColor = Main.hslToRgb(hue, 1f, 0.85f);
            baseColor.A = 0;

            Texture2D glowTexture = TextureAssets.Extra[98].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTexture.Size() * 0.5f;
            float pulse = MathHelper.Lerp(0.8f, 1.2f, Utilities.Cos01(Main.GlobalTimeWrappedHourly % 30f * MathHelper.TwoPi * 6f));
            float appearanceInterpolant = Utilities.InverseLerpBump(0f, 30f, Lifetime - 20f, Lifetime, Time) * pulse * 0.8f;
            Color outerColor = baseColor * appearanceInterpolant;
            Color innerColor = baseColor * appearanceInterpolant * 0.5f;
            Vector2 largeScale = new Vector2(0.8f, 6f) * appearanceInterpolant;
            Vector2 smallScale = new Vector2(0.8f, 2f) * appearanceInterpolant;
            Main.EntitySpriteDraw(glowTexture, drawPosition, null, outerColor, MathHelper.PiOver2, origin, largeScale, 0);
            Main.EntitySpriteDraw(glowTexture, drawPosition, null, outerColor, 0f, origin, smallScale, 0);
            Main.EntitySpriteDraw(glowTexture, drawPosition, null, innerColor, MathHelper.PiOver2, origin, largeScale * 0.6f, 0);
            Main.EntitySpriteDraw(glowTexture, drawPosition, null, innerColor, 0f, origin, smallScale * 0.6f, 0);

            return false;
        }
    }
}
