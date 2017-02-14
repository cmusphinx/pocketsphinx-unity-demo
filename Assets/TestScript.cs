using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Pocketsphinx;
using System.IO;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(AudioSource))]
public class TestScript : MonoBehaviour {

	AudioSource source_mic;
	AudioSource source_reply;

	Text text;
	Decoder d;

	int countdown;
	int MAX_COUNTDOWN = 50;

	void Start () {
		Debug.Log ("Started");

		SetupDecoder ();

		AudioSource [] sources = GetComponents<AudioSource>();
		source_mic = sources [0];
		source_reply = sources [1];

		source_mic.clip = Microphone.Start(null, true, 1, 44100);
		source_mic.Play ();

		text = GetComponentInChildren<Text> ();

	}

	void SetupDecoder() {
		Debug.Log("!!!!!! Initializing decoder");
		Config c = Decoder.DefaultConfig();
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) {
			c.SetString ("-hmm", "/usr/local/share/pocketsphinx/model/en-us/en-us");
			c.SetString ("-dict", "/usr/local/share/pocketsphinx/model/en-us/cmudict-en-us.dict");
		} else if (Application.platform == RuntimePlatform.Android) {
			c.SetString ("-hmm", "/sdcard/Android/data/edu.cmu.sphinx.pocketsphinx/files/sync/en-us-ptm");
			c.SetString ("-dict", "/sdcard/Android/data/edu.cmu.sphinx.pocketsphinx/files/sync/cmudict-en-us.dict");
		}
		//c.SetString ("-rawlogdir", "/tmp");

		c.SetString ("-keyphrase", "oh unity");
		c.SetFloat ("-kws_threshold", 1e-30);

		c.SetFloat ("-samprate", 44100);
		c.SetInt ("-nfft", 2048);

		d = new Decoder(c);
		d.StartUtt ();
		Debug.Log ("!!!!! Done initializing decoder");
	}

	void Update() {
		if (countdown > 0) {
			Debug.Log ("Countdown " + countdown);

			if (countdown == MAX_COUNTDOWN) {
				text.text = "Yes master";
				source_reply.Play ();
			}
			countdown--;
			if (countdown == 0) {
				text.text = "Say \"oh unity\"";
				source_mic.Play ();
			}

		}
	}
		
	static byte[] convertToBytes (float[] data, int channels)
	{
		float tot = 0;
		byte[] byteData = new byte[data.Length / channels * 2];
		for (int i = 0; i < data.Length / channels; i++) {
			float sum = 0;
			for (int j = 0; j < channels; j++) {
				sum += data [i * channels + j];
			}
			tot += sum * sum;
			short val = (short)(sum / channels * 20000); // volume
			byteData [2 * i] = (byte) (val & 0xff);
			byteData [2 * i + 1] = (byte) (val >> 8);
		}
		//Debug.Log (Math.Sqrt(tot / data.Length / channels));
		return byteData;
	}

	void OnAudioFilterRead(float[] data, int channels) {

		byte[] byteData = convertToBytes (data, channels);

		d.ProcessRaw(byteData, byteData.Length, false, false);
		// Start countdown if hypothesis detected
		if (d.Hyp() != null) {
			Debug.Log ("Detected keyword");
			d.EndUtt ();
			d.StartUtt ();

			countdown = MAX_COUNTDOWN;
		}
	}

}
