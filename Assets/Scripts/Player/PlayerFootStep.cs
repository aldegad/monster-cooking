using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFootStep : MonoBehaviour
{
    [Header("Footsteps")]

    [Tooltip("Footstep source")]
    [SerializeField] private AudioSource footstepSource;

    [Tooltip("Distance for ground texture checker")]
    [SerializeField] private float groundCheckDistance = 1.0f;

    [Tooltip("Footsteps playing rate")]
    [SerializeField][Range(1f, 10f)] private float runfootstepRate = 1f;

    [Tooltip("Footstep rate when player running")]
    [SerializeField][Range(1f, 10f)] private float sprintFootstepRate = 1.5f;

    [Tooltip("Add textures for this layer and add sounds to be played for this texture")]
    public List<GroundLayer> groundLayers = new List<GroundLayer>();

    private PlayerAnimation playerAnimation;
    private Terrain _terrain;
    private TerrainData _terrainData;
    private TerrainLayer[] _terrainLayers;
    private AudioClip _previousClip;
    private Texture2D _currentTexture;
    private RaycastHit _groundHit;
    private float _nextFootstep;

    private void Start()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void Update()
    {
        GetTerrainData();
    }

    private void FixedUpdate()
    {
        // update footstep
        GroundChecker();
        FootStepChecker();
    }

    //Getting all terrain data for footstep system
    private void GetTerrainData()
    {
        if (_terrain != null) return;

        if (SceneManager.GetActiveScene().name == "GameScene" && Terrain.activeTerrain)
        {
            _terrain = Terrain.activeTerrain;
            _terrainData = _terrain.terrainData;
            _terrainLayers = _terrain.terrainData.terrainLayers;
        }
    }

    //Check where the controller is now and identify the texture of the component
    private void GroundChecker()
    {
        if (_terrainLayers == null) return;

        Ray checkerRay = new Ray(transform.position + (Vector3.up * 0.1f), Vector3.down);

        if (Physics.Raycast(checkerRay, out _groundHit, groundCheckDistance))
        {
            if (_groundHit.collider.GetComponent<Terrain>())
            {
                _currentTexture = _terrainLayers[GetTerrainTexture(transform.position)].diffuseTexture;
            }
            if (_groundHit.collider.GetComponent<Renderer>())
            {
                _currentTexture = GetRendererTexture();
            }
        }
    }

    private void FootStepChecker()
    {
        if (playerAnimation.isRun || playerAnimation.isSprint)
        {
            float currentFootstepRate = (playerAnimation.isSprint ? sprintFootstepRate : runfootstepRate);

            if (_nextFootstep >= 100f)
            {
                {
                    PlayFootstep();
                    _nextFootstep = 0;
                }
            }
            _nextFootstep += (currentFootstepRate * 5f);
        }
    }

    //Play a footstep sound depending on the specific texture
    private void PlayFootstep()
    {
        for (int i = 0; i < groundLayers.Count; i++)
        {
            for (int k = 0; k < groundLayers[i].groundTextures.Length; k++)
            {
                if (_currentTexture == groundLayers[i].groundTextures[k])
                {
                    footstepSource.PlayOneShot(RandomClip(groundLayers[i].footstepSounds));
                }
            }
        }
    }

    //Return an array of textures depending on location of the controller on terrain
    private float[] GetTerrainTexturesArray(Vector3 controllerPosition)
    {
        _terrain = Terrain.activeTerrain;
        _terrainData = _terrain.terrainData;
        Vector3 terrainPosition = _terrain.transform.position;

        int positionX = (int)(((controllerPosition.x - terrainPosition.x) / _terrainData.size.x) * _terrainData.alphamapWidth);
        int positionZ = (int)(((controllerPosition.z - terrainPosition.z) / _terrainData.size.z) * _terrainData.alphamapHeight);

        float[,,] layerData = _terrainData.GetAlphamaps(positionX, positionZ, 1, 1);

        float[] texturesArray = new float[layerData.GetUpperBound(2) + 1];
        for (int n = 0; n < texturesArray.Length; ++n)
        {
            texturesArray[n] = layerData[0, 0, n];
        }
        return texturesArray;
    }

    //Returns the zero index of the prevailing texture based on the controller location on terrain
    private int GetTerrainTexture(Vector3 controllerPosition)
    {
        float[] array = GetTerrainTexturesArray(controllerPosition);
        float maxArray = 0;
        int maxArrayIndex = 0;

        for (int n = 0; n < array.Length; ++n)
        {

            if (array[n] > maxArray)
            {
                maxArrayIndex = n;
                maxArray = array[n];
            }
        }
        return maxArrayIndex;
    }

    //Returns the current main texture of renderer where the controller is located now
    private Texture2D GetRendererTexture()
    {
        Texture2D texture;
        texture = (Texture2D)_groundHit.collider.gameObject.GetComponent<Renderer>().material.mainTexture;
        return texture;
    }

    //Returns an audio clip from an array, prevents a newly played clip from being repeated and randomize pitch
    private AudioClip RandomClip(AudioClip[] clips)
    {
        int attempts = 2;
        footstepSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);

        AudioClip selectedClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        while (selectedClip == _previousClip && attempts > 0)
        {
            selectedClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            attempts--;
        }
        _previousClip = selectedClip;
        return selectedClip;
    }
}

[Serializable]
public class GroundLayer
{
    public string layerName;
    public Texture2D[] groundTextures;
    public AudioClip[] footstepSounds;
}