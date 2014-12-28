#region Includes
using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
#endregion

namespace FreeLux
{
    internal static class FreeLux
    {
        private const string ChampionName = "Lux";
        private static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Player = ObjectManager.Player;
        public static Spell Q, W, E, R;
        public static SpellSlot IgniteSlot;
        public static Items.Item DeathfireGrasp;
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

            /* Items */
            DeathfireGrasp = new Items.Item(3128, 750);

            /* Menu */
            #region Menu
            Menu = new Menu("Free" + ChampionName, ChampionName, true);

            Menu orbwalkerMenu = new Menu("Orbwakler", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            Menu targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Menu.AddSubMenu(targetSelectorMenu);

            Menu comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("comboQ", "Use Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboOnlyUltToKill", "Only use R to kill").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboIgnite", "Use Ignite").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboDFG", "Use DFG in Combo").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            Menu laneClearMenu = new Menu("Lane Clear", "Lane Clear");
            laneClearMenu.AddItem(new MenuItem("laneClearQ", "Use Q").SetValue(false));
            laneClearMenu.AddItem(new MenuItem("laneClearE", "Use E").SetValue(true));
            laneClearMenu.AddItem(new MenuItem("laneClearENumber", "Min # of Minions in E Width to Use E:").SetValue(new Slider(4, 1, 10)));
            laneClearMenu.AddItem(new MenuItem("laneClearMinMana", "Lane Clear Min Mana %").SetValue(new Slider(60)));
            Menu.AddSubMenu(laneClearMenu);

            Menu mixedMenu = new Menu("Harass/Mixed", "Mixed");
            mixedMenu.AddItem(new MenuItem("mixedQ", "Use Q").SetValue(true));
            mixedMenu.AddItem(new MenuItem("mixedE", "Use E").SetValue(false));
            mixedMenu.AddItem(new MenuItem("mixedQE", "Use E if Q lands").SetValue(true));
            mixedMenu.AddItem(new MenuItem("mixedMinMana", "Harass Min Mana %").SetValue(new Slider(50)));
            Menu.AddSubMenu(mixedMenu);

            Menu autoShieldMenu = new Menu("Auto Shield", "Auto Shield");
            autoShieldMenu.AddItem(new MenuItem("selfAutoShield", "Automatically Shield Self").SetValue(true));
            autoShieldMenu.AddItem(new MenuItem("selfAutoShieldPercentage", "Self Auto Sheild Min HP %")).SetValue(new Slider(30));
            autoShieldMenu.AddItem(new MenuItem("selfAutoShieldMinMana", "Self Auto Shield Min Mana %")).SetValue(new Slider(20));
            autoShieldMenu.AddItem(new MenuItem("allyAutoShield", "Automatically Shield Allies").SetValue(new KeyBind('H', KeyBindType.Toggle)));
            autoShieldMenu.AddItem(new MenuItem("allyAutoShieldPercentage", "Ally Auto Shield Min HP %")).SetValue(new Slider(40));
            autoShieldMenu.AddItem(new MenuItem("allyAutoShieldMinMana", "Ally Auto Shield Min Mana %")).SetValue(new Slider(40));
            Menu.AddSubMenu(autoShieldMenu);

            Menu potionManagerMenu = new Menu("Potion Manager","Potion Manager");
            PotionHelper.AddToMenu(potionManagerMenu);
            Menu.AddSubMenu(potionManagerMenu);

