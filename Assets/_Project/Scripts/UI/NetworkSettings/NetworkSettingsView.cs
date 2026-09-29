using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI.NetworkSettings
{
    public class NetworkSettingsView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField latency;
        [SerializeField] private TMP_InputField dropChance;
        [SerializeField] private TMP_InputField retransmit;

        public event Action<int, float, int> Changed;

        
        private void Awake()
        {
            latency.onEndEdit.AddListener(OnEdited);
            dropChance.onEndEdit.AddListener(OnEdited);
            retransmit.onEndEdit.AddListener(OnEdited);
        }

        public void Show(int latencyMs, float drop, int retransmitMs)
        {
            latency.SetTextWithoutNotify(latencyMs.ToString());
            dropChance.SetTextWithoutNotify(drop.ToString(CultureInfo.InvariantCulture));
            retransmit.SetTextWithoutNotify(retransmitMs.ToString());
        }

        private void OnEdited(string _)
        {
            if (!int.TryParse(latency.text, out int latencyMs))
                return;

            if (!float.TryParse(dropChance.text, NumberStyles.Float, CultureInfo.InvariantCulture, out float drop))
                return;

            if (!int.TryParse(retransmit.text, out int retransmitMs))
                return;

            Changed?.Invoke(latencyMs, drop, retransmitMs);
        }
    }
}
