using _Project.Scripts.NetworkLayer.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace _Project.Scripts.UI.SceneRestart
{
    public class SceneRestartBinder : MonoBehaviour
    {
        [SerializeField] private SceneRestartView view;

        private SceneRestartPresenter _presenter;

        [Inject]
        public void Construct(InProcessHub hub)
        {
            _presenter = new SceneRestartPresenter(view, hub);
        }
    }
}
