using LeagueSharp;
using LeagueSharp.Common;

namespace FreeLux
{
    internal static class MathHelper
    {
        private const int InternalBasicAttackRange = 550;
        private const int InternalIgniteRange = 600;

        private static Obj_AI_Hero Player
        {
            get { return FreeLux.Player; }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (FreeLux.Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (FreeLux.E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (FreeLux.R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            if (Items.HasItem(FreeLux.DeathfireGrasp.Id))
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg);
                damage = damage * 1.2;
            }

            if (ObjectManager.Player.GetSpellSlot("SummonerIgnite") != SpellSlot.Unknown)
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            //damage += Player.GetAutoAttackDamage(enemy, true) * 2;

            if (Actions.HasIllumination(enemy))
            {
                damage += GetPassiveProcDamage();
            }

            return (float) damage;
        }

        public static string GetDamageString(Obj_AI_Base enemy)
        {
            double qDamage, eDamage, rDamage, iDamage;
            string str = "";

            qDamage = (FreeLux.Q.IsReady()) ? Player.GetSpellDamage(enemy, SpellSlot.Q) : 0.0d;
            eDamage = (FreeLux.E.IsReady()) ? Player.GetSpellDamage(enemy, SpellSlot.E) : 0.0d;
            rDamage = (FreeLux.R.IsReady()) ? Player.GetSpellDamage(enemy, SpellSlot.R) : 0.0d;
            iDamage = (FreeLux.IgniteSlot.IsReady()) ? Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite) : 0.0d;


            if (enemy.Health < qDamage)
                str = "Q Kill!";
            else if (enemy.Health < eDamage)
                str = "E Kill!";
            else if (enemy.Health < rDamage)
                str = "R Kill!";
            else if (enemy.Health < iDamage)
                str = "Ignite Kill!";
            else if (enemy.Health < qDamage + iDamage)
                str = "Q+Ignite Kill!";
            else if (enemy.Health < eDamage + iDamage)
                str = "E+Ignite Kill!";
            else if (enemy.Health < rDamage + iDamage)
                str = "R+Ignite Kill!";
            else if (enemy.Health < qDamage + eDamage)
                str = "Q+E Kill!";
            else if (enemy.Health < qDamage + rDamage)
                str = "Q+R Kill!";
            else if (enemy.Health < eDamage + rDamage)
                str = "E+R Kill!";
            else if (enemy.Health < qDamage + eDamage + iDamage)
                str = "Q+E+Ignite Kill!";
            else if (enemy.Health < qDamage + rDamage + iDamage)
                str = "Q+R+Ignite Kill!";
            else if (enemy.Health < eDamage + rDamage + iDamage)
                str = "E+R+Ignite Kill!";
            else if (enemy.Health < qDamage + eDamage + rDamage)
                str = "Q+E+R Kill!";
            else if (enemy.Health < qDamage + eDamage + rDamage + iDamage)
                str = "Q+E+R+Ignite Kill!";
            else if (enemy.Health < GetComboDamage(enemy))
                str = "Full Combo Kill!";
            else
                str = "Cannot Kill!";
            return str;
        }

        public static double GetPassiveProcDamage()
        {
            return 10 + 8 * Player.Level + (Player.FlatMagicDamageMod + Player.BaseAbilityDamage) * 0.2d;
        }

        public static double GetIgniteDamage(Obj_AI_Hero enemy)
        {
            return Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
        }

        public static double GetRDamage(Obj_AI_Hero enemy)
        {
            return Player.GetSpellDamage(enemy, SpellSlot.R);
        }
    }
}
