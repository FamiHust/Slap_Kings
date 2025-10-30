using UnityEngine;
using UnityEngine.UI;

namespace Duc
{
    public class ButtonSoundSetup : MonoBehaviour
    {
        [ContextMenu("Auto Setup All Buttons")]
        public void AutoSetupAllButtons()
        {
            Button[] buttons = FindObjectsOfType<Button>(true);
            
            int addedCount = 0;
            int skippedCount = 0;
            
            foreach (Button button in buttons)
            {
                if (button == null) continue;
                
                ButtonSoundHelper helper = button.GetComponent<ButtonSoundHelper>();
                if (helper != null)
                {
                    skippedCount++;
                    continue;
                }
                
                button.gameObject.AddComponent<ButtonSoundHelper>();
                addedCount++;
            }
        }
        
        [ContextMenu("Remove All ButtonSoundHelper")]
        public void RemoveAllButtonSoundHelpers()
        {
            ButtonSoundHelper[] helpers = FindObjectsOfType<ButtonSoundHelper>(true);
            
            int removedCount = 0;
            
            foreach (ButtonSoundHelper helper in helpers)
            {
                if (helper != null)
                {
                    DestroyImmediate(helper);
                    removedCount++;
                }
            }
        }
    }
}
