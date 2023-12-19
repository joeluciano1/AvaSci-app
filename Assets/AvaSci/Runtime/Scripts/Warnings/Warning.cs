using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace LightBuzz.AvaSci.Warnings
{
    /// <summary>
    /// Represents a warning message.
    /// </summary>
    public class Warning : MonoBehaviour
    {
        [SerializeField] protected TMPro.TMP_Text _label;

        /// <summary>
        /// Indicates whether the warning should be displayed.
        /// </summary>
        protected bool _display = false;

        /// <summary>
        /// The warning message text.
        /// </summary>
        protected string _message = string.Empty;
        public GameObject MyMessage;
        /// <summary>
        /// Checks if the warning should be displayed.
        /// Call this base method to toggle a warning's visibility.
        /// The implementation should be warning-specific and should be handled by child classes.
        /// </summary>
        /// <param name="frame">The <see cref="FrameData"/> to check.</param>
        /// <param name="body">The <see cref="Body"/> skeleton data to check.</param>
        /// <param name="movement">The <see cref="Movement"/> data to check.</param>
        public async virtual void Check(FrameData frame = null, Body body = null, Movement movement = null)
        {
            if (!_display)
            {
                if (MyMessage!=null)
                {

                    //MyMessage.SetActive(false);
                    
                }
            }
            else
            {
                if (MyMessage==null)
                {
                    //gameObject.SetActive(true);
                    Warning warning = Instantiate(this, this.transform.parent);
                    warning.gameObject.SetActive(true);
                    warning._label.text = _message;
                    MyMessage = warning.gameObject;
                    warning.transform.DOMoveY(1, 3f);
                    warning.GetComponent<Image>().DOFade(0, 3);
                    warning._label.DOFade(0, 3);

                    await System.Threading.Tasks.Task.Delay(3000);
                    Destroy(warning.gameObject);
                }
            }
        }
    }
}