/*************************************************
Brandon Koederitz
2/23/2025
2/23/2025
Broadcasts events that manage the beginning and ending of an experience.
FishNet, NaughtyAttributes
***************************************************/
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace GraffitiGala
{
    public enum ExperienceState
    {
        Disconnected = 0,
        Waiting = 1,
        Loading = 2,
        Playing = 3,
        Finished = 4
    }

    public class ExperienceManager : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField, Tooltip("The number of clients that need to ready before the experience starts.")] 
        private int readyNumber = 3;
        [SerializeField] private ExperienceEvents serverEvents;
        [SerializeField] private ExperienceEvents clientEvents;

        private readonly SyncList<NetworkConnection> readiedConnections = new();
        private readonly SyncVar<int> serverReadyCount = new();
        private readonly SyncVar<ExperienceState> syncState = new();
        private static ExperienceManager instance;

        #region Nested Classes
        [Serializable]
        private class ExperienceEvents
        {
            [SerializeField, Tooltip("Called when the client disconnects from the server.")] 
            private UnityEvent OnDisconnect;
            [SerializeField, Tooltip("Called when the client connects to the server and is now waiting for the " +
                "experience to start.")] 
            private UnityEvent OnWaiting;
            [SerializeField, Tooltip("Called when the experience begins to load once all clients are ready.")] 
            private UnityEvent OnLoading;
            [SerializeField, Tooltip("Called when the experience begins and the players can begin drawing.")] 
            private UnityEvent OnPlaying;
            [SerializeField, Tooltip("Called when the experience finishes after the timer is up.")] 
            private UnityEvent OnFinished;

            /// <summary>
            /// Calls the event related to moving to the given state.
            /// </summary>
            /// <param name="state"></param>
            internal void CallEvent(ExperienceState state)
            {
                switch (state)
                {
                    case ExperienceState.Disconnected:
                        OnDisconnect?.Invoke();
                        break;
                    case ExperienceState.Waiting:
                        OnWaiting?.Invoke();
                        break;
                    case ExperienceState.Loading:
                        OnLoading?.Invoke();
                        break;
                    case ExperienceState.Playing:
                        OnPlaying?.Invoke();
                        break;
                    case ExperienceState.Finished:
                        OnFinished?.Invoke();
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        #region Setup
        /// <summary>
        /// Initializes the private singleton instance.
        /// </summary>
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                Debug.LogError("Duplicate ExperienceManager found.");
                return;
            }
            else
            {
                instance = this;
            }
        }

        /// <summary>
        /// Subscribe and unsubscribe changes to the current state.
        /// </summary>
        private void OnEnable()
        {
            syncState.OnChange += SyncState_OnChange;
        }
        private void OnDisable()
        {
            syncState.OnChange -= SyncState_OnChange;
        }

        /// <summary>
        /// Makes the server the owner of this object.
        /// </summary>
        public override void OnStartClient()
        {
            if (base.IsServerStarted && !base.IsOwner)
            {
                Debug.Log("This is the server");
                this.GiveOwnership(InstanceFinder.ClientManager.Connection);
                Server_SetReadyCount(readyNumber);
            }
        }
        #endregion

        #region Sync Value Setters
        /// <summary>
        /// Sets the number of clients that need to ready before the experience starts.
        /// </summary>
        /// <remarks>
        /// Server cant use a normal private variable so I need to assign a SyncVariable and SyncVars can only be
        /// modified with a ServerRPC
        /// </remarks>
        /// <param name="readyCount">The number of clients that need to ready before the experience starts.</param>
        [ServerRpc]
        private void Server_SetReadyCount(int readyCount)
        {
            serverReadyCount.Value = readyCount;
        }

        /// <summary>
        /// Sets the state of the experience on the server.
        /// </summary>
        /// <param name="state">The state to set.</param>
        [ServerRpc]
        private void Server_SetState(ExperienceState state)
        {
            syncState.Value = state;
        }
        #endregion

        #region State Change Handling
        public static ExperienceState GetState()
        {
            if (instance == null)
            {
                Debug.LogError("No instance of the ExperienceManager exists.");
                return ExperienceState.Disconnected;
            }
            return instance.syncState.Value;
        }

        /// <summary>
        /// Changes the current state of the experience from the server.
        /// </summary>
        /// <remarks>
        /// Only the server is allowed to call this function.
        /// </remarks>
        /// <param name="state">The new state to move to.</param>
        public static void SetState(ExperienceState state)
        {
            if (instance == null)
            {
                Debug.LogError("No instance of the ExperienceManager exists.");
                return;
            }
            instance.SetStateServerCheck(state);
        }

        /// <summary>
        /// Sets the state by rerouting through the instance to check if this functions is being run by the server.
        /// </summary>
        /// <param name="state">The new state to move to.</param>
        private void SetStateServerCheck(ExperienceState state)
        {
            if (base.IsServerStarted)
            {
                Server_SetState(state);
            }
        }

        /// <summary>
        /// Handles changes to experience state events for all clients across the network and the server.
        /// </summary>
        /// <param name="prev">The previous experience state.</param>
        /// <param name="next">The new experience state.</param>
        /// <param name="asServer">Whether this callback is being run on the server.</param>
        private void SyncState_OnChange(ExperienceState prev, ExperienceState next, bool asServer)
        {
            if (asServer)
            {
                serverEvents.CallEvent(next);
            }
            clientEvents.CallEvent(next);
        }
        #endregion

        #region Readying
        /// <summary>
        /// Readys this client.  Once enough clients are ready, the experience will start.
        /// </summary>
        public static void ReadyClient()
        {
            if (instance == null)
            {
                Debug.LogError("No instance of the ExperienceManager exists.");
                return;
            }
            NetworkConnection thisConnection = InstanceFinder.ClientManager.Connection;
            instance.Server_ReadyClient(thisConnection);
        }

        /// <summary>
        /// Readies a given client.  Once enough clients are ready, the experience will start.
        /// </summary>
        /// <param name="readyConnection">The network connection to mark as readied.</param>
        [ServerRpc(RequireOwnership = false)]
        private void Server_ReadyClient(NetworkConnection readyConnection)
        {
            readiedConnections.Add(readyConnection);
            // Once enough clients have readied, then the server begins the experience.
            if (readyConnection.Objects.Count >= serverReadyCount.Value)
            {
                // Call just Server_SetState here because we're already in an RPC.  We cant access the instance and
                // dont need to check if we are the server.
                Server_SetState(ExperienceState.Loading);
                // Clear the readied connections.
                readyConnection.Objects.Clear();
            }
        }
        #endregion

        #region Test Functions
        [Button]
        private void BeginExperience()
        {
            SetState(ExperienceState.Playing);
        }

        [Button]
        private void EndExperience()
        {
            SetState(ExperienceState.Finished);
        }
        #endregion
    }

}