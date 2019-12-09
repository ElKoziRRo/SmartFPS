using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject player;
    public GameObject mainCam;
    [Header("Health")]
    [Space]
    public Text healthText;
    public Image heartFill;
    public Image hurtMask;
    public float healthDF = 0.0f;
    private float alpha = 0.0f;
    [Space]

    [Header("Damage Indicator")]
    [Space]
    public GameObject damageIndicatorRoot;
    public Image damageIndicatorImage;
    public float damageDF = 0.0f;
    public float rotationOffset;
    private float damageAlpha = 0.0f;
    private Vector3 damageDir;
    [Space]

    [Header("Run Stamina")]
    [Space]
    public GameObject rootUI;
    public Slider staminaBar;
    [Space]

    [Header("Weapon Interface")]
    [Space]
    public Text weaponName;
    public Text currentAmmo;
    public Text totalAmmo;
    public GameObject semiModeRoot;
    public GameObject autoModeRoot;
    public enum fireMode { Semi, Auto };
    public fireMode gunType;

    public static UIManager instance;

    void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        HealthUI();
        DamageIndicatorUI();
    }   

    /// <summary>
    /// Update Health Amount to UI
    /// </summary>
    void HealthUI()
    {
        int health = (int)player.GetComponent<HealthController>().health;
        healthText.text = health.ToString();
        heartFill.fillAmount = player.GetComponent<HealthController>().health / 100;

        if (healthDF > 0.0f)
        {
            healthDF -= Time.deltaTime;
            alpha = healthDF;

            if (health > 0)
            {
                Color col = hurtMask.color;
                col.a = alpha;
                hurtMask.color = col;
            }
        }
    }

    /// <summary>
    /// Update guns Mode in UI
    /// </summary>
    /// <param name="type"></param>
    void UpdateGunType(fireMode type)
    {
        if (gunType == fireMode.Auto)
        {
            semiModeRoot.SetActive(false);
            autoModeRoot.SetActive(true);
        }
        else if(gunType == fireMode.Semi)
        {
            autoModeRoot.SetActive(false);
            semiModeRoot.SetActive(true);
        }

    }

    /// <summary>
    /// Show Run Stamina Bar
    /// </summary>
    /// <param name="value"></param>
    /// <param name="showBar"></param>
    public void RunStaminaUI(float value, bool showBar)
    {
        staminaBar.value = value;

        if (showBar) rootUI.GetComponent<CanvasGroup>().alpha += Time.deltaTime;

        else rootUI.GetComponent<CanvasGroup>().alpha -= Time.deltaTime / 2;
    }

    public void ShowDamageIndicator(Vector3 origin)
    {
        damageDir = origin;
    }

    void DamageIndicatorUI()
    {

        Vector3 relativePos = damageDir - player.transform.position;
        relativePos.y = 0;
        relativePos.Normalize();
        Vector3 _forward = Camera.main.transform.forward;

        float getPos = Vector3.Dot(_forward, relativePos);

        if (Vector3.Cross(_forward, relativePos).y < 0)
        {
            rotationOffset = (1f - getPos) * 90;
        }
        else
        {
            rotationOffset = (1f - getPos) * -90;
        }
        damageIndicatorRoot.transform.rotation = Quaternion.Euler(0, 0, rotationOffset);

        if (damageDF > 0.0f)
        {
            damageDF -= Time.deltaTime;
            damageAlpha = damageDF;

            if (damageDF > 0)
            {
                Color col = damageIndicatorImage.color;
                col.a = damageAlpha;
                damageIndicatorImage.color = col;
            }
        }
    }
}

