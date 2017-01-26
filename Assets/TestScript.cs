using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pocketsphinx;
using System.IO;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class TestScript : MonoBehaviour {

	AudioSource source;
	Text text;
	Decoder d;

	int countdown;
	int MAX_COUNTDOWN = 50;

	void Start () {
		Debug.Log ("Started");

		SetupDecoder ();

		source = GetComponent<AudioSource>();
		source.volume = 0.05f; // Important because we can not mute
		source.clip = Microphone.Start(null, true, 1, 44100);
		while (Microphone.GetPosition (null) <= 0) {	
			// Waiting
		}
		source.Play ();

		text = GetComponentInChildren<Text> ();

	}

	void SetupDecoder() {
		Config c = Decoder.DefaultConfig();
		c.SetString ("-hmm", "/usr/local/share/pocketsphinx/model/en-us/en-us");
		c.SetString ("-dict", "/usr/local/share/pocketsphinx/model/en-us/cmudict-en-us.dict");
		//c.SetString ("-rawlogdir", "/tmp");

		c.SetString ("-keyphrase", "oh unity");
		c.SetFloat ("-kws_threshold", 1e-30);

		c.SetFloat ("-samprate", 44100);
		c.SetInt ("-nfft", 2048);

		d = new Decoder(c);
		d.StartUtt ();
	}

	void Update() {
		if (countdown > 0) {
			Debug.Log ("Countdown " + countdown);

			if (countdown == MAX_COUNTDOWN) {
				text.text = "Yes master";
			}
			countdown--;
			if (countdown == 0) {
				text.text = "Say \"oh unity\"";
			}

		}
	}
		
	static byte[] convertToBytes (float[] data, int channels)
	{
		byte[] byteData = new byte[data.Length / channels * 2];
		for (int i = 0; i < data.Length / channels; i++) {
			float sum = 0;
			for (int j = 0; j < channels; j++) {
				sum += data [i * channels + j];
			}
			short val = (short)(sum / channels * 20000 / 0.05); // volume
			byteData [2 * i] = (byte) (val & 0xff);
			byteData [2 * i + 1] = (byte) (val >> 8);
		}
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
