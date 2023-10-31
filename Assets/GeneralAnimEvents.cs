using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralAnimEvents : MonoBehaviour
{
    [SerializeField] private ParticleSystem connectedParticles;
    [SerializeField] private GameObject connectedParticlesGO;

    [SerializeField] private bool playOnAwake;
    [SerializeField] private bool disableOnClose;

    private void OnEnable()
    {
        if(playOnAwake)
        {
            if(connectedParticles)
            {
                ActivateConnectedEffect();
            }

            if(connectedParticlesGO)
            {
                ActivateConnectedEffectGO();
            }
        }
    }

    private void OnDisable()
    {
        if (disableOnClose)
        {
            if (connectedParticles)
            {
                DeActivateConnectedEffect();
            }

            if (connectedParticlesGO)
            {
                DeActivateConnectedEffectGO();
            }
        }
    }

    public void ActivateConnectedEffect()
    {
        connectedParticles.gameObject.SetActive(true);
    }
    public void ActivateConnectedEffectGO()
    {
        connectedParticlesGO.SetActive(true);
    }
    public void DeActivateConnectedEffect()
    {
        connectedParticles.gameObject.SetActive(false);
    }
    public void DeActivateConnectedEffectGO()
    {
        connectedParticlesGO.SetActive(false);
    }
    
}
