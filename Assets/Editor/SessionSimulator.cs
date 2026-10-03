using System.Collections.Generic;
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
        /// Orders, comma separated: "pop0" = build tower 0 (Pop Blaster) on slot 0, "puz2" = tower 1 on
        /// slot 2, "zap1" = tower 2 on slot 1, "up0" = upgrade the tower on slot 0. Empty = never build.
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
            var slots = (SlotManager)typeof(PlayerActions).GetField("_slots", Private).GetValue(actions);
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
                while (orders.Count > 0 && TryOrder(orders.Peek(), slots, towers)) orders.Dequeue();

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
            log.Append($"| collected={economy.Collected} leaks={economy.Leaks} pending=[{string.Join(",", orders)}]");
            Debug.Log("[SessionSimulator] " + log);
            return log.ToString();
        }

        static bool TryOrder(string order, SlotManager slots, TowerSystem towers)
        {
            int slot = order[order.Length - 1] - '0';
            string verb = order.Substring(0, order.Length - 1);
            var target = slots.Slots[slot];
            switch (verb)
            {
                case "pop": return towers.TryBuild(target, 0);
                case "puz": return towers.TryBuild(target, 1);
                case "zap": return towers.TryBuild(target, 2);
                case "up": return target.Tower != null && towers.TryUpgrade(target.Tower);
                default: return true; // unknown order: skip it
            }
        }

        static T Get<T>(GameInstaller installer, string field) => (T)typeof(GameInstaller).GetField(field, Private).GetValue(installer);
    }
}
