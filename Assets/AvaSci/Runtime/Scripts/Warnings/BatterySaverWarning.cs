using System;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

namespace LightBuzz.AvaSci.Warnings
{
    /// <summary>
    /// Checks whether the device is in low power mode.
    /// This warning is only available on iOS devices.
    /// </summary>
    public class BatterySaverWarning : Warning
    {
        private DateTime _date;

        /// <summary>
        /// Creates a new <see cref="BatterySaverWarning"/> instance.
        /// </summary>
        public BatterySaverWarning()
        {
            _date = DateTime.Now;
            _message = "Low power mode is turned on.";
        }

        public override void Check(FrameData frame = null, Body body = null, Movement movement = null)
        {
            base.Check();

            if ((DateTime.Now - _date).TotalSeconds >= 1.0)
            {
                _date = DateTime.Now;

                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
#if UNITY_IPHONE

                    _display = UnityEngine.iOS.Device.lowPowerModeEnabled;

#endif
                }
            }
        }
    }
}