            Menu drawingMenu = new Menu("Drawing", "Drawing");
            drawingMenu.AddItem(new MenuItem("drawEnabled", "Drawings Enabled").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawQ", "Draw Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "Draw W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawE", "Draw E").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawR", "Draw R").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawMinimapR", "Draw R on Minimap").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawFullComboKillIndicator", "Draw Combo Kill Indicator").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawCurrentMode", "Draw Current Mode").SetValue(true));
            Menu.AddSubMenu(drawingMenu);

            Menu otherMenu = new Menu("Other", "Other");
            otherMenu.AddItem(new MenuItem("packetCast", "Use Packet Casting?").SetValue(false));
            otherMenu.AddItem(new MenuItem("RKillSteal", "Use R to Kill Steal").SetValue(true));
            otherMenu.AddItem(new MenuItem("killRecalling", "Use R to Kill Recalling Enemies").SetValue(true));;
            otherMenu.AddItem(new MenuItem("autoQGapcloser", "Auto Q Gapclosers").SetValue(true));
            Menu.AddSubMenu(otherMenu);
            Menu.AddToMainMenu();
            #endregion

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            GameObject.OnCreate += Actions.GameObject_OnCreate;
            GameObject.OnDelete += Actions.GameObject_OnDelete;
            AntiGapcloser.OnEnemyGapcloser += Actions.AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnTeleport += Actions.Obj_AI_Base_OnTeleport;

            Game.PrintChat("FreeLux loaded. Tactical decision, summoner!");
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {
            PacketCast = Menu.Item("packetCast").GetValue<bool>();
            Console.Clear();

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

            // We don't want to interrupt our back ;)
            if (!Player.IsRecalling())
            {
                if (Menu.Item("allyAutoShield").GetValue<KeyBind>().Active)
                    Actions.AutoShieldAlly();
                if (Menu.Item("selfAutoShield").GetValue<bool>())
                    Actions.AutoShieldSelf();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead || !Menu.Item("drawEnabled").GetValue<bool>())
                return;

            bool drawQ = Menu.Item("drawQ").GetValue<bool>();
            bool drawW = Menu.Item("drawW").GetValue<bool>();
            bool drawE = Menu.Item("drawE").GetValue<bool>();
            bool drawR = Menu.Item("drawR").GetValue<bool>();
            bool drawMinimapR = Menu.Item("drawMinimapR").GetValue<bool>();
            bool drawCurrentMode = Menu.Item("drawCurrentMode").GetValue<bool>();

            Color color = Color.Green;
            Vector3 playerPosition = ObjectManager.Player.Position;
            var playerPositionOnScreen = Drawing.WorldToScreen(playerPosition);

            if (drawQ) Utility.DrawCircle(playerPosition, Q.Range, color);
            if (drawW) Utility.DrawCircle(playerPosition, W.Range, color);
            if (drawE) Utility.DrawCircle(playerPosition, E.Range, color);
            if (drawR) Utility.DrawCircle(playerPosition, R.Range, color);

            if (drawCurrentMode)
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 36, playerPositionOnScreen.Y + 41, Color.Black, "Mode: Combo");
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 35, playerPositionOnScreen.Y + 40, Color.Lime, "Mode: Combo");
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 36, playerPositionOnScreen.Y + 41, Color.Black, "Mode: Lane Clear");
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 35, playerPositionOnScreen.Y + 40, Color.Lime, "Mode: Lane Clear");
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 36, playerPositionOnScreen.Y + 41, Color.Black, "Mode: Mixed");
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 35, playerPositionOnScreen.Y + 40, Color.Lime, "Mode: Mixed");
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 36, playerPositionOnScreen.Y + 41, Color.Black, "Mode: Last Hit");
                        Drawing.DrawText(
                            playerPositionOnScreen.X - 35, playerPositionOnScreen.Y + 40, Color.Lime, "Mode: Last Hit");
                        break;
                }
            }

            foreach (var h in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy))
            {
                var hero = h;
                var enemyPositionOnScreen = Drawing.WorldToScreen(hero.Position);
                if (!hero.IsDead)
                {
                    Drawing.DrawText(
                        enemyPositionOnScreen.X - 36, enemyPositionOnScreen.Y + 41, Color.Black,
                        MathHelper.GetDamageString(hero));
                    Drawing.DrawText(
                        enemyPositionOnScreen.X - 35, enemyPositionOnScreen.Y + 40, Color.OrangeRed,
                        MathHelper.GetDamageString(hero));
                }
            }

            // I think this will draw the range of Final Spark on the minimap?
            if (/*Player.Level >= 6 &&*/ drawMinimapR)
                Utility.DrawCircle(playerPosition, R.Range, Color.DeepSkyBlue, 2, 30, true);
            
        }
    }
}
