using System;
using System.Collections.Generic;
using UnityEngine;

public static class scr_Models
{
    #region - Player -

    public enum PlayerStance
    {
        Stand,
        Crouch
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float ViewXSensitivity;
        public float ViewYSensitivity;

        public bool ViewXInverted;
        public bool ViewYInverted;

        [Header("Movement - Settings")]
        public bool HoldSprint;
        public float MovementSmoothing;

        [Header("Movement - Running")]
        public float RunningForwardSpeed;
        public float RunningStrafeSpeed;

        [Header("Movement - Walking")]
        public float WalkingForwardSpeed;
        public float WalkingBackwardSpeed;
        public float WalkingStrafeSpeed;

        [Header("Jumping")]
        public float JumpingHeight;
        public float JumpingFalloff;
        public float FallingSmoothing;

        [Header("Speed Effectors")]
        public float SpeedEffector = 1;
        public float CrouchSpeedEffector;
        public float FallingSpeedEffector;
    }

    [Serializable]

    public class CharacterStance
    {
        public CapsuleCollider StanceCollider;
    }

    #endregion
}
