/**
 * Smart FPS Kit
 * This Class handle Stamina,Thirst, Hungry and Temperature
**/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurvivalController : MonoBehaviour
{
    [Header("Run Stamina")]
    public float currentRunStamina = 100f; //Stamina for Run
    public float maxRunStamina = 100f;
    [Range(0, 1)] public float staminaDecreaseRate = 0.5f;
    [Range(0, 1)] public float staminaIncreaseRate = 0.5f;
    public bool runCooldown = false;
    public AudioSource breathAudioSource;
    [Space(5)]

    public GameObject gameManager;

    // Use this for initialization
    void Start()
    {
        breathAudioSource.loop = true;
        breathAudioSource.clip = gameManager.GetComponent<SoundManager>().soundProfile.breathSound;
        breathAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        RunStaminaFunctionality();
    }

    void RunStaminaFunctionality()
    {
        if (currentRunStamina <= 1)
        {
            currentRunStamina = 1;
            runCooldown = true;
            GetComponent<PlayerController>().canRun = false;
            GetComponent<PlayerController>().isRunning = false;
        }

        if (currentRunStamina >= 40 && FootstepController.canRun)
        {
            GetComponent<PlayerController>().canRun = true;
            runCooldown = false;
        }
        if (GetComponent<PlayerController>().isRunning && GetComponent<PlayerController>().canRun)
        {
            UIManager.instance.RunStaminaUI(currentRunStamina, true);
            currentRunStamina -= staminaDecreaseRate * Time.deltaTime * 10;
        }
        else
        {
            UIManager.instance.RunStaminaUI(currentRunStamina, false);
            currentRunStamina += staminaIncreaseRate * Time.deltaTime * 10;
        }

        if (currentRunStamina >= maxRunStamina) currentRunStamina = maxRunStamina; //Fix

        //Sound Functionality
        if (currentRunStamina <= 50) breathAudioSource.volume = Mathf.Lerp(breathAudioSource.volume, (100 - currentRunStamina) / 200f, Time.deltaTime);
        else breathAudioSource.volume -= Time.deltaTime / 7;
    }

}
