using UnityEngine;

namespace GRstory.UISystem
{
    public class PauseUI : BaseUI
    {
        public override void OnUIActive()
        {
            base.OnUIActive();

            Time.timeScale = 0f;
        }

        public override void OnUIDeactive()
        {
            base.OnUIDeactive();

            Time.timeScale = 1f;
        }

        public void OnClickResumeButton()
        {
            UIManager.Instance.DeactiveUI<PauseUI>();
        }

        public void OnClickSettingsButton()
        {

        }

        public void OnClickTitleButton()
        {

        }

        public void OnClickExitButton()
        {

        }

    }
}
