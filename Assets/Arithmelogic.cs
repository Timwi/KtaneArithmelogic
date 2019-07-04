using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using KModkit;
using UnityEngine;

using Rnd = UnityEngine.Random;

public class Arithmelogic : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    public KMSelectable SubmitButton;
    public TextMesh SubmitButtonSymbol;
    public KMSelectable[] NumberButtons;
    public TextMesh[] NumberDisplays;
    public TextMesh EquationText;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private int[][] selectableValues;
    private int submitSymbol;
    private int[] offsets;
    private int abOperator;
    private int bcOperator;
    private bool abParen;
    private readonly int[] currentDisplays = new int[3];
    private bool isSolved = false;

    private static readonly string[] operators = new string[] { "∧", "∨", "⊻", "→", "|", "↓", "↔", "←" };

    private static readonly string[] symbols = new string[22]
    {
        "©", "Ѯ", "★", "Җ", "Ѭ", "₠", "Ϡ", "Ѧ",
        "æ", "Ԇ", "ϫ", "Ӭ", "Ͼ",  "Ѫ", "Ҩ",
        "Ϙ", "ζ", "Ͽ", "ƛ", "€", "☆", "œ"
    };

    //"the symbol is equal to " + 
    private static readonly string[] symbolValueNames = new string[22]
    {
        "the submit symbol's position in the manual's table",
        "the earliest position of serial number letters in the English alphabet",
        "the average serial number digit, rounded up",
        "the number of indicators",
        "the number of battery holders",
        "triple the number of lit indicators",
        "the day of the month when the bomb was activated",
        "the lowest serial number digit plus five",
        "the total number of ports",
        "the number of unlit indicators times four",
        "the number of serial number consonants times five",
        "the number of batteries",
        "the sum of the serial number's digits",
        "the latest position of serial number letters in the English alphabet",
        "the total number of modules on bomb modulo 25",
        "the number of serial number vowels times six",
        "the number of batteries plus indicators",
        "the number of lit indicators plus port plates",
        "the number of port plates",
        "the highest serial number digit",
        "fifteen",
        "the number of unlit indicators plus battery holders"
    };

    //"a number is true if " + 
    private static readonly string[] symbolConditionNames = new string[22]
    {
        "that number is even",
        "that number is a multiple of 7 or 13",
        "that number modulo 3 = 1",
        "that number is prime",
        "any of that number's digits are odd",
        "that number modulo 5 = 2 or 4",
        "that number's digits add up to 9 to 13",
        "that number's last two digits are within two of each other",
        "that number modulo 7 = 1, 3, or 6",
        "that number contains a 3 or 6",
        "that number's digital root is odd",
        "that number is a multiple of 4",
        "that number's digits add up to an odd number",
        "that number is odd",
        "that number's digits don't add up to 7 to 11",
        "that number is a multiple of 6",
        "that number's digital root is even",
        "that number is composite",
        "that number's digits add up to an even number",
        "that number contains a 2 or 9",
        "that number modulo 4 = 1",
        "that number's last two digits are at least five apart"
    };

    private static readonly int[] primes = new int[]
    {
        2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47,
        53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101,
        103, 107, 109, 113, 127, 131, 137, 139, 149,
        151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199
    };

    private static bool isPrime(int n)
    {
        if (n > 2 && n % 2 == 0)
            return false;
        if (n <= 202)
            return primes.Contains(n);

        // In case someone has hundreds of widgets on a bomb, this will be slower to calculate but at least it will work correctly
        var max = (int) Math.Sqrt(n);
        for (int i = 3; i <= max; i += 2)
            if (n % i == 0)
                return false;
        return true;
    }

    void Start()
    {
        _moduleId = _moduleIdCounter++;

        for (int i = 0; i < 3; i++)
            NumberButtons[i].OnInteract = handleButtonPress(i);
        SubmitButton.OnInteract += doSubmit;

        abOperator = Rnd.Range(0, 8);
        bcOperator = Rnd.Range(0, 8);
        abParen = Rnd.Range(0, 2) != 0;
        var symbolIxs = Enumerable.Range(0, symbols.Length).ToList().Shuffle();
        offsets = symbolIxs.Take(3).Select(ix => getSymbolValue(ix)).ToArray();
        submitSymbol = symbolIxs[3];

        var equationText = string.Format(abParen ? @"({0} {1} {2}) {3} {4}" : @"{0} {1} ({2} {3} {4})", symbols[symbolIxs[0]], operators[abOperator], symbols[symbolIxs[1]], operators[bcOperator], symbols[symbolIxs[2]]);
        EquationText.text = equationText;
        SubmitButtonSymbol.text = symbols[submitSymbol];

        selectableValues = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            selectableValues[i] = new int[4];
            // Make sure there's at least one true total and at least one false total for each display
            do
            {
                selectableValues[i][0] = Rnd.Range(10, 22);
                for (int vSet = 1; vSet < 4; vSet++)
                    selectableValues[i][vSet] = selectableValues[i][vSet - 1] + Rnd.Range(4, 10);
            }
            while (selectableValues[i].All(v => isNumberTrue(v + offsets[i])) || selectableValues[i].All(v => !isNumberTrue(v + offsets[i])));
        }

        UpdateNumberDisplays();

        Debug.LogFormat("[Arithmelogic #{0}] The given statement is {1}.", _moduleId, equationText);
        Debug.LogFormat("[Arithmelogic #{0}] (Table positions are {1}, {2}, and {3} for A, B, and C respectively.)", _moduleId, symbolIxs[0] + 1, symbolIxs[1] + 1, symbolIxs[2] + 1);
        for (int i = 0; i < 3; i++)
            Debug.LogFormat("[Arithmelogic #{0}] {4}'s symbol, {1}, is equal to {2}, which is {3}.", _moduleId, symbols[symbolIxs[i]], symbolValueNames[symbolIxs[i]], offsets[i], "ABC"[i]);

        Debug.LogFormat("[Arithmelogic #{0}] The submit button's symbol, {1}, means a number is true if {2}.", _moduleId, symbols[submitSymbol], symbolConditionNames[submitSymbol]);

        for (int i = 0; i < 3; i++)
            Debug.LogFormat("[Arithmelogic #{0}] {1}’s values are {2}, which become {3}, which are {4}.",
                _moduleId, "ABC"[i], selectableValues[i].Join("/"), selectableValues[i].Select(v => v + offsets[i]).Join("/"), selectableValues[i].Select(v => isNumberTrue(v + offsets[i])).Join("/"));
    }

    private KMSelectable.OnInteractHandler handleButtonPress(int i)
    {
        return delegate
        {
            NumberButtons[i].AddInteractionPunch(.5f);
            if (isSolved)
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, NumberButtons[i].transform);
            else
            {
                Audio.PlaySoundAtTransform("beep1", NumberButtons[i].transform);
                currentDisplays[i] = (currentDisplays[i] + 1) % 4;
                UpdateNumberDisplays();
            }
            return false;
        };
    }

    private void UpdateNumberDisplays()
    {
        for (int i = 0; i < 3; i++)
            NumberDisplays[i].text = selectableValues[i][currentDisplays[i]].ToString();
    }

    bool doSubmit()
    {
        if (!isSolved)
        {
            Debug.LogFormat("[Arithmelogic #{0}] Submitting values: {1}", _moduleId, selectableValues.Select((v, ix) => v[currentDisplays[ix]]).Join(", "));

            var abcValues = Enumerable.Range(0, 3).Select(ix => selectableValues[ix][currentDisplays[ix]] + offsets[ix]).ToArray();
            var abcTruths = abcValues.Select(isNumberTrue).ToArray();
            for (int i = 0; i < 3; i++)
                Debug.LogFormat("[Arithmelogic #{0}] {1} = {2} + {3} = {4}, which is {5}.",
                    _moduleId, "ABC"[i], offsets[i], selectableValues[i][currentDisplays[i]], abcValues[i], abcTruths[i]);

            var finalTruth = false;
            if (abParen)
            {
                finalTruth = figureTruth(figureTruth(abcTruths[0], abcTruths[1], abOperator), abcTruths[2], bcOperator);
                Debug.LogFormat("[Arithmelogic #{0}] ({4} {1} {5}) {2} {6} is {3}.", _moduleId, operators[abOperator], operators[bcOperator], finalTruth, abcTruths[0], abcTruths[1], abcTruths[2]);
            }
            else
            {
                finalTruth = figureTruth(abcTruths[0], figureTruth(abcTruths[1], abcTruths[2], bcOperator), abOperator);
                Debug.LogFormat("[Arithmelogic #{0}] {4} {1} ({5} {2} {6}) is {3}.", _moduleId, operators[abOperator], operators[bcOperator], finalTruth, abcTruths[0], abcTruths[1], abcTruths[2]);
            }

            var wrongString = "";
            if (!finalTruth)
                wrongString = "The statement was false! ";

            for (int i = 0; i < 3; i++)
            {
                for (int testIx = 3; testIx > currentDisplays[i]; testIx--)
                {
                    if (isNumberTrue(offsets[i] + selectableValues[i][testIx]) == abcTruths[i])
                    {
                        wrongString += string.Format("Element {0}’s display could have a higher value ({1} as opposed to the selected {2}) and A would still be {3}. ",
                            "ABC"[i], selectableValues[i][testIx], selectableValues[i][currentDisplays[i]], abcTruths[i]);
                        break;
                    }
                }
            }

            if (wrongString == "")
            {
                Debug.LogFormat("[Arithmelogic #{0}] Module disarmed!", _moduleId);
                isSolved = true;
                SubmitButtonSymbol.text = "!!";
                Audio.PlaySoundAtTransform("beep2", Module.transform);
                Module.HandlePass();
            }
            else
            {
                Debug.LogFormat("[Arithmelogic #{0}] {1}Strike given!", _moduleId, wrongString);
                wrongString = "";
                Module.HandleStrike();
            }
        }
        return false;
    }

    bool figureTruth(bool valA, bool valB, int ourOperator)
    {
        switch (ourOperator)
        {
            case 0: return valA && valB;
            case 1: return valA || valB;
            case 2: return valA ^ valB;
            case 3: return (!valA) || valB;
            case 4: return !(valA && valB);
            case 5: return !(valA || valB);
            case 6: return !(valA ^ valB);
            case 7: return valA || (!valB);
            default: return true;
        }
    }

    bool isNumberTrue(int n)
    {
        var nStr = n.ToString();
        var digitSum = nStr.Select(d => d - '0').Sum();
        var lastTwo = nStr.Substring(nStr.Length - 2);
        switch (submitSymbol)
        {
            case 0: return n % 2 == 0;
            case 1: return n % 7 == 0 || n % 13 == 0;
            case 2: return n % 3 == 1;
            case 3: return primes.Contains(n);
            case 4: return n.ToString().Any(d => "13579".Contains(d)); // any digit is odd
            case 5: return n % 5 == 2 || n % 5 == 4;
            case 6: return digitSum >= 9 && digitSum <= 13;
            case 7: return lastTwo[0] - lastTwo[1] >= -2 && lastTwo[0] - lastTwo[1] <= 2;
            case 8: return n % 7 == 1 || n % 7 == 3 || n % 7 == 6;
            case 9: return nStr.Contains("3") || nStr.Contains("6");
            case 10: return ((n + 1) % 9) % 2 == 0; // digital root is odd
            case 11: return n % 4 == 0;
            case 12: return digitSum % 2 == 1;
            case 13: return n % 2 != 0;
            case 14: return digitSum < 7 || digitSum > 11;
            case 15: return n % 6 == 0;
            case 16: return ((n + 1) % 9) % 2 == 1; // digital root is even
            case 17: return n > 3 && !isPrime(n);
            case 18: return digitSum % 2 == 0;
            case 19: return nStr.Contains("2") || nStr.Contains("9");
            case 20: return n % 4 == 1;
            default: return lastTwo[0] - lastTwo[1] <= -5 || lastTwo[0] - lastTwo[1] >= 5;
        }
    }

    int getSymbolValue(int sym)
    {
        var sn = Bomb.GetSerialNumber();
        var snSum = Bomb.GetSerialNumberNumbers().Sum();
        switch (sym)
        {
            case 0: //submit symbol pos. in table
                return submitSymbol + 1;
            case 1: //earliest SN letter position
                return Bomb.GetSerialNumberLetters().Min() - 'A' + 1;
            case 2: //average SN digit, round up
                var numDigits = Bomb.GetSerialNumberNumbers().Count();
                return (snSum % numDigits == 0) ? (snSum / numDigits) : (snSum / numDigits + 1);
            case 3: //indicators
                return Bomb.GetIndicators().Count();
            case 4: //holders
                return Bomb.GetBatteryHolderCount();
            case 5: //triple on indicators
                return 3 * Bomb.GetOnIndicators().Count();
            case 6: //day of month
                return DateTime.Now.Date.Day;
            case 7: //low sn digit plus five
                return 5 + Bomb.GetSerialNumberNumbers().Min();
            case 8: //ports
                return Bomb.GetPorts().Count();
            case 9: //unlits * 4
                return 4 * Bomb.GetOffIndicators().Count();
            case 10: //consonants * 5
                return 5 * sn.Count(ch => "BCDFGHJKLMNPQRSTVWXYZ".Contains(ch));
            case 11: //batteries
                return Bomb.GetBatteryCount();
            case 12: //sum of SN digits
                return snSum;
            case 13: //latest position
                return Bomb.GetSerialNumberLetters().Max() - 'A' + 1;
            case 14: //bomb modules
                return Bomb.GetModuleNames().Count() % 25;
            case 15: // SN vowels * 6
                return 6 * sn.Count(ch => "AEIOU".Contains(ch));
            case 16: //bat + ind
                return Bomb.GetBatteryCount() + Bomb.GetIndicators().Count();
            case 17: //lit + plates
                return Bomb.GetOnIndicators().Count() + Bomb.GetPortPlateCount();
            case 18: //plates
                return Bomb.GetPortPlateCount();
            case 19: //hi digit
                return Bomb.GetSerialNumberNumbers().Max();
            case 20: //15
                return 15;
            case 21: //unlit + holders
                return Bomb.GetOffIndicators().Count() + Bomb.GetBatteryHolderCount();
            default:
                return sym;
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} cycle | !{0} cycle A/B/C | !{0} toggle [press each screen once] | !{0} submit 97 98 99";
#pragma warning restore 414

    private IEnumerable<object> CycleScreen(int ix)
    {
        for (int cNum = 0; cNum < 4; cNum++)
        {
            yield return new WaitForSeconds(1.2f);
            yield return new[] { NumberButtons[ix] };
            yield return "trycancel";
        }
    }

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*(?:cycle|c)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            for (int i = 0; i < 3; i++)
                foreach (var obj in CycleScreen(i))
                    yield return obj;
            yield break;
        }

        if (Regex.IsMatch(command, @"^\s*(?:toggle|t)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return NumberButtons;
            yield break;
        }

        Match m;
        if ((m = Regex.Match(command, @"^\s*(?:cycle|c)\s+([abc])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            yield return null;
            foreach (var obj in CycleScreen("abcABC".IndexOf(m.Groups[1].Value[0]) % 3))
                yield return obj;
            yield break;
        }

        if ((m = Regex.Match(command, @"^\s*(?:submit|s|enter|go)\s+(\d+)\s+(\d+)\s+(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
        {
            int[] values = new int[4];
            if (!int.TryParse(m.Groups[1].Value, out values[0]) || !int.TryParse(m.Groups[2].Value, out values[1]) || !int.TryParse(m.Groups[3].Value, out values[2]) ||
                Enumerable.Range(0, selectableValues.Length).Any(ix => !selectableValues[ix].Contains(values[ix])))
            {
                yield return "sendtochaterror Those numbers aren’t valid. Check they’re all there.";
                yield break;
            }

            yield return null;
            for (int i = 0; i < 3; i++)
                yield return Enumerable.Repeat(NumberButtons[i], (Array.IndexOf(selectableValues[i], values[i]) - currentDisplays[i] + 4) % 4);
            yield return new[] { SubmitButton };
            yield break;
        }
    }
}
