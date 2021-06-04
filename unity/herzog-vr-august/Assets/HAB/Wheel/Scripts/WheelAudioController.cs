using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioClip))]
public class WheelAudioController : MonoBehaviour
{
    public AudioClip clip;

    private Rigidbody body;
    private Quaternion lastRotation;
    private float threshold;
    private float distance;
    private AudioSource[] channels;
    private int channelIndex;
    void Start()
    {
        distance = 0f;
        threshold = 23.5f;
        channelIndex = 0;
        body = GetComponent<Rigidbody>();
        channels = GetComponents<AudioSource>();
        foreach (var item in channels)
        {
            item.clip = clip;
            item.spatialBlend = 1f;
            item.loop = false;
            item.playOnAwake = false;
            item.volume = 0.25f;
        }
    }
    void FixedUpdate()
    {
        distance += body.angularVelocity.magnitude;
        if (distance > threshold)
        {
            distance = threshold - distance;
            PlayClipOnNextChannel();
        }
    }

    private void PlayClipOnNextChannel()
    {
        AudioSource channel = channels[channelIndex];
        channel.pitch = Mathf.Min(0.8f + body.angularVelocity.magnitude * 0.1f, 1.1f);
        channel.pitch += Random.Range(-0.05f, 0.05f);
        channel.volume = Mathf.Min(body.angularVelocity.magnitude * 0.1f, 0.2f);
        channel.Play();
        // Debug.Log(channel.pitch + "\n" + channel.volume);
        // Debug.Log("Playing " + channel.clip + " on channel " + channelIndex);
        channelIndex = (channelIndex + 1) % channels.Length;
    }
}
