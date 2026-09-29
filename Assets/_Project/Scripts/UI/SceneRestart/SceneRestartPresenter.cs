using _Project.Scripts.NetworkLayer.Transport;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.UI.SceneRestart
{
    public class SceneRestartPresenter
    {
        private readonly InProcessHub _hub;

        public SceneRestartPresenter(SceneRestartView view, InProcessHub hub)
        {
            _hub = hub;
            view.Clicked += OnClick;
        }

        private void OnClick()
        {
            _hub.Dispose();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
