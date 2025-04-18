/*************************************************
Brandon Koederitz
3/22/2025
3/22/2025
Displays a countdown to all players when the experience is about to begin.
FishNet
***************************************************/
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using TMPro;
using UnityEngine;

namespace GraffitiGala.Admin
{
    public class StartingCountdown : NetworkBehaviour
    {
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private GameObject countdownObject;
        [SerializeField] private float countdownTime;
        [SerializeField] private string endingMessage = "GO!";
        [SerializeField] private float endingMessageDuration;

        private readonly SyncTimer syncedCountdown = new SyncTimer();
        private bool isCountdown;
        private Coroutine soundCount = null;

        /// <summary>
        /// Subscribe/Unsubscribe from the timer's OnChange function.
        /// </summary>
        private void OnEnable()
        {
            syncedCountdown.OnChange += Countdown_OnChange;
        }

        private void OnDisable()
        {
            syncedCountdown.OnChange -= Countdown_OnChange;
        }

        /// <summary>
        /// Sets the host as the owner of this object when the host client starts.
        /// </summary>
        public override void OnStartClient()
        {
            // Sets the host as the owner of this object.
            if (base.IsHostStarted && !base.IsOwner)
            {
                this.GiveOwnership(InstanceFinder.ClientManager.Connection);
            }
        }

        /// <summary>
        /// Handles the timer changing states.
        /// </summary>
        /// <param name="op">The current state that the timer moves to.</param>
        /// <param name="prev">???</param>
        /// <param name="next">???</param>
        /// <param name="asServer">Whether this callback is being run on the server or on a client.</param>
        private void Countdown_OnChange(SyncTimerOperation op, float prev, float next, bool asServer)
        {
            switch (op)
            {
                case SyncTimerOperation.Start:
                    OnCountdownStart(asServer);
                    break;
                case SyncTimerOperation.Stop:
                case SyncTimerOperation.Finished:
                    OnCountdownFinish(asServer);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Handles behaviour that happens when the countdown starts.
        /// </summary>
        /// <param name="asServer">Whether this callback is being run on the server or not.</param>
        private void OnCountdownStart(bool asServer)
        {
            isCountdown = true;
            if (asServer)
            {

            }
            StartCoroutine(CountdownUpdateRoutine());
        }

        /// <summary>
        /// Handles behaviour that happens when the countdown finishes or stops.
        /// </summary>
        /// <param name="asServer">Whether this callback is being run on the server or not.</param>
        private void OnCountdownFinish(bool asServer)
        {
            isCountdown = false;
            if (asServer)
            {
                // Has the server move the experience into the playing state once the countdown is finished.
                ExperienceManager.SetState(ExperienceState.Playing);
            }
        }

        /// <summary>
        /// Continually updates the displayed countdown on screen while the countdown is going.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CountdownUpdateRoutine()
        {
            if (soundCount == null)
            {
                soundCount = StartCoroutine(ConcurrentForSound());
            }
            countdownObject.gameObject.SetActive(true);
            while (isCountdown)
            {
                syncedCountdown.Update();
                countdownText.text = ((int)syncedCountdown.Remaining + 1).ToString();
                yield return null;
            }
            // Displays the ending message, then dissappears after a short duration.
            countdownText.text = endingMessage;
            yield return new WaitForSeconds(endingMessageDuration);
            countdownObject.gameObject.SetActive(false);
            // Clears the countdown text to avoid possible jittering when the countdown begins.
            countdownText.text = "";
        }

        private IEnumerator ConcurrentForSound()
        {
            bool triggerShake = false;
            while (isCountdown)
            {
                if (!triggerShake && syncedCountdown.Remaining <= 1.2 && FindObjectOfType<BuildManager>().BuildTypeRef == BuildType.Admin)
                {
                    AudioManager.instance.PlayOneShot(FMODEventsManager.instance.GameStart, Vector3.zero);
                    triggerShake = true;
                }
                yield return new WaitForSeconds(.1f);
            }
            soundCount = null;
        }

        /// <summary>
        /// Starts the countdown.
        /// </summary>
        public void StartCountdown()
        {
            // Only allow the owner of the countdown (the host) to start it.
            if (base.IsOwner)
            {
                syncedCountdown.StartTimer(countdownTime, true);
            }
        }

        /// <summary>
        /// Resets the countdown.
        /// </summary>
        public void ResetCountdown()
        {
            countdownObject.gameObject.SetActive(false);
            isCountdown = false;
        }
    }
}