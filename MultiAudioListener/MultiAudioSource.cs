﻿/*
 * Copyright (c) 2016 Gaël Vanhalst
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *    1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 
 *    2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 
 *    3. This notice may not be removed or altered from any source
 *    distribution.
 */

//Activate this pragma in case the sub audio needs to be shown in hierachy for debugging purposes.
//#define ShowSubAudioSourcesInHierachy

using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Assets.MultiAudioListener;
using UnityEngine.Audio;

namespace Assets.MultiAudioListener
{
    public class MultiAudioSource : MonoBehaviour
    {
        #region AudioSourceProperties

        //Properties from the normal audiosource

        [SerializeField] private AudioClip _audioClip = null;

        public AudioClip AudioClip
        {
            get { return _audioClip; }
            set
            {
                _audioClip = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.clip = value;
                }
            }
        }

        [SerializeField] private AudioMixerGroup _output = null;

        public AudioMixerGroup Output
        {
            get { return _output; }
            set
            {
                _output = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.outputAudioMixerGroup = value;
                }
            }
        }

        [SerializeField] private bool _mute = false;

        public bool Mute
        {
            get { return _mute; }
            set
            {
                _mute = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.mute = value;
                }
            }
        }

        [SerializeField] private bool _bypassEffects = true;

        public bool BypassEffects
        {
            get { return _bypassEffects; }
            set
            {
                _bypassEffects = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.bypassEffects = value;
                }
            }
        }

        [SerializeField] private bool _bypassListenerEffects = true;

        public bool BypassListenerEffects
        {
            get { return _bypassListenerEffects; }
            set
            {
                _bypassListenerEffects = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.bypassListenerEffects = value;
                }
            }
        }

        [SerializeField] private bool _bypassReverbZone = true;

        public bool BypassReverbZone
        {
            get { return _bypassReverbZone; }
            set
            {
                _bypassReverbZone = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.bypassReverbZones = value;
                }
            }
        }

        [SerializeField] private bool _playOnAwake = false;

        public bool PlayOnAwake
        {
            get { return _playOnAwake; }
            set
            {
                _playOnAwake = value;
                //Play on awake gets handles by the main audiosource, so subAudioSources don't need it
            }
        }

        [SerializeField] private bool _loop = false;

        public bool Loop
        {
            get { return _loop; }
            set
            {
                _loop = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.loop = value;
                }
            }
        }

        [Range(0, 256)] [SerializeField] private int _priority = 128;

        public int Priority
        {
            get { return _priority; }
            set
            {
                _priority = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.priority = value;
                }
            }
        }

        [Range(0.0f, 1.0f)] [SerializeField] private float _volume = 1.0f;

        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.volume = value*subAudioSource.Key.Volume;
                }
            }
        }

        [Range(-3.0f, 3.0f)] [SerializeField] private float _pitch = 1.0f;

        public float Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.pitch = value;
                }
            }
        }

		[Range(0.0f, 360.0f)] [SerializeField] private float _spread = 0.0f;

        public float Spread
        {
            get { return _spread; }
            set
            {
                _spread = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.spread = value;
                }
            }
        }

        [SerializeField] private AudioRolloffMode _volumeRolloff = AudioRolloffMode.Logarithmic;

        public AudioRolloffMode VolumeRolloff
        {
            get { return _volumeRolloff; }
            set
            {
                _volumeRolloff = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.rolloffMode = value;
                }
            }
        }

        [SerializeField] private float _minDistance = 1.0f;

        public float MinDistance
        {
            get { return _minDistance; }
            set
            {
                _minDistance = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.minDistance = value;
                }
            }
        }

        [SerializeField] private float _maxDistance = 500.0f;

        public float MaxDistance
        {
            get { return _maxDistance; }
            set
            {
                _maxDistance = value;
                foreach (var subAudioSource in _subAudioSources)
                {
                    subAudioSource.Value.maxDistance = value;
                }
            }
        }

		//Add custom curves when needed

		#endregion
		//Extra options
		public bool OnlyPlayForClosestCamera = true;

		//Internal components

		private Dictionary<VirtualMultiAudioListener, AudioSource> _subAudioSources =
            new Dictionary<VirtualMultiAudioListener, AudioSource>();

        private bool _isPlaying = false;
        private bool _isPaused = false;

        public bool IsPaused
        {
            get { return _isPaused; }
        }

        public bool IsPlaying
        {
            get { return _isPlaying && !_isPaused; }
        }

        private void OnEnable()
        {
            if (_playOnAwake)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            if (_isPlaying)
            {
                Stop();
            }
        }

        /// <summary>
        /// Start playing the sound
        /// </summary>
        public void Play()
        {
            if (!_isPlaying)
            {
                _isPlaying = true;

                //We subscribe to these events so sub audio sources can be added or removed if needed
                MainMultiAudioListener.OnVirtualAudioListenerAdded += VirtualAudioListenerAdded;
                MainMultiAudioListener.OnVirtualAudioListenerRemoved += VirtualAudioListenerRemoved;

                bool hardwareChannelsLeft = true;

                //Create all sub audio sources
                var virtualAudioListeners = MainMultiAudioListener.VirtualAudioListeners;
                for (int i = 0; i < virtualAudioListeners.Count; i++)
                {
                    CreateSubAudioSource(virtualAudioListeners[i],ref hardwareChannelsLeft);
                }
            }
            else
            {
                foreach (var audioSource in _subAudioSources)
                {
                    audioSource.Value.Play();
                }
            }

            _isPaused = false;
        }

        /// <summary>
        /// Stop playing the sound
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying) return;
            _isPaused = false;
            _isPlaying = false;

            MainMultiAudioListener.OnVirtualAudioListenerAdded -= VirtualAudioListenerAdded;
            MainMultiAudioListener.OnVirtualAudioListenerRemoved -= VirtualAudioListenerRemoved;

            //Remove all old subAudio
            foreach (var subAudioSource in _subAudioSources)
            {
                if (subAudioSource.Value != null)
                {
                    MainMultiAudioListener.EnquequeAudioSourceInPool(subAudioSource.Value);
                }
            }
            _subAudioSources.Clear();
        }

        /// <summary>
        /// Pause the sound
        /// </summary>
        public void Pause()
        {
            if (!_isPlaying || _isPaused) return;

            _isPaused = true;
            foreach (var subAudioSource in _subAudioSources)
            {
                subAudioSource.Value.Pause();
            }
        }

        /// <summary>
        /// Unpause the sound if it was paused
        /// </summary>
        public void UnPause()
        {
            if (!_isPaused) return;

            _isPaused = false;
            foreach (var subAudioSource in _subAudioSources)
            {
                subAudioSource.Value.UnPause();
            }
        }

        void LateUpdate()
        {
            if (_isPlaying || _isPaused)
            {
                 //Closest audioCulling
                AudioSource closestAudio = null;
                float distanceClosestAudio = 0;
				bool isCurrentlyPlaying = false;

                foreach (var subAudioSource in _subAudioSources)
                {
					//We set the mute on the correct value before we cull
					subAudioSource.Value.mute = Mute;

					//ClosestAudioCulling
					if (OnlyPlayForClosestCamera)
					{
                        var distance = (subAudioSource.Key.transform.position - transform.position).sqrMagnitude;

                        if ((closestAudio == null) || (distance < distanceClosestAudio))
                        {
							if (closestAudio != null)
								closestAudio.mute = true;

							closestAudio = subAudioSource.Value;
							closestAudio.volume = Volume * subAudioSource.Key.Volume;
							distanceClosestAudio = distance;
						}
                        else
							subAudioSource.Value.mute = true;
                    }
					MoveSubAudioSourceToNeededLocation(subAudioSource.Key, subAudioSource.Value);
					isCurrentlyPlaying |= subAudioSource.Value.isPlaying;
				}

				if (!isCurrentlyPlaying && !_isPaused)
					Stop();
			}
        }

        private void VirtualAudioListenerAdded(VirtualMultiAudioListener virtualAudioListener)
        {
            bool hardwareChannelsLeft = true;
            CreateSubAudioSource(virtualAudioListener,ref hardwareChannelsLeft);
        }

        private void VirtualAudioListenerRemoved(VirtualMultiAudioListener virtualAudioListener)
        {
            var audioSource = _subAudioSources[virtualAudioListener];
            _subAudioSources.Remove(virtualAudioListener);

            if (audioSource != null)
            {
                MainMultiAudioListener.EnquequeAudioSourceInPool(audioSource);
            }
        }

        private void CreateSubAudioSource(VirtualMultiAudioListener virtualAudioListener,ref bool hardWareChannelsLeft)
        {
            var audioSource = CreateAudioSource( "Sub Audio Source "+ virtualAudioListener.Num, ref hardWareChannelsLeft);
            _subAudioSources.Add(virtualAudioListener, audioSource);
            audioSource.volume = Volume*virtualAudioListener.Volume;

            //Do transform
            MoveSubAudioSourceToNeededLocation(virtualAudioListener, audioSource);
        }

        private void MoveSubAudioSourceToNeededLocation(VirtualMultiAudioListener virtualListener,
            AudioSource subAudioSource)
        {
            //There is no main listener so translation is not needed
            if (MainMultiAudioListener.Main == null) return;
			// Get the position of the object relative to the virtual listener
			Vector3 localPos = virtualListener.transform.InverseTransformPoint(transform.position);

			//Get the relative distance
			Vector3 distance = MainMultiAudioListener.Main.transform.TransformPoint(localPos) - subAudioSource.transform.position;

			//If the distance is too small don't move it if not set the position of the subAudioSource object relative to the main audio listener
			if (distance.sqrMagnitude > 1)
				subAudioSource.transform.Translate(distance);
		}

        private AudioSource CreateAudioSource(string nameSubAudioSource,ref bool hardwareChannelsLeft)
        {
            AudioSource audioSource = MainMultiAudioListener.GetAudioSourceFromPool();
            //If no audiosource was given by pool, make a new one
            if (audioSource == null)
            {
                var subAudioSourceGameObject = new GameObject(nameSubAudioSource);
                audioSource = subAudioSourceGameObject.AddComponent<AudioSource>();
            }
            else
            {
                audioSource.gameObject.name = nameSubAudioSource;
            }
#if !ShowSubAudioSourcesInHierachy
            //We hide the sub audio source in hierarchy so that it doesn't flood it
            audioSource.gameObject.hideFlags = HideFlags.HideInHierarchy;
			
#else
			audioSource.transform.SetParent(MainMultiAudioListener.Main.transform);
#endif

			SetAllValuesAudioSource(audioSource);


            if (_isPaused)
				audioSource.Pause();
            
			if (_isPlaying && hardwareChannelsLeft)
			{
				audioSource.Play();
				//If this sound gets culled all following will be too
				if (!audioSource.isPlaying)
				{
					hardwareChannelsLeft = false;
				}
			}
			//All audio should be fully 3d
			audioSource.spatialBlend = 1.0f;
			//All audio doppler effect and reverb should be  zero
			audioSource.dopplerLevel = 0f;
			audioSource.reverbZoneMix = 0f;
			return audioSource;
        }

        /// <summary>
        /// This refreshes all the properties of the subaudiosources, to guarantee that they are in sync with the main properties
        /// </summary>
        public void RefreshAllPropertiesAudioSources()
        {
            foreach (var subAudioSource in _subAudioSources)
            {
                SetAllValuesAudioSource(subAudioSource.Value);
            }
        }

        private void SetAllValuesAudioSource(AudioSource audioSource)
        {
			audioSource.clip = AudioClip;
			audioSource.timeSamples = 0;
			audioSource.outputAudioMixerGroup = Output;
            audioSource.loop = Loop;
            audioSource.mute = Mute;
            audioSource.bypassEffects = BypassEffects;
            audioSource.bypassListenerEffects = BypassListenerEffects;
            audioSource.bypassReverbZones = BypassReverbZone;
            audioSource.pitch = Pitch;
            audioSource.priority = Priority;
			audioSource.spread = Spread;
            audioSource.rolloffMode = VolumeRolloff;
            audioSource.minDistance = MinDistance;
            audioSource.maxDistance = MaxDistance;
			audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
			audioSource.playOnAwake = false;
		}
    }
}