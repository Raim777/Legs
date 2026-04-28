using UnityEngine;

namespace Character
{
    public class FootstepView : MonoBehaviour
    {
        [SerializeField] private CharacterMotor motor;
        
        // TODO: replace with FMOD audio manager
        [Header("Footstep Audio")]
        [SerializeField] private AudioClip landingAudioClip;
        [SerializeField] private AudioClip[] footstepAudioClips;
        [Range(0, 1)] [SerializeField] private float footstepAudioVolume = 0.5f;
        [SerializeField] private AudioSource[] audioPool;

        private int _audioPoolIndex;

        private void PlayPooledClip(AudioClip clip, float volume)
        {
            if (clip == null) return;
            AudioSource src = audioPool[_audioPoolIndex];
            _audioPoolIndex = (_audioPoolIndex + 1) % audioPool.Length;
            src.clip = clip;
            src.volume = volume;
            src.Play();
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (!(animationEvent.animatorClipInfo.weight > 0.5f)) return;
            if (footstepAudioClips.Length <= 0) return;

            int index = Random.Range(0, footstepAudioClips.Length);
            PlayPooledClip(footstepAudioClips[index], footstepAudioVolume);
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                PlayPooledClip(landingAudioClip, footstepAudioVolume);
            }
        }
    }
}