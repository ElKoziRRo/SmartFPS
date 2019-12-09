using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour {

    [Header("Attributes")]
    public float health = 100f;
    public float maxHealth = 100f;
    private float damageThreshold;
    public bool canRegenerate = true;
    public float regenerateRate = 0.5f;
    [Space]

    [Header("Sound")]
    public AudioSource healthAudioSource;
    public AudioSource hurtAudioSource;
    private SoundStake soundProfile;

    public static HealthController instance;
    // Use this for initialization
    void Start()
    {
        instance = this;
        soundProfile = GameObject.Find("GameManager").GetComponent<SoundManager>().soundProfile;
        hurtAudioSource.clip = soundProfile.heartbeatSound;
        hurtAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (canRegenerate)
            Regeneration();

        if (Input.GetKeyDown(KeyCode.K))
        {
            Vector3 ranPos = new Vector3(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100));
            Debug.Log(ranPos);
            TakeDamage(Random.Range(1, 30), ranPos);
        }
        if (Input.GetKeyDown(KeyCode.H))
            health++;
    }

    void Regeneration()
    {
        if (health < maxHealth)
            health += Time.deltaTime * regenerateRate;
    }
    public void TakeDamage(float amount, Vector3 source)
    {
        damageThreshold = amount;
        health -= amount;
        UIManager.instance.healthDF = DamageRatio();
        healthAudioSource.clip = soundProfile.hurtSounds[Random.Range(0, soundProfile.hurtSounds.Length)];
        healthAudioSource.volume = 1f;
        healthAudioSource.Play();
        UIManager.instance.ShowDamageIndicator(source);
        UIManager.instance.damageDF = DamageRatio() * 1.5f;
    }
    float DamageRatio()
    {
        var dr = 0.0f;
        if (damageThreshold <= 10)
            dr = 0.8f;
        else if (damageThreshold <= 20)
            dr = 1.4f;
        else if (damageThreshold <= 30)
            dr = 2f;
        else if (damageThreshold <= 40)
            dr = 2.2f;
        else if (damageThreshold > 50)
            dr = 2.5f;
        return dr;
    }

}
