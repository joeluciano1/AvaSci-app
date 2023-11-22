using System;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

namespace LightBuzz.AvaSci.Warnings
{
    /// <summary>
    /// A collection of <see cref="Warning"/>s.
    /// </summary>
    [Serializable]
    public class WarningCollection
    {
        [SerializeField] private Warning[] _warnings;

        /// <summary>
        /// Returns the <see cref="Warning"/>s of this collection.
        /// </summary>
        public Warning[] Warnings => _warnings;

        /// <summary>
        /// Checks and loads all warnings.
        /// </summary>
        /// <param name="frame">The <see cref="FrameData"/> to check.</param>
        /// <param name="body">The <see cref="Body"/> skeleton data to check.</param>
        /// <param name="movement">The <see cref="Movement"/> data to check.</param>
        public void Load(FrameData frame = null, Body body = null, Movement movement = null)
        {
            foreach (Warning warning in _warnings)
            {
                warning.Check(frame, body, movement);
            }
        }
    }
}