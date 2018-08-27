using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace YourCommonTools
{

	/******************************************
	 * 
	 * SoundsController
	 * 
	 * Class that handles the sounds played in the app
	 * 
	 * @author Esteban Gallardo
	 */
	public class SoundsController : MonoBehaviour
	{
        // ----------------------------------------------
        // EVENTS
        // ----------------------------------------------
        public const string EVENT_SOUNDSCONTROLLER_AUDIO_DATA = "EVENT_SOUNDSCONTROLLER_AUDIO_DATA";

        // ----------------------------------------------
        // SINGLETON
        // ----------------------------------------------
        private static SoundsController _instance;

		public static SoundsController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType<SoundsController>();
					_instance.Initialize();
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------
		public const string SOUND_COOCKIE = "SOUND_COOCKIE";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public AudioClip[] Sounds;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private AudioSource m_audio1;
		private AudioSource m_audio2;
		private bool m_enabled = true;
		private bool m_enableFX = true;
		private bool m_enableMelodies = true;
		private bool m_hasBeenInitialized = false;
        private int m_requestAudioData = 0;
        private string m_microphoneDeviceName = "";

        public AudioSource Audio1
        {
            get { return m_audio1; }
        }
        public AudioSource Audio2
        {
            get { return m_audio2; }
        }
        public bool Enabled
		{
			get { return m_enabled; }
			set
			{
				m_enabled = value;
				if (!m_enabled)
				{
					StopAllSounds();
				}
				PlayerPrefs.SetInt(SOUND_COOCKIE, (m_enabled ? 1 : 0));
			}
		}

		public bool EnableFX
		{
			get { return m_enableFX; }
			set { m_enableFX = value; }
		}
		public bool EnableMelodies
		{
			get { return m_enableMelodies; }
			set { m_enableMelodies = value; }
		}
		public float VolumeLoop
		{
			get { return m_audio1.volume; }
			set { m_audio1.volume = value; }
		}
        public int RequestAudioData
        {
            get { return m_requestAudioData; }
            set { m_requestAudioData = value; }
        }

        // ----------------------------------------------
        // CONSTRUCTOR
        // ----------------------------------------------	
        // -------------------------------------------
        /* 
		 * Constructor
		 */
        public SoundsController()
		{
		}

		// ----------------------------------------------
		// INIT/DESTROY
		// ----------------------------------------------	

		// -------------------------------------------
		/* 
		 * Destroy audio's gameObject		
		 */
		public void Initialize()
		{
			if (m_hasBeenInitialized) return;
			m_hasBeenInitialized = true;

			AudioSource[] aSources = GetComponents<AudioSource>();
			if (aSources != null)
			{
				if (aSources.Length > 0) m_audio1 = aSources[0];
				if (aSources.Length > 1) m_audio2 = aSources[1];
			}

			m_enabled = (PlayerPrefs.GetInt(SOUND_COOCKIE, 1) == 1);            
        }

		// -------------------------------------------
		/* 
		 * Release resources on unload
		 */
		void OnDestroy()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * Destroy audio's gameObject		
		 */
		public void Destroy()
		{
			if (m_audio1 != null) Destroy(m_audio1);
			if (m_audio2 != null) Destroy(m_audio2);
		}

		// -------------------------------------------
		/* 
		 * StopAllSounds
		 */
		public void StopAllSounds()
		{
			if (m_audio1 != null) m_audio1.Stop();
			if (m_audio2 != null) m_audio2.Stop();
		}

		// -------------------------------------------
		/* 
		 * Play loop
		 */
		public void PlaySoundLoop(AudioClip _audio)
		{
			if (_audio == null) return;
			if (!m_enabled) return;
			if (!m_enableMelodies) return;

			if (m_audio1 != null)
			{
				m_audio1.clip = _audio;
				m_audio1.loop = true;
				if (!m_audio1.isPlaying)
				{
					m_audio1.Play();
				}
			}
		}

		// -------------------------------------------
		/* 
		 * StopAllSounds
		 */
		public void StopMainLoop()
		{
			if (m_audio1 != null)
			{
				m_audio1.clip = null;
				m_audio1.Stop();
			}
		}

		// -------------------------------------------
		/* 
		 * PlayLoopSound
		 */
		public void PlayLoopSound(string _audioName)
		{
			for (int i = 0; i < Sounds.Length; i++)
			{
				if (Sounds[i] != null)
				{
					if (Sounds[i].name == _audioName)
					{
						PlaySoundLoop(Sounds[i]);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * PlaySingleSound
		 */
		public void PlaySingleSound(string _audioName, bool _force = false)
		{
			for (int i = 0; i < Sounds.Length; i++)
			{
				if (Sounds[i] != null)
				{
					if (Sounds[i].name == _audioName)
					{
						PlaySingleSound(Sounds[i], _force);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * PlaySingleSound
		 */
		public void PlaySingleSound(AudioClip _audio, bool _force = false)
		{
			if (!_force)
			{
				if (!m_enableFX) return;
				if (!m_enabled) return;
			}

			if (_audio != null)
			{
				if (m_audio2 != null)
				{
					m_audio2.PlayOneShot(_audio);
				}					
			}
		}

        // -------------------------------------------
        /* 
		 * PlaySingleSound
		 */
        public void StartMicrophoneRecording(int _totalTimeMicrophone)
        {
            if (Microphone.devices.Length > 0)
            {
                m_microphoneDeviceName = Microphone.devices[0];

                m_audio1.volume = 1f;
                m_audio1.clip = null;
                m_audio1.loop = false; // Set the AudioClip to loop
                m_audio1.mute = true; // Mute the sound, we don't want the player to hear it
                m_audio1.clip = Microphone.Start(m_microphoneDeviceName, false, _totalTimeMicrophone, 44100);
                while (!(Microphone.GetPosition(m_microphoneDeviceName) > 0)) { } // Wait until the recording has started
                m_audio1.Play();
            }
            else
            {
                m_microphoneDeviceName = "";
                return;
            }
        }

        // -------------------------------------------
        /* 
		 * StopMicrophoneRecording
		 */
        public void StopMicrophoneRecording()
        {
            m_requestAudioData = Microphone.GetPosition(m_microphoneDeviceName);
            SoundsController.Instance.Audio1.Stop();
            Microphone.End(m_microphoneDeviceName);
        }

        // -------------------------------------------
        /* 
		 * PlayRecordedSound
		 */
        public bool IsPlayingRecordedSound()
        {
            return m_audio2.isPlaying;
        }

        // -------------------------------------------
        /* 
        * PlayRecordedSound
        */
        public void PlayRecordedSound(float[] _floatData)
        {
            if (m_audio2 == null) return;

            m_audio2.clip = null;
            m_audio2.clip = AudioClip.Create("Voice User", _floatData.Length, 1, 44100, false);
            m_audio2.clip.SetData(_floatData, 0);
            m_audio2.loop = false;
            m_audio2.volume = 1;
            m_audio2.Play();
        }

        // -------------------------------------------
        /* 
		 * PlaySingleSound
		 */
        void Update()
        {
            if (m_requestAudioData > 0)
            {
                float[] clipData = new float[m_requestAudioData];
                m_requestAudioData = 0;
                m_audio1.clip.GetData(clipData, 0);                
                BasicSystemEventController.Instance.DispatchBasicSystemEvent(EVENT_SOUNDSCONTROLLER_AUDIO_DATA, clipData);                
            }
        }
    }
}