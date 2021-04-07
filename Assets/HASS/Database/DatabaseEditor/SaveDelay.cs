using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HASS.Database.DatabaseEditor
{
    /// <summary>
    /// Class used for auto saving the Database when changes are made after a delay
    /// Incremently adds to the delay the more that is typed so the database is not constantly saving
    /// </summary>
    class SaveDelay
    {
        private float baseDelay;
        private float additiveDelay;
        private float timer = 0;
        private bool saving = false;
        private Action OnSave;
        private Thread thread = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseDelay">Start countdown at</param>
        /// <param name="additiveDelay">Number of seconds to add after each input</param>
        /// <param name="OnSave">Action to call once the timer finishes</param>
        public SaveDelay(float baseDelay, float additiveDelay, Action OnSave)
        {
            this.baseDelay = baseDelay;
            this.additiveDelay = additiveDelay;
            this.OnSave = OnSave;
        }

        /// <summary>
        /// Saves the Database When timer reaches 0.
        /// Adds saveDelay to the totalDelay and starts the saving thread if it wasn't started yet
        /// </summary>
        public void Save()
        {
            if (!saving)
            {
                timer = baseDelay;
                thread = new Thread(_Save);
                thread.Start();
            } 
            else
            {
                timer += additiveDelay;
            }
        }

        private void _Save()
        {
            saving = true;
            while (timer > 0)
            {
                Thread.Sleep((int)(additiveDelay * 1000));
                timer -= additiveDelay;
            }

            UnityEngine.Debug.Log("auto save");
            OnSave();

            saving = false;
            thread.Abort();
        }
    }
}
