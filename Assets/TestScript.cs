using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pocketsphinx;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class TestScript : MonoBehaviour {

	AudioSource source;
	Decoder d;

	void Start () {
		Debug.Log ("Started");

		source = GetComponent<AudioSource>();
		source.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
		source.mute = true;
		source.Play ();
		SetupDecoder ();
	}

	void SetupDecoder() {
		Config c = Decoder.DefaultConfig();
		c.SetString("-hmm", "/usr/local/share/pocketsphinx/model/en-us/en-us");
		c.SetString("-lm", "/usr/local/share/pocketsphinx/model/en-us/en-us.lm.bin");
		c.SetString("-dict", "/usr/local/share/pocketsphinx/model/en-us/cmudict-en-us.dict");
		Decoder d = new Decoder(c);

		byte[] data = File.ReadAllBytes("goforward.raw");
		d.StartUtt();
		d.ProcessRaw(data, data.Length, false, false);
		d.EndUtt();

		Debug.Log(string.Format("Result is '{0}'", d.Hyp().Hypstr));

		foreach (Segment s in d.Seg()) {
			Debug.Log(s);
		}

	}

	void Update() {
		float[] data = new float[2048];
		source.GetOutputData (data, 0);
	//	Debug.Log ("Captured data");
	}
}
