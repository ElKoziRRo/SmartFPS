using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    public Animator animator;
    public string IdleAnimation = "Idle";
    public string FireAnimation = "Fire";
    public string ReloadAnimation = "Reload";
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            animator.Play(IdleAnimation);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            animator.Play(FireAnimation);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            animator.Play(ReloadAnimation);
    }
}
