using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralAnimEvents : MonoBehaviour
{
    [SerializeField] private ParticleSystem connectedParticles;
    [SerializeField] private GameObject connectedParticlesGO;

    public void ActivateConnectedEffect()
    {
        connectedParticles.gameObject.SetActive(true);
    }
    public void ActivateConnectedEffectGO()
    {
        connectedParticlesGO.SetActive(true);
    }
}
