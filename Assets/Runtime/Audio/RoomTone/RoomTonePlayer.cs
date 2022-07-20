using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Zenject;

namespace SupremacyHangar.Runtime.Audio.RoomTone
{
    public class RoomTonePlayer : MonoBehaviour
    {
        private class PlayingRoomTone
        {
            public RoomTone RoomTone;
            public float FadeStartTime;
            public float FadeEndTime;
            public PlayingState State;
            public AudioSource Source;
        }

        private enum PlayingState
        {
            Stopped,
            FadingIn,
            Playing,
            FadingOut
        }
        
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField] private float fadeTime;

        private readonly Queue<AudioSource> availableSources = new();
        private readonly Queue<PlayingRoomTone> availableRoomTones = new();
        private readonly List<PlayingRoomTone> changingRoomTones = new();
        private readonly Dictionary<RoomTone, PlayingRoomTone> playingRoomTones = new();
        private readonly Dictionary<RoomTone, PlayingRoomTone> fadingRoomTones = new();

        private SignalBus bus;

        [Inject]
        public void InjectDependencies(SignalBus bus)
        {
            this.bus = bus;
            bus.Subscribe<RoomTone>(OnRoomToneChanged);
        }

        
        public void Update()
        {
            if (fadingRoomTones.Count == 0) return;
            foreach (var pair in fadingRoomTones)
            {
                var playingRoomTone = pair.Value;
                if (playingRoomTone.FadeEndTime < Time.time)
                {
                    changingRoomTones.Add(pair.Value);
                    if (playingRoomTone.State == PlayingState.FadingOut)
                    {
                        playingRoomTone.State = PlayingState.Stopped;
                        playingRoomTone.Source.Stop();
                        playingRoomTone.Source.clip = null;
                        playingRoomTone.Source.volume = 0;
                        availableSources.Enqueue(playingRoomTone.Source);
                        playingRoomTone.Source = null;
                    }
                    else
                    {
                        playingRoomTone.Source.volume = 1;
                        playingRoomTone.State = PlayingState.Playing;
                    }
                }
                else
                {
                    float progress = (Time.time - playingRoomTone.FadeStartTime) / fadeTime; 
                    if (playingRoomTone.State == PlayingState.FadingOut)
                    {
                        playingRoomTone.Source.volume = Mathf.Lerp(1, 0, progress);
                    }
                    else
                    {
                        playingRoomTone.Source.volume = Mathf.Lerp(0, 1, progress);
                    }
                }
            }

            if (changingRoomTones.Count == 0) return;

            foreach (var playingRoomTone in changingRoomTones)
            {
                fadingRoomTones.Remove(playingRoomTone.RoomTone);
                if (playingRoomTone.State == PlayingState.Stopped)
                {
                    playingRoomTone.RoomTone = null;
                    availableRoomTones.Enqueue(playingRoomTone);
                }
                else
                {
                    playingRoomTones.Add(playingRoomTone.RoomTone, playingRoomTone);
                }
            }
            
            changingRoomTones.Clear();
        }
        
        public void OnDisable()
        {
            if (bus != null)
            {
                bus.TryUnsubscribe<RoomTone>(OnRoomToneChanged);
            }
        }

        private void OnRoomToneChanged(RoomTone newRoomTone)
        {
            if (playingRoomTones.ContainsKey(newRoomTone)) return;

            if (fadingRoomTones.ContainsKey(newRoomTone))
            {
                var playingRoomTone = fadingRoomTones[newRoomTone];
                if (playingRoomTone.State == PlayingState.FadingOut)
                {
                    playingRoomTone.State = PlayingState.FadingIn;
                    playingRoomTone.FadeStartTime = Time.time - (playingRoomTone.Source.volume) * fadeTime;
                    playingRoomTone.FadeEndTime = playingRoomTone.FadeStartTime + fadeTime;
                }

                foreach (var pair in fadingRoomTones)
                {
                    if (pair.Key == newRoomTone) continue;
                    var fadingRoomTone = pair.Value;
                    if (fadingRoomTone.State == PlayingState.FadingOut) continue;
                    fadingRoomTone.State = PlayingState.FadingOut;
                    fadingRoomTone.FadeStartTime = Time.time - (1-fadingRoomTone.Source.volume) * fadeTime;
                    fadingRoomTone.FadeEndTime = fadingRoomTone.FadeStartTime + fadeTime;
                }
                
                foreach (var pair in playingRoomTones)
                {
                    fadingRoomTones.Add(pair.Key, pair.Value);
                    pair.Value.State = PlayingState.FadingOut;
                    pair.Value.FadeStartTime = Time.time;
                }
            }
            else
            {
                fadingRoomTones.Add(newRoomTone, SetupNewPlaybackSource(newRoomTone)); 
                foreach (KeyValuePair<RoomTone,PlayingRoomTone> pair in playingRoomTones)
                {
                    fadingRoomTones.Add(pair.Key, pair.Value);
                    var playingRoomTone = pair.Value;
                    playingRoomTone.State = PlayingState.FadingOut;
                    playingRoomTone.FadeStartTime = Time.time;
                    playingRoomTone.FadeEndTime = Time.time + fadeTime;
                }
            }
            
            foreach (KeyValuePair<RoomTone, PlayingRoomTone> pair in fadingRoomTones)
            {
                playingRoomTones.Remove(pair.Key);
            }
        }

        private PlayingRoomTone SetupNewPlaybackSource(RoomTone roomTone)
        {
            var playingRoomTone = GetPlayingRoomTone();
            var source = GetAudioSource();
            source.clip = roomTone.Clip;
            source.volume = 0;
            source.Play();
            playingRoomTone.Source = source;
            playingRoomTone.RoomTone = roomTone;
            playingRoomTone.State = PlayingState.FadingIn;
            playingRoomTone.FadeStartTime = Time.time;
            playingRoomTone.FadeEndTime = Time.time + fadeTime;
            return playingRoomTone;
        }

        private AudioSource GetAudioSource()
        {
            if (availableSources.TryDequeue(out var source)) return source;
            source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = mixerGroup;
            source.playOnAwake = false;
            source.loop = true;
            return source;
        }

        private PlayingRoomTone GetPlayingRoomTone()
        {
            if (availableRoomTones.TryDequeue(out var playingRoomTone)) return playingRoomTone;
            return new PlayingRoomTone();
        }
    }
}