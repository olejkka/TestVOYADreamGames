using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.Cell
{
    public class CellView : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Button button;

        public event Action Clicked;

        
        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        public void SetColor(Color color)
        {
            image.color = color;
        }

        private void OnClick()
        {
            Clicked?.Invoke();
        }
    }
}
