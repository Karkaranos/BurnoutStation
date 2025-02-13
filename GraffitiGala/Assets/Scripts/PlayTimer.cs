/*************************************************
Brandon Koederitz
2/9/2025
2/9/2025
Syncronized timer that limits a plyer's time in the experience.
FishNet
***************************************************/
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GraffitiGala.Drawing;
using GraffitiGala.UI;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GraffitiGala
{
    public class PlayTimer : NetworkBehaviour
    {
        [SerializeField] private TimeDisplayer displayer;
        [SerializeField, Tooltip("The amount of time in seconds that this timer will run for.")] 
        private float time = 120f;
        [SerializeField]
        private UnityEvent OnFinishServer;
        [SerializeField]
        private UnityEvent OnFinishClient;
        private readonly SyncTimer timer = new();

        private bool isStarted;
        private Coroutine displayUpdateRoutine;

        #region Properties
        public float RemainingTime
        {
            get
            {
                timer.Update();
                return timer.Remaining;
            }
        }
        public float NormalizedTime
        {
            get
            {
                timer.Update();
                return timer.Remaining / time;
            }
        }
        public float NormalizedProgress
        {
            get
            {
                timer.Update();
                return timer.Elapsed / time;
            }
        }
        #endregion

        /// <summary>
        /// Gives the MeshNetBrush script a reference to this current play timer on client start.
        /// </summary>
        public override void OnStartClient()
        {
            //base.OnStartClient();

            if (!MeshNetBrush.SetPlayTimer(this))
            {
                Destroy(gameObject);
                Debug.LogWarning("Duplicate PlayTimer was found.  Please ensure that only one play timer exists " +
                    "at a time.");
                return;
            }

        }

        /// <summary>
        /// Subscribes and unsubscribes the OnChange function to the timer's OnChange event.
        /// This OnChange function will rout certain events that need to occur when the timer reaches certain 
        /// milestones.
        /// </summary>
        private void OnEnable()
        {
            timer.OnChange += Timer_OnChange;
        }
        private void OnDisable()
        {
            timer.OnChange -= Timer_OnChange;
        }

        /// <summary>
        /// Handles changes to the timer state.
        /// </summary>
        /// <param name="op">The current state that the timer moves to.</param>
        /// <param name="prev">???</param>
        /// <param name="next">???</param>
        /// <param name="asServer">Whether this callback is being run on the server or on a client.</param>
        private void Timer_OnChange(SyncTimerOperation op, float prev, float next, bool asServer)
        {
            switch (op)
            {
                case SyncTimerOperation.Start:
                    OnTimerStart();
                    break;
                case SyncTimerOperation.Pause:
                    break;
                case SyncTimerOperation.Stop:
                    OnTimerStop();
                    break;
                case SyncTimerOperation.Finished:
                    OnTimerFinish(asServer);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Marks this timer as started and handles behaviour that should happen when this timer starts.
        /// </summary>
        private void OnTimerStart()
        {
            isStarted = true;
            if(displayUpdateRoutine != null)
            {
                StopCoroutine(displayUpdateRoutine);
            }
            displayUpdateRoutine = StartCoroutine(TimerDisplayUpdate());
        }

        /// <summary>
        /// Marks this timer as stopped and handles behaviour that should happen when this timer stops.
        /// </summary>
        private void OnTimerStop()
        {
            isStarted = false;
            // Sets the displayer to display no time remaining.
            displayer.LoadTime(0);
        }

        /// <summary>
        /// Plays events that occur when this timer starts and marks this as stopped.
        /// </summary>
        /// <param name="asServer">Whether this function is being run on the server or on a client.</param>
        private void OnTimerFinish(bool asServer)
        {
            if (asServer)
            {
                OnFinishServer?.Invoke();
            }
            OnFinishClient?.Invoke();
            isStarted = false;
        }

        /// <summary>
        /// Continually updates the timer's display while it is started.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator TimerDisplayUpdate()
        {
            while (isStarted)
            {
                // Dont update the timer's display if it is paused.
                if(timer.Paused) { yield return null; }

                displayer.LoadTime(NormalizedTime);
                yield return null;
            }
            displayUpdateRoutine = null;
        }

        /// <summary>
        /// Starts the timer from the beginning.
        /// </summary>
        [Button]
        public void StartTimer()
        {
            timer.StartTimer(time, true);
            Debug.Log("Timer Started");
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        [Button]
        public void StopTimer()
        {
            timer.StopTimer(true);
            Debug.Log("Timer Stopped");
        }
    }
}