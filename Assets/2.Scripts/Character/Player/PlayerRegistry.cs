using System;
using UnityEngine;

namespace GRstory.Character
{
    public static class PlayerRegistry
    {
        public static PlayerBehaviour CurrentPlayerBehaviour { get; private set; }
        public static event Action<PlayerBehaviour> OnPlayerChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            CurrentPlayerBehaviour = null;
            OnPlayerChanged = null;
        }

        public static void RegisterPlayer(PlayerBehaviour playerBehaviour)
        {
            if (CurrentPlayerBehaviour == playerBehaviour) return;

            CurrentPlayerBehaviour = playerBehaviour;
            OnPlayerChanged?.Invoke(playerBehaviour);
        }


    }
}
