using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnEnterTriggerFireworks : MonoBehaviour
{
    public bool isTrail, isExplosion;

    void OnParticleTrigger()
    {

        ParticleSystem ps = GetComponent<ParticleSystem>();

        // particles
        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
        List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();

        // get
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        int numExit = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);

        // iterate
        for (int i = 0; i < numEnter; i++)
        {
            if (i == 0)
            {
                if (isTrail)
                {
                    Debug.Log("Bla");
                    SoundManager.instance.PlayFireworksRandom();
                }
            }

            ParticleSystem.Particle p = enter[i];
            //p.startColor = new Color32(255, 0, 0, 255);
            enter[i] = p;

            if (isExplosion)
            {
                SoundManager.instance.PlayFireworksExplosionsRandom();
            }
        }
        for (int i = 0; i < numExit; i++)
        {
            ParticleSystem.Particle p = exit[i];
            //p.startColor = new Color32(0, 255, 0, 255);
            exit[i] = p;
        }

        // set
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);
    }
}
