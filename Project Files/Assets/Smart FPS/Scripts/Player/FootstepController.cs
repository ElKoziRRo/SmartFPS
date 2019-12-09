
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepController : MonoBehaviour
{
    #region Attributes
    public enum _type { TerrainTextureAndPhysicsMaterial, TerrainTextureAndTag}; // Identify Terrain Texture and Physics Material or Terrain Texture and Tag
    public _type detectionType;

    public enum _trigger {Smart, Manual}; // Update Surface Automatically (Smart) or Update manually by other script. Up to client
    public _trigger triggerMode;

    public enum _mode { StepCycle, AnimationEvent }; // Play sound according to step cycle or playing through Animation Event
    public _mode playingMode;


    #endregion

    #region Tag Variables
    [Serializable]
    public class TagAttribute
    {
        public string name; // Name of the tag
        public string tagName; // Actual Tag name
        public AudioClip[] footstepSounds; // Footstep sound that will play when player on this tag
        public float walkSpeed; //Player Walk Speed on this Tag
        public float runSpeed; //Player Run speed on this tag
        public bool canRun; // Can Player Run on this Tag
    }

    public TagAttribute[] tags;
    #endregion

    #region Material Variables
    [Serializable]
    public class MaterialAttribute
    {
        public string name; //Name of the Physics Material
        public PhysicMaterial material; 
        public AudioClip[] footstepSounds; // Footstep sound that will play when player is on that Phyisics Material
        public float walkSpeed; //Player walk speed on this Physics Material
        public float runSpeed; //Player Run speed on this Physics Material
        public bool canRun; //Can Player run on this Physics Material
    }

    public MaterialAttribute[] physicsMaterials;
    #endregion

    #region Texture Variables
    [Serializable]
    public class TextureAttributes
    {
        public string name; //Texture Name
        public int index; // Index of Texture (Must be increment from 0)
        public Texture texture; // Actual Texture of that surface
        public AudioClip[] footstepSounds; // Sound that will play 
        public float walkSpeed; //Player Walk speed on this Texture
        public float runSpeed; //Player Run speed on this Texture
        public bool canRun; //Can player run on this Texture 
    }

    public TextureAttributes[] textureAttributes;
    #endregion

    #region Settings
    [Header("Settings")]
    [Space]
    public float footstepVolume; 
    private AudioClip[] footstepSound;
    public float jumpLandVolume;
    public float jumpVolume;
    public AudioSource footstepAudioSource;
    public AudioSource slideAudioSource;
    public float stepInterval = 3.4f;
    public float runstepLenghten = 0.7f;
    private float stepCycle;
    private float nextStep;
    #endregion

    #region Private Variables
    public static bool canRun;
    private bool onTerrain = false;
    private Terrain terrain;   // Store Current Terrain
    private bool onMesh = false;
    private string currentMeshTag; // Store Current Tag Runtime 
    private PhysicMaterial currentPhysicsMaterial; // Store Current Physics Material Runtime 

    private CharacterController controller;
    private PlayerController player;
    private SoundStake soundProfile;
    #endregion

    // Use this for initialization
    void Start ()
    {
        controller = GetComponent<CharacterController>();
        player = GetComponent<PlayerController>();
        soundProfile = GameObject.Find("GameManager").GetComponent<SoundManager>().soundProfile;
        InvokeRepeating("GetSurfaceSound", 0, 0.1f); // As getting terrain material is complex algorithm, we will can that function each 0.5s for optimization
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    /// <summary>
    /// Call this function from animation event if you want to make footstep sound according to animation. Make sure to change 'Playing Mode' to Animation Cycle.
    /// So, this method will ignore all functionalities for Step Cycle else it will throw some error to Console.
    /// </summary>
    public void PlayFootstepSound()
    {
        if (playingMode == _mode.StepCycle)
        {
            if (controller.velocity.sqrMagnitude > 0 && (player.horizontalAxisInput != 0 || player.verticalAxisInput != 0))
            {
                stepCycle += (controller.velocity.magnitude + (player.currentSpeed * (!player.isRunning ? 1f : runstepLenghten))) * Time.fixedDeltaTime;
            }

            if (!(stepCycle > nextStep))
            {
                return;
            }

            nextStep = stepCycle + stepInterval;
        }

        footstepAudioSource.clip = footstepSound[UnityEngine.Random.Range(0, footstepSound.Length)];
        footstepAudioSource.volume = footstepVolume;
        footstepAudioSource.Play();
    }

    public void PlaySlideSound()
    {
        slideAudioSource.loop = true;
        slideAudioSource.clip = soundProfile.slideSound;
        slideAudioSource.Play();
        slideAudioSource.loop = false;
    }
    public void JumpLandSound()
    {
        footstepAudioSource.clip = soundProfile.jumpLandSound;
        footstepAudioSource.volume = jumpLandVolume;
        footstepAudioSource.Play();

        if (playingMode == _mode.StepCycle)
        {
            nextStep = +stepCycle + 0.5f;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Update while Smart mode is on
        if (triggerMode == _trigger.Smart)
        {
            if (hit.collider.GetComponent<TerrainCollider>() != null)
            {
                onTerrain = true;
                onMesh = false;
                terrain = hit.collider.GetComponent<Terrain>();
            }
            else if (hit.collider.GetComponent<MeshCollider>() != null || hit.collider.GetComponent<BoxCollider>() != null)
            {
                onMesh = true;
                onTerrain = false;
                currentMeshTag = hit.collider.tag;
            }
        }
    }

    void GetSurfaceSound()
    {
        if (onTerrain)
        {
            TerrainData terrainData = terrain.terrainData;

            #region Texture Mix
            // calculate which splat map cell the worldPos falls within (ignoring y)
            int mapX = (int)(((transform.position.x - terrain.GetPosition().x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((transform.position.z - terrain.GetPosition().z) / terrainData.size.z) * terrainData.alphamapHeight);

            // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            // extract the 3D array data to a 1D array:
            float[] textureMix = new float[splatmapData.GetUpperBound(2) + 1];

            for (int n = 0; n < textureMix.Length; n++)
            {
                textureMix[n] = splatmapData[0, 0, n];
            }
            #endregion

            #region Texture Index
            float maxMix = 0;
            int maxIndex = 0;

            // loop through each mix value and find the maximum
            for (int n = 0; n < textureMix.Length; n++)
            {
                if (textureMix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = textureMix[n];
                }
            }

            int textureIndex = maxIndex;
            #endregion

            #region Set Footstep Sound
            string textureName = terrainData.splatPrototypes[textureIndex].texture.name;

            foreach (var texture in textureAttributes)
            {
                if (texture.texture.name == textureName)
                {
                    footstepSound = textureAttributes[texture.index].footstepSounds;
                    canRun = textureAttributes[texture.index].canRun;
                    GetComponent<PlayerController>().canRun = textureAttributes[texture.index].canRun;
                    GetComponent<PlayerController>().walkSpeed = textureAttributes[texture.index].walkSpeed;
                    GetComponent<PlayerController>().runSpeed = textureAttributes[texture.index].runSpeed;
                }
            }
            #endregion
        }

        else if (onMesh)
        {
            //Physics Material
            if (detectionType == _type.TerrainTextureAndPhysicsMaterial)
            {
                foreach (var physicsMaterial in physicsMaterials)
                {
                    if (currentPhysicsMaterial == physicsMaterial.material)
                    {
                        footstepSound = physicsMaterial.footstepSounds;
                        canRun = physicsMaterial.canRun;
                        GetComponent<PlayerController>().canRun = physicsMaterial.canRun;
                       /* if(!physicsMaterial.canRun)
                            if (GetComponent<PlayerController>().isRunning)
                                GetComponent<PlayerController>().isRunning = false;  */
                        GetComponent<PlayerController>().walkSpeed = physicsMaterial.walkSpeed;
                        GetComponent<PlayerController>().runSpeed = physicsMaterial.runSpeed;

                    }
                }
            }

            //Tag
            else if (detectionType == _type.TerrainTextureAndTag)
            {
                foreach (var tag in tags)
                {
                    if (currentMeshTag == tag.tagName)
                    {
                        footstepSound = tag.footstepSounds;
                        canRun = tag.canRun;
                        GetComponent<PlayerController>().canRun = tag.canRun;
                      /* if (!tag.canRun)
                            if (GetComponent<PlayerController>().isRunning)
                                GetComponent<PlayerController>().isRunning = false; */
                        GetComponent<PlayerController>().walkSpeed = tag.walkSpeed;
                        GetComponent<PlayerController>().runSpeed = tag.runSpeed;

                    }
                }
            }
        }
    }

}
