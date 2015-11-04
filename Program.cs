using System;
using System.Linq;
using Ensage;
using SharpDX;
using SharpDX.Direct3D9;

namespace RoshanTimer
{
    internal class Program
    {
        private static Font font;
        private static bool scriptLoaded;
        private static bool roshanDead;
        private static float roshanDeathTime;
        private static float roshanRespawnMinTime;
        private static float roshanRespawnMaxTime;

        private static void Main(string[] args)
        {
            Game.OnUpdate += Game_OnUpdate;
            Game.OnFireEvent += Game_OnGameEvent;

            font = new Font(
            Drawing.Direct3DDevice9,
            new FontDescription
            {

                FaceName = "Arial",
                Height = 17,
                OutputPrecision = FontPrecision.Outline,
                Quality = FontQuality.Default
            });

            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame)
            {
                if (scriptLoaded)
                {
                    scriptLoaded = false;
                    Console.WriteLine("> RoshanTimer unloaded");
                }
                return;
            }

            if (!scriptLoaded)
            {
                roshanRespawnMinTime = 0;
                roshanRespawnMaxTime = 0;
                roshanDead = false;
                scriptLoaded = true;
                Console.WriteLine("> RoshanTimer loaded");
                return;
            }

            if (roshanDead)
            {
                if (getRoshan() == null)
                    return;

                if (getRoshan().IsAlive)
                    roshanDead = false;
            }

        }
        static void Game_OnGameEvent(FireEventEventArgs args)
        {
            if (!scriptLoaded)
                return;

            if (args.GameEvent.Name == "dota_roshan_kill")
            {
                roshanDead = true;
                roshanDeathTime = Game.GameTime;
                roshanRespawnMinTime = roshanDeathTime + 480;
                roshanRespawnMaxTime = roshanDeathTime + 660;
                Game.ExecuteCommand("chatwheel_say 53");
                Game.ExecuteCommand("chatwheel_say 57");
            }
        }
        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            font.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;

            var player = ObjectMgr.LocalPlayer;
            if (player == null || player.Team == Team.Observer)
                return;

            string text;
            if (roshanDead)
            {
                var mmin = (int)roshanRespawnMinTime / 60;
                var smin = (int)roshanRespawnMinTime - (60 * mmin);
                var mmax = (int)roshanRespawnMaxTime / 60;
                var smax = (int)roshanRespawnMaxTime - (60 * mmax);
                var mminr = (int)(roshanRespawnMinTime - Game.GameTime) / 60;
                var sminr = (int)(roshanRespawnMinTime - Game.GameTime) - (60 * mminr);
                var mmaxr = (int)(roshanRespawnMaxTime - Game.GameTime) / 60;
                var smaxr = (int)(roshanRespawnMaxTime - Game.GameTime) - (60 * mmaxr);

                if (mmin <= 0 && smin <= 0)
                    text = String.Format("Roshan: Dead\nRespawn time: {0}:{1} - {2}:{3} ({4}:{5})", mmin, smin, mmax, smax, mmaxr, smaxr);
                else
                    text = String.Format("Roshan: Dead\nRespawn time: {0}:{1} - {2}:{3} ({4}:{5} - {6}:{7})", mmin, smin, mmax, smax, mminr, sminr, mmaxr, smaxr);

                if (mmaxr <= 0 && smaxr <= 0)
                    roshanDead = false;
            }
            else text = "Roshan: Alive";

            font.DrawText(null, text, 3, 500, roshanDead ? Color.Red : Color.Green);
        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            font.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            font.OnLostDevice();
        }
        static Unit getRoshan()
        {
            var roshan = ObjectMgr.GetEntities<Unit>().Where(x => x.ClassID == ClassID.CDOTA_Unit_Roshan);
            foreach (var a in roshan)
            {
                return a;
            }
            return null;
        }
    }
}