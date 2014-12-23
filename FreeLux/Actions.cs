using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;

namespace FreeLux
{
    internal static class Actions
    {
        private const string InternalPassiveName = "luxilluminatingfraulein";
        private const string InternalEName = "LuxLightstrike_tar_green";
        private const string InternalBindingName = "LuxLightBindingMis";
        private const int InternalDfgId = 3128;
        private const int InternalDfgRange = 750;
        private const int InternalBasicAttackRange = 550;
        private const int InternalIgniteRange = 600;
        private static GameObject luxEObject;

        public static void Combo()
        {
            bool useQ, useW, useE, useR, useDFG, useIgnite;
            useQ = FreeLux.Menu.Item("comboQ").GetValue<bool>();
            useW = FreeLux.Menu.Item("comboW").GetValue<bool>();
            useE = FreeLux.Menu.Item("comboE").GetValue<bool>();
            useR = FreeLux.Menu.Item("comboR").GetValue<bool>();
            useDFG = FreeLux.Menu.Item("comboDFG").GetValue<bool>();
            useIgnite = FreeLux.Menu.Item("comboIgnite").GetValue<bool>();

            var target = SimpleTs.GetTarget(FreeLux.Q.Range, SimpleTs.DamageType.Magical);

            if (target == null)
                return;

            if (luxEObject != null)
            {
                var targetsInE =
                    ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(luxEObject.BoundingRadius)).ToList();
                if (targetsInE.Any(t => !HasIllumination(t)))
                    FreeLux.E.Cast(FreeLux.PacketCast);
            }

            if (!target.IsValidTarget())
                return;

            if (HasIllumination(target) && target.IsValidTarget(InternalBasicAttackRange))
                FreeLux.Player.IssueOrder(GameObjectOrder.AttackUnit, target); 
            else if (HasIllumination(target))
                return;

            if (useDFG)
                if (Items.CanUseItem(InternalDfgId) && Items.HasItem(InternalDfgId) && target.IsValidTarget(InternalDfgRange))
                    Items.UseItem(InternalDfgId, target);

            if (FreeLux.Q.IsReady() && useQ && target.IsValidTarget(FreeLux.Q.Range) && !HasIllumination(target))
                CastQ(target);

            if (FreeLux.W.IsReady() && useW)
                FreeLux.W.Cast(target, FreeLux.PacketCast);

            if (FreeLux.E.IsReady() && useE && target.IsValidTarget(FreeLux.E.Range) && luxEObject == null)
            {
                if (HasLightBinding(target))
                    FreeLux.E.Cast(target, FreeLux.PacketCast);
                else
                {
                    FreeLux.E.CastIfHitchanceEquals(target, HitChance.High, FreeLux.PacketCast);
                }
                if (HasIllumination(target) && target.IsValidTarget(InternalBasicAttackRange))
                    FreeLux.Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }

            if (target.IsDead)
                return;
            if (FreeLux.R.IsReady() || !useR || HasIllumination(target))
                return;

            // Maybe add an option to optimize AOE damage by trying to hit more than one enemy champion?

            if (FreeLux.Menu.Item("comboOnlyUltToKill").GetValue<bool>())
            {
                if (ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) >= target.Health)
                    FreeLux.R.Cast(target, FreeLux.PacketCast);
            }
            else
            {
                FreeLux.R.Cast(target, FreeLux.PacketCast);
            }

