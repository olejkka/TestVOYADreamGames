using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.NetworkButtons
{
    public class ConnectView : MonoBehaviour
    {
        [SerializeField] private Button button;

        public event Action Clicked;

        
        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Clicked?.Invoke();
        }
    }
}
