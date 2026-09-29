using UnityEngine;

namespace _Project.Scripts.UI.SceneRestart
{
    public class SceneRestartBinder : MonoBehaviour
    {
        [SerializeField] private SceneRestartView view;

        private SceneRestartPresenter _presenter;

        
        private void Awake()
        {
            _presenter = new SceneRestartPresenter(view);
        }
    }
}
