using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSea : MonoBehaviour {

    // Delagate (Callback function)
    public delegate void ParticleFunc(int x, int z);

    // Variables for the particles
    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particlesArray;
    private int particleCount;

    [Header("Particle Settings")]
    [SerializeField]    private int particleCountX = 25;
    [SerializeField]    private int particleCountZ = 25;
    [SerializeField]    private float spacing = 0.25f;
    [SerializeField]    private Gradient colorGradient;
    [SerializeField]    private bool useColorPercentage = false;

    [Header("Noise Settings")]
    [SerializeField]    private float noiseSpeedX = 0.01f;
    [SerializeField]    private float noiseSpeedZ = 0.01f;
    [SerializeField]    private float noiseScale = 0.2f;
    [SerializeField]    private float heightScale = 3f;
    private float perlinNoiseAnimX = 0.01f;
    private float perlinNoiseAnimZ = 0.01f;


    void Start() {
        // Get ParticleSystem component
        particleSystem = GetComponent<ParticleSystem>();

        // Saves the number of particles
        particleCount = particleCountX * particleCountZ;

        // Sets the max amount of particles for the particle system
        ParticleSystem.MainModule particleSystem_main = particleSystem.main;
        particleSystem_main.maxParticles = particleCount;

        // Emit the requested amount of particles
        particleSystem.Emit(particleCount);

        // Initialize the array
        particlesArray = new ParticleSystem.Particle[particleCount];
        particleSystem.GetParticles(particlesArray);
    }

    void Update() {

        // Updates the position of every particle to a perlin noise generated position
        ForEachParticle((int x, int z) => { 
            float yPos = Mathf.PerlinNoise(x * noiseScale + perlinNoiseAnimX, z * noiseScale + perlinNoiseAnimZ) * heightScale;        
            Vector3 newParticlePos = new Vector3(x * spacing, yPos, z * spacing);
            SetParticlePosition(x, z, newParticlePos);

            SetParticleColor(x, z, colorGradient.Evaluate((useColorPercentage) ? (yPos / heightScale) : yPos));
        });

        perlinNoiseAnimX += noiseSpeedX; perlinNoiseAnimZ += noiseSpeedZ;     
        Sync(); // Update the particle system
    }


    // Calls a function for each particle in the system 
    public void ForEachParticle(ParticleFunc func) {
        for (int x = 0; x < particleCountX; x++) // Going through X positions...
            for (int z = 0; z < particleCountZ; z++) // Going through Z positions...
                func(x, z);
    }

    // Find the index of a specific particle using more logical coordinates 
    private int GetParticleIndex(int xPos, int zPos) {
        return (xPos * particleCountZ + zPos);
    }

    // Set the position of a specific particle using coordinates
    private void SetParticlePosition(int xPos, int zPos, Vector3 position) { 
        int index = GetParticleIndex(xPos, zPos);
        particlesArray[index].position = position;
    }

    // Set the color of a specific particle using coordinates
    private void SetParticleColor(int xPos, int zPos, Color color) { 
        int index = GetParticleIndex(xPos, zPos);
        particlesArray[index].startColor = color;
    }

    // Assign the potentially updated array back to the particle system
    private void Sync() { 
        particleSystem.SetParticles(particlesArray, particlesArray.Length);
    }
}