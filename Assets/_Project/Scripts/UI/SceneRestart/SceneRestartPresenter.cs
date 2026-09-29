using UnityEngine.SceneManagement;

namespace _Project.Scripts.UI.SceneRestart
{
    public class SceneRestartPresenter
    {
        public SceneRestartPresenter(SceneRestartView view)
        {
            view.Clicked += OnClick;
        }

        private void OnClick()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
