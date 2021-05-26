using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmoticonMathScript : MonoBehaviour
{
	public KMAudio audio;
	public KMSelectable[] buttons;

	public TextMesh displayText;

	private readonly string[][] emojiNumbers = new string[][]
	{
		new string[] {"!", "\"", "#"},
		new string[] {"$", "%", "&"},
		new string[] {"\'", "(", ")"},
		new string[] {"*", "+", ","},
		new string[] {"-", ".", "/"},
		new string[] {"0", "1", "2"},
		new string[] {"3", "4", "5"},
		new string[] {"6", "7", "8"},
		new string[] {"9", ":", ";"},
		new string[] {"<", "=", ">"},
		new string[] {"?", "@", "A"},
		new string[] {"B", "C", "D"},
		new string[] {"E", "F", "G" }
	};
	private float[] sizeVals = new float[] { 0.0032f, 0.0028f, 0.0023f };
	private string generatedProblem;
	private string input = "";
	private string sign = "";
	private string answer;

	static int moduleIdCounter = 1;
	int moduleId;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		foreach (KMSelectable obj in buttons)
		{
			KMSelectable pressed = obj;
			pressed.OnInteract += delegate () { PressButton(pressed); return false; };
		}
	}

	void Start()
	{
		displayText.text = string.Empty;
		GenerateProblem();
		GetComponent<KMBombModule>().OnActivate += OnActivate;
	}

	void OnActivate()
	{
		displayText.gameObject.transform.localScale = new Vector3(sizeVals[generatedProblem.Length - 3], sizeVals[generatedProblem.Length - 3], 0.005f);
		displayText.text = generatedProblem;
	}

	void PressButton(KMSelectable pressed)
	{
		audio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (Array.IndexOf(buttons, pressed) == 10)
		{
			if (sign.Equals("-"))
			{
				sign = "";
				Debug.LogFormat("[Emoticon Math #{0}] You pressed Minus. Sign is now positive.", moduleId);
			}
			else
			{
				sign = "-";
				Debug.LogFormat("[Emoticon Math #{0}] You pressed Minus. Sign is now negative. Remember that this sticks even across strikes!", moduleId);
			}
		}
		else if (Array.IndexOf(buttons, pressed) == 11)
		{
			if (sign + input == answer)
			{
				GetComponent<KMBombModule>().HandlePass();
				Debug.LogFormat("[Emoticon Math #{0}] You submitted “{1}”. This is correct. Module solved.", moduleId, sign + input);
			}
			else
			{
				GetComponent<KMBombModule>().HandleStrike();
				Debug.LogFormat("[Emoticon Math #{0}] You submitted “{1}”. This is wrong. Strike!", moduleId, sign + input);
			}
			input = "";
		}
		else
		{
			input += Array.IndexOf(buttons, pressed);
			Debug.LogFormat("[Emoticon Math #{0}] You pressed {1}. Input is now “{2}”.", moduleId, Array.IndexOf(buttons, pressed), sign + input);
		}
	}

	void GenerateProblem()
	{
		int[] nums = new int[2];
		nums[0] = UnityEngine.Random.Range(0, 100);
		nums[1] = UnityEngine.Random.Range(0, 100);
		int sign = UnityEngine.Random.Range(10, 13);
		for (int i = 0; i < nums[0].ToString().Length; i++)
		{
			int rand = UnityEngine.Random.Range(0, 3);
			generatedProblem += emojiNumbers[int.Parse(nums[0].ToString()[i].ToString())][rand];
		}
		int rand2 = UnityEngine.Random.Range(0, 3);
		generatedProblem += emojiNumbers[sign][rand2];
		for (int i = 0; i < nums[1].ToString().Length; i++)
		{
			int rand = UnityEngine.Random.Range(0, 3);
			generatedProblem += emojiNumbers[int.Parse(nums[1].ToString()[i].ToString())][rand];
		}
		string log;
		switch (sign)
		{
			case 10:
				answer = (nums[0] + nums[1]).ToString();
				log = nums[0] + " + " + nums[1];
				break;
			case 11:
				answer = (nums[0] - nums[1]).ToString();
				log = nums[0] + " - " + nums[1];
				break;
			default:
				answer = (nums[0] * nums[1]).ToString();
				log = nums[0] + " * " + nums[1];
				break;
		}
		Debug.LogFormat("[Emoticon Math #{0}] Puzzle on module: “{1}”", moduleId, generatedProblem);
		Debug.LogFormat("[Emoticon Math #{0}] Decoded puzzle: “{1}”", moduleId, log);
		Debug.LogFormat("[Emoticon Math #{0}] Expected answer: “{1}”", moduleId, answer);
	}

	//twitch plays
	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Submit an answer using !{0} submit -47.";
	private bool negativeActiveTP = false;
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] commands = command.ToLowerInvariant().Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

		int check;
		if (commands.Length != 2 || commands[0] != "submit" || !int.TryParse(commands[1], out check)) yield break;
		List<int> buttonIndexes = new List<int>();
		bool negative = false;

		int index = 0;
		foreach (char c in commands[1])
		{
			if (c == '-' && index == 0)
			{
				negative = true;
			}
			else
			{
				int num;
				if (int.TryParse(c.ToString(), out num))
				{
					buttonIndexes.Add(num);
				}
				else
				{
					yield break;
				}
			}

			index++;
		}

		yield return null;
		if (negative != negativeActiveTP)
		{
			buttons[10].OnInteract();
			negativeActiveTP = negative;
			yield return new WaitForSeconds(0.1f);
		}

		foreach (int ind in buttonIndexes) { buttons[ind].OnInteract(); yield return new WaitForSeconds(0.1f); }

		buttons[11].OnInteract();
	}

	IEnumerator TwitchHandleForcedSolve()
	{
		string tempAns = answer.Replace("-", "");
		if (input.Length > tempAns.Length)
		{
			GetComponent<KMBombModule>().HandlePass();
			yield break;
		}
		else
		{
			for (int i = 0; i < input.Length; i++)
			{
				if (input[i] != tempAns[i])
                {
					GetComponent<KMBombModule>().HandlePass();
					yield break;
				}
			}
		}
		if ((answer.Contains("-") && sign == "") || (!answer.Contains("-") && sign == "-"))
        {
			buttons[10].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
		int start = input.Length;
		for (int i = start; i < tempAns.Length; i++)
        {
			buttons[int.Parse(tempAns[i].ToString())].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
		buttons[11].OnInteract();
	}
}