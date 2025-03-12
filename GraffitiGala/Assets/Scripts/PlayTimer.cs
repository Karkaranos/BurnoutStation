/*************************************************
Brandon Koederitz
2/9/2025
2/23/2025
Syncronized timer that limits a plyer's time in the experience.
FishNet
***************************************************/
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FMOD.Studio;
using GraffitiGala.Drawing;
using GraffitiGala.UI;
using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GraffitiGala
{
    public class PlayTimer : NetworkBehaviour
    {
        [SerializeField] private TimeDisplayer displayer;
        [SerializeField] 
        private float time = 120f;
        [SerializeField, Tooltip("Disables sound effects to avoid FMOD errors.")]
        private bool playSoundEffects;
        [Header("Events (Obsolete)")]
        [Header("Client Events")]
        [ReadOnly, SerializeField, Tooltip("Called on all clients when the timer begins.")]
        private UnityEvent OnBeginClient;
        [ReadOnly, SerializeField, Tooltip("Called on all clients when the timer finishes.")]
        private UnityEvent OnFinishClient;
        [Header("Server Events")]
        [ReadOnly, SerializeField, Tooltip("Called on server/admin clients when the timer begins.")]
        private UnityEvent OnBeginServer;
        [ReadOnly, SerializeField, Tooltip("Called on server/admin clients when the timer finishes.")]
        private UnityEvent OnFinishServer;
        private readonly SyncTimer timer = new();

        private bool isStarted;
        private Coroutine displayUpdateRoutine;

        //public static event Action OnBeginClientStatic;
        //public static event Action OnBeginServerStatic;
        //public static event Action OnFinishClientStatic;
        //public static event Action OnFinishServerStatic;

        private EventInstance countdown;

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
                return timer.Remaining / timer.Duration;
            }
        }
        public float NormalizedProgress
        {
            get
            {
                timer.Update();
                return timer.Elapsed / timer.Duration;
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
            // Sets the host as the owner of this object.
            if (base.IsHostStarted && !base.IsOwner)
            {
                this.GiveOwnership(InstanceFinder.ClientManager.Connection);
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
                    OnTimerStart(asServer);
                    break;
                case SyncTimerOperation.Pause:
                    break;
                case SyncTimerOperation.Stop:
                    OnTimerStop(asServer);
                    break;
                case SyncTimerOperation.Finished:
                    OnTimerFinish(asServer);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Called upon the first frame update of this object. Instantiates audio references
        /// </summary>
        private void Awake()
        {
            if (playSoundEffects)
            {
                countdown = AudioManager.instance.CreateEventInstance(FMODEventsManager.instance.Timer);
            }
        }

        /// <summary>
        /// Marks this timer as started and handles behaviour that should happen when this timer starts.
        /// </summary>
        private void OnTimerStart(bool asServer)
        {
            // Starts a coroutine that displays changes to this timer on the UI.
            isStarted = true;
            if(displayUpdateRoutine != null)
            {
                StopCoroutine(displayUpdateRoutine);
            }
            displayUpdateRoutine = StartCoroutine(TimerDisplayUpdate());

            if (asServer)
            {
                //OnBeginServerStatic?.Invoke();
                //OnBeginServer?.Invoke();
                if (playSoundEffects)
                {
                    countdown.start();
                }

            }
            //OnBeginClientStatic?.Invoke();
            //OnBeginClient?.Invoke();
        }

        /// <summary>
        /// Marks this timer as stopped and handles behaviour that should happen when this timer stops.
        /// </summary>
        private void OnTimerStop(bool asServer)
        {
            isStarted = false;
            // Sets the displayer to display no time remaining.
            if(displayer != null)
            {
                displayer.LoadTime(0);
            }
        }

        /// <summary>
        /// Plays events that occur when this timer starts and marks this as stopped.
        /// </summary>
        /// <param name="asServer">Whether this function is being run on the server or on a client.</param>
        private void OnTimerFinish(bool asServer)
        {
            if (asServer)
            {
                //OnFinishServer?.Invoke();
                //OnFinishServerStatic?.Invoke();
                if (playSoundEffects)
                {
                    countdown.stop(STOP_MODE.IMMEDIATE);
                    AudioManager.instance.PlayOneShot(FMODEventsManager.instance.Ring, Vector3.zero);
                }

                // Instead of the timer managing events that happen on finish, simply tell the experience manager
                // to change to the finished state.
                ExperienceManager.SetState(ExperienceState.Finished);
            }
            //OnFinishClient?.Invoke();
            //OnFinishClientStatic?.Invoke();
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
        //[Button]
        public void StartTimer()
        {
            timer.StartTimer(time, true);
            //Debug.Log("Timer Started");
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        //[Button]
        public void StopTimer()
        {
            timer.StopTimer(true);
            //Debug.Log("Timer Stopped");
        }
    }
}