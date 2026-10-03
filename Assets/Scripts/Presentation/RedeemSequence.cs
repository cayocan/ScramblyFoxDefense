using System.Collections.Generic;
using ScramblyFoxDefense.Core;
using UnityEngine;

namespace ScramblyFoxDefense.Presentation
{
    /// <summary>
    /// Redeem: demo coins arc from the board into the Reward Vault, which glows. A tap speeds it up.
    /// Does not depend on animating a chest lid (the kit has none, GDD section 6).
    /// </summary>
    public sealed class RedeemSequence
    {
        const int CoinCount = 12;
        const float FlightTime = 0.7f;
        const float Stagger = 0.1f;
        const float GlowTime = 0.8f;
        const float FastForward = 2.5f;

        sealed class Flight
        {
            public GameObject Coin;
            public Vector3 From;
            public float Delay;
        }

        readonly ObjectPool _coins;
        readonly Transform _vault;
        readonly Renderer[] _vaultRenderers;
        readonly Transform[] _sources;
        readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        readonly List<Flight> _flights = new List<Flight>();
        Vector3 _vaultScale;
        float _time;
        float _speed;

        public bool Running { get; private set; }
        public bool Finished => Running && _time >= TotalDuration;

        float TotalDuration => (CoinCount - 1) * Stagger + FlightTime + GlowTime;

        public RedeemSequence(GameObject coinPrefab, Transform fxRoot, Transform vault, Transform[] sources)
        {
            _coins = new ObjectPool(coinPrefab, fxRoot, CoinCount);
            _vault = vault;
            _vaultRenderers = vault.GetComponentsInChildren<Renderer>();
            _sources = sources;
        }

        public void Begin()
        {
            Running = true;
            _time = 0f;
            _speed = 1f;
            _vaultScale = _vault.localScale;
            _flights.Clear();
            for (int i = 0; i < CoinCount; i++)
            {
                var coin = _coins.Get();
                var from = _sources[i % _sources.Length].position + Vector3.up * 0.5f;
                coin.transform.position = from;
                _flights.Add(new Flight { Coin = coin, From = from, Delay = i * Stagger });
            }
        }

        public bool SpeedUp()
        {
            if (!Running) return false;
            _speed = FastForward;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!Running) return;
            _time += deltaTime * _speed;

            Vector3 target = _vault.position + Vector3.up * 0.8f;
            for (int i = _flights.Count - 1; i >= 0; i--)
            {
                var flight = _flights[i];
                float t = (_time - flight.Delay) / FlightTime;
                if (t <= 0f) continue;
                if (t >= 1f)
                {
                    _coins.Release(flight.Coin);
                    _flights.RemoveAt(i);
                    continue;
                }
                var position = Vector3.Lerp(flight.From, target, t) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 1.5f;
                flight.Coin.transform.position = position;
                flight.Coin.transform.Rotate(0f, 720f * deltaTime, 0f, Space.World);
            }

            // Vault glows and swells while coins land, then settles.
            float landed = Mathf.Clamp01((_time - FlightTime) / ((CoinCount - 1) * Stagger + GlowTime));
            float glow = Mathf.Sin(landed * Mathf.PI) * 0.6f;
            _block.SetFloat("_Flash", glow);
            foreach (var renderer in _vaultRenderers) renderer.SetPropertyBlock(_block);
            _vault.localScale = _vaultScale * (1f + glow * 0.25f);
        }
    }
}
