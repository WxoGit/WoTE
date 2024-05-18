﻿using System.Collections.Generic;
using System.Linq;
using Luminance.Common.StateMachines;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// The set of phase 1 attack combinations that may be selected.
        /// </summary>
        public static List<List<EmpressAIType>> Phase1AttackCombos => new()
        {
            new() { EmpressAIType.BasicPrismaticBolts, EmpressAIType.ButterflyBurstDashes },
            new() { EmpressAIType.RadialStarBurst, EmpressAIType.TwirlingPetalSun, EmpressAIType.PrismaticBoltDashes },
            new() { EmpressAIType.OutwardRainbows, EmpressAIType.SequentialDashes, EmpressAIType.ConvergingTerraprismas },
            new() { EmpressAIType.OutwardRainbows, EmpressAIType.ButterflyBurstDashes },
            new() { EmpressAIType.ConvergingTerraprismas, EmpressAIType.RadialStarBurst },
        };

        /// <summary>
        /// The set of phase 2 attack combinations that may be selected.
        /// </summary>
        public static List<List<EmpressAIType>> Phase2AttackCombos => new()
        {
            new() { EmpressAIType.BasicPrismaticBolts, EmpressAIType.ButterflyBurstDashes, EmpressAIType.OrbitReleasedTerraprismas },
            new() { EmpressAIType.BasicPrismaticBolts2, EmpressAIType.ButterflyBurstDashes, EmpressAIType.OrbitReleasedTerraprismas },
            new() { EmpressAIType.RadialStarBurst, EmpressAIType.TwirlingPetalSun, EmpressAIType.PrismaticBoltDashes },
            new() { EmpressAIType.OutwardRainbows, EmpressAIType.SequentialDashes, EmpressAIType.ConvergingTerraprismas },
            new() { EmpressAIType.OutwardRainbows, EmpressAIType.ButterflyBurstDashes, EmpressAIType.OrbitReleasedTerraprismas },
            new() { EmpressAIType.ConvergingTerraprismas, EmpressAIType.RadialStarBurst, EmpressAIType.BasicPrismaticBolts2 },
            new() { EmpressAIType.PrismaticBoltSpin, EmpressAIType.OrbitReleasedTerraprismas, EmpressAIType.PrismaticBoltDashes },
            new() { EmpressAIType.LanceWallSupport },
        };

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_ResetCycle()
        {
            StateMachine.RegisterTransition(EmpressAIType.ResetCycle, null, false, () => true, () =>
            {
                StateMachine.StateStack.Clear();

                var phaseCycle = ChooseNextCycle();
                for (int i = phaseCycle.Count - 1; i >= 0; i--)
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[phaseCycle[i]]);
            });
        }

        /// <summary>
        /// Selects a given value based on the Empress' current phase.
        /// </summary>
        /// <typeparam name="T">The type of value to use/</typeparam>
        /// <param name="phase1Value">The value to use in the first phase.</param>
        /// <param name="phase2Value">The value to use in the second phase.</param>
        public T ByPhase<T>(T phase1Value, T phase2Value)
        {
            if (Phase2)
                return phase2Value;

            return phase1Value;
        }

        /// <summary>
        /// Chooses an attack state cycle that the empress should perform at random, based on phase.
        /// </summary>
        /// <returns>The new attack state cycle to perform.</returns>
        public List<EmpressAIType> ChooseNextCycle()
        {
            List<EmpressAIType> phaseCycle;

            int tries = 0;
            var statesToAvoid = PreviousStatesReversed.Take(ByPhase(3, 4));
            do
            {
                phaseCycle = Main.rand.Next(Phase2 ? Phase2AttackCombos : Phase1AttackCombos);
                tries++;
            }
            while (!StateCycleIsValid(statesToAvoid, phaseCycle) && tries <= 50);

            phaseCycle = [EmpressAIType.EventideLances];

            if (phaseCycle[0] == EmpressAIType.LanceWallSupport)
            {
                phaseCycle.Insert(1, Main.rand.Next(AcceptableAttacksForLanceWallSupport));
                PreviousStates.Add(phaseCycle[1]);
            }

            return phaseCycle;
        }

        public bool StateCycleIsValid(IEnumerable<EmpressAIType> statesToAvoid, List<EmpressAIType> chosenCycle)
        {
            if (PrismaticOverload_ShouldntDoButterflyDashes && chosenCycle.Contains(EmpressAIType.ButterflyBurstDashes))
                return false;

            if (statesToAvoid.Any(chosenCycle.Contains))
                return false;

            return true;
        }
    }
}
