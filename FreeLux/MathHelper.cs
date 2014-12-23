using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace FreeLux
{
    internal static class MathHelper
    {
        private const int InternalDfgId = 3128;
        private const int InternalDfgRange = 750;
        private const int InternalBasicAttackRange = 550;
        private const int InternalIgniteRange = 600;

        private static Obj_AI_Hero Player
        {
            get { return FreeLux.Player; }
        }

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (FreeLux.Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (FreeLux.E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (FreeLux.R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            if (Items.HasItem(3128))
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg);
                damage = damage * 1.2;
            }

            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            damage += Player.GetAutoAttackDamage(enemy, true) * 2;

            if (Actions.HasIllumination(enemy))
            {
                damage += 10 + 8 * Player.Level + (Player.FlatMagicDamageMod + Player.BaseAbilityDamage)*0.2d;
            }

            return (float) damage;
        }
    }
}
