namespace ServerLagger
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Menu;
    using Menu = Ensage.Common.Menu.Menu;
    using MenuItem = Ensage.Common.Menu.MenuItem;

    internal class Program
    {
        private static bool _loaded;
        private static Hero _me;
        private static readonly Menu Menu = new Menu("LAGGER", "LG", true);
        private static void Main()
        {

            var laggermenu = new Menu("Lagger 1", "Lagger");
            Menu.AddItem(new MenuItem("Lagger.Enable", "Enable lagger").SetValue(false)).DontSave();
            Menu.AddItem(new MenuItem("Lagger.Count", "Count").SetValue(new Slider(4000, 1, 50000))).DontSave();
            Menu.AddItem(new MenuItem("Lagger.Delay", "Delay").SetValue(new Slider(100, 1, 1000))).DontSave();
            Menu.AddItem(new MenuItem("Lagger.CMD", "CMD").SetValue(new StringList(new[] { "god", "kill", "explode" }, 0))).DontSave();
             
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
            Thread myThread = new Thread(func);
            myThread.Start();
            Console.WriteLine("LAGGER injected");
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!_loaded)
            {
                _me = ObjectMgr.LocalHero;
                if (!Game.IsInGame || _me == null)
                {
                    return;
                }
                // sv_cheats анлок на всякий случай
                var list = new Dictionary<string, int>
                {
                    { "sv_cheats", 1 }
                };
                foreach (var data in list)
                {
                    var var = Game.GetConsoleVar(data.Key);
                    var.RemoveFlags(ConVarFlags.Cheat);
                    var.SetValue(data.Value);
                }
                _loaded = true;
                Console.WriteLine("> LAGGER Loaded");
            }

            if (!Game.IsInGame || _me == null)
            {
                _loaded = false;
                Menu.Item("Lagger.Enable").SetValue(false);
                Console.WriteLine("> LAGGER Unloaded");
                return;
            }
        }

        static void func()
        {
            while (true)
            {
                if (!_loaded) continue;

                if (Menu.Item("Lagger.Enable").GetValue<bool>())
                {
                    if (!Utils.SleepCheck("update"))
                        continue;

                    var count = Menu.Item("Lagger.Count").GetValue<Slider>().Value;
                    var delay = Menu.Item("Lagger.Delay").GetValue<Slider>().Value;
                    string str = Menu.Item("Lagger.CMD").GetValue<StringList>().SelectedValue;

                    for (var i = 0; i < count; i++)
                        Game.ExecuteCommand(str);

                    Utils.Sleep(delay, "update");
                }
            }
        }
    }
}

