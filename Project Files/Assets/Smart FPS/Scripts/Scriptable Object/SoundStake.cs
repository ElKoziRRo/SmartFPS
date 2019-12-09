using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Profile", menuName = "Smart FPS Kit/Create New Profile/Sound Profile")]
public class SoundStake : ScriptableObject
{
    [Space(2)]
    [Header("Player")]
    public AudioClip[] hurtSounds;
    public AudioClip[] dieSounds;
    public AudioClip healthSound;
    public AudioClip heartbeatSound;
    public AudioClip breathSound;
    public AudioClip jumpLandSound;
    public AudioClip jumpStartSound;
    public AudioClip slideSound;

}