            if (useIgnite && IsIgniteKillable(FreeLux.Player, target))
            {
                if (FreeLux.IgniteSlot != SpellSlot.Unknown &&
                    FreeLux.Player.SummonerSpellbook.CanUseSpell(FreeLux.IgniteSlot) == SpellState.Ready &&
                    target.IsValidTarget(InternalIgniteRange))
                    FreeLux.Player.SummonerSpellbook.CastSpell(FreeLux.IgniteSlot, target);
            }
        }

        public static void Mixed()
        {
            throw new NotImplementedException();
        }

        public static void KillSteal()
        {
            throw new NotImplementedException();
        }

        public static void LaneClear()
        {
            throw new NotImplementedException();
        }

        public static void AutoShieldAlly()
        {
            int autoShieldPercentage = FreeLux.Menu.Item("allyAutoShieldPercentage").GetValue<int>();
            if (FreeLux.W.IsReady() &&
                FreeLux.Player.Mana / FreeLux.Player.MaxMana * 100 >=
                FreeLux.Menu.Item("allyAutoShieldMinMana").GetValue<Slider>().Value)
            {
                var leastHealthAllyInRange =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(a => a.IsAlly && a.Distance(FreeLux.Player.Position) < FreeLux.W.Range)
                        .OrderBy(a => a.Health / a.MaxHealth * 100)
                        .FirstOrDefault();

                if (leastHealthAllyInRange != null && leastHealthAllyInRange.Health < autoShieldPercentage)
                    FreeLux.W.Cast(leastHealthAllyInRange, FreeLux.PacketCast);
            }
        }

        public static void AutoShieldSelf()
        {
            int autoShieldPercentage = FreeLux.Menu.Item("selfAutoShieldPercentage").GetValue<int>();
            if (FreeLux.W.IsReady() &&
                FreeLux.Player.Mana / FreeLux.Player.MaxMana * 100 >=
                FreeLux.Menu.Item("selfAutoShieldMinMana").GetValue<Slider>().Value)
            {
                // Check to see if there is an ally in range that we can shield too, since we're already trying to shield ourself.
                var leastHealthAllyInRange =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(a => a.IsAlly && a.Distance(FreeLux.Player.Position) < FreeLux.W.Range)
                        .OrderBy(a => a.Health / a.MaxHealth * 100)
                        .FirstOrDefault();
                if (leastHealthAllyInRange != null && leastHealthAllyInRange.Health < autoShieldPercentage)
                    FreeLux.W.Cast(leastHealthAllyInRange, FreeLux.PacketCast);
                else
                {
                    // If not, just go ahead and shield ourself
                    FreeLux.W.Cast(Game.CursorPos,FreeLux.PacketCast);
                }
            }
        }


        public static bool HasIllumination(Obj_AI_Base target)
        {
            return target.HasBuff(InternalPassiveName);
        }

        public static bool HasLightBinding(Obj_AI_Base target)
        {
            return target.HasBuff(InternalBindingName);
        }

        private static bool IsIgniteKillable(Obj_AI_Hero source, Obj_AI_Base target)
        {
            return Damage.GetSummonerSpellDamage(source, target, Damage.SummonerSpell.Ignite) >= target.Health;
        }

        public static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains(InternalEName))
                luxEObject = sender;
        }

        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains(InternalEName))
                luxEObject = null;
        }

        public static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (FreeLux.Menu.Item("autoQGapcloser").GetValue<bool>())
                FreeLux.Q.Cast(gapcloser.Sender, FreeLux.PacketCast);
        }

        // Q Logic from ChewyMoonsLux
        // Source: https://github.com/ChewyMoon/ChewyMoonScripts/blob/master/ChewyMoonsLux/SpellCombo.cs
        public static bool AnalyzeQ(PredictionInput input, PredictionOutput output)
        {
            var posList = new List<Vector3> { ObjectManager.Player.ServerPosition, output.CastPosition };
            var collision = Collision.GetCollision(posList, input);
            var minions = collision.Count(collisionObj => collisionObj.IsMinion);
            return minions > 1;
        }

        public static void CastQ(Obj_AI_Hero target)
        {
            Console.Clear();

            var prediction = FreeLux.Q.GetPrediction(target, true);
            var minions = prediction.CollisionObjects.Count(thing => thing.IsMinion);

            /*if (FreeLux.Debug)
            {
                Console.WriteLine("Minions: {0}\nToo Many: {1}", minions, minions > 1);
            }*/

            if (minions > 1)
                return;

            //FreeLux.Q.Cast(prediction.CastPosition, FreeLux.PacketCast);
            if (prediction.Hitchance == HitChance.High)
                FreeLux.Q.Cast(prediction.CastPosition, FreeLux.PacketCast);
        }
    }
}
