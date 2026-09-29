using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
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
