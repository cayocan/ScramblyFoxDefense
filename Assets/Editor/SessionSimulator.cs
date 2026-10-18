using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ScramblyFoxDefense.Core;
using ScramblyFoxDefense.Gameplay;
using ScramblyFoxDefense.Presentation;
using UnityEngine;

namespace ScramblyFoxDefense.EditorTools
{
    /// <summary>
    /// Headless balance check: in Play Mode, ticks the real services with a fixed step and a scripted
    /// player (build/upgrade orders executed as soon as they are affordable). Reports the flow, leaks
    /// and coins. Run from code: SessionSimulator.Run("pop0,pop1,up0").
    /// Enter Play Mode and pause it right away (editor_play, then editor_pause): with Unity in the
    /// foreground, real frames would otherwise advance the session before the simulation starts.
    /// </summary>
    public static class SessionSimulator
    {
        const float Step = 0.02f;
        const float MaxSeconds = 150f;
        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Orders, comma separated: "pop0" = build tower 0 (Pop Blaster) on reference spot 0, "puz2" = tower 1
        /// on spot 2, "zap1" = tower 2 on spot 1, "up0" = upgrade the tower on spot 0. Empty = never build.
        /// Spots are MainSceneBuilder.ReferenceSpots (the former slots) and go through the placement rules.
        /// </summary>
        public static string Run(string plan)
        {
            var installer = Object.FindFirstObjectByType<GameInstaller>();
            if (installer == null || !Application.isPlaying) return "Enter Play Mode with the Main scene first.";

            var enemies = Get<EnemySystem>(installer, "_enemies");
            var towers = Get<TowerSystem>(installer, "_towers");
            var machine = Get<GameStateMachine>(installer, "_machine");
            var coinFx = Get<CoinPopFx>(installer, "_coinFx");
            var actions = Get<PlayerActions>(installer, "_actions");
            var placement = (PlacementValidator)typeof(PlayerActions).GetField("_placement", Private).GetValue(actions);
            float groundHeight = Get<Transform[]>(installer, "pathWaypoints")[0].position.y;
            var spots = MainSceneBuilder.ReferenceSpots.Select(cell => MainSceneBuilder.CellToWorld(cell) + Vector3.up * groundHeight).ToArray();
            var economy = (Economy)typeof(EnemySystem).GetField("_economy", Private).GetValue(enemies);

            var orders = new Queue<string>(string.IsNullOrWhiteSpace(plan) ? new string[0] : plan.Split(','));
            int kills = 0, leaks = 0;
            enemies.Killed += _ => kills++;
            enemies.Leaked += _ => leaks++;

            var log = new StringBuilder($"plan=[{plan}] ");
            string state = "";
            float time = 0f;
            while (time < MaxSeconds)
            {
                while (orders.Count > 0 && TryOrder(orders.Peek(), spots, placement, towers, log)) orders.Dequeue();

                machine.Tick(Step);
                enemies.Tick(Step);
                towers.Tick(Step);
                coinFx.Tick(Step);
                time += Step;

                string current = machine.Current.GetType().Name.Replace("State", "");
                if (current == state) continue;
                log.Append($"| {time:F1}s {current} k{kills} l{leaks} w{economy.Wallet} ");
                state = current;
                if (current == "EndCard") break;
            }
            var fox = (FoxHealth)typeof(EnemySystem).GetField("_fox", Private).GetValue(enemies);
            log.Append($"| collected={economy.Collected} leaks={economy.Leaks} fox={fox.Current}/{fox.Max} pending=[{string.Join(",", orders)}]");
            Debug.Log("[SessionSimulator] " + log);
            return log.ToString();
        }

        static bool TryOrder(string order, Vector3[] spots, PlacementValidator placement, TowerSystem towers, StringBuilder log)
        {
            var spot = spots[order[order.Length - 1] - '0'];
            string verb = order.Substring(0, order.Length - 1);
            int type = verb == "pop" ? 0 : verb == "puz" ? 1 : verb == "zap" ? 2 : -1;
            if (type >= 0)
            {
                // Same rule as a player drop: an invalid spot is skipped and reported, never built.
                if (!placement.IsValid(spot))
                {
                    log.Append($"| {order} invalid ");
                    return true;
                }
                return towers.TryBuild(spot, type);
            }
            if (verb != "up") return true; // unknown order: skip it
            var tower = towers.Towers.FirstOrDefault(t => (t.Transform.position - spot).sqrMagnitude < 0.01f);
            return tower != null && towers.TryUpgrade(tower);
        }

        static T Get<T>(GameInstaller installer, string field) => (T)typeof(GameInstaller).GetField(field, Private).GetValue(installer);
    }
}
