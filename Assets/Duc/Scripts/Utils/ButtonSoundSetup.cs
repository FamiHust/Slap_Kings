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
                
                // Check if ButtonSoundHelper already exists
                ButtonSoundHelper helper = button.GetComponent<ButtonSoundHelper>();
                if (helper != null)
                {
                    skippedCount++;
                    continue;
                }
                
                // Add ButtonSoundHelper
                button.gameObject.AddComponent<ButtonSoundHelper>();
                addedCount++;
            }
            
            Debug.Log($"ButtonSoundSetup: Added ButtonSoundHelper to {addedCount} buttons. Skipped {skippedCount} buttons that already have it.");
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
            
            Debug.Log($"ButtonSoundSetup: Removed {removedCount} ButtonSoundHelper components.");
        }
    }
}
