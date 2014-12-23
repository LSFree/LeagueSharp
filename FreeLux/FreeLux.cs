#region Includes
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using NewTargetSelector = LeagueSharp.Common.SimpleTs;
#endregion

namespace FreeLux
{
    internal class FreeLux
    {
        private static string ChampionName = "Lux";
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static SpellSlot IgniteSlot;
        public static bool PacketCast;
        public static Menu Menu;

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampionName)
                return;

            /* Spells */
            Q = new Spell(SpellSlot.Q, 1175); // Light Binding
            W = new Spell(SpellSlot.W, 1075); // Prismatic Barrier
            E = new Spell(SpellSlot.E, 1100); // Lucient Singularity
            R = new Spell(SpellSlot.R, 3340); // Final Spark

            // Spell.SetSkillshot(float delay, float width, float speed, bool collision, SkillshotType type)
            Q.SetSkillshot(0.25f, 80f, 1200f, true, SkillshotType.SkillshotLine); // to get collision objects
            W.SetSkillshot(0.25f, 150f, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 275f, 1300f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.35f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);

            /* Summoner Spells */
            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            /* Menu */
            #region Menu
            Menu = new Menu("Free" + ChampionName, ChampionName, true);

            Menu orbwalkerMenu = new Menu("Orbwakler", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            Menu targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu); // CHANGE THIS TO TargetSelector.AddToMenu(targetSelectorMenu) FOR 4.21
            Menu.AddSubMenu(targetSelectorMenu);

            Menu comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("comboQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboW", "Use W").SetValue(false));
            comboMenu.AddItem(new MenuItem("comboE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboOnlyUltToKill", "Only use R to kill").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboIgnite", "Use Ignite").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboDFG", "Use DFG in Combo").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            Menu laneClearMenu = new Menu("Lane Clear", "Lane Clear");
            comboMenu.AddItem(new MenuItem("laneClearQ", "Use Q").SetValue(false));
            comboMenu.AddItem(new MenuItem("laneClearE", "Use E").SetValue(true));
            Menu.AddSubMenu(laneClearMenu);

            Menu mixedMenu = new Menu("Harass/Mixed", "Mixed");
            comboMenu.AddItem(new MenuItem("mixedQ", "Use Q").SetValue(false));
            comboMenu.AddItem(new MenuItem("mixedE", "Use E").SetValue(true));
            Menu.AddSubMenu(mixedMenu);

            Menu autoShieldMenu = new Menu("Auto Shield", "Auto Shield");
            autoShieldMenu.AddItem(new MenuItem("allyAutoShield", "Automatically Shield Allies").SetValue(new KeyBind('h', KeyBindType.Toggle)));
            autoShieldMenu.AddItem(new MenuItem("allyAutoShieldPercentage", "Ally Auto Shield %")).SetValue(new Slider(40));
            autoShieldMenu.AddItem(new MenuItem("allyAutoShieldMinMana", "Ally Auto Shield Min Mana %")).SetValue(new Slider(40));
            autoShieldMenu.AddItem(new MenuItem("selfAutoShield", "Automatically Shield Self").SetValue(true));
            autoShieldMenu.AddItem(new MenuItem("selfAutoShieldPercentage", "Self Auto Sheild %")).SetValue(new Slider(40));
            autoShieldMenu.AddItem(new MenuItem("selfAutoShieldMinMana", "Self Auto Shield Min Mana %")).SetValue(new Slider(20));
            Menu.AddSubMenu(autoShieldMenu);

            Menu drawingMenu = new Menu("Drawing", "Drawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawFullComboKillIndicator", "Draw Combo Kill Indicator").SetValue(true));
            Menu.AddSubMenu(drawingMenu);

            Menu otherMenu = new Menu("Other", "Other");
            drawingMenu.AddItem(new MenuItem("packetCast", "Use Packet Casting?").SetValue(false));
            drawingMenu.AddItem(new MenuItem("RKillSteal", "Use R to Kill Steal").SetValue(true));
            drawingMenu.AddItem(new MenuItem("autoQGapcloser", "Auto Q Gapclosers").SetValue(true));
            Menu.AddSubMenu(otherMenu);
            Menu.AddToMainMenu();
            #endregion

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            GameObject.OnCreate += Actions.GameObject_OnCreate;
            GameObject.OnDelete += Actions.GameObject_OnDelete;
            AntiGapcloser.OnEnemyGapcloser += Actions.AntiGapcloser_OnEnemyGapcloser;

            Game.PrintChat("FreeLux loaded. Tactical decision, summoner!");
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            PacketCast = Menu.Item("packetCast").GetValue<bool>();

            if (Menu.Item("RKillSteal").GetValue<bool>()) Actions.KillSteal();

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Actions.Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Actions.LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Actions.Mixed();
                    break;
            }

            if (Menu.Item("allyAutoShield").GetValue<bool>()) Actions.AutoShieldAlly();
            if (Menu.Item("selfAutoShield").GetValue<bool>()) Actions.AutoShieldSelf();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            bool drawQ = Menu.Item("drawQ").GetValue<bool>();
            bool drawW = Menu.Item("drawW").GetValue<bool>();
            bool drawE = Menu.Item("drawE").GetValue<bool>();
            bool drawR = Menu.Item("drawR").GetValue<bool>();

            Color color = Color.Green;

            Vector3 playerPosition = ObjectManager.Player.Position;

            if (drawQ) Utility.DrawCircle(playerPosition, Q.Range, color);
            if (drawW) Utility.DrawCircle(playerPosition, W.Range, color);
            if (drawE) Utility.DrawCircle(playerPosition, E.Range, color);
            if (drawR) Utility.DrawCircle(playerPosition, R.Range, color);

            foreach (var h in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
            {
                var hero = h;
                var text = new Render.Text("", hero, new Vector2(20, 50), 18, new ColorBGRA(255, 255, 255, 255));
                text.VisibleCondition +=
                    s =>
                        Render.OnScreen(Drawing.WorldToScreen(hero.Position)) &&
                        Menu.Item("drawFullComboKillIndicator").GetValue<bool>();
                if (MathHelper.GetComboDamage(hero) >= hero.Health)
                    text.text = "Full Combo Kill!";
                else
                    text.text = "";
                text.OutLined = true;
                text.Add();
            }
        }
    }
}
