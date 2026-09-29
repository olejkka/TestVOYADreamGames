using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI.GameResult
{
    public class GameResultView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        
        public void Show(string result)
        {
            text.text = result;
            gameObject.SetActive(true);
        }
    }
}